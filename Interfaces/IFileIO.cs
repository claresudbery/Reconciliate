using System.Collections.Generic;

namespace Interfaces
{
    public interface IFileIO<TRecordType> where TRecordType : ICSVRecord, new()
    {
        string File_path { get; set; }
        string File_name { get; set; }
        void Set_file_paths(string file_path, string file_name);
        List<TRecordType> Load(List<string> file_contents, char? override_separator = null);
        void Write_to_file_as_source_lines(string new_file_name, List<string> source_lines);
        void Write_to_csv_file(string file_suffix, List<string> csv_lines);
        void Write_back_to_main_spreadsheet(ICSVFile<TRecordType> csv_file, string worksheet_name);
    }
}