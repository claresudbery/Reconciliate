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
            List<BankRecord> records_as_bank_records = File.Records.Select(x => x.ConvertToBankRecord()).ToList();
            foreach (var bank_record in records_as_bank_records)
            {
                pendingFile.Records.Add(bank_record);
            }
        }

        public void UpdateExpectedIncomeRecordWhenMatched(ICSVRecord sourceRecord, ICSVRecord match)
        {
            if (File.Records != null)
            {
                var matching_records = File.Records.Where(
                    x => x.Description.RemovePunctuation() == match.Description.RemovePunctuation()
                         && x.Date == match.Date
                         && x.MainAmount() == match.MainAmount());

                if (matching_records.Count() == 1)
                {
                    var matching_record = matching_records.ElementAt(0);

                    matching_record.Match = sourceRecord;
                    matching_record.Matched = true;
                    matching_record.DatePaid = sourceRecord.Date;
                    matching_record.TotalPaid = sourceRecord.MainAmount();
                }
            }
        }
    }
}