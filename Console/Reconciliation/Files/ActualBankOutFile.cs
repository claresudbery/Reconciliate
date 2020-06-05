using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class ActualBankOutFile : IDataFile<ActualBankRecord>
    {
        public ICSVFile<ActualBankRecord> File { get; set; }

        public ActualBankOutFile(ICSVFile<ActualBankRecord> csv_file)
        {
            File = csv_file;
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
            File.Filter_for_negative_records_only();
        }

        public ActualBankRecord Get_balance_row()
        {
            File.Populate_records_from_original_file_load();

            var sum_of_all_amounts = File.Records.Sum(x => x.Amount);
            DateTime last_row_date = File.Records
                .OrderBy(x => x.Date).ToList()
                .Last()
                .Date;
            var most_recent_records = File.Records.Where(x => x.Date == last_row_date).ToList();
            if (most_recent_records.Count == 1 && most_recent_records[0].Amount.Double_equals(0))
            {
                File.Records.RemoveAll(x => x.Date == last_row_date);
                last_row_date = File.Records
                    .OrderBy(x => x.Date).ToList()
                    .Last()
                    .Date;
                most_recent_records = File.Records.Where(x => x.Date == last_row_date).ToList();
            }
            DateTime earliest_row_date = File.Records
                .OrderBy(x => x.Date).ToList()
                .First()
                .Date;
            var earliest_records = File.Records.Where(x => x.Date == earliest_row_date).ToList();

            var balance_row = most_recent_records
                .First(x => earliest_records.Any(y => x.Balance.Double_equals(y.Balance + y.Amount)));

            Refresh_file_contents();
            return balance_row;
        }
    }
}