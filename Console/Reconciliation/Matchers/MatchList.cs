using System.Collections.Generic;
using System.Linq;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    class MatchList
    {
        public double TargetAmount { get; set; }
        public List<double> Matches { get; set; }

        public double ActualAmount()
        {
            return Matches.Sum();
        }

        public bool ExactMatch()
        {
            return ActualAmount().Equals(TargetAmount);
        }
    }
}