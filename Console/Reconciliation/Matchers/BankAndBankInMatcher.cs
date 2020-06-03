using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.Delegates;
using Interfaces.DTOs;

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
            var file_loader = new FileLoader(_input_output);
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
            return Debug_find_expense_matches(source_record, owned_file);
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

        private IEnumerable<MatchList> Find_match_lists(double target_amount, IEnumerable<double> candidates)
        {
            List<MatchList> results = new List<MatchList>();

            var concrete_candidates = candidates.ToList();
            concrete_candidates.RemoveAll(x => x > target_amount);
            double candidate_total = concrete_candidates.Sum();

            if (candidate_total.Equals(target_amount))
            {
                results.Add(new MatchList
                {
                    TargetAmount = target_amount,
                    Matches = new List<double>(concrete_candidates)
                });
            }
            else if (candidate_total < target_amount)
            {
                // !! We need this to sometimes be an empty list!
                results.Add(new MatchList
                {
                    TargetAmount = target_amount,
                    Matches = new List<double>(concrete_candidates)
                });
            }
            else
            {
                foreach (double candidate in concrete_candidates)
                {
                    List<double> new_candidates = new List<double>(concrete_candidates);
                    new_candidates.Remove(candidate);
                    double new_target = target_amount - candidate;
                    IEnumerable<MatchList> new_match_lists = Find_match_lists(new_target, new_candidates);
                    foreach (MatchList match_list in new_match_lists)
                    {
                        var new_result = new MatchList
                        {
                            TargetAmount = target_amount,
                            Matches = match_list.Matches.Concat(new List<double> {candidate}).ToList()
                        };
                        if (!results.Any(x => x.Matches.SequenceEqual(new_result.Matches)))
                        {
                            results.Add(new_result);
                        }
                    }
                }
            }

            return results;
        }
    }

    class MatchList
    {
        public double TargetAmount { get; set; }
        public List<double> Matches { get; set; }

        public double ActualAmount()
        {
            return Matches.Sum();
        }

        public bool ExactMatch()
        {
            return ActualAmount().Equals(TargetAmount);
        }
    }
}