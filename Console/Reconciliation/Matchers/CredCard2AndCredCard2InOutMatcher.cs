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
    internal class CredCard2AndCredCard2InOutMatcher : IMatcher
    {
        private readonly IInputOutput _input_output;

        public CredCard2AndCredCard2InOutMatcher(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public void Do_matching(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var loading_info = new CredCard2AndCredCard2InOutLoader().Loading_info();
            loading_info.File_paths = main_file_paths;
            var file_loader = new FileLoader(_input_output);
            ReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliation_interface
                = file_loader.Load_files_and_merge_data<CredCard2Record, CredCard2InOutRecord>(loading_info, spreadsheet_factory, this);
            reconciliation_interface?.Do_the_matching();
        }

        public void Do_preliminary_stuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Do_amazon_transaction_matching(reconciliator, reconciliation_interface);
        }

        private void Do_amazon_transaction_matching<TThirdPartyType, TOwnedType>(
            IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
            IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Filter_for_all_amazon_transactions_from_cred_card2(reconciliator);
            Filter_for_all_amazon_transactions_from_cred_card2_in_out(reconciliator);
            reconciliator.Set_match_finder(Find_Amazon_matches);
            reconciliator.Set_record_matcher(Match_specified_records);

            reconciliation_interface.Do_semi_automatic_matching();

            reconciliator.Refresh_files();
            reconciliator.Reset_match_finder();
            reconciliator.Reset_record_matcher();
        }

        public void Finish()
        {
        }

        public void Filter_for_all_amazon_transactions_from_cred_card2<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.Filter_third_party_file(Is_not_third_party_amazon_transaction);
        }

        public bool Is_not_third_party_amazon_transaction<TThirdPartyType>(TThirdPartyType cred_card2_record) where TThirdPartyType : ICSVRecord, new()
        {
            return !cred_card2_record.Description.Remove_punctuation().ToUpper().Contains(ReconConsts.Amazon_description);
        }

        public void Filter_for_all_amazon_transactions_from_cred_card2_in_out<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.Filter_owned_file(Is_not_owned_amazon_transaction);
        }

        public bool Is_not_owned_amazon_transaction<TOwnedType>(TOwnedType cred_card2_in_out_record) where TOwnedType : ICSVRecord, new()
        {
            return !cred_card2_in_out_record.Description.Remove_punctuation().ToUpper().Contains(ReconConsts.Amazon_description);
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
                Create_new_combined_record(record_for_matching, match_index, owned_file);
            }
            Match_records(record_for_matching.SourceRecord, record_for_matching.Matches[match_index].Actual_records[0]);
        }

        public void Create_new_combined_record<TThirdPartyType, TOwnedType>(
                RecordForMatching<TThirdPartyType> record_for_matching,
                int match_index,
                ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            foreach (var actual_record in record_for_matching.Matches[match_index].Actual_records)
            {
                owned_file.Remove_record_permanently((TOwnedType)actual_record);
            }
            TOwnedType new_match = New_combined_record<TThirdPartyType, TOwnedType>(record_for_matching, match_index);

            record_for_matching.Matches[match_index].Actual_records.Clear();
            record_for_matching.Matches[match_index].Actual_records.Add(new_match);
            owned_file.Add_record_permanently(new_match);
        }

        private TOwnedType New_combined_record<TThirdPartyType, TOwnedType>(
                RecordForMatching<TThirdPartyType> record_for_matching,
                int match_index)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            var new_match = new TOwnedType
            {
                Date = record_for_matching.SourceRecord.Date,
                Description = Create_new_description<TThirdPartyType, TOwnedType>(record_for_matching, match_index)
            };
            (new_match as CredCard2InOutRecord).Unreconciled_amount = record_for_matching.SourceRecord.Main_amount();
            return new_match;
        }

        private void Match_records<TThirdPartyType>(TThirdPartyType source, ICSVRecord match) where TThirdPartyType : ICSVRecord, new()
        {
            match.Matched = true;
            (source as ICSVRecord).Matched = true;
            match.Match = source;
            (source as ICSVRecord).Match = match;
        }

        private string Create_new_description<TThirdPartyType, TOwnedType>(
                RecordForMatching<TThirdPartyType> record_for_matching,
                int match_index)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            var combined_amounts = $"{Get_description_without_amazon(record_for_matching.Matches[match_index].Actual_records[0])}";
            for (int count = 1; count < record_for_matching.Matches[match_index].Actual_records.Count; count++)
            {
                var match = record_for_matching.Matches[match_index].Actual_records[count];
                combined_amounts += $", {Get_description_without_amazon(match)}";
            }

            var sum_of_all_matches = record_for_matching.Matches[match_index].Actual_records.Sum(x => x.Main_amount());
            var expense_amounts_match = record_for_matching.SourceRecord.Main_amount()
                .Double_equals(sum_of_all_matches);
            var extra_text = expense_amounts_match
                ? ""
                : $"{ReconConsts.AmazonTransactionsDontAddUp} ({sum_of_all_matches.To_csv_string(true)})"; 

            return $"{ReconConsts.SeveralAmazonTransactions} ({combined_amounts}){extra_text}";
        }

        private string Get_description_without_amazon(ICSVRecord match)
        {
            string new_description = match.Description;

            var amazon_index_with_space = match.Description.IndexOf($"{ReconConsts.Amazon_description} ", StringComparison.CurrentCultureIgnoreCase);
            if (amazon_index_with_space >= 0)
            {
                new_description = match.Description.Remove(amazon_index_with_space, ReconConsts.Amazon_description.Length + 1);
            }
            else
            {
                var amazon_index_no_space = match.Description.IndexOf(ReconConsts.Amazon_description, StringComparison.CurrentCultureIgnoreCase);
                if (amazon_index_no_space >= 0)
                {
                    new_description = match.Description.Remove(amazon_index_no_space, ReconConsts.Amazon_description.Length);
                }
            }

            new_description = $"{new_description} {match.Main_amount().To_csv_string(true)}";
            return new_description;
        }

        public IEnumerable<IPotentialMatch> Find_Amazon_matches<TThirdPartyType, TOwnedType>
            (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            return Debug_find_Amazon_matches(source_record as CredCard2Record, owned_file as ICSVFile<CredCard2InOutRecord>);
        }

        private IEnumerable<IPotentialMatch> Debug_find_Amazon_matches(CredCard2Record source_record, ICSVFile<CredCard2InOutRecord> owned_file)
        {
            var result = new List<IPotentialMatch>();
            var random_number_generator = new Random();

            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 4);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);

            return result;
        }

        private static void Add_set_of_overlapping_matches(
            Random random_number_generator,
            ICSVFile<CredCard2InOutRecord> owned_file,
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