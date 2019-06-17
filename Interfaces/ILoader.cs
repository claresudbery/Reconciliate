using Interfaces.DTOs;

namespace Interfaces
{
    public interface ILoader<TThirdPartyType, TOwnedType> 
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        IDataFile<TThirdPartyType> Create_new_third_party_file(IFileIO<TThirdPartyType> third_party_file_io);

        IDataFile<TOwnedType> Create_new_owned_file(IFileIO<TOwnedType> owned_file_io);

        void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<TOwnedType> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info);
    }
}