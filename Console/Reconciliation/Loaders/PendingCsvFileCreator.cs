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
            using (var fileStream = File.OpenRead(filePath))
            using (var reader = new StreamReader(fileStream))
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

            var allLines = ReadLines(_pendingSourceFilePath);

            PopulateBankOutCsv(allLines);
            PopulateBankInCsv(allLines);
            PopulateCredCard2Csv(allLines);
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
            IEnumerable<string> relevantLines = FindSubsetOfLines(allLines, headerString).ToList();
            File.WriteAllLines(filePath, relevantLines);
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
            var subsetOfLines = new List<string>();
            int lineIndex = 0;
            bool headerFound = false;
            while (headerFound == false && lineIndex < lines.Count)
            {
                if (lines[lineIndex].ToUpper().StartsWith(subsectionHeader.ToUpper()))
                {
                    headerFound = true;
                }
                lineIndex++;
            }

            if (headerFound)
            {
                bool subsetStillHasLines = true;
                while (subsetStillHasLines && lineIndex < lines.Count)
                {
                    if (lines[lineIndex] == String.Empty)
                    {
                        subsetStillHasLines = false;
                    }
                    else
                    {
                        subsetOfLines.Add(lines[lineIndex]);
                    }
                    lineIndex++;
                }
            }

            return subsetOfLines;
        }
    }
}