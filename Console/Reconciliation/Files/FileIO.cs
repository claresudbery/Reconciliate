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

        public FileIO(ISpreadsheetRepoFactory spreadsheet_factory, string file_path = "", string file_name = "")
        {
            _spreadsheet_factory = spreadsheet_factory;
            Set_file_paths(file_path, file_name);
        }

        public void Set_file_paths(string file_path, string file_name)
        {
            File_path = file_path;
            File_name = file_name;
        }

        public void Write_to_csv_file(string file_suffix, List<string> csv_lines)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + File_name + "-" + file_suffix + ".csv"))
            {
                foreach (string line in csv_lines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void Write_to_file_as_source_lines(string new_file_name, List<string> source_lines)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + new_file_name + ".csv"))
            {
                foreach (string line in source_lines)
                {
                    output_file.WriteLine(line);
                }
            }
        }

        public void Append_to_file_as_source_line(string source_line)
        {
            using (StreamWriter output_file = new StreamWriter(File_path + "/" + File_name + ".csv", true))
            {
                output_file.WriteLine(source_line);
            }
        }

        public void Write_back_to_main_spreadsheet(ICSVFile<TRecordType> csv_file, string worksheet_name)
        {
            ISpreadsheetRepo spreadsheet_repo = _spreadsheet_factory.Create_spreadsheet_repo();
            var spreadsheet = new Spreadsheet(spreadsheet_repo);
            try
            {
                Write_back_to_spreadsheet(spreadsheet, csv_file, worksheet_name);
                spreadsheet_repo.Dispose();
            }
            catch (Exception)
            {
                spreadsheet_repo.Dispose();
                throw;
            }
        }

        public void Write_back_to_spreadsheet(ISpreadsheet spreadsheet, ICSVFile<TRecordType> csv_file, string worksheet_name)
        {
            spreadsheet.Delete_unreconciled_rows(worksheet_name);
            spreadsheet.Append_csv_file(worksheet_name, csv_file);
        }

        public List<TRecordType> Load(List<string> file_contents, char? override_separator = null)
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
                            new_record.Load(new_line, override_separator);
                        }
                        catch (Exception exception)
                        {
                            throw new FormatException(
                                message: $"Input file {File_name}.csv not in correct format: " + new_record.Source_line + ": " + exception.Message,
                                innerException: exception.InnerException);
                        }

                        file_contents.Add(new_line);
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