using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Reconciliators
{
    internal class CredCard2Reconciliator : IReconciliator
    {
        private readonly Reconciliator<CredCard2Record, CredCard2InOutRecord> _reconciliator;
        public ICSVFile<CredCard2Record> Third_party_file { get; set; }
        public ICSVFile<CredCard2InOutRecord> Owned_file { get; set; }

        public CredCard2Reconciliator(
            IFileIO<CredCard2Record> credCard2FileIO,
            IFileIO<CredCard2InOutRecord> credCard2InOutFileIO)
        {
            Third_party_file = new CSVFile<CredCard2Record>(credCard2FileIO);
            Third_party_file.Load();

            Owned_file = new CSVFile<CredCard2InOutRecord>(credCard2InOutFileIO);
            Owned_file.Load();

            _reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(
                Third_party_file, 
                Owned_file,
                CredCard2AndCredCard2InOutData.LoadingInfo.Third_party_file_load_action,
                CredCard2AndCredCard2InOutData.LoadingInfo.Sheet_name);
        }

        public void Filter_for_negative_records_only()
        {
            Third_party_file.Filter_for_negative_records_only();
        }

        public void Filter_for_positive_records_only()
        {
            Third_party_file.Filter_for_positive_records_only();
        }

        public void Swap_signs_of_all_amounts()
        {
            Third_party_file.Swap_signs_of_all_amounts();
        }

        public bool Find_reconciliation_matches_for_next_third_party_record()
        {
            return _reconciliator.Find_reconciliation_matches_for_next_third_party_record();
        }

        public bool Move_to_next_unmatched_third_party_record()
        {
            return _reconciliator.Move_to_next_unmatched_third_party_record_for_manual_matching();
        }

        public bool Not_at_end()
        {
            return _reconciliator.Not_at_end();
        }

        public string Current_source_record_as_string()
        {
            return _reconciliator.Current_source_record_as_string();
        }

        public ConsoleLine Current_source_record_as_console_line()
        {
            return _reconciliator.Current_source_record_as_console_line();
        }

        public string Current_source_description()
        {
            return _reconciliator.Current_source_description();
        }

        public void Delete_current_third_party_record()
        {
            _reconciliator.Delete_current_third_party_record();
        }

        public void Delete_specific_third_party_record(int specifiedIndex)
        {
            _reconciliator.Delete_specific_third_party_record(specifiedIndex);
        }

        public void Delete_specific_owned_record_from_list_of_matches(int specifiedIndex)
        {
            _reconciliator.Delete_specific_owned_record_from_list_of_matches(specifiedIndex);
        }

        public int Num_potential_matches()
        {
            return _reconciliator.Num_potential_matches();
        }

        public List<IPotentialMatch> Current_potential_matches()
        {
            return _reconciliator.Current_potential_matches();
        }

        public void Mark_latest_match_index(int matchIndex)
        {
            _reconciliator.Mark_latest_match_index(matchIndex);
        }

        public void Match_non_matching_record(int matchIndex)
        {
            _reconciliator.Match_non_matching_record(matchIndex);
        }

        public List<ConsoleLine> Get_final_matches_for_console()
        {
            return _reconciliator.Get_final_matches_for_console();
        }

        public void Do_auto_matching()
        {
            _reconciliator.Return_auto_matches();
        }

        public List<ConsoleLine> Get_auto_matches_for_console()
        {
            return _reconciliator.Get_auto_matches_for_console();
        }

        public void Remove_auto_match(int matchIndex)
        {
            _reconciliator.Remove_auto_match(matchIndex);
        }

        public void Remove_multiple_auto_matches(List<int> matchIndices)
        {
            _reconciliator.Remove_multiple_auto_matches(matchIndices);
        }

        public void Remove_final_match(int matchIndex)
        {
            _reconciliator.Remove_final_match(matchIndex);
        }

        public void Remove_multiple_final_matches(List<int> matchIndices)
        {
            _reconciliator.Remove_multiple_final_matches(matchIndices);
        }

        public int Num_third_party_records()
        {
            return Third_party_file.Records.Count;
        }

        public int Num_owned_records()
        {
            return Owned_file.Records.Count;
        }

        public int Num_matched_third_party_records()
        {
            return Third_party_file.Num_matched_records();
        }

        public int Num_matched_owned_records()
        {
            return Owned_file.Num_matched_records();
        }

        public int Num_unmatched_third_party_records()
        {
            return Third_party_file.Num_unmatched_records();
        }

        public int Num_unmatched_owned_records()
        {
            return Owned_file.Num_unmatched_records();
        }

        public List<string> Unmatched_third_party_records()
        {
            return Third_party_file.Unmatched_records_as_csv();
        }

        public List<string> Unmatched_owned_records()
        {
            return Owned_file.Unmatched_records_as_csv();
        }

        public void Rewind()
        {
            _reconciliator.Rewind();
        }

        public void Finish(string fileSuffix)
        {
            _reconciliator.Finish(fileSuffix);
        }

        public void Match_current_record(int matchIndex)
        {
            _reconciliator.Match_current_record(matchIndex);
        }

        public bool Move_to_next_unmatched_third_party_record_for_manual_matching()
        {
            return _reconciliator.Move_to_next_unmatched_third_party_record_for_manual_matching();
        }

        public void Refresh_files()
        {
            _reconciliator.Refresh_files();
        }
    }
}