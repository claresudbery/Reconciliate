using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class FileLoader
    {
        private readonly IInputOutput _inputOutput;

        public FileLoader(IInputOutput inputOutput)
        {
            _inputOutput = inputOutput;
        }

        public ReconciliationInterface<TThirdPartyType, TOwnedType>
            LoadCorrectFiles<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo,
                ISpreadsheetRepoFactory spreadsheetFactory,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine("Loading data...");

            ReconciliationInterface<TThirdPartyType, TOwnedType> reconciliationInterface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheetRepo = spreadsheetFactory.CreateSpreadsheetRepo();
                var spreadsheet = new Spreadsheet(spreadsheetRepo);
                BudgetingMonths budgetingMonths = RecursivelyAskForBudgetingMonths(spreadsheet);
                var pendingFileIO = new FileIO<TOwnedType>(spreadsheetFactory);
                var thirdPartyFileIO = new FileIO<TThirdPartyType>(spreadsheetFactory);
                var ownedFileIO = new FileIO<TOwnedType>(spreadsheetFactory);
                var pendingFile = new CSVFile<TOwnedType>(pendingFileIO);

                reconciliationInterface =
                    LoadFilesAndMergeData<TThirdPartyType, TOwnedType>(
                        spreadsheet, pendingFileIO, pendingFile, thirdPartyFileIO, ownedFileIO, budgetingMonths, dataLoadingInfo, matcher);
            }
            finally
            {
                spreadsheetFactory.DisposeOfSpreadsheetRepo();
            }

            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("");

            return reconciliationInterface;
        }

        // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
        // We load it up into memory.
        // Then some budget amounts are added to that file (in memory).
        // Other budget amounts (like CredCard1 balance) have been written directly to the spreadsheet before this.
        // Then we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
        // Then we write all that data away into the 'owned' csv file (eg BankOut.csv). Then we read it back in again!
        // Also we load up the third party data, and pass it all on to the reconciliation interface.
        public ReconciliationInterface<TThirdPartyType, TOwnedType>
            LoadFilesAndMergeData<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                IFileIO<TOwnedType> pendingFileIO,
                ICSVFile<TOwnedType> pendingFile,
                IFileIO<TThirdPartyType> thirdPartyFileIO,
                IFileIO<TOwnedType> ownedFileIO,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            LoadPendingData(pendingFileIO, pendingFile, dataLoadingInfo);
            MergeBudgetData(spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            MergeOtherData(spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            MergeUnreconciledData(spreadsheet, pendingFile, dataLoadingInfo);
            var reconciliator = LoadThirdPartyAndOwnedFilesIntoReconciliator<TThirdPartyType, TOwnedType>(dataLoadingInfo, thirdPartyFileIO, ownedFileIO);
            var reconciliationInterface = CreateReconciliationInterface(dataLoadingInfo, reconciliator, matcher);
            return reconciliationInterface;
        }

        public void LoadPendingData<TThirdPartyType, TOwnedType>(
                IFileIO<TOwnedType> pendingFileIO,
                ICSVFile<TOwnedType> pendingFile,
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine(
                "Loading data from pending file (which you should have already split out, if necessary)...");
            pendingFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.PendingFileName);
            pendingFile.Load(true, dataLoadingInfo.DefaultSeparator);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pendingFile.ConvertSourceLineSeparators(dataLoadingInfo.DefaultSeparator, dataLoadingInfo.LoadingSeparator);
        }

        private void MergeBudgetData<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo)
                where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine("Merging budget data with pending data...");
            spreadsheet.AddBudgetedMonthlyDataToPendingFile(budgetingMonths, pendingFile, dataLoadingInfo.MonthlyBudgetData);
            if (null != dataLoadingInfo.AnnualBudgetData)
            {
                spreadsheet.AddBudgetedAnnualDataToPendingFile(budgetingMonths, pendingFile, dataLoadingInfo.AnnualBudgetData);
            }
        }

        private void MergeOtherData<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            dataLoadingInfo.Loader.MergeBespokeDataWithPendingFile(_inputOutput, spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
        }

        private void MergeUnreconciledData<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pendingFile,
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile<TOwnedType>(dataLoadingInfo.SheetName, pendingFile);

            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pendingFile.UpdateSourceLinesForOutput(dataLoadingInfo.LoadingSeparator);
            pendingFile.WriteToFileAsSourceLines(dataLoadingInfo.FilePaths.OwnedFileName);

            _inputOutput.OutputLine("...");
        }

        private Reconciliator<TThirdPartyType, TOwnedType>
            LoadThirdPartyAndOwnedFilesIntoReconciliator<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo,
                IFileIO<TThirdPartyType> thirdPartyFileIO,
                IFileIO<TOwnedType> ownedFileIO)
            where TThirdPartyType : ICSVRecord, new() where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine("Loading data back in from 'owned' and 'third party' files...");
            thirdPartyFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.ThirdPartyFileName);
            ownedFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.OwnedFileName);
            var thirdPartyFile = dataLoadingInfo.Loader.CreateNewThirdPartyFile(thirdPartyFileIO);
            var ownedFile = dataLoadingInfo.Loader.CreateNewOwnedFile(ownedFileIO);

            var reconciliator = new Reconciliator<TThirdPartyType, TOwnedType>(
                dataLoadingInfo,
                thirdPartyFile,
                ownedFile);

            return reconciliator;
        }

        private ReconciliationInterface<TThirdPartyType, TOwnedType>
            CreateReconciliationInterface<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> dataLoadingInfo,
                Reconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _inputOutput.OutputLine("Creating reconciliation interface...");
            var reconciliationInterface = new ReconciliationInterface<TThirdPartyType, TOwnedType>(
                new InputOutput(),
                reconciliator,
                dataLoadingInfo.ThirdPartyDescriptor,
                dataLoadingInfo.OwnedFileDescriptor,
                matcher);
            return reconciliationInterface;
        }

        public BudgetingMonths RecursivelyAskForBudgetingMonths(ISpreadsheet spreadsheet)
        {
            DateTime nextUnplannedMonth = GetNextUnplannedMonth(spreadsheet);
            int lastMonthForBudgetPlanning = GetLastMonthForBudgetPlanning(spreadsheet, nextUnplannedMonth.Month);
            var budgetingMonths = new BudgetingMonths
            {
                NextUnplannedMonth = nextUnplannedMonth.Month,
                LastMonthForBudgetPlanning = lastMonthForBudgetPlanning,
                StartYear = nextUnplannedMonth.Year
            };
            if (lastMonthForBudgetPlanning != 0)
            {
                budgetingMonths.LastMonthForBudgetPlanning = ConfirmBudgetingMonthChoicesWithUser(budgetingMonths, spreadsheet);
            }
            return budgetingMonths;
        }

        private DateTime GetNextUnplannedMonth(ISpreadsheet spreadsheet)
        {
            DateTime defaultMonth = DateTime.Today;
            DateTime nextUnplannedMonth = defaultMonth;
            bool badInput = false;
            try
            {
                nextUnplannedMonth = spreadsheet.GetNextUnplannedMonth();
            }
            catch (Exception)
            {
                string newMonth = _inputOutput.GetInput(ReconConsts.CantFindMortgageRow);
                try
                {
                    if (!String.IsNullOrEmpty(newMonth) && Char.IsDigit(newMonth[0]))
                    {
                        int actualMonth = Convert.ToInt32(newMonth);
                        if (actualMonth < 1 || actualMonth > 12)
                        {
                            badInput = true;
                        }
                        else
                        {
                            var year = defaultMonth.Year;
                            if (actualMonth < defaultMonth.Month)
                            {
                                year++;
                            }
                            nextUnplannedMonth = new DateTime(year, actualMonth, 1);
                        }
                    }
                    else
                    {
                        badInput = true;
                    }
                }
                catch (Exception)
                {
                    badInput = true;
                }
            }

            if (badInput)
            {
                _inputOutput.OutputLine(ReconConsts.DefaultUnplannedMonth);
                nextUnplannedMonth = defaultMonth;
            }

            return nextUnplannedMonth;
        }

        private int GetLastMonthForBudgetPlanning(ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            string nextUnplannedMonthAsString = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nextUnplannedMonth);
            var requestToEnterMonth = String.Format(ReconConsts.EnterMonths, nextUnplannedMonthAsString);
            string month = _inputOutput.GetInput(requestToEnterMonth);
            int result = 0;

            try
            {
                if (!String.IsNullOrEmpty(month) && Char.IsDigit(month[0]))
                {
                    result = Convert.ToInt32(month);
                    if (result < 1 || result > 12)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore it and return zero by default.
            }

            result = HandleZeroMonthChoiceResult(result, spreadsheet, nextUnplannedMonth);
            return result;
        }

        private int ConfirmBudgetingMonthChoicesWithUser(BudgetingMonths budgetingMonths, ISpreadsheet spreadsheet)
        {
            var newResult = budgetingMonths.LastMonthForBudgetPlanning;
            string input = GetResponseToBudgetingMonthsConfirmationMessage(budgetingMonths);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                newResult = budgetingMonths.LastMonthForBudgetPlanning;
            }
            else
            {
                // Recursion ftw!
                newResult = GetLastMonthForBudgetPlanning(spreadsheet, budgetingMonths.NextUnplannedMonth);
            }

            return newResult;
        }

        private string GetResponseToBudgetingMonthsConfirmationMessage(BudgetingMonths budgetingMonths)
        {
            string firstMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.NextUnplannedMonth);
            string secondMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.LastMonthForBudgetPlanning);

            int monthSpan = budgetingMonths.NumBudgetingMonths();

            var confirmationText = String.Format(ReconConsts.ConfirmMonthInterval, firstMonth, secondMonth, monthSpan);

            return _inputOutput.GetInput(confirmationText);
        }

        private int HandleZeroMonthChoiceResult(int chosenMonth, ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            var newResult = chosenMonth;
            if (chosenMonth == 0)
            {
                var input = _inputOutput.GetInput(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    newResult = 0;
                    _inputOutput.OutputLine(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    newResult = GetLastMonthForBudgetPlanning(spreadsheet, nextUnplannedMonth);
                }
            }
            return newResult;
        }
    }
}