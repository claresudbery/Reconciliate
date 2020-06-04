using System;

namespace Interfaces.Extensions
{
    public static class DoubleExtensions
    {
        public static double Proximity_score(this double source, double other_amount)
        {
            var result = Math.Abs(source - other_amount);
            return result;
        }

        public static bool Double_equals(this double source, double other_amount)
        {
            return Math.Abs(source - other_amount) < 0.001;
        }

        public static bool Double_less_than(this double source, double other_amount)
        {
            return Math.Round((decimal)source, 2) < Math.Round((decimal)other_amount, 2);
        }

        public static bool Double_greater_than(this double source, double other_amount)
        {
            return Math.Round((decimal)source, 2) > Math.Round((decimal)other_amount, 2);
        }
    }
}