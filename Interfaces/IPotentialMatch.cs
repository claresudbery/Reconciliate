using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IPotentialMatch
    {
        IList<ICSVRecord> Actual_records { get; set; }
        bool Amount_match { get; set; }
        bool Full_text_match { get; set; }
        bool Partial_text_match { get; set; }
        Rankings Rankings { get; set; }
        List<ConsoleLine> Console_lines { get; set; }
    }
}