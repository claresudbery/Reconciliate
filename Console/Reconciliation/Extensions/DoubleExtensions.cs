using System;
using ConsoleCatchall.Console.Reconciliation.Utils;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class DoubleExtensions
    {
        public static string To_csv_string(this double source, bool format_currency)
        {
            if (format_currency)
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

        public static double Proximity_score(this double source, double other_amount)
        {
            var result = Math.Abs(source - other_amount);
            return result;
        }

        public static bool Double_equals(this double source, double other_amount)
        {
            return Math.Abs(source - other_amount) < 0.0001;
        }
    }
}