using System.Collections.Generic;

namespace Interfaces.Delegates
{
    public delegate IEnumerable<IPotentialMatch> MatchFindingDelegate<TThirdPartyType, TOwnedType>
        (TThirdPartyType sourceRecord, ICSVFile<TOwnedType> ownedFile)
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new();
}