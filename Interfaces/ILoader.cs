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

        void Do_actions_which_require_third_party_data_access(
            IDataFile<TThirdPartyType> third_party_file,
            IDataFile<TOwnedType> owned_file,
            ISpreadsheet spreadsheet,
            IInputOutput input_output);

        void Generate_ad_hoc_data(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<TOwnedType> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info);
    }
}