using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard2AndCredCard2InOutData
    {
        public static DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> LoadingInfo =
            new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultCredCard2FileName,
                    OwnedFileName = ReconConsts.DefaultCredCard2InOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultCredCard2InOutPendingFileName,
                SheetName = MainSheetNames.CredCard2,
                ThirdPartyDescriptor = ReconConsts.CredCard2Descriptor,
                OwnedFileDescriptor = ReconConsts.CredCard2InOutDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.CredCard2,
                    EndDivider = Dividers.SODDTotal,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 5
                },
                AnnualBudgetData = null
            };
        public DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> TempLoadingInfo()
        {
            return new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultCredCard2FileName,
                    OwnedFileName = ReconConsts.DefaultCredCard2InOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultCredCard2InOutPendingFileName,
                SheetName = MainSheetNames.CredCard2,
                ThirdPartyDescriptor = ReconConsts.CredCard2Descriptor,
                OwnedFileDescriptor = ReconConsts.CredCard2InOutDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.CredCard2,
                    EndDivider = Dividers.SODDTotal,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 5
                },
                AnnualBudgetData = null
            };
        }
    }
}