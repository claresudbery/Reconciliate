using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IPotentialMatch
    {
        IList<ICSVRecord> ActualRecords { get; set; }
        bool AmountMatch { get; set; }
        bool FullTextMatch { get; set; }
        bool PartialTextMatch { get; set; }
        Rankings Rankings { get; set; }
        List<ConsoleLine> ConsoleLines { get; set; }
    }
}