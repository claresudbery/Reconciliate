using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CSVFile<TRecordType> : ICSVFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public IFileIO<TRecordType> FileIO { get; set; }

        public List<string> FileContents { get; set; }
        // _sourceRecords is held separately from Records because sometimes we filter for negative or positive records only,
        // but we still want to keep hold of all the original source records.
        private List<TRecordType> SourceRecords { get; set; }
        public List<TRecordType> Records { get; set; }

        public CSVFile(IFileIO<TRecordType> fileIO)
        {
            FileIO = fileIO;
        }

        public void Load(
            bool loadFile = true, 
            char? overrideSeparator = null,
            bool orderOnLoad = true)
        {
            FileContents = new List<string>();
            Records = new List<TRecordType>();
            SourceRecords = new List<TRecordType>();
            if (loadFile)
            {
                SourceRecords = FileIO.Load(FileContents, overrideSeparator);
                if (SourceRecords != null)
                {
                    if (orderOnLoad)
                    {
                        OrderByDate();
                    }
                    PopulateRecordsFromOriginalFileLoad();
                }
            }
        }

        public void Reload()
        {
            FileContents.Clear();
            Records.Clear();
            SourceRecords.Clear();
            SourceRecords = FileIO.Load(FileContents);
            PopulateRecordsFromOriginalFileLoad();
        }

        public void PopulateSourceRecordsFromRecords()
        {
            SourceRecords.Clear();
            foreach (var record in Records)
            {
                SourceRecords.Add(record);
            }
        }

        public void PopulateRecordsFromOriginalFileLoad()
        {
            Records.Clear();
            foreach (var record in SourceRecords)
            {
                Records.Add(record);
            }
        }

        public void RemoveRecordPermanently(TRecordType recordToRemove)
        {
            SourceRecords.Remove(recordToRemove);
            Records.Remove(recordToRemove);
        }

        public void AddRecordPermanently(TRecordType recordToAdd)
        {
            SourceRecords.Add(recordToAdd);
            Records.Add(recordToAdd);
        }

        private void OrderByDate()
        {
            SourceRecords = SourceRecords.OrderBy(x => x.Date).ToList();
        }

        public void FilterForPositiveRecordsOnly()
        {
            PopulateRecordsFromOriginalFileLoad();
            Records.RemoveAll(x => x.MainAmountIsNegative());
        }

        public void FilterForNegativeRecordsOnly()
        {
            PopulateRecordsFromOriginalFileLoad();
            Records.RemoveAll(x => !x.MainAmountIsNegative());
            foreach (var record in Records)
            {
                record.MakeMainAmountPositive();
            }
        }

        public void RemoveRecords(System.Predicate<TRecordType> filterPredicate)
        {
            PopulateRecordsFromOriginalFileLoad();
            Records.RemoveAll(filterPredicate);
        }

        public void SwapSignsOfAllAmounts()
        {
            PopulateRecordsFromOriginalFileLoad();
            foreach (var record in Records)
            {
                record.SwapSignOfMainAmount();
            }
        }

        public void CopyRecordsToCsvFile(ICSVFile<TRecordType> targetFile)
        {
            foreach (var record in Records)
            {
                targetFile.Records.Add(record);
            }
        }

        public void ResetAllMatches()
        {
            foreach (var record in Records)
            {
                record.Matched = false;
            }
        }

        public int NumMatchedRecords()
        {
            return Records.Count(x => x.Matched);
        }

        public int NumUnmatchedRecords()
        {
            return Records.Count(x => !x.Matched);
        }

        public List<string> UnmatchedRecordsAsCsv()
        {
            return Records
                .Where(x => !x.Matched)
                .OrderBy(x => x.Date)
                .Select(record => record.ToCsv())
                .ToList();
        }

        public List<string> MatchedRecordsAsCsv()
        {
            return Records
                .Where(x => x.Matched)
                .OrderBy(x => x.Date)
                .Select(record => record.ToCsv())
                .ToList();
        }

        public List<string> AllRecordsAsCsv()
        {
            return Records
                .OrderByDescending(x => x.Matched)
                .ThenBy(x => x.Date)
                .Select(record => record.ToCsv())
                .ToList();
        }

        public List<string> AllRecordsAsSourceLines()
        {
            return Records
                .OrderBy(x => x.Date)
                .Select(record => record.SourceLine)
                .ToList();
        }

        public void WriteToCsvFile(string fileSuffix)
        {
            FileIO.WriteToCsvFile(fileSuffix, AllRecordsAsCsv());
        }

        public void WriteToFileAsSourceLines(string newFileName)
        {
            FileIO.WriteToFileAsSourceLines(newFileName, AllRecordsAsSourceLines());
        }

        public void WriteBackToMainSpreadsheet(string worksheetName)
        {
            FileIO.WriteBackToMainSpreadsheet(this, worksheetName);
        }

        public void ConvertSourceLineSeparators(char originalSeparator, char newSeparator)
        {
            foreach (var record in Records)
            {
                record.ConvertSourceLineSeparators(originalSeparator, newSeparator);
            }
        }

        public void UpdateSourceLinesForOutput(char outputSeparator)
        {
            foreach (var record in Records)
            {
                record.UpdateSourceLineForOutput(outputSeparator);
            }
        }

        public List<TRecordType> RecordsOrderedForSpreadsheet()
        {
            var divider = new TRecordType {Divider = true};
            var matchedRecords = Records.Where(x => x.Matched);
            var unmatchedRecords = Records.Where(x => !x.Matched);

            var result = matchedRecords
                .OrderBy(x => x.Date)
                .ToList();
            if (result.Count > 0)
            {
                result.Add(divider);
            }
            result.AddRange(unmatchedRecords.OrderBy(x => x.Date));

            return result;
        }
    }
}