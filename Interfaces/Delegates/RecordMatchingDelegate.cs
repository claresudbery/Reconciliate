using Interfaces.DTOs;

namespace Interfaces.Delegates
{
    public delegate void RecordMatchingDelegate<TThirdPartyType, TOwnedType>
            (RecordForMatching<TThirdPartyType> record_for_matching, 
            int match_index,
            ICSVFile<TOwnedType> owned_file)
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new();
}