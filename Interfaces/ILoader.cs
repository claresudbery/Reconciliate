using Interfaces.DTOs;

namespace Interfaces
{
    public interface ILoader<TThirdPartyType, TOwnedType> 
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        IDataFile<TThirdPartyType> CreateNewThirdPartyFile(IFileIO<TThirdPartyType> thirdPartyFileIO);

        IDataFile<TOwnedType> CreateNewOwnedFile(IFileIO<TOwnedType> ownedFileIO);

        void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<TOwnedType> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo);
    }
}