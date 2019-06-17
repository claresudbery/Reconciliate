using System;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class DateExtensions
    {
        public static double Proximity_score(this DateTime source, DateTime otherDate)
        {
            var result = Math.Abs(source.Subtract(otherDate).TotalDays);
            return result;
        }
    }
}