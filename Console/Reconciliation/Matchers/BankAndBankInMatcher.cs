using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class BankAndBankInMatcher : IMatcher
    {
        public List<ExpectedIncomeRecord> MatchedExpectedIncomeRecords;

        private readonly IInputOutput _input_output;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;
        private readonly IBankAndBankInLoader _bank_and_bank_in_loader;

        public BankAndBankInMatcher(
            IInputOutput input_output, 
            ISpreadsheetRepoFactory spreadsheet_factory,
            IBankAndBankInLoader bank_and_bank_in_loader)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
            _bank_and_bank_in_loader = bank_and_bank_in_loader;
            MatchedExpectedIncomeRecords = new List<ExpectedIncomeRecord>();
        }

        public void Do_matching(FilePaths main_file_paths)
        {
            var loading_info = ((BankAndBankInLoader)_bank_and_bank_in_loader).Loading_info();
            loading_info.File_paths = main_file_paths;
            var file_loader = new FileLoader(_input_output);
            ReconciliationInterface<ActualBankRecord, BankRecord> reconciliation_interface
                = file_loader.Load_files_and_merge_data<ActualBankRecord, BankRecord>(loading_info, _spreadsheet_factory, this);
            reconciliation_interface?.Do_the_matching();
        }

        public void Do_preliminary_stuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Do_employer_expense_matching(reconciliator, reconciliation_interface);
        }

        private void Do_employer_expense_matching<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Filter_for_all_expense_transactions_from_actual_bank_in(reconciliator);
            Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);
            reconciliator.Set_match_finder(Find_expense_matches);
            reconciliator.Set_record_matcher(Match_specified_records);

            reconciliation_interface.Do_semi_automatic_matching();

            reconciliator.Refresh_files();
            reconciliator.Reset_match_finder();
            reconciliator.Reset_record_matcher();
        }

        public void Filter_for_all_expense_transactions_from_actual_bank_in<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.Filter_third_party_file(Is_not_expense_transaction);
        }

        public bool Is_not_expense_transaction<TThirdPartyType>(TThirdPartyType actual_bank_record) where TThirdPartyType : ICSVRecord, new()
        {
            return actual_bank_record.Description.Remove_punctuation().ToUpper()
                   != ReconConsts.Employer_expense_description;
        }

        public void Filter_for_all_wages_rows_and_expense_transactions_from_expected_in<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.Filter_owned_file(Is_not_wages_row_or_expense_transaction);
        }

        public bool Is_not_wages_row_or_expense_transaction<TOwnedType>(TOwnedType bank_record) where TOwnedType : ICSVRecord, new()
        {
            return (bank_record as BankRecord).Type != Codes.Expenses
                && !bank_record.Description.Contains(ReconConsts.Employer_expense_description);
        }

        public List<ConsoleLine> Get_all_expense_transactions_from_actual_bank_in<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Filter_for_all_expense_transactions_from_actual_bank_in(reconciliator);
            return reconciliator.Third_party_file.Records.Select(x => x.To_console()).ToList();
        }

        public List<ConsoleLine> Get_all_wages_rows_and_expense_transactions_from_expected_in<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);
            return reconciliator.Owned_file.Records.Select(x => x.To_console()).ToList();
        }

        public void Match_specified_records<TThirdPartyType, TOwnedType>(
                RecordForMatching<TThirdPartyType> record_for_matching,
                int match_index,
                ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            if (record_for_matching.Matches[match_index].Actual_records.Count > 1)
            {
                var new_match = new TOwnedType
                {
                    Date = record_for_matching.SourceRecord.Date,
                    Description = Create_new_description(record_for_matching.Matches[match_index])
                };
                (new_match as BankRecord).Unreconciled_amount = record_for_matching.Matches[match_index].Actual_records.Sum(x => x.Main_amount());
                (new_match as BankRecord).Type = (record_for_matching.Matches[match_index].Actual_records[0] as BankRecord).Type;
                foreach (var actual_record in record_for_matching.Matches[match_index].Actual_records)
                {
                    _bank_and_bank_in_loader.Update_expected_income_record_when_matched(record_for_matching.SourceRecord, (TOwnedType)actual_record);
                    owned_file.Remove_record_permanently((TOwnedType)actual_record);
                }
                record_for_matching.Matches[match_index].Actual_records.Clear();
                record_for_matching.Matches[match_index].Actual_records.Add(new_match);
                owned_file.Add_record_permanently(new_match);
            }
            else
            {
                _bank_and_bank_in_loader.Update_expected_income_record_when_matched(
                    record_for_matching.SourceRecord,
                    (TOwnedType)record_for_matching.Matches[match_index].Actual_records[0]);
            }
            Match_records(record_for_matching.SourceRecord, record_for_matching.Matches[match_index].Actual_records[0]);
        }

        private void Match_records<TThirdPartyType>(TThirdPartyType source, ICSVRecord match) where TThirdPartyType : ICSVRecord, new()
        {
            match.Matched = true;
            (source as ICSVRecord).Matched = true;
            match.Match = source;
            (source as ICSVRecord).Match = match;
        }

        private string Create_new_description(IPotentialMatch potential_match)
        {
            var combined_amounts = potential_match.Actual_records[0].Main_amount().To_csv_string(true);
            for (int count = 1; count < potential_match.Actual_records.Count; count++)
            {
                combined_amounts += $", {potential_match.Actual_records[count].Main_amount().To_csv_string(true)}";
            }
            return $"{ReconConsts.SeveralExpenses} ({combined_amounts})";
        }

        public IEnumerable<IPotentialMatch> Find_expense_matches<TThirdPartyType, TOwnedType>
                (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            return Debug_find_expense_matches(source_record as ActualBankRecord, owned_file as ICSVFile<BankRecord>);
        }

        public IEnumerable<IPotentialMatch> Standby_find_expense_matches<TThirdPartyType, TOwnedType>
                (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            var result = new List<PotentialMatch>();
            if (owned_file.Records[0].Main_amount() == source_record.Main_amount())
            {
                var actual_records = new List<ICSVRecord>();
                actual_records.Add(owned_file.Records[0]);
                result.Add(new PotentialMatch {Actual_records = actual_records});
            }
            return result;
        }

        public void Finish()
        {
            _bank_and_bank_in_loader.Finish();
        }

        public void Debug_preliminary_stuff<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            List<ConsoleLine> all_expense_transactions_from_actual_bank_in = Get_all_expense_transactions_from_actual_bank_in(reconciliator);
            _input_output.Output_line("***********");
            _input_output.Output_line("All Expense Transactions From ActualBank In:");
            _input_output.Output_all_lines(all_expense_transactions_from_actual_bank_in);

            List<ConsoleLine> all_expense_transactions_from_expected_in = Get_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);
            _input_output.Output_line("***********");
            _input_output.Output_line("All Expense Transactions From Expected In:");
            _input_output.Output_all_lines(all_expense_transactions_from_expected_in);

            reconciliator.Refresh_files();

            _input_output.Get_input(ReconConsts.EnterAnyKeyToContinue);
        }

        private IEnumerable<IPotentialMatch> Debug_find_expense_matches(ActualBankRecord source_record, ICSVFile<BankRecord> owned_file)
        {
            var result = new List<IPotentialMatch>();
            var random_number_generator = new Random();

            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);

            return result;
        }

        private static void Add_set_of_overlapping_matches(
            Random random_number_generator,
            ICSVFile<BankRecord> owned_file, 
            List<IPotentialMatch> result, 
            int num_matches)
        {
            var unmatched_records = owned_file.Records.Where(x => !x.Matched).ToList();
            var max_rand = unmatched_records.Count - 1;
            if (max_rand >= 0)
            {
                var new_match = new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>(),
                    Console_lines = new List<ConsoleLine>(),
                    Rankings = new Rankings { Amount = 0, Date = 0, Combined = 0 },
                    Amount_match = true,
                    Full_text_match = true,
                    Partial_text_match = true
                };
                for (int count = 1; count <= num_matches; count++)
                {
                    if (max_rand >= 0)
                    {
                        var random_index = random_number_generator.Next(0, max_rand);
                        var next_record = unmatched_records[random_index];
                        new_match.Actual_records.Add(next_record);
                        new_match.Console_lines.Add(next_record.To_console());
                        unmatched_records.Remove(next_record);
                        max_rand--;
                    }
                }
                result.Add(new_match);
            }
        }
    }
}