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
            int divider_row_number = FindRowNumberOfLastDividerRow(sheetName);
            int last_row_number = _spreadsheetIO.LastRowNumber(sheetName);

            for (int row_number = divider_row_number + 1; row_number <= last_row_number; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.ReadFromSpreadsheetRow(_spreadsheetIO.ReadSpecifiedRow(sheetName, row_number));
                csvFile.Records.Add(csv_record);
            }

            csvFile.Records = csvFile.Records.OrderBy(record => record.Date).ToList();
        }

        public void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            _spreadsheetIO.AppendCsvFile(sheetName, csvFile);
        }

        public void DeleteUnreconciledRows(string sheetName)
        {
            int last_row_number = _spreadsheetIO.LastRowNumber(sheetName);
            int divider_row_number = FindRowNumberOfLastDividerRow(sheetName);
            _spreadsheetIO.DeleteSpecifiedRows(sheetName, divider_row_number + 1, last_row_number);
        }

        public ICSVFile<TRecordType> ReadUnreconciledRowsAsCsvFile<TRecordType>(
            ICSVFileFactory<TRecordType> csvFileFactory,
            String sheetName) where TRecordType : ICSVRecord, new()
        {
            var csv_file = csvFileFactory.CreateCSVFile(false);

            AddUnreconciledRowsToCsvFile(sheetName, csv_file);

            return csv_file;
        }

        public List<BankRecord> GetAllMonthlyBankInBudgetItems(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int last_row_number = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<BankRecord>(
                budgetItemListData.SheetName,
                first_row_number,
                last_row_number,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<BankRecord> GetAllMonthlyBankOutBudgetItems(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int last_row_number = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<BankRecord>(
                budgetItemListData.SheetName,
                first_row_number,
                last_row_number,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<CredCard1InOutRecord> GetAllMonthlyCredCard1BudgetItems(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int last_row_number = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<CredCard1InOutRecord>(
                budgetItemListData.SheetName,
                first_row_number,
                last_row_number,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<CredCard2InOutRecord> GetAllMonthlyCredCard2BudgetItems(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int last_row_number = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<CredCard2InOutRecord>(
                budgetItemListData.SheetName,
                first_row_number,
                last_row_number,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public List<TRecordType> GetAllAnnualBudgetItems<TRecordType>(BudgetItemListData budgetItemListData)
            where TRecordType : ICSVRecord, new()
        {
            int first_row_number = _spreadsheetIO
                                     .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.StartDivider) + 1;
            int last_row_number = _spreadsheetIO
                                    .FindRowNumberOfLastRowContainingCell(budgetItemListData.SheetName, budgetItemListData.EndDivider) - 1;

            return _spreadsheetIO.GetRowsAsRecords<TRecordType>(
                budgetItemListData.SheetName,
                first_row_number,
                last_row_number,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber);
        }

        public double GetSecondChildPocketMoneyAmount(string shortDateTime)
        {
            const int dateColumn = 1;
            const int amountColumn = 4;
            var sheet_name = PocketMoneySheetNames.SecondChild;
            int row = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(sheet_name, shortDateTime, dateColumn);

            return _spreadsheetIO.GetAmount(sheet_name, row, amountColumn);
        }

        public double GetPlanningExpensesAlreadyDone()
        {
            var sheet_name = PlanningSheetNames.Expenses;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.GetAmount(sheet_name, row, column);
        }

        public double GetPlanningMoneyPaidByGuests()
        {
            var sheet_name = PlanningSheetNames.Deposits;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.GetAmount(sheet_name, row, column);
        }

        public void InsertNewRowOnExpectedOut(double newAmount, string newNotes)
        {
            const int codeColumn = 4;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int new_row_number = _spreadsheetIO.FindRowNumberOfFirstRowContainingCell(
                MainSheetNames.ExpectedOut,
                ReconConsts.ExpectedOutInsertionPoint,
                codeColumn);

            var cell_values = new Dictionary<int, object>()
            {
                { amountColumn, newAmount },
                { notesColumn, newNotes }
            };

            _spreadsheetIO.InsertNewRow(MainSheetNames.ExpectedOut, new_row_number, cell_values);
        }

        public void AddNewTransactionToCredCard3(DateTime newDate, double newAmount, string newDescription)
        {
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;

            var cell_values = new Dictionary<int, object>()
            {
                { dateColumn, newDate.ToOADate() },
                { amountColumn, newAmount },
                { descriptionColumn, newDescription }
            };

            _spreadsheetIO.AppendNewRow(MainSheetNames.CredCard3, cell_values);
        }

        public void AddNewTransactionToSavings(DateTime newDate, double newAmount)
        {
            var sheet_name = MainSheetNames.Savings;

            int first_column_number = 1;
            int row_number = _spreadsheetIO.FindFirstEmptyRowInColumn(sheet_name, first_column_number);

            _spreadsheetIO.UpdateDate(sheet_name, row_number, first_column_number, newDate);
            _spreadsheetIO.UpdateAmount(sheet_name, row_number, first_column_number + 1, newAmount);
        }

        public DateTime GetNextUnplannedMonth()
        {
            string mortgage_row_description = GetBudgetItemDescription(Codes.Code042);
            BankRecord bank_record = GetLastBankOutRecordWithSpecifiedDescription(mortgage_row_description);

            DateTime next_unplanned_month = bank_record.Date.AddMonths(1);

            return next_unplanned_month;
        }

        private BankRecord GetLastBankOutRecordWithSpecifiedDescription(string description)
        {
            var bank_record = new BankRecord();

            var row_number_of_last_relevant_payment = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(
                MainSheetNames.BankOut,
                description,
                BankRecord.DescriptionIndex + 1);
            var bank_out_row = _spreadsheetIO.ReadSpecifiedRow(
                MainSheetNames.BankOut,
                row_number_of_last_relevant_payment);
            bank_record.ReadFromSpreadsheetRow(bank_out_row);

            return bank_record;
        }

        private string GetBudgetItemDescription(string budgetItemCode)
        {
            var bank_record = new BankRecord();

            int budget_out_code_column = 1;
            const int budgetOutFirstBankRecordColumnNumber = 2;
            const int budgetOutLastBankRecordColumnNumber = 6;

            var row_number_of_budget_item = _spreadsheetIO.FindRowNumberOfLastRowContainingCell(
                MainSheetNames.BudgetOut,
                budgetItemCode,
                budget_out_code_column);
            var budget_item_row = _spreadsheetIO.ReadSpecifiedRow(
                MainSheetNames.BudgetOut,
                row_number_of_budget_item,
                budgetOutFirstBankRecordColumnNumber,
                budgetOutLastBankRecordColumnNumber);
            bank_record.ReadFromSpreadsheetRow(budget_item_row);

            return bank_record.Description;
        }

        public void AddBudgetedBankInDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = GetAllMonthlyBankInBudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedBankOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData monthlyBudgetItemListData,
            BudgetItemListData annualBudgetItemListData)
        {
            var monthly_records = GetAllMonthlyBankOutBudgetItems(monthlyBudgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                monthly_records,
                pendingFile,
                budgetingMonths);

            var annual_records = GetAllAnnualBudgetItems<BankRecord>(annualBudgetItemListData);
            AddRecordsToPendingFileForRecordsThatHaveMatchingMonths(
                annual_records,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedCredCard1InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = GetAllMonthlyCredCard1BudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        public void AddBudgetedCredCard2InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = GetAllMonthlyCredCard2BudgetItems(budgetItemListData);
            AddRecordsToPendingFileForEverySpecifiedMonth(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        private void AddRecordsToPendingFileForEverySpecifiedMonth<TRecordType>(
            IEnumerable<TRecordType> baseRecords, 
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgetingMonths.LastMonthForBudgetPlanning >= budgetingMonths.NextUnplannedMonth
                ? budgetingMonths.LastMonthForBudgetPlanning
                : budgetingMonths.LastMonthForBudgetPlanning + 12;
            for (int month = budgetingMonths.NextUnplannedMonth; month <= final_month; month++)
            {
                int new_month = month;
                int new_year = budgetingMonths.StartYear;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }
                var new_monthly_records = baseRecords.Select(
                    x => (TRecordType)
                        WithCorrectDaysPerMonth(x.Copy(), new_year, new_month));
                pendingFile.Records.AddRange(new_monthly_records);
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private void AddRecordsToPendingFileForRecordsThatHaveMatchingMonths<TRecordType>(
            IEnumerable<TRecordType> baseRecords,
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgetingMonths.LastMonthForBudgetPlanning >= budgetingMonths.NextUnplannedMonth
                ? budgetingMonths.LastMonthForBudgetPlanning
                : budgetingMonths.LastMonthForBudgetPlanning + 12;
            for (int month = budgetingMonths.NextUnplannedMonth; month <= final_month; month++)
            {
                var new_month = month;
                var new_year = budgetingMonths.StartYear;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }
                var new_annual_records = baseRecords
                    .Where(x => x.Date.Month == new_month)
                    .Select(x => (TRecordType)
                        WithCorrectDaysPerMonth(x.Copy(), new_year, x.Date.Month));
                pendingFile.Records.AddRange(new_annual_records.ToList());
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private ICSVRecord WithCorrectDaysPerMonth(ICSVRecord record, int newYear, int newMonth)
        {
            int days_in_month = DateTime.DaysInMonth(newYear, newMonth);
            int day = record.Date.Day > days_in_month
                ? days_in_month
                : record.Date.Day;
            return record.WithDate(new DateTime(newYear, newMonth, day));
        }

        public TRecordType GetMostRecentRowContainingText<TRecordType>(
                string sheetName, 
                string textToSearchFor,
                List<int> expectedColumnNumbers)
            where TRecordType : ICSVRecord, new()
        {
            var row_number = _spreadsheetIO.FindRowNumberOfLastRowWithCellContainingText(
                sheetName, 
                textToSearchFor,
                expectedColumnNumbers);
            var csv_record = new TRecordType();
            csv_record.ReadFromSpreadsheetRow(_spreadsheetIO.ReadSpecifiedRow(sheetName, row_number));

            return csv_record;
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