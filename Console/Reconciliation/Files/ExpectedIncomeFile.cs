using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class ExpectedIncomeFile : IDataFile<ExpectedIncomeRecord>
    {
        public ICSVFile<ExpectedIncomeRecord> File { get; set; }

        public ExpectedIncomeFile(ICSVFile<ExpectedIncomeRecord> file)
        {
            File = file;
        }

        public void Load(bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true)
        {
            File.Load(loadFile, overrideSeparator, orderOnLoad);
            RefreshFileContents();
        }

        public void RefreshFileContents()
        {
            FilterForEmployerExpensesOnly();
        }

        public void FilterForEmployerExpensesOnly()
        {
            File.RemoveRecords(x => x.Code != Codes.Expenses);
        }

        public void CopyToPendingFile(ICSVFile<BankRecord> pendingFile)
        {
            List<BankRecord> recordsAsBankRecords = File.Records.Select(x => x.ConvertToBankRecord()).ToList();
            foreach (var bankRecord in recordsAsBankRecords)
            {
                pendingFile.Records.Add(bankRecord);
            }
        }

        public void UpdateExpectedIncomeRecordWhenMatched(ICSVRecord sourceRecord, ICSVRecord match)
        {
            if (File.Records != null)
            {
                var matchingRecords = File.Records.Where(
                    x => x.Description.RemovePunctuation() == match.Description.RemovePunctuation()
                         && x.Date == match.Date
                         && x.MainAmount() == match.MainAmount());

                if (matchingRecords.Count() == 1)
                {
                    var matchingRecord = matchingRecords.ElementAt(0);

                    matchingRecord.Match = sourceRecord;
                    matchingRecord.Matched = true;
                    matchingRecord.DatePaid = sourceRecord.Date;
                    matchingRecord.TotalPaid = sourceRecord.MainAmount();
                }
            }
        }
    }
}