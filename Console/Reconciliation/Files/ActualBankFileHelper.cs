using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class ActualBankFileHelper
    {
        public static void Mark_last_bank_row(IList<ActualBankRecord> source_records)
        {
            ActualBankRecord last_record = Get_last_bank_out_row(source_records);
            last_record.LastTransactionMarker = ReconConsts.LastOnlineTransaction;
        }

        public static ActualBankRecord Get_last_bank_out_row(IList<ActualBankRecord> source_records)
        {
            ActualBankRecord last_record = new ActualBankRecord();
            if (source_records.Count > 0)
            {
                DateTime last_row_date = source_records.Max(x => x.Date);
                last_record = source_records.First();

                if (last_record.Date != last_row_date)
                {
                    last_record = source_records.Last();

                    if (last_record.Date != last_row_date)
                    {
                        last_record = source_records.OrderBy(x => x.Date).Last();
                    }
                }
            }

            return last_record;
        }
    }
}