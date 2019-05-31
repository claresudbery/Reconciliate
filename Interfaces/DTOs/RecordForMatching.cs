using System.Collections.Generic;

namespace Interfaces.DTOs
{
    public class RecordForMatching<TThirdPartyType> where TThirdPartyType : ICSVRecord, new()
    {
        public TThirdPartyType SourceRecord;
        public List<IPotentialMatch> Matches { get; private set; }

        public RecordForMatching(
            TThirdPartyType sourceRecord,
            List<IPotentialMatch> matches)
        {
            SourceRecord = sourceRecord;
            Matches = matches;
        }
    }
}