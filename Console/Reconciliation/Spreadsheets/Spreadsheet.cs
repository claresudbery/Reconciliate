using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class Spreadsheet : ISpreadsheet
    {
        private readonly ISpreadsheetRepo _spreadsheetIO;

        public Spreadsheet(ISpreadsheetRepo spreadsheetIO)
        {
            _spreadsheetIO = spreadsheetIO;
        }

        public ICellRow ReadLastRow(String sheetName)
        {
            return _spreadsheetIO.ReadLastRow(sheetName);
        }

        public String ReadLastRowAsCsv(String sheetName, ICSVRecord csvRecord)
        {
            return _spreadsheetIO.ReadLastRowAsCsv(sheetName, csvRecord);
        }

        public String ReadSpecifiedRowAsCsv(String sheetName, int rowNumber, ICSVRecord csvRecord)
        {
            csvRecord.ReadFromSpreadsheetRow(_spreadsheetIO.ReadSpecifiedRow(sheetName, rowNumber));
            return csvRecord.ToCsv();
        }

        public int FindRowNumberOfLastDividerRow(string sheetName)
        {
            return _spreadsheetIO.FindRowNumberOfLastRowContainingCell(
                sheetName,
                Dividers.DividerText);
        }

        public void AddUnreconciledRowsToCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            int dividerRowNumber = FindRowNumberOfLastDividerRow(sheetName);
            int lastRowNumber = _spreadsheetIO.LastRowNumber(sheetName);

            for (int rowNumber = dividerRowNumber + 1; rowNumber <= lastRowNumber; rowNumber++)
            {
                var csvRecord = new TRecordType();
                csvRecord.ReadFromSpreadsheetRow(_spreadsheetIO.ReadSpecifiedRow(sheetName, rowNumber));
                csvFile.Records.Add(csvRecord);
            }

            csvFile.Records = csvFile.Records.OrderBy(record => record.Date).ToList();
        }

        public void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            _spreadsheetIO.AppendCsvFile(sheetName, csvFile);
        }

        public void DeleteUnreconciledRows(string sheetName)
        {
            int lastRowNumber = _spreadsheetIO.LastRowNumber(sheetName);
            int dividerRowNumber = FindRowNumberOfLastDividerRow(sheetName);
            _spreadsheetIO.DeleteSpecifiedRows(sheetName, dividerRowNumber + 1, lastRowNumber);
        }

        public ICSVFile<TRecordType> ReadUnreconciledRowsAsCsvFile<TRecordType>(
            ICSVFileFactory<TRecordType> csvFileFactory,
            String sheetName) where TRecordType : ICSVRecord, new()
        {
            var csvFile = csvFileFactory.CreateCSVFile(false);

            AddUnreconciledRowsToCsvFile(sheetName, csvFile);

            return csvFile;
        }

        public List<BankRecord> GetAllMonthlyBankInBudgetItems(BudgetItemListData budgetItemListData)
        {
            int firstRowNumber = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int lastRowNumber = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<BankRecord>(
                budgetItemListData.SheetName,
                firstRowNumber,
                lastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<BankRecord> GetAllMonthlyBankOutBudgetItems(BudgetItemListData budgetItemListData)
        {
            int firstRowNumber = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int lastRowNumber = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<BankRecord>(
                budgetItemListData.SheetName,
                firstRowNumber,
                lastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<CredCard1InOutRecord> GetAllMonthlyCredCard1BudgetItems(BudgetItemListData budgetItemListData)
        {
            int firstRowNumber = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int lastRowNumber = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<CredCard1InOutRecord>(
                budgetItemListData.SheetName,
                firstRowNumber,
                lastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<CredCard2InOutRecord> GetAllMonthlyCredCard2BudgetItems(BudgetItemListData budgetItemListData)
        {
            int firstRowNumber = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int lastRowNumber = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<CredCard2InOutRecord>(
                budgetItemListData.SheetName,
                firstRowNumber,
                lastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<TRecordType> GetAllAnnualBudgetItems<TRecordType>(BudgetItemListData budgetItemListData)
            where TRecordType : ICSVRecord, new()
        {
            int firstRowNumber = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int lastRowNumber = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<TRecordType>(
                budgetItemListData.SheetName,
                firstRowNumber,
                lastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public double GetSecondChildPocketMoneyAmount(string shortDateTime)
        {
            const int dateColumn = 1;
            const int amountColumn = 4;
            var sheetName = PocketMoneySheetNames.SecondChild;
            int row = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(sheetName, shortDateTime, dateColumn);

            return _spreadsheetIO.GetAmount(sheetName, row, amountColumn);
        }

        public double GetPlanningExpensesAlreadyDone()
        {
            var sheetName = PlanningSheetNames.Expenses;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.GetAmount(sheetName, row, column);
        }

        public double GetPlanningMoneyPaidByGuests()
        {
            var sheetName = PlanningSheetNames.Deposits;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.GetAmount(sheetName, row, column);
        }

        public void InsertNewRowOnExpectedOut(double newAmount, string newNotes)
        {
            const int codeColumn = 4;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int newRowNumber = _spreadsheetIO.FindRowNumberOfFirstRowContainingCell(
                MainSheetNames.ExpectedOut,
                ReconConsts.ExpectedOutInsertionPoint,
                codeColumn);

            var cellValues = new Dictionary<int, object>()
            {
                { amountColumn, newAmount },
                { notesColumn, newNotes }
            };

            _spreadsheetIO.InsertNewRow(MainSheetNames.ExpectedOut, newRowNumber, cellValues);
        }

        public void AddNewTransactionToCredCard3(DateTime newDate, double newAmount, string newDescription)
        {
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;

            var cellValues = new Dictionary<int, object>()
            {
                { dateColumn, newDate.ToOADate() },
                { amountColumn, newAmount },
                { descriptionColumn, newDescription }
            };

            _spreadsheetIO.AppendNewRow(MainSheetNames.CredCard3, cellValues);
        }

        public void AddNewTransactionToSavings(DateTime newDate, double newAmount)
        {
            var sheetName = MainSheetNames.Savings;

            int firstColumnNumber = 1;
            int rowNumber = _spreadsheetIO.FindFirstEmptyRowInColumn(sheetName, firstColumnNumber);

            _spreadsheetIO.UpdateDate(sheetName, rowNumber, firstColumnNumber, newDate);
            _spreadsheetIO.UpdateAmount(sheetName, rowNumber, firstColumnNumber + 1, newAmount);
        }

        public DateTime GetNextUnplannedMonth()
        {
            string mortgageRowDescription = GetBudgetItemDescription(Codes.Code042);
            BankRecord bankRecord = GetLastBankOutRecordWithSpecifiedDescription(mortgageRowDescription);

            DateTime nextUnplannedMonth = bankRecord.Date.AddMonths(1);

            return nextUnplannedMonth;
        }

        private BankRecord GetLastBankOutRecordWithSpecifiedDescription(string description)
        {
            var bankRecord = new BankRecord();

            var rowNumberOfLastRelevantPayment = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(
                MainSheetNames.BankOut,
                description,
                BankRecord.DescriptionIndex + 1);
            var bankOutRow = _spreadsheetIO.ReadSpecifiedRow(
                MainSheetNames.BankOut,
                rowNumberOfLastRelevantPayment);
            bankRecord.ReadFromSpreadsheetRow(bankOutRow);

            return bankRecord;
        }

        private string GetBudgetItemDescription(string budgetItemCode)
        {
            var bankRecord = new BankRecord();

            int budgetOutCodeColumn = 1;
            const int budgetOutFirstBankRecordColumnNumber = 2;
            const int budgetOutLastBankRecordColumnNumber = 6;

            var rowNumberOfBudgetItem = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(
                MainSheetNames.BudgetOut,
                budgetItemCode,
                budgetOutCodeColumn);
            var budgetItemRow = _spreadsheetIO.ReadSpecifiedRow(
                MainSheetNames.BudgetOut,
                rowNumberOfBudgetItem,
                budgetOutFirstBankRecordColumnNumber,
                budgetOutLastBankRecordColumnNumber);
            bankRecord.ReadFromSpreadsheetRow(budgetItemRow);

            return bankRecord.Description;
        }

        public void AddBudgetedBankInDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var baseRecords = GetAllMonthlyBankInBudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                baseRecords,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedBankOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData monthlyBudgetItemListData,
            BudgetItemListData annualBudgetItemListData)
        {
            var monthlyRecords = GetAllMonthlyBankOutBudgetItems(monthlyBudgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                monthlyRecords,
                pendingFile,
                budgetingMonths);

            var annualRecords = GetAllAnnualBudgetItems<BankRecord>(annualBudgetItemListData);
            AddRecordsToPendingFileForRecordsThatHaveMatchingMonths(
                annualRecords,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedCredCard1InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var baseRecords = GetAllMonthlyCredCard1BudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                baseRecords,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedCredCard2InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var baseRecords = GetAllMonthlyCredCard2BudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                baseRecords,
                pendingFile,
                budgetingMonths);
        }

        private void AddRecordsToPendingFileForEverySpecifiedMonth<TRecordType>(
            IEnumerable<TRecordType> baseRecords, 
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var finalMonth = budgetingMonths.LastMonthForBudgetPlanning >= budgetingMonths.NextUnplannedMonth
                ? budgetingMonths.LastMonthForBudgetPlanning
                : budgetingMonths.LastMonthForBudgetPlanning + 12;
            for (int month = budgetingMonths.NextUnplannedMonth; month <= finalMonth; month++)
            {
                int newMonth = month;
                int newYear = budgetingMonths.StartYear;
                if (month > 12)
                {
                    newMonth = month - 12;
                    newYear = newYear + 1;
                }
                var newMonthlyRecords = baseRecords.Select(
                    x => (TRecordType)
                        WithCorrectDaysPerMonth(x.Copy(), newYear, newMonth));
                pendingFile.Records.AddRange(newMonthlyRecords);
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private void AddRecordsToPendingFileForRecordsThatHaveMatchingMonths<TRecordType>(
            IEnumerable<TRecordType> baseRecords,
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var finalMonth = budgetingMonths.LastMonthForBudgetPlanning >= budgetingMonths.NextUnplannedMonth
                ? budgetingMonths.LastMonthForBudgetPlanning
                : budgetingMonths.LastMonthForBudgetPlanning + 12;
            for (int month = budgetingMonths.NextUnplannedMonth; month <= finalMonth; month++)
            {
                var newMonth = month;
                var newYear = budgetingMonths.StartYear;
                if (month > 12)
                {
                    newMonth = month - 12;
                    newYear = newYear + 1;
                }
                var newAnnualRecords = baseRecords
                    .Where(x => x.Date.Month == newMonth)
                    .Select(x => (TRecordType)
                        WithCorrectDaysPerMonth(x.Copy(), newYear, x.Date.Month));
                pendingFile.Records.AddRange(newAnnualRecords.ToList());
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private ICSVRecord WithCorrectDaysPerMonth(ICSVRecord record, int newYear, int newMonth)
        {
            int daysInMonth = DateTime.DaysInMonth(newYear, newMonth);
            int day = record.Date.Day > daysInMonth
                ? daysInMonth
                : record.Date.Day;
            return record.WithDate(new DateTime(newYear, newMonth, day));
        }

        public TRecordType GetMostRecentRowContainingText<TRecordType>(
                string sheetName, 
                string textToSearchFor,
                List<int> expectedColumnNumbers)
            where TRecordType : ICSVRecord, new()
        {
            var rowNumber = _spreadsheetIO.FindRowNumberOfLastRowWithCellContainingText(
                sheetName, 
                textToSearchFor,
                expectedColumnNumbers);
            var csvRecord = new TRecordType();
            csvRecord.ReadFromSpreadsheetRow(_spreadsheetIO.ReadSpecifiedRow(sheetName, rowNumber));

            return csvRecord;
        }

        public void UpdateBalanceOnTotalsSheet(
            string balanceCode,
            double newBalance,
            string newText,
            int balanceColumn,
            int textColumn,
            int codeColumn)
        {
            _spreadsheetIO.UpdateAmountAndText(
                MainSheetNames.Totals,
                balanceCode,
                newBalance,
                newText,
                balanceColumn,
                textColumn,
                codeColumn);
        }
    }
}