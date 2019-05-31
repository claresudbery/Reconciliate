using System.Collections.Generic;

namespace Interfaces
{
    public interface IFileIO<TRecordType> where TRecordType : ICSVRecord, new()
    {
        string FilePath { get; set; }
        string FileName { get; set; }
        void SetFilePaths(string filePath, string fileName);
        List<TRecordType> Load(List<string> fileContents, char? overrideSeparator = null);
        void WriteToFileAsSourceLines(string newFileName, List<string> sourceLines);
        void WriteToCsvFile(string fileSuffix, List<string> csvLines);
        void WriteBackToMainSpreadsheet(ICSVFile<TRecordType> csvFile, string worksheetName);
    }
}