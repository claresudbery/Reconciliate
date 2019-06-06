using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class DummyLoader : ILoader<ActualBankRecord, BankRecord>
    {
        public DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo()
        {
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
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
                Loader = this,
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

        public void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<ActualBankRecord, BankRecord> dataLoadingInfo)
        {
        }
    }
}