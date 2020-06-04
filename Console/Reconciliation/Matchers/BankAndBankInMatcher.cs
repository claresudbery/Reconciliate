using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class BankAndBankInMatcher : IMatcher
    {
        public List<ExpectedIncomeRecord> MatchedExpectedIncomeRecords;

        private readonly IInputOutput _input_output;
        private readonly IBankAndBankInLoader _bank_and_bank_in_loader;

        public BankAndBankInMatcher(
            IInputOutput input_output, 
            IBankAndBankInLoader bank_and_bank_in_loader)
        {
            _input_output = input_output;
            _bank_and_bank_in_loader = bank_and_bank_in_loader;
            MatchedExpectedIncomeRecords = new List<ExpectedIncomeRecord>();
        }

        public void Do_matching(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var loading_info = ((BankAndBankInLoader)_bank_and_bank_in_loader).Loading_info();
            loading_info.File_paths = main_file_paths;
            var file_loader = new FileLoader(_input_output, new Clock());
            ReconciliationInterface<ActualBankRecord, BankRecord> reconciliation_interface
                = file_loader.Load_files_and_merge_data<ActualBankRecord, BankRecord>(loading_info, spreadsheet_factory, this);
            reconciliation_interface?.Do_the_matching();
        }

        public void Do_preliminary_stuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Do_employer_expense_matching(
                reconciliator as IReconciliator<ActualBankRecord, BankRecord>, 
                reconciliation_interface as IReconciliationInterface<ActualBankRecord, BankRecord>);
        }

        private void Do_employer_expense_matching(
                IReconciliator<ActualBankRecord, BankRecord> reconciliator,
                IReconciliationInterface<ActualBankRecord, BankRecord> reconciliation_interface)
        {
            Filter_for_all_expense_transactions_from_actual_bank_in(reconciliator);
            Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);
            reconciliator.Set_match_finder(Find_expense_matches);
            reconciliator.Set_record_matcher(Match_specified_records);

            reconciliation_interface.Do_semi_automatic_matching();

            reconciliator.Refresh_files();
            Remove_expense_rows_that_didnt_get_matched(reconciliator);
            reconciliator.Reset_match_finder();
            reconciliator.Reset_record_matcher();
        }

        public void Finish()
        {
            _bank_and_bank_in_loader.Finish();
        }

        public void Filter_for_all_expense_transactions_from_actual_bank_in(IReconciliator<ActualBankRecord, BankRecord> reconciliator)
        {
            reconciliator.Filter_third_party_file(Is_not_expense_transaction);
        }

        public bool Is_not_expense_transaction(ActualBankRecord actual_bank_record)
        {
            return actual_bank_record.Description.Remove_punctuation().ToUpper()
                   != ReconConsts.Employer_expense_description;
        }

        public void Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(IReconciliator<ActualBankRecord, BankRecord> reconciliator)
        {
            reconciliator.Filter_owned_file(Is_not_wages_row_or_expense_transaction);
        }

        public bool Is_not_wages_row_or_expense_transaction(BankRecord bank_record)
        {
            return bank_record.Type != Codes.Expenses
                && !bank_record.Description.Contains(ReconConsts.Employer_expense_description);
        }

        public void Remove_expense_rows_that_didnt_get_matched(IReconciliator<ActualBankRecord, BankRecord> reconciliator)
        {
            reconciliator.Filter_owned_file(Is_unmatched_expense_row);
        }

        public bool Is_unmatched_expense_row(BankRecord bank_record)
        {
            return bank_record.Type == Codes.Expenses
                   && bank_record.Matched == false
                   && bank_record.Match == null;
        }

        public void Match_specified_records(
                RecordForMatching<ActualBankRecord> record_for_matching,
                int match_index,
                ICSVFile<BankRecord> owned_file)
        {
            if (record_for_matching.Matches[match_index].Actual_records.Count > 1)
            {
                Create_new_combined_record(record_for_matching, match_index, owned_file);
            }
            else
            {
                _bank_and_bank_in_loader.Update_expected_income_record_when_matched(
                    record_for_matching.SourceRecord,
                    (BankRecord)record_for_matching.Matches[match_index].Actual_records[0]);
            }
            Match_records(
                record_for_matching.SourceRecord,
                (record_for_matching.Matches[match_index].Actual_records[0] as BankRecord));
        }

        public void Create_new_combined_record(
                RecordForMatching<ActualBankRecord> record_for_matching,
                int match_index,
                ICSVFile<BankRecord> owned_file)
        {
            var sum_of_all_matches =
                record_for_matching.Matches[match_index].Actual_records.Sum(x => x.Main_amount());
            var missing_balance = record_for_matching.SourceRecord.Main_amount() - sum_of_all_matches;
            var expense_amounts_match = record_for_matching.SourceRecord.Main_amount()
                .Double_equals(sum_of_all_matches);

            BankRecord new_match = New_combined_record(
                record_for_matching, 
                match_index, 
                expense_amounts_match,
                sum_of_all_matches);
            Update_expected_income_and_owned_files(
                record_for_matching, 
                match_index, 
                expense_amounts_match,
                sum_of_all_matches,
                missing_balance,
                owned_file);

            record_for_matching.Matches[match_index].Actual_records.Clear();
            record_for_matching.Matches[match_index].Actual_records.Add(new_match);
            owned_file.Add_record_permanently(new_match);
        }

        private void Update_expected_income_and_owned_files(
                RecordForMatching<ActualBankRecord> record_for_matching,
                int match_index,
                bool expense_amounts_match,
                double sum_of_all_matches,
                double missing_balance,
                ICSVFile<BankRecord> owned_file)
        {
            foreach (var actual_record in record_for_matching.Matches[match_index].Actual_records)
            {
                _bank_and_bank_in_loader.Update_expected_income_record_when_matched(record_for_matching.SourceRecord, actual_record);
                owned_file.Remove_record_permanently((BankRecord)actual_record);
            }

            if (!expense_amounts_match)
            {
                _bank_and_bank_in_loader.Create_new_expenses_record_to_match_balance(
                    record_for_matching.SourceRecord,
                    missing_balance);
            }
        }

        private BankRecord New_combined_record(
                RecordForMatching<ActualBankRecord> record_for_matching,
                int match_index,
                bool expense_amounts_match,
                double sum_of_all_matches)
        {
            var new_match = new BankRecord
            {
                Date = record_for_matching.SourceRecord.Date,
                Description = Create_new_description(record_for_matching.Matches[match_index], expense_amounts_match, sum_of_all_matches)
            };
            new_match.Unreconciled_amount = record_for_matching.SourceRecord.Main_amount();
            new_match.Type = (record_for_matching.Matches[match_index].Actual_records[0] as BankRecord).Type;
            return new_match;
        }

        private void Match_records(ActualBankRecord source, BankRecord match)
        {
            match.Matched = true;
            source.Matched = true;
            match.Match = source;
            source.Match = match;
        }

        private string Create_new_description(
            IPotentialMatch potential_match, 
            bool expense_amounts_match,
            double sum_of_all_matches)
        {
            var combined_amounts = potential_match.Actual_records[0].Main_amount().To_csv_string(true);
            for (int count = 1; count < potential_match.Actual_records.Count; count++)
            {
                combined_amounts += $", {potential_match.Actual_records[count].Main_amount().To_csv_string(true)}";
            }

            var extra_text = expense_amounts_match
                ? ""
                : $"{ReconConsts.ExpensesDontAddUp} ({sum_of_all_matches.To_csv_string(true)})";

            return $"{ReconConsts.SeveralExpenses} ({combined_amounts}){extra_text}";
        }

        public IEnumerable<IPotentialMatch> Find_expense_matches(ActualBankRecord source_record, ICSVFile<BankRecord> owned_file)
        {
            var generic_matcher = new MultipleAmountMatcher<ActualBankRecord, BankRecord>();
            return generic_matcher.Find_matches(source_record, owned_file);
        }
    }
}