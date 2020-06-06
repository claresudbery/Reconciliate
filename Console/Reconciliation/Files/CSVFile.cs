using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CSVFile<TRecordType> : ICSVFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public IFileIO<TRecordType> File_io { get; set; }

        public List<string> File_contents { get; set; }
        // SourceRecords is held separately from Records because sometimes we filter for negative or positive records only,
        // but we still want to keep hold of all the original source records.
        public List<TRecordType> Records { get; set; }
        public List<TRecordType> SourceRecords { get; set; }

        public CSVFile(IFileIO<TRecordType> file_io)
        {
            File_io = file_io;
        }

        public void Load(
            bool load_file = true, 
            char? override_separator = null,
            bool order_on_load = true)
        {
            File_contents = new List<string>();
            Records = new List<TRecordType>();
            SourceRecords = new List<TRecordType>();
            if (load_file)
            {
                SourceRecords = File_io.Load(File_contents, override_separator);
                if (SourceRecords != null)
                {
                    if (order_on_load)
                    {
                        Order_by_date();
                    }
                    Populate_records();
                }
            }
        }

        public void Reload()
        {
            File_contents.Clear();
            Records.Clear();
            SourceRecords.Clear();
            SourceRecords = File_io.Load(File_contents);
            Populate_records();
        }

        public void Populate_source_records_from_records()
        {
            SourceRecords.Clear();
            foreach (var record in Records)
            {
                SourceRecords.Add(record);
            }
        }

        public void Populate_records_from_original_file_load()
        {
            Records.Clear();
            foreach (var record in SourceRecords)
            {
                var new_record = new TRecordType();
                new_record.Copy_from(record);
                Records.Add(new_record);
            }
        }

        public void Populate_records()
        {
            Records.Clear();
            foreach (var record in SourceRecords)
            {
                Records.Add(record);
            }
        }

        public void Remove_record_permanently(TRecordType record_to_remove)
        {
            SourceRecords.Remove(record_to_remove);
            Records.Remove(record_to_remove);
        }

        public void Add_record_permanently(TRecordType record_to_add)
        {
            SourceRecords.Add(record_to_add);
            Records.Add(record_to_add);
        }

        private void Order_by_date()
        {
            SourceRecords = SourceRecords.OrderBy(x => x.Date).ToList();
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

        public void Remove_records(System.Predicate<TRecordType> filter_predicate)
        {
            Populate_records();
            Records.RemoveAll(filter_predicate);
        }

        public void Swap_signs_of_all_amounts()
        {
            Populate_records();
            foreach (var record in Records)
            {
                record.Swap_sign_of_main_amount();
            }
        }

        public void Copy_records_to_csv_file(ICSVFile<TRecordType> target_file)
        {
            foreach (var record in Records)
            {
                target_file.Records.Add(record);
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

        public List<string> All_records_as_output_source_lines()
        {
            return Records
                .OrderBy(x => x.Date)
                .Select(record => record.OutputSourceLine)
                .ToList();
        }

        public void Write_to_csv_file(string file_suffix)
        {
            File_io.Write_to_csv_file(file_suffix, All_records_as_csv());
        }

        public void Write_to_file_as_source_lines(string new_file_name)
        {
            File_io.Write_to_file_as_source_lines(new_file_name, All_records_as_output_source_lines());
        }

        public void Write_back_to_main_spreadsheet(string worksheet_name)
        {
            File_io.Write_back_to_main_spreadsheet(this, worksheet_name);
        }

        public void Convert_source_line_separators(char original_separator, char new_separator)
        {
            foreach (var record in Records)
            {
                record.Convert_source_line_separators(original_separator, new_separator);
            }
        }

        public void Update_source_lines_for_output(char output_separator)
        {
            foreach (var record in Records)
            {
                record.Update_source_line_for_output(output_separator);
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