using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInLoader : IBankAndBankInLoader, ILoader<ActualBankRecord, BankRecord>
    {
        private ExpectedIncomeFile _expectedIncomeFile;
        private CSVFile<ExpectedIncomeRecord> _expectedIncomeCSVFile;

        public BankAndBankInLoader(ISpreadsheetRepoFactory spreadsheetRepoFactory)
        {
            var expectedIncomeFileIO = new FileIO<ExpectedIncomeRecord>(spreadsheetRepoFactory);
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
            inputOutput.OutputLine(ReconConsts.LoadingExpenses);
            _expectedIncomeCSVFile.Load(false);

            spreadsheet.AddUnreconciledRowsToCsvFile<ExpectedIncomeRecord>(MainSheetNames.ExpectedIn, _expectedIncomeFile.File);
            _expectedIncomeCSVFile.PopulateSourceRecordsFromRecords();
            _expectedIncomeFile.FilterForEmployerExpensesOnly();

            _expectedIncomeFile.CopyToPendingFile(pendingFile);
            _expectedIncomeCSVFile.PopulateRecordsFromOriginalFileLoad();
        }

        public void UpdateExpectedIncomeRecordWhenMatched(ICSVRecord sourceRecord, ICSVRecord matchedRecord)
        {
            _expectedIncomeFile.UpdateExpectedIncomeRecordWhenMatched(sourceRecord, matchedRecord);
        }

        public void Finish()
        {
            _expectedIncomeFile.Finish();
        }
    }
}