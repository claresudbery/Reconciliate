using Interfaces.DTOs;

namespace Interfaces
{
    public interface IMatcher
    {
        void Do_matching(FilePaths main_file_paths);

        void Do_preliminary_stuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new();

        void Finish();
    }
}