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
            using (StreamWriter output_file = new StreamWriter(FilePath + "/" + FileName + "-" + fileSuffix + ".csv"))
            {
                foreach (string line in csvLines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void WriteToFileAsSourceLines(string newFileName, List<string> sourceLines)
        {
            using (StreamWriter output_file = new StreamWriter(FilePath + "/" + newFileName + ".csv"))
            {
                foreach (string line in sourceLines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void AppendToFileAsSourceLine(string sourceLine)
        {
            using (StreamWriter output_file = new StreamWriter(FilePath + "/" + FileName + ".csv", true))
            {
                output_file.WriteLine(sourceLine);
            }
        }

        public void WriteBackToMainSpreadsheet(ICSVFile<TRecordType> csvFile, string worksheetName)
        {
            ISpreadsheetRepo spreadsheet_repo = _spreadsheetFactory.CreateSpreadsheetRepo();
            var spreadsheet = new Spreadsheet(spreadsheet_repo);
            try
            {
                WriteBackToSpreadsheet(spreadsheet, csvFile, worksheetName);
                spreadsheet_repo.Dispose();
            }
            catch (Exception)
            {
                spreadsheet_repo.Dispose();
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
            using (var file_stream = File.OpenRead(FilePath + "/" + FileName + ".csv"))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    var new_line = reader.ReadLine();
                    while (LineIsHeader(new_line))
                    {
                        new_line = reader.ReadLine();
                    }

                    if (new_line != null)
                    {
                        var new_record = new TRecordType();
                        try
                        {
                            new_record.Load(new_line, overrideSeparator);
                        }
                        catch (Exception exception)
                        {
                            throw new FormatException(
                                message: "Input not in correct format: " + new_record.SourceLine + ": " + exception.Message,
                                innerException: exception.InnerException);
                        }

                        fileContents.Add(new_line);
                        records.Add(new_record);
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