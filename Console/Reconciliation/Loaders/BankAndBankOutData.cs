using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal static class BankAndBankOutData
    {
        public static DataLoadingInformation LoadingInfo =
            new DataLoadingInformation
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_bank_file_name,
                    Owned_file_name = ReconConsts.DefaultBankOutFileName
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.DefaultBankOutPendingFileName,
                Sheet_name = MainSheetNames.Bank_out,
                Third_party_descriptor = ReconConsts.Bank_descriptor,
                Owned_file_descriptor = ReconConsts.BankOutDescriptor,
                Monthly_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.SOD_ds,
                    End_divider = Dividers.Cred_card1,
                    First_column_number = 2,
                    Last_column_number = 6
                },
                Annual_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.Annual_sod_ds,
                    End_divider = Dividers.Annual_total,
                    First_column_number = 2,
                    Last_column_number = 6
                },
                Third_party_file_load_action = ThirdPartyFileLoadAction.FilterForNegativeRecordsOnly
            };
    }
}