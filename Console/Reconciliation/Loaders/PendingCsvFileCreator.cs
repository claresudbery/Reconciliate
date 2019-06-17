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
        public static string CredCard2InOutPendingFileName = ReconConsts.DefaultCredCard2InOutPendingFileName;
        public const string PendingSourceFileName = "Pending.txt";

        private readonly string _bankOutPendingFilePath;
        private readonly string _bankInPendingFilePath;
        private readonly string _credCard2InOutPendingFilePath;
        private readonly string _pendingSourceFilePath;

        private readonly String _filePath;

        public PendingCsvFileCreator(String filePath)
        {
            _filePath = filePath;

            _bankOutPendingFilePath = _filePath + "/" + BankOutPendingFileName + ".csv";
            _bankInPendingFilePath = _filePath + "/" + BankInPendingFileName + ".csv";
            _credCard2InOutPendingFilePath = _filePath + "/" + CredCard2InOutPendingFileName + ".csv";
            _pendingSourceFilePath = _filePath + "/" + PendingSourceFileName;
        }

        private IEnumerable<string> ReadLines(string filePath)
        {
            var lines = new List<string>();
            using (var file_stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return lines;
        }

        public void CreateAndPopulateAllCsvs()
        {
            CreateFiles();

            var all_lines = ReadLines(_pendingSourceFilePath);

            PopulateBankOutCsv(all_lines);
            PopulateBankInCsv(all_lines);
            PopulateCredCard2Csv(all_lines);
        }

        private void PopulateBankOutCsv(IEnumerable<string> allLines)
        {
            PopulateCsvFile(allLines, ReconConsts.BankOutHeader, _bankOutPendingFilePath);
        }

        private void PopulateBankInCsv(IEnumerable<string> allLines)
        {
            PopulateCsvFile(allLines, ReconConsts.BankInHeader, _bankInPendingFilePath);
        }

        private void PopulateCredCard2Csv(IEnumerable<string> allLines)
        {
            PopulateCsvFile(allLines, ReconConsts.CredCard2Header, _credCard2InOutPendingFilePath);
        }

        private void PopulateCsvFile(IEnumerable<string> allLines, string headerString, string filePath)
        {
            IEnumerable<string> relevant_lines = FindSubsetOfLines(allLines, headerString).ToList();
            File.WriteAllLines(filePath, relevant_lines);
        }

        private void CreateFiles()
        {
            File.Create(_bankOutPendingFilePath).Dispose();
            File.Create(_bankInPendingFilePath).Dispose();
            File.Create(_credCard2InOutPendingFilePath).Dispose();
        }

        private IEnumerable<string> FindSubsetOfLines(IEnumerable<string> allLines, string subsectionHeader)
        {
            var lines = allLines.ToList();
            var subset_of_lines = new List<string>();
            int line_index = 0;
            bool header_found = false;
            while (header_found == false && line_index < lines.Count)
            {
                if (lines[line_index].ToUpper().StartsWith(subsectionHeader.ToUpper()))
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