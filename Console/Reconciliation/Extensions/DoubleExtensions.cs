using System;
using ConsoleCatchall.Console.Reconciliation.Utils;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class DoubleExtensions
    {
        public static string ToCsvString(this double source, bool formatCurrency)
        {
            if (formatCurrency)
            {
                var result = string.Format(StringHelper.Culture(), "{0:C}", source);
                if (result.Contains(","))
                {
                    result = result.EncaseInEscapedQuotesIfNotAlreadyEncased();
                }
                return result;
            }
            else
            {
                return source.ToString();
            }
        }

        public static double ProximityScore(this double source, double otherAmount)
        {
            var result = Math.Abs(source - otherAmount);
            return result;
        }
    }
}