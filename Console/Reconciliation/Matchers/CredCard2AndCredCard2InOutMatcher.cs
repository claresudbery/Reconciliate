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
            return cred_card2_record.Description.Remove_punctuation().ToUpper()
                   != ReconConsts.Amazon_description;
        }

        public void Filter_for_all_amazon_transactions_from_cred_card2_in_out<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.Filter_owned_file(Is_not_owned_amazon_transaction);
        }

        public bool Is_not_owned_amazon_transaction<TOwnedType>(TOwnedType cred_card2_in_out_record) where TOwnedType : ICSVRecord, new()
        {
            return cred_card2_in_out_record.Description.Remove_punctuation().ToUpper()
                   != ReconConsts.Amazon_description;
        }

        public IEnumerable<IPotentialMatch> Find_Amazon_matches<TThirdPartyType, TOwnedType>
            (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            return Debug_find_Amazon_matches(source_record as ActualBankRecord, owned_file as ICSVFile<BankRecord>);
        }

        public void Match_specified_records<TThirdPartyType, TOwnedType>(
            RecordForMatching<TThirdPartyType> record_for_matching,
            int match_index,
            ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
        }

        private IEnumerable<IPotentialMatch> Debug_find_Amazon_matches(ActualBankRecord source_record, ICSVFile<BankRecord> owned_file)
        {
            var result = new List<IPotentialMatch>();
            var random_number_generator = new Random();

            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);

            return result;
        }

        public IEnumerable<IPotentialMatch> Standby_find_Amazon_matches<TThirdPartyType, TOwnedType>
            (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            var result = new List<PotentialMatch>();
            if (owned_file.Records[0].Main_amount() == source_record.Main_amount())
            {
                var actual_records = new List<ICSVRecord>();
                actual_records.Add(owned_file.Records[0]);
                result.Add(new PotentialMatch { Actual_records = actual_records });
            }
            return result;
        }

        public void Debug_preliminary_stuff<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
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