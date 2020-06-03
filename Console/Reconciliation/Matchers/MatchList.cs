using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    class MatchList
    {
        public double TargetAmount { get; set; }
        public IList<ICSVRecord> Matches { get; set; }

        public double ActualAmount()
        {
            return Matches.Sum(x => x.Main_amount());
        }

        public bool ExactMatch()
        {
            return ActualAmount().Equals(TargetAmount);
        }
    }
}