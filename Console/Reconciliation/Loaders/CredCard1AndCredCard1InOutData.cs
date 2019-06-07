using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal static class CredCard1AndCredCard1InOutData
    {
        public static DataLoadingInformation LoadingInfo =
            new DataLoadingInformation
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultCredCard1FileName,
                    OwnedFileName = ReconConsts.DefaultCredCard1InOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultCredCard1InOutPendingFileName,
                SheetName = MainSheetNames.CredCard1,
                ThirdPartyDescriptor = ReconConsts.CredCard1Descriptor,
                OwnedFileDescriptor = ReconConsts.CredCard1InOutDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.CredCard1,
                    EndDivider = Dividers.CredCard2,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 5
                },
                AnnualBudgetData = null,
                // CredCard1Record.Load is already multiplying all mounts by -1, so no need to SwapSignsOfAllAmounts here.
                ThirdPartyFileLoadAction = ThirdPartyFileLoadAction.NoAction
            };
    }
}