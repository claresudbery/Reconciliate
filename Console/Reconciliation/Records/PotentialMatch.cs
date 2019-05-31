using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class PotentialMatch : IPotentialMatch
    {
        public const int PartialDateMatchThreshold = 3;
        public const int PartialAmountMatchThreshold = 2;

        public IList<ICSVRecord> ActualRecords { get; set; }

        public bool AmountMatch { get; set; }
        public bool FullTextMatch { get; set; }
        public bool PartialTextMatch { get; set; }
        public Rankings Rankings { get; set; }

        public List<ConsoleLine> ConsoleLines { get; set; }
    }
}