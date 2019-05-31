using System.Collections.Generic;

namespace Interfaces
{
    public interface ICSVFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        List<string> FileContents { get; set; }
        List<TRecordType> Records { get; set; }
        IFileIO<TRecordType> FileIO { get; set; }

        void Load(bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true);
        void Reload();
        void FilterForPositiveRecordsOnly();
        void FilterForNegativeRecordsOnly();
        void RemoveRecords(System.Predicate<TRecordType> filterPredicate);
        void ResetAllMatches();
        int NumMatchedRecords();
        int NumUnmatchedRecords();
        List<string> UnmatchedRecordsAsCsv();
        List<string> MatchedRecordsAsCsv();
        List<string> AllRecordsAsCsv();
        List<string> AllRecordsAsSourceLines();
        void WriteToCsvFile(string fileSuffix);
        void WriteToFileAsSourceLines(string newFileName);
        void WriteBackToMainSpreadsheet(string worksheetName);
        void ConvertSourceLineSeparators(char originalSeparator, char newSeparator);
        List<TRecordType> RecordsOrderedForSpreadsheet();
        void UpdateSourceLinesForOutput(char outputSeparator);
        void SwapSignsOfAllAmounts();
        void CopyRecordsToCsvFile(ICSVFile<TRecordType> targetFile);
        void PopulateSourceRecordsFromRecords();
        void PopulateRecordsFromOriginalFileLoad();
        void RemoveRecordPermanently(TRecordType recordToRemove);
        void AddRecordPermanently(TRecordType recordToAdd);
    }
}