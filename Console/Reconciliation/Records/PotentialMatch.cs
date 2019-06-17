using System.Collections.Generic;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class PotentialMatch : IPotentialMatch
    {
        public const int PartialDateMatchThreshold = 3;
        public const int PartialAmountMatchThreshold = 2;

        public IList<ICSVRecord> Actual_records { get; set; }

        public bool Amount_match { get; set; }
        public bool Full_text_match { get; set; }
        public bool Partial_text_match { get; set; }
        public Rankings Rankings { get; set; }

        public List<ConsoleLine> Console_lines { get; set; }
    }
}