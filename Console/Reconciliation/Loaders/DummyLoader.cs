using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class DummyLoader
    {
        public static DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo = 
            new DataLoadingInformation<ActualBankRecord, BankRecord>
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
                SheetName = MainSheetNames.BankOut,
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