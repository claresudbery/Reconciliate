using System.Collections.Generic;

namespace Interfaces.Delegates
{
    public delegate IEnumerable<IPotentialMatch> MatchFindingDelegate<TThirdPartyType, TOwnedType>
        (TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new();
}