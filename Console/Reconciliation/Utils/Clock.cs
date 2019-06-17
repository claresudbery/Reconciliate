using System;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class Clock : IClock
    {
        public DateTime Now_date_time()
        {
            return DateTime.Now;
        }
    }
}