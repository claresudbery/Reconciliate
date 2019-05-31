using System;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class Clock : IClock
    {
        public DateTime NowDateTime()
        {
            return DateTime.Now;
        }
    }
}