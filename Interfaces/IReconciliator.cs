using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IReconciliator
    {
        bool Find_reconciliation_matches_for_next_third_party_record();
        bool Move_to_next_unmatched_third_party_record_for_manual_matching();
        bool Not_at_end();
        string Current_source_record_as_string();
        List<IPotentialMatch> Current_potential_matches();
        void Match_current_record(int match_index);
        int Num_third_party_records();
        int Num_owned_records();
        int Num_matched_third_party_records();
        int Num_matched_owned_records();
        int Num_unmatched_third_party_records();
        int Num_unmatched_owned_records();
        List<string> Unmatched_third_party_records();
        List<string> Unmatched_owned_records();
        void Rewind();
        void Finish(string file_suffix);
        ConsoleLine Current_source_record_as_console_line();
        string Current_source_description();
        void Delete_current_third_party_record();
        void Delete_specific_third_party_record(int specified_index);
        void Delete_specific_owned_record_from_list_of_matches(int specified_index);
        int Num_potential_matches();
        void Do_auto_matching();
        List<ConsoleLine> Get_final_matches_for_console();
        List<ConsoleLine> Get_auto_matches_for_console();
        void Remove_auto_match(int match_index);
        void Remove_multiple_auto_matches(List<int> match_indices);
        void Remove_final_match(int match_index);
        void Remove_multiple_final_matches(List<int> match_indices);
        void Refresh_files();
    }
}