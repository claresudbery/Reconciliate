using Interfaces.DTOs;

namespace Interfaces
{
    public interface IMatcher
    {
        void DoMatching(FilePaths mainFilePaths);

        void DoPreliminaryStuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliationInterface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new();

        void Finish();
    }
}