using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class CredCard2AndCredCard2InOutMatcher : IMatcher
    {
        private readonly IInputOutput _input_output;

        private string _match_description_qualifier = "";
        private string _transactions_dont_add_up = "";
        private string _several_transactions = "";

        public CredCard2AndCredCard2InOutMatcher(IInputOutput input_output)
        {
            _input_output = input_output;
            SetAmazonStrings();
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
            Do_amazon_transaction_matching(
                reconciliator as IReconciliator<CredCard2Record, CredCard2InOutRecord>, 
                reconciliation_interface as IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>);

            Do_iTunes_transaction_matching(
                reconciliator as IReconciliator<CredCard2Record, CredCard2InOutRecord>,
                reconciliation_interface as IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>);
        }

        private void Do_amazon_transaction_matching(
            IReconciliator<CredCard2Record, CredCard2InOutRecord> reconciliator,
            IReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliation_interface)
        {
            SetAmazonStrings();
            Do_transaction_matching(reconciliator, reconciliation_interface);
        }

        private void Do_iTunes_transaction_matching(
            IReconciliator<CredCard2Record, CredCard2InOutRecord> reconciliator,
            IReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliation_interface)
        {
            SetiTunesStrings();
            Do_transaction_matching(reconciliator, reconciliation_interface);
        }

        private void Do_transaction_matching(
            IReconciliator<CredCard2Record, CredCard2InOutRecord> reconciliator,
            IReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliation_interface)
        {
            Filter_matching_transactions_from_cred_card2(reconciliator);
            Filter_matching_transactions_from_cred_card2_in_out(reconciliator);
            reconciliator.Set_match_finder(Find_matches);
            reconciliator.Set_record_matcher(Match_specified_records);

            reconciliator.Rewind();
            reconciliation_interface.Do_semi_automatic_matching();

            reconciliator.Refresh_files();
            reconciliator.Reset_match_finder();
            reconciliator.Reset_record_matcher();
        }

        public void Finish()
        {
        }

        public void SetAmazonStrings()
        {
            _match_description_qualifier = ReconConsts.Amazon_description;
            _transactions_dont_add_up = ReconConsts.TransactionsDontAddUp;
            _several_transactions = ReconConsts.SeveralAmazonTransactions;
        }

        public void SetiTunesStrings()
        {
            _match_description_qualifier = ReconConsts.iTunes_description;
            _transactions_dont_add_up = ReconConsts.TransactionsDontAddUp;
            _several_transactions = ReconConsts.SeveraliTunesTransactions;
        }

        public void Filter_matching_transactions_from_cred_card2(IReconciliator<CredCard2Record, CredCard2InOutRecord> reconciliator)
        {
            reconciliator.Filter_third_party_file(Is_not_third_party_matching_transaction);
        }

        public bool Is_not_third_party_matching_transaction(CredCard2Record cred_card2_record)
        {
            return !cred_card2_record.Description.ToUpper().Contains(_match_description_qualifier.ToUpper());
        }

        public void Filter_matching_transactions_from_cred_card2_in_out(IReconciliator<CredCard2Record, CredCard2InOutRecord> reconciliator)
        {
            reconciliator.Filter_owned_file(Is_not_owned_matching_transaction);
        }

        public bool Is_not_owned_matching_transaction(CredCard2InOutRecord cred_card2_in_out_record)
        {
            return !cred_card2_in_out_record.Description.ToUpper().Contains(_match_description_qualifier.ToUpper());
        }

        public void Match_specified_records(
            RecordForMatching<CredCard2Record> record_for_matching,
            int match_index,
            ICSVFile<CredCard2InOutRecord> owned_file)
        {
            if (record_for_matching.Matches[match_index].Actual_records.Count > 1)
            {
                Create_new_combined_record(record_for_matching, match_index, owned_file);
            }
            Match_records(record_for_matching.SourceRecord, record_for_matching.Matches[match_index].Actual_records[0]);
        }

        public void Create_new_combined_record(
                RecordForMatching<CredCard2Record> record_for_matching,
                int match_index,
                ICSVFile<CredCard2InOutRecord> owned_file)
        {
            foreach (var actual_record in record_for_matching.Matches[match_index].Actual_records)
            {
                owned_file.Remove_record_permanently((CredCard2InOutRecord)actual_record);
            }
            CredCard2InOutRecord new_match = New_combined_record(record_for_matching, match_index);

            record_for_matching.Matches[match_index].Actual_records.Clear();
            record_for_matching.Matches[match_index].Actual_records.Add(new_match);
            owned_file.Add_record_permanently(new_match);
        }

        private CredCard2InOutRecord New_combined_record(
                RecordForMatching<CredCard2Record> record_for_matching,
                int match_index)
        {
            var new_match = new CredCard2InOutRecord
            {
                Date = record_for_matching.SourceRecord.Date,
                Description = Create_new_description(record_for_matching, match_index)
            };
            new_match.Unreconciled_amount = record_for_matching.SourceRecord.Main_amount();
            return new_match;
        }

        private void Match_records(CredCard2Record source, ICSVRecord match)
        {
            match.Matched = true;
            source.Matched = true;
            match.Match = source;
            source.Match = match;
        }

        private string Create_new_description(
                RecordForMatching<CredCard2Record> record_for_matching,
                int match_index)
        {
            var combined_amounts = $"{Get_description_without_specific_qualifier(record_for_matching.Matches[match_index].Actual_records[0])}";
            for (int count = 1; count < record_for_matching.Matches[match_index].Actual_records.Count; count++)
            {
                var match = record_for_matching.Matches[match_index].Actual_records[count];
                combined_amounts += $", {Get_description_without_specific_qualifier(match)}";
            }

            var sum_of_all_matches = record_for_matching.Matches[match_index].Actual_records.Sum(x => x.Main_amount());
            var expense_amounts_match = record_for_matching.SourceRecord.Main_amount()
                .Double_equals(sum_of_all_matches);
            var extra_text = expense_amounts_match
                ? ""
                : $"{_transactions_dont_add_up} ({sum_of_all_matches.To_csv_string(true)})"; 

            return $"{_several_transactions} ({combined_amounts}){extra_text}";
        }

        private string Get_description_without_specific_qualifier(ICSVRecord match)
        {
            string new_description = match.Description;

            var qualifier_index_with_space = match.Description.IndexOf($"{_match_description_qualifier} ", StringComparison.CurrentCultureIgnoreCase);
            if (qualifier_index_with_space >= 0)
            {
                new_description = match.Description.Remove(qualifier_index_with_space, _match_description_qualifier.Length + 1);
            }
            else
            {
                var qualifier_index_no_space = match.Description.IndexOf(_match_description_qualifier, StringComparison.CurrentCultureIgnoreCase);
                if (qualifier_index_no_space >= 0)
                {
                    new_description = match.Description.Remove(qualifier_index_no_space, _match_description_qualifier.Length);
                }
            }

            new_description = $"{new_description} {match.Main_amount().To_csv_string(true)}";
            return new_description;
        }

        public IEnumerable<IPotentialMatch> Find_matches(CredCard2Record source_record, ICSVFile<CredCard2InOutRecord> owned_file)
        {
            var generic_matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();
            return generic_matcher.Find_matches(source_record, owned_file);
        }
    }
}