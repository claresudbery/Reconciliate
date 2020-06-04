using System;
using System.Collections.Generic;
using Interfaces.Extensions;

namespace Interfaces
{
    public class CsvRecordComparer : IEqualityComparer<ICSVRecord>
    {
        // CSV records are equal if their descriptions, dates and amounts are equal.
        public bool Equals(ICSVRecord x, ICSVRecord y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Description == y.Description 
                   && x.Main_amount().Double_equals(y.Main_amount())
                   && x.Date == y.Date;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(ICSVRecord csvRecord)
        {
            if (Object.ReferenceEquals(csvRecord, null)) return 0;

            int hashCsvRecordDesc = csvRecord.Description == null ? 0 : csvRecord.Description.GetHashCode();
            int hashCsvRecordAmount = csvRecord.Main_amount().GetHashCode();
            int hashCsvRecordDate = csvRecord.Date.GetHashCode();

            return hashCsvRecordDesc ^ hashCsvRecordAmount ^ hashCsvRecordDate;
        }
    }
}