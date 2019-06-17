using System.Collections.Generic;

namespace Interfaces
{
    public interface IFileIO<TRecordType> where TRecordType : ICSVRecord, new()
    {
        string File_path { get; set; }
        string File_name { get; set; }
        void Set_file_paths(string filePath, string fileName);
        List<TRecordType> Load(List<string> fileContents, char? overrideSeparator = null);
        void Write_to_file_as_source_lines(string newFileName, List<string> sourceLines);
        void Write_to_csv_file(string fileSuffix, List<string> csvLines);
        void Write_back_to_main_spreadsheet(ICSVFile<TRecordType> csvFile, string worksheetName);
    }
}