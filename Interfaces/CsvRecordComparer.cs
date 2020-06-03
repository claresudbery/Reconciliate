using System;
using System.Collections.Generic;

namespace Interfaces
{
    public class CsvRecordComparer : IEqualityComparer<ICSVRecord>
    {
        // CSV records are equal if their descriptions and amounts are equal.
        public bool Equals(ICSVRecord x, ICSVRecord y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the objects' properties are equal.
            return x.Description == y.Description && x.Main_amount() == y.Main_amount();
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(ICSVRecord csvRecord)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(csvRecord, null)) return 0;

            // Get hash code for the Description field if it is not null.
            int hashCSVRecordName = csvRecord.Description == null ? 0 : csvRecord.Description.GetHashCode();

            // Get hash code for the Main_amount field.
            int hashCSVRecordCode = csvRecord.Main_amount().GetHashCode();

            // Calculate the hash code for the object.
            return hashCSVRecordName ^ hashCSVRecordCode;
        }
    }
}