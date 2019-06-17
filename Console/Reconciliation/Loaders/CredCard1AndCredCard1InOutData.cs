using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal static class CredCard1AndCredCard1InOutData
    {
        public static DataLoadingInformation LoadingInfo =
            new DataLoadingInformation
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_cred_card1_file_name,
                    Owned_file_name = ReconConsts.Default_cred_card1_in_out_file_name
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.Default_cred_card1_in_out_pending_file_name,
                Sheet_name = MainSheetNames.Cred_card1,
                Third_party_descriptor = ReconConsts.Cred_card1_descriptor,
                Owned_file_descriptor = ReconConsts.Cred_card1_in_out_descriptor,
                Monthly_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.Cred_card1,
                    End_divider = Dividers.Cred_card2,
                    First_column_number = 2,
                    Last_column_number = 5
                },
                Annual_budget_data = null,
                // CredCard1Record.Load is already multiplying all mounts by -1, so no need to SwapSignsOfAllAmounts here.
                Third_party_file_load_action = ThirdPartyFileLoadAction.NoAction
            };
    }
}