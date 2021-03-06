using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class ExpectedIncomeFile : IDataFile<ExpectedIncomeRecord>
    {
        public ICSVFile<ExpectedIncomeRecord> File { get; set; }

        public ExpectedIncomeFile(ICSVFile<ExpectedIncomeRecord> file)
        {
            File = file;
        }

        public void Load(bool load_file = true,
            char? override_separator = null,
            bool order_on_load = true)
        {
            File.Load(load_file, override_separator, order_on_load);
            Refresh_file_contents();
        }

        public void Refresh_file_contents()
        {
            Filter_for_employer_expenses_and_bank_transactions_only();
        }

        public void Filter_for_employer_expenses_and_bank_transactions_only()
        {
            File.Remove_records(x => x.Code != Codes.Expenses && x.Code != Codes.ExpectedInBankTransaction);
        }

        public void Copy_to_pending_file(ICSVFile<BankRecord> pending_file)
        {
            List<BankRecord> records_as_bank_records = File.Records.Select(x => x.Convert_to_bank_record()).ToList();
            foreach (var bank_record in records_as_bank_records)
            {
                pending_file.Records.Add(bank_record);
            }
        }

        public void Create_new_expenses_record_to_match_balance(ICSVRecord source_record, double balance)
        {
            if (File.Records != null)
            {
                File.Records.Add( new ExpectedIncomeRecord
                {
                    Description = ReconConsts.UnknownExpense,
                    Unreconciled_amount = balance,
                    Match = source_record,
                    Matched = true,
                    Date = source_record.Date,
                    Date_paid = source_record.Date,
                    Total_paid = source_record.Main_amount(),
                    Code = Codes.Expenses
                });
            }
        }

        public void Update_expected_income_record_when_matched(ICSVRecord source_record, ICSVRecord match)
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

                    matching_record.Match = source_record;
                    matching_record.Matched = true;
                    matching_record.Date_paid = source_record.Date;
                    matching_record.Total_paid = source_record.Main_amount();
                }
            }
        }

        public void Finish()
        {
            foreach (var matched_record in File.Records.Where(x => x.Matched))
            {
                matched_record.Reconcile();
            }
            File.Write_back_to_main_spreadsheet(MainSheetNames.Expected_in);
        }
    }
}