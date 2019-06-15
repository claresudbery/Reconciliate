using System;
using System.Collections.Generic;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class FileIO<TRecordType> : IFileIO<TRecordType> where TRecordType : ICSVRecord, new()
    {
        private readonly ISpreadsheetRepoFactory _spreadsheetFactory;
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public FileIO(ISpreadsheetRepoFactory spreadsheetFactory, string filePath = "", string fileName = "")
        {
            _spreadsheetFactory = spreadsheetFactory;
            SetFilePaths(filePath, fileName);
        }

        public void SetFilePaths(string filePath, string fileName)
        {
            FilePath = filePath;
            FileName = fileName;
        }

        public void WriteToCsvFile(string fileSuffix, List<string> csvLines)
        {
            using (StreamWriter outputFile = new StreamWriter(FilePath + "/" + FileName + "-" + fileSuffix + ".csv"))
            {
                foreach (string line in csvLines)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        public void WriteToFileAsSourceLines(string newFileName, List<string> sourceLines)
        {
            using (StreamWriter outputFile = new StreamWriter(FilePath + "/" + newFileName + ".csv"))
            {
                foreach (string line in sourceLines)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        public void AppendToFileAsSourceLine(string sourceLine)
        {
            using (StreamWriter outputFile = new StreamWriter(FilePath + "/" + FileName + ".csv", true))
            {
                outputFile.WriteLine(sourceLine);
            }
        }

        public void WriteBackToMainSpreadsheet(ICSVFile<TRecordType> csvFile, string worksheetName)
        {
            ISpreadsheetRepo spreadsheetRepo = _spreadsheetFactory.CreateSpreadsheetRepo();
            var spreadsheet = new Spreadsheet(spreadsheetRepo);
            try
            {
                WriteBackToSpreadsheet(spreadsheet, csvFile, worksheetName);
                spreadsheetRepo.Dispose();
            }
            catch (Exception)
            {
                spreadsheetRepo.Dispose();
                throw;
            }
        }

        public void WriteBackToSpreadsheet(ISpreadsheet spreadsheet, ICSVFile<TRecordType> csvFile, string worksheetName)
        {
            spreadsheet.DeleteUnreconciledRows(worksheetName);
            spreadsheet.AppendCsvFile(worksheetName, csvFile);
        }

        public List<TRecordType> Load(List<string> fileContents, char? overrideSeparator = null)
        {
            var records = new List<TRecordType>();
            using (var fileStream = File.OpenRead(FilePath + "/" + FileName + ".csv"))
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    var newLine = reader.ReadLine();
                    while (LineIsHeader(newLine))
                    {
                        newLine = reader.ReadLine();
                    }

                    if (newLine != null)
                    {
                        var newRecord = new TRecordType();
                        try
                        {
                            newRecord.Load(newLine, overrideSeparator);
                        }
                        catch (Exception exception)
                        {
                            throw new FormatException(
                                message: "Input not in correct format: " + newRecord.SourceLine + ": " + exception.Message,
                                innerException: exception.InnerException);
                        }

                        fileContents.Add(newLine);
                        records.Add(newRecord);
                    }
                }
            }

            return records;
        }

        private bool LineIsHeader(string line)
        {
            return line != null
                   && (line == "" || !Char.IsDigit(line[0]));
        }
    }
}