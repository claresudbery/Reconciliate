namespace Interfaces
{
    public interface IReconciliationInterface<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        void Do_semi_automatic_matching();
    }
}