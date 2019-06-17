using System;
using ConsoleCatchall.Console.Reconciliation.Utils;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class DoubleExtensions
    {
        public static string To_csv_string(this double source, bool formatCurrency)
        {
            if (formatCurrency)
            {
                var result = string.Format(StringHelper.Culture(), "{0:C}", source);
                if (result.Contains(","))
                {
                    result = result.Encase_in_escaped_quotes_if_not_already_encased();
                }
                return result;
            }
            else
            {
                return source.ToString();
            }
        }

        public static double Proximity_score(this double source, double otherAmount)
        {
            var result = Math.Abs(source - otherAmount);
            return result;
        }
    }
}