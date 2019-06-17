using System.Collections.Generic;

namespace Interfaces.DTOs
{
    public class RecordForMatching<TThirdPartyType> where TThirdPartyType : ICSVRecord, new()
    {
        public TThirdPartyType SourceRecord;
        public List<IPotentialMatch> Matches { get; private set; }

        public RecordForMatching(
            TThirdPartyType source_record,
            List<IPotentialMatch> matches)
        {
            SourceRecord = source_record;
            Matches = matches;
        }
    }
}