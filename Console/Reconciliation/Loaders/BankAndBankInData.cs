using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal static class BankAndBankInData
    {
        public static DataLoadingInformation LoadingInfo =
            new DataLoadingInformation
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_bank_file_name,
                    Owned_file_name = ReconConsts.DefaultBankInFileName
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.DefaultBankInPendingFileName,
                Sheet_name = MainSheetNames.Bank_in,
                Third_party_descriptor = ReconConsts.Bank_descriptor,
                Owned_file_descriptor = ReconConsts.BankInDescriptor,
                Monthly_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_in,
                    Start_divider = Dividers.Date,
                    End_divider = Dividers.Total,
                    First_column_number = 2,
                    Last_column_number = 6
                },
                Annual_budget_data = null,
                Third_party_file_load_action = ThirdPartyFileLoadAction.FilterForPositiveRecordsOnly
            };
    }
}