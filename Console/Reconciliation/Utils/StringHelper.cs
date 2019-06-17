using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal static class StringHelper
    {
        public static System.Globalization.CultureInfo Culture()
        {
            return new System.Globalization.CultureInfo("en-GB");
        }

        public static string[] Make_sure_there_are_at_least_enough_string_values(int min_values_required, string[] source_array)
        {
            var list = new List<string>(sourceArray);

            if (sourceArray.Length < minValuesRequired)
            {
                list.AddRange(Enumerable.Repeat(String.Empty, minValuesRequired - sourceArray.Length));
            }

            return list.ToArray();
        }
    }
}