using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CSVFile<TRecordType> : ICSVFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public IFileIO<TRecordType> File_io { get; set; }

        public List<string> File_contents { get; set; }
        // _sourceRecords is held separately from Records because sometimes we filter for negative or positive records only,
        // but we still want to keep hold of all the original source records.
        private List<TRecordType> Source_records { get; set; }
        public List<TRecordType> Records { get; set; }

        public CSVFile(IFileIO<TRecordType> fileIO)
        {
            File_io = fileIO;
        }

        public void Load(
            bool loadFile = true, 
            char? overrideSeparator = null,
            bool orderOnLoad = true)
        {
            File_contents = new List<string>();
            Records = new List<TRecordType>();
            Source_records = new List<TRecordType>();
            if (loadFile)
            {
                Source_records = File_io.Load(File_contents, overrideSeparator);
                if (Source_records != null)
                {
                    if (orderOnLoad)
                    {
                        Order_by_date();
                    }
                    Populate_records_from_original_file_load();
                }
            }
        }

        public void Reload()
        {
            File_contents.Clear();
            Records.Clear();
            Source_records.Clear();
            Source_records = File_io.Load(File_contents);
            Populate_records_from_original_file_load();
        }

        public void Populate_source_records_from_records()
        {
            Source_records.Clear();
            foreach (var record in Records)
            {
                Source_records.Add(record);
            }
        }

        public void Populate_records_from_original_file_load()
        {
            Records.Clear();
            foreach (var record in Source_records)
            {
                Records.Add(record);
            }
        }

        public void Remove_record_permanently(TRecordType recordToRemove)
        {
            Source_records.Remove(recordToRemove);
            Records.Remove(recordToRemove);
        }

        public void Add_record_permanently(TRecordType recordToAdd)
        {
            Source_records.Add(recordToAdd);
            Records.Add(recordToAdd);
        }

        private void Order_by_date()
        {
            Source_records = Source_records.OrderBy(x => x.Date).ToList();
        }

        public void Filter_for_positive_records_only()
        {
            Populate_records_from_original_file_load();
            Records.RemoveAll(x => x.Main_amount_is_negative());
        }

        public void Filter_for_negative_records_only()
        {
            Populate_records_from_original_file_load();
            Records.RemoveAll(x => !x.Main_amount_is_negative());
            foreach (var record in Records)
            {
                record.Make_main_amount_positive();
            }
        }

        public void Remove_records(System.Predicate<TRecordType> filterPredicate)
        {
            Populate_records_from_original_file_load();
            Records.RemoveAll(filterPredicate);
        }

        public void Swap_signs_of_all_amounts()
        {
            Populate_records_from_original_file_load();
            foreach (var record in Records)
            {
                record.Swap_sign_of_main_amount();
            }
        }

        public void Copy_records_to_csv_file(ICSVFile<TRecordType> targetFile)
        {
            foreach (var record in Records)
            {
                targetFile.Records.Add(record);
            }
        }

        public void Reset_all_matches()
        {
            foreach (var record in Records)
            {
                record.Matched = false;
            }
        }

        public int Num_matched_records()
        {
            return Records.Count(x => x.Matched);
        }

        public int Num_unmatched_records()
        {
            return Records.Count(x => !x.Matched);
        }

        public List<string> Unmatched_records_as_csv()
        {
            return Records
                .Where(x => !x.Matched)
                .OrderBy(x => x.Date)
                .Select(record => record.To_csv())
                .ToList();
        }

        public List<string> Matched_records_as_csv()
        {
            return Records
                .Where(x => x.Matched)
                .OrderBy(x => x.Date)
                .Select(record => record.To_csv())
                .ToList();
        }

        public List<string> All_records_as_csv()
        {
            return Records
                .OrderByDescending(x => x.Matched)
                .ThenBy(x => x.Date)
                .Select(record => record.To_csv())
                .ToList();
        }

        public List<string> All_records_as_source_lines()
        {
            return Records
                .OrderBy(x => x.Date)
                .Select(record => record.Source_line)
                .ToList();
        }

        public void Write_to_csv_file(string fileSuffix)
        {
            File_io.Write_to_csv_file(fileSuffix, All_records_as_csv());
        }

        public void Write_to_file_as_source_lines(string newFileName)
        {
            File_io.Write_to_file_as_source_lines(newFileName, All_records_as_source_lines());
        }

        public void Write_back_to_main_spreadsheet(string worksheetName)
        {
            File_io.Write_back_to_main_spreadsheet(this, worksheetName);
        }

        public void Convert_source_line_separators(char originalSeparator, char newSeparator)
        {
            foreach (var record in Records)
            {
                record.Convert_source_line_separators(originalSeparator, newSeparator);
            }
        }

        public void Update_source_lines_for_output(char outputSeparator)
        {
            foreach (var record in Records)
            {
                record.Update_source_line_for_output(outputSeparator);
            }
        }

        public List<TRecordType> Records_ordered_for_spreadsheet()
        {
            var divider = new TRecordType {Divider = true};
            var matched_records = Records.Where(x => x.Matched);
            var unmatched_records = Records.Where(x => !x.Matched);

            var result = matched_records
                .OrderBy(x => x.Date)
                .ToList();
            if (result.Count > 0)
            {
                result.Add(divider);
            }
            result.AddRange(unmatched_records.OrderBy(x => x.Date));

            return result;
        }
    }
}