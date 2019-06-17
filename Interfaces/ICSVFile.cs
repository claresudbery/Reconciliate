using System.Collections.Generic;

namespace Interfaces
{
    public interface ICSVFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        List<string> File_contents { get; set; }
        List<TRecordType> Records { get; set; }
        IFileIO<TRecordType> File_io { get; set; }

        void Load(bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true);
        void Reload();
        void Filter_for_positive_records_only();
        void Filter_for_negative_records_only();
        void Remove_records(System.Predicate<TRecordType> filterPredicate);
        void Reset_all_matches();
        int Num_matched_records();
        int Num_unmatched_records();
        List<string> Unmatched_records_as_csv();
        List<string> Matched_records_as_csv();
        List<string> All_records_as_csv();
        List<string> All_records_as_source_lines();
        void Write_to_csv_file(string fileSuffix);
        void Write_to_file_as_source_lines(string newFileName);
        void Write_back_to_main_spreadsheet(string worksheetName);
        void Convert_source_line_separators(char originalSeparator, char newSeparator);
        List<TRecordType> Records_ordered_for_spreadsheet();
        void Update_source_lines_for_output(char outputSeparator);
        void Swap_signs_of_all_amounts();
        void Copy_records_to_csv_file(ICSVFile<TRecordType> targetFile);
        void Populate_source_records_from_records();
        void Populate_records_from_original_file_load();
        void Remove_record_permanently(TRecordType recordToRemove);
        void Add_record_permanently(TRecordType recordToAdd);
    }
}