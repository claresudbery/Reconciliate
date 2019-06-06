using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInData
    {
        public static DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo =
            new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultBankFileName,
                    OwnedFileName = ReconConsts.DefaultBankInFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultBankInPendingFileName,
                SheetName = MainSheetNames.BankIn,
                ThirdPartyDescriptor = ReconConsts.BankDescriptor,
                OwnedFileDescriptor = ReconConsts.BankInDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetIn,
                    StartDivider = Dividers.Date,
                    EndDivider = Dividers.Total,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                },
                AnnualBudgetData = null
            };
        public DataLoadingInformation<ActualBankRecord, BankRecord> TempLoadingInfo()
        { 
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultBankFileName,
                    OwnedFileName = ReconConsts.DefaultBankInFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultBankInPendingFileName,
                SheetName = MainSheetNames.BankIn,
                ThirdPartyDescriptor = ReconConsts.BankDescriptor,
                OwnedFileDescriptor = ReconConsts.BankInDescriptor,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetIn,
                    StartDivider = Dividers.Date,
                    EndDivider = Dividers.Total,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                },
                AnnualBudgetData = null
            };
        }
    }
}