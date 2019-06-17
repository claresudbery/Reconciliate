using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class PendingCsvFileCreator
    {
        public static string BankInPendingFileName = ReconConsts.DefaultBankInPendingFileName;
        public static string BankOutPendingFileName = ReconConsts.DefaultBankOutPendingFileName;
        public static string CredCard2InOutPendingFileName = ReconConsts.Default_cred_card2_in_out_pending_file_name;
        public const string PendingSourceFileName = "Pending.txt";

        private readonly string _bank_out_pending_file_path;
        private readonly string _bank_in_pending_file_path;
        private readonly string _cred_card2_in_out_pending_file_path;
        private readonly string _pending_source_file_path;

        private readonly String _file_path;

        public PendingCsvFileCreator(String file_path)
        {
            _file_path = file_path;

            _bank_out_pending_file_path = _file_path + "/" + BankOutPendingFileName + ".csv";
            _bank_in_pending_file_path = _file_path + "/" + BankInPendingFileName + ".csv";
            _cred_card2_in_out_pending_file_path = _file_path + "/" + CredCard2InOutPendingFileName + ".csv";
            _pending_source_file_path = _file_path + "/" + PendingSourceFileName;
        }

        private IEnumerable<string> Read_lines(string file_path)
        {
            var lines = new List<string>();
            using (var file_stream = File.OpenRead(file_path))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return lines;
        }

        public void Create_and_populate_all_csvs()
        {
            Create_files();

            var all_lines = Read_lines(_pending_source_file_path);

            Populate_bank_out_csv(all_lines);
            Populate_bank_in_csv(all_lines);
            Populate_cred_card2_csv(all_lines);
        }

        private void Populate_bank_out_csv(IEnumerable<string> all_lines)
        {
            Populate_csv_file(all_lines, ReconConsts.Bank_out_header, _bank_out_pending_file_path);
        }

        private void Populate_bank_in_csv(IEnumerable<string> all_lines)
        {
            Populate_csv_file(all_lines, ReconConsts.Bank_in_header, _bank_in_pending_file_path);
        }

        private void Populate_cred_card2_csv(IEnumerable<string> all_lines)
        {
            Populate_csv_file(all_lines, ReconConsts.Cred_card2_header, _cred_card2_in_out_pending_file_path);
        }

        private void Populate_csv_file(IEnumerable<string> all_lines, string header_string, string file_path)
        {
            IEnumerable<string> relevant_lines = Find_subset_of_lines(all_lines, header_string).ToList();
            File.WriteAllLines(file_path, relevant_lines);
        }

        private void Create_files()
        {
            File.Create(_bank_out_pending_file_path).Dispose();
            File.Create(_bank_in_pending_file_path).Dispose();
            File.Create(_cred_card2_in_out_pending_file_path).Dispose();
        }

        private IEnumerable<string> Find_subset_of_lines(IEnumerable<string> all_lines, string subsection_header)
        {
            var lines = all_lines.ToList();
            var subset_of_lines = new List<string>();
            int line_index = 0;
            bool header_found = false;
            while (header_found == false && line_index < lines.Count)
            {
                if (lines[line_index].ToUpper().StartsWith(subsection_header.ToUpper()))
                {
                    header_found = true;
                }
                line_index++;
            }

            if (header_found)
            {
                bool subset_still_has_lines = true;
                while (subset_still_has_lines && line_index < lines.Count)
                {
                    if (lines[line_index] == String.Empty)
                    {
                        subset_still_has_lines = false;
                    }
                    else
                    {
                        subset_of_lines.Add(lines[line_index]);
                    }
                    line_index++;
                }
            }

            return subset_of_lines;
        }
    }
}