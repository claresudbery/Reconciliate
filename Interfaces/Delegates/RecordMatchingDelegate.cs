using Interfaces.DTOs;

namespace Interfaces.Delegates
{
    public delegate void RecordMatchingDelegate<TThirdPartyType, TOwnedType>
            (RecordForMatching<TThirdPartyType> recordForMatching, 
            int matchIndex,
            ICSVFile<TOwnedType> ownedFile)
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new();
}