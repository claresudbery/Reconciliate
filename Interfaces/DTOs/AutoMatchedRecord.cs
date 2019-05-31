namespace Interfaces.DTOs
{
    public class AutoMatchedRecord<TThirdPartyType> where TThirdPartyType : ICSVRecord, new()
    {
        public TThirdPartyType SourceRecord;
        public IPotentialMatch Match { get; set; }
        public int Index { get; private set; }

        public AutoMatchedRecord(
            TThirdPartyType sourceRecord,
            IPotentialMatch match,
            int index)
        {
            SourceRecord = sourceRecord;
            Match = match;
            Index = index;
        }
    }
}