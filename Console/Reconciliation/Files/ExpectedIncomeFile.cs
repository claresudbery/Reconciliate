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
            Refresh_file_contents();
        }

        public void Refresh_file_contents()
        {
            Filter_for_employer_expenses_only();
        }

        public void Filter_for_employer_expenses_only()
        {
            File.Remove_records(x => x.Code != Codes.Expenses);
        }

        public void Copy_to_pending_file(ICSVFile<BankRecord> pendingFile)
        {
            List<BankRecord> records_as_bank_records = File.Records.Select(x => x.Convert_to_bank_record()).ToList();
            foreach (var bank_record in records_as_bank_records)
            {
                pendingFile.Records.Add(bank_record);
            }
        }

        public void Update_expected_income_record_when_matched(ICSVRecord sourceRecord, ICSVRecord match)
        {
            if (File.Records != null)
            {
                var matching_records = File.Records.Where(
                    x => x.Description.Remove_punctuation() == match.Description.Remove_punctuation()
                         && x.Date == match.Date
                         && x.Main_amount() == match.Main_amount());

                if (matching_records.Count() == 1)
                {
                    var matching_record = matching_records.ElementAt(0);

                    matching_record.Match = sourceRecord;
                    matching_record.Matched = true;
                    matching_record.Date_paid = sourceRecord.Date;
                    matching_record.Total_paid = sourceRecord.Main_amount();
                }
            }
        }
    }
}