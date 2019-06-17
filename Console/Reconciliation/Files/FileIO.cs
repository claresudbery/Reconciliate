using System;
using System.Collections.Generic;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class FileIO<TRecordType> : IFileIO<TRecordType> where TRecordType : ICSVRecord, new()
    {
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;
        public string File_path { get; set; }
        public string File_name { get; set; }

        public FileIO(ISpreadsheetRepoFactory spreadsheetFactory, string filePath = "", string fileName = "")
        {
            _spreadsheet_factory = spreadsheetFactory;
            Set_file_paths(filePath, fileName);
        }

        public void Set_file_paths(string filePath, string fileName)
        {
            File_path = filePath;
            File_name = fileName;
        }

        public void Write_to_csv_file(string fileSuffix, List<string> csvLines)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + File_name + "-" + fileSuffix + ".csv"))
            {
                foreach (string line in csvLines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void Write_to_file_as_source_lines(string newFileName, List<string> sourceLines)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + newFileName + ".csv"))
            {
                foreach (string line in sourceLines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void Append_to_file_as_source_line(string sourceLine)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + File_name + ".csv", true))
            {
                output_file.WriteLine(sourceLine);
            }
        }

        public void Write_back_to_main_spreadsheet(ICSVFile<TRecordType> csvFile, string worksheetName)
        {
            ISpreadsheetRepo spreadsheet_repo = _spreadsheet_factory.Create_spreadsheet_repo();
            var spreadsheet = new Spreadsheet(spreadsheet_repo);
            try
            {
                Write_back_to_spreadsheet(spreadsheet, csvFile, worksheetName);
                spreadsheet_repo.Dispose();
            }
            catch (Exception)
            {
                spreadsheet_repo.Dispose();
                throw;
            }
        }

        public void Write_back_to_spreadsheet(ISpreadsheet spreadsheet, ICSVFile<TRecordType> csvFile, string worksheetName)
        {
            spreadsheet.Delete_unreconciled_rows(worksheetName);
            spreadsheet.Append_csv_file(worksheetName, csvFile);
        }

        public List<TRecordType> Load(List<string> fileContents, char? overrideSeparator = null)
        {
            var records = new List<TRecordType>();
            using (var file_stream = File.OpenRead(File_path + "/" + File_name + ".csv"))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    var new_line = reader.ReadLine();
                    while (Line_is_header(new_line))
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
                                message: "Input not in correct format: " + new_record.Source_line + ": " + exception.Message,
                                innerException: exception.InnerException);
                        }

                        fileContents.Add(new_line);
                        records.Add(new_record);
                    }
                }
            }

            return records;
        }

        private bool Line_is_header(string line)
        {
            return line != null
                   && (line == "" || !Char.IsDigit(line[0]));
        }
    }
}