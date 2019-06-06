using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutData
    {
        public static DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo =
            new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultBankFileName,
                    OwnedFileName = ReconConsts.DefaultBankOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultBankOutPendingFileName,
                SheetName = MainSheetNames.BankOut,
                ThirdPartyDescriptor = ReconConsts.BankDescriptor,
                OwnedFileDescriptor = ReconConsts.BankOutDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.SODDs,
                    EndDivider = Dividers.CredCard1,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                },
                AnnualBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.AnnualSODDs,
                    EndDivider = Dividers.AnnualTotal,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                }
            };
    }
}