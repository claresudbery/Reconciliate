using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    class MatchList
    {
        public double TargetAmount { get; set; }
        public IList<ICSVRecord> Matches { get; set; }

        public double Actual_amount()
        {
            return Matches.Sum(x => x.Main_amount());
        }

        public bool Exact_match()
        {
            return Actual_amount().Double_equals(TargetAmount);
        }

        public double Distance_from_target()
        {
            return TargetAmount - Actual_amount();
        }
    }
}