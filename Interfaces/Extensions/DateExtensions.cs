using System;

namespace Interfaces.Extensions
{
    public static class DateExtensions
    {
        public static double Proximity_score(this DateTime source, DateTime other_date)
        {
            var result = Math.Abs(source.Subtract(other_date).TotalDays);
            return result;
        }
    }
}