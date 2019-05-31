namespace Interfaces
{
    public interface IReconciliationInterface<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        void DoSemiAutomaticMatching();
    }
}