using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Reconciliators
{
    internal class BankReconciliatorNew<TThirdPartyType, TOwnedType> : IReconciliator 
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        private readonly Reconciliator<TThirdPartyType, TOwnedType> _reconciliator;
        public ICSVFile<TThirdPartyType> Third_party_file { get; set; }
        public ICSVFile<TOwnedType> Owned_file { get; set; }

        public BankReconciliatorNew(
            IFileIO<TThirdPartyType> actual_bank_file_io,
            IFileIO<TOwnedType> bank_file_io,
            DataLoadingInformation loading_info)
        {
            Third_party_file = new CSVFile<TThirdPartyType>(actual_bank_file_io);
            Third_party_file.Load();

            Owned_file = new CSVFile<TOwnedType>(bank_file_io);
            Owned_file.Load();

            _reconciliator = new Reconciliator<TThirdPartyType, TOwnedType>(
                Third_party_file,
                Owned_file,
                loading_info.Third_party_file_load_action,
                loading_info.Sheet_name);
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

        public void Delete_specific_third_party_record(int specified_index)
        {
            _reconciliator.Delete_specific_third_party_record(specified_index);
        }

        public void Delete_specific_owned_record_from_list_of_matches(int specified_index)
        {
            _reconciliator.Delete_specific_owned_record_from_list_of_matches(specified_index);
        }

        public int Num_potential_matches()
        {
            return _reconciliator.Num_potential_matches();
        }

        public List<IPotentialMatch> Current_potential_matches()
        {
            return _reconciliator.Current_potential_matches();
        }

        public void Mark_latest_match_index(int match_index)
        {
            _reconciliator.Mark_latest_match_index(match_index);
        }

        public void Match_non_matching_record(int match_index)
        {
            _reconciliator.Match_non_matching_record(match_index);
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

        public void Remove_auto_match(int match_index)
        {
            _reconciliator.Remove_auto_match(match_index);
        }

        public void Remove_multiple_auto_matches(List<int> match_indices)
        {
            _reconciliator.Remove_multiple_auto_matches(match_indices);
        }

        public void Remove_final_match(int match_index)
        {
            _reconciliator.Remove_final_match(match_index);
        }

        public void Remove_multiple_final_matches(List<int> match_indices)
        {
            _reconciliator.Remove_multiple_final_matches(match_indices);
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

        public void Finish(string file_suffix)
        {
            _reconciliator.Finish(file_suffix);
        }

        public void Match_current_record(int match_index)
        {
            _reconciliator.Match_current_record(match_index);
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