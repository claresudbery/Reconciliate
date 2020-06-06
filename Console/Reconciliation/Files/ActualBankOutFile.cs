using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
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
            File.Load(load_file, override_separator, false);
            Mark_last_bank_row();
            if (order_on_load)
            {
                File.Order_by_date();
            }
            Refresh_file_contents();
        }

        public void Refresh_file_contents()
        {
            File.Filter_for_negative_records_only();
        }

        public IEnumerable<ActualBankRecord> Get_potential_balance_rows()
        {
            File.Populate_records_from_original_file_load();
            var potential_balance_rows = new List<ActualBankRecord>();

            if (File.Records.Count > 0)
            {
                var candidate_record_list = File.Records;

                DateTime last_row_date = candidate_record_list.Max(x => x.Date);
                var records_from_last_day = candidate_record_list.Where(x => x.Date == last_row_date).ToList();

                if (records_from_last_day.Count == 1 && records_from_last_day[0].Balance.Double_equals(0))
                {
                    candidate_record_list.RemoveAll(x => x.Date == last_row_date);
                    last_row_date = candidate_record_list.Max(x => x.Date);
                    records_from_last_day = candidate_record_list.Where(x => x.Date == last_row_date).ToList();
                }

                var sum_of_all_amounts = candidate_record_list.Sum(x => x.Amount);
                DateTime earliest_row_date = candidate_record_list.Min(x => x.Date);
                var records_from_first_day = candidate_record_list.Where(x => x.Date == earliest_row_date).ToList();

                potential_balance_rows = Find_records_from_last_day_whose_balance_matches_sum_of_amounts_and_a_record_from_first_day(
                        records_from_first_day,
                        records_from_last_day,
                        sum_of_all_amounts)
                    .ToList();
            }

            Refresh_file_contents();
            return potential_balance_rows;
        }

        private IEnumerable<ActualBankRecord> Find_records_from_last_day_whose_balance_matches_sum_of_amounts_and_a_record_from_first_day(
            IEnumerable<ActualBankRecord> records_from_first_day, 
            IEnumerable<ActualBankRecord> records_from_last_day,
            double sum_of_all_amounts)
        {
            return records_from_last_day
                .Where(x => records_from_first_day
                    .Any(y => x.Balance.Double_equals(y.Balance + sum_of_all_amounts - y.Amount)));
        }

        private void Mark_last_bank_row()
        {
            ActualBankRecord last_record = Get_last_bank_out_row();
            last_record.LastTransactionMarker = ReconConsts.LastOnlineTransaction;
        }

        public ActualBankRecord Get_last_bank_out_row()
        {
            ActualBankRecord last_record = new ActualBankRecord();
            if (File.SourceRecords.Count > 0)
            {
                var negative_records = File.SourceRecords.Where(x => x.Amount < 0).ToList();
                DateTime last_row_date = negative_records.Max(x => x.Date);
                last_record = negative_records.First();

                if (last_record.Date != last_row_date)
                {
                    last_record = negative_records.Last();

                    if (last_record.Date != last_row_date)
                    {
                        last_record = negative_records.OrderBy(x => x.Date).Last();
                    }
                }
            }

            return last_record;
        }
    }
}