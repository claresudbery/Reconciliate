using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInData : ILoader<ActualBankRecord, BankRecord>
    {
        private ExpectedIncomeFile _expectedIncomeFile;
        private CSVFile<ExpectedIncomeRecord> _expectedIncomeCSVFile;

        public BankAndBankInData()
        {
            var expectedIncomeFileIO = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            _expectedIncomeCSVFile = new CSVFile<ExpectedIncomeRecord>(expectedIncomeFileIO);
            _expectedIncomeFile = new ExpectedIncomeFile(_expectedIncomeCSVFile);
        }

        public DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo()
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
                Loader = this,
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

        public IDataFile<ActualBankRecord> CreateNewThirdPartyFile(IFileIO<ActualBankRecord> thirdPartyFileIO)
        {
            var csvFile = new CSVFile<ActualBankRecord>(thirdPartyFileIO);
            return new ActualBankInFile(csvFile);
        }

        public IDataFile<BankRecord> CreateNewOwnedFile(IFileIO<BankRecord> ownedFileIO)
        {
            var csvFile = new CSVFile<BankRecord>(ownedFileIO);
            return new GenericFile<BankRecord>(csvFile);
        }

        public void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<ActualBankRecord, BankRecord> dataLoadingInfo)
        {
            // When we don't have loaders any more, this functionality will have to exist in another switch statement,
            // in ReconciliationIntro, with separate functions for each type. See ReconciliationIntro.MergeOtherData.
            // Just copy this Code wholesale - no need to trace back through commits.
            inputOutput.OutputLine(ReconConsts.LoadingExpenses);
            var expectedIncomeFileIO = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expectedIncomeCSVFile = new CSVFile<ExpectedIncomeRecord>(expectedIncomeFileIO);
            expectedIncomeCSVFile.Load(false);
            var expectedIncomeFile = new ExpectedIncomeFile(expectedIncomeCSVFile);
            spreadsheet.AddUnreconciledRowsToCsvFile<ExpectedIncomeRecord>(MainSheetNames.ExpectedIn, expectedIncomeFile.File);
            expectedIncomeCSVFile.PopulateSourceRecordsFromRecords();
            expectedIncomeFile.FilterForEmployerExpensesOnly();
            expectedIncomeFile.CopyToPendingFile(pendingFile);
            expectedIncomeCSVFile.PopulateRecordsFromOriginalFileLoad();
        }
    }
}