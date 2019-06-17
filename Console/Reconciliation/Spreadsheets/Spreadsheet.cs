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

        public ICellRow Read_last_row(String sheetName)
        {
            return _spreadsheetIO.Read_last_row(sheetName);
        }

        public String Read_last_row_as_csv(String sheetName, ICSVRecord csvRecord)
        {
            return _spreadsheetIO.Read_last_row_as_csv(sheetName, csvRecord);
        }

        public String Read_specified_row_as_csv(String sheetName, int rowNumber, ICSVRecord csvRecord)
        {
            csvRecord.Read_from_spreadsheet_row(_spreadsheetIO.Read_specified_row(sheetName, rowNumber));
            return csvRecord.To_csv();
        }

        public int Find_row_number_of_last_divider_row(string sheetName)
        {
            return _spreadsheetIO.Find_row_number_of_last_row_containing_cell(
                sheetName,
                Dividers.Divider_text);
        }

        public void Add_unreconciled_rows_to_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            int divider_row_number = Find_row_number_of_last_divider_row(sheetName);
            int last_row_number = _spreadsheetIO.Last_row_number(sheetName);

            for (int row_number = divider_row_number + 1; row_number <= last_row_number; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.Read_from_spreadsheet_row(_spreadsheetIO.Read_specified_row(sheetName, row_number));
                csvFile.Records.Add(csv_record);
            }

            csvFile.Records = csvFile.Records.OrderBy(record => record.Date).ToList();
        }

        public void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            _spreadsheetIO.Append_csv_file(sheetName, csvFile);
        }

        public void Delete_unreconciled_rows(string sheetName)
        {
            int last_row_number = _spreadsheetIO.Last_row_number(sheetName);
            int divider_row_number = Find_row_number_of_last_divider_row(sheetName);
            _spreadsheetIO.Delete_specified_rows(sheetName, divider_row_number + 1, last_row_number);
        }

        public ICSVFile<TRecordType> Read_unreconciled_rows_as_csv_file<TRecordType>(
            ICSVFileFactory<TRecordType> csvFileFactory,
            String sheetName) where TRecordType : ICSVRecord, new()
        {
            var csv_file = csvFileFactory.Create_csv_file(false);

            Add_unreconciled_rows_to_csv_file(sheetName, csv_file);

            return csv_file;
        }

        public List<BankRecord> Get_all_monthly_bank_in_budget_items(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.Start_divider) + 1;
            int last_row_number = _spreadsheetIO
                                    .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.End_divider) - 1;

            return _spreadsheetIO.Get_rows_as_records<BankRecord>(
                budgetItemListData.Sheet_name,
                first_row_number,
                last_row_number,
                budgetItemListData.First_column_number,
                budgetItemListData.Last_column_number);
        }

        public List<BankRecord> Get_all_monthly_bank_out_budget_items(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.Start_divider) + 1;
            int last_row_number = _spreadsheetIO
                                    .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.End_divider) - 1;

            return _spreadsheetIO.Get_rows_as_records<BankRecord>(
                budgetItemListData.Sheet_name,
                first_row_number,
                last_row_number,
                budgetItemListData.First_column_number,
                budgetItemListData.Last_column_number);
        }

        public List<CredCard1InOutRecord> Get_all_monthly_cred_card1_budget_items(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.Start_divider) + 1;
            int last_row_number = _spreadsheetIO
                                    .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.End_divider) - 1;

            return _spreadsheetIO.Get_rows_as_records<CredCard1InOutRecord>(
                budgetItemListData.Sheet_name,
                first_row_number,
                last_row_number,
                budgetItemListData.First_column_number,
                budgetItemListData.Last_column_number);
        }

        public List<CredCard2InOutRecord> Get_all_monthly_cred_card2_budget_items(BudgetItemListData budgetItemListData)
        {
            int first_row_number = _spreadsheetIO
                                     .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.Start_divider) + 1;
            int last_row_number = _spreadsheetIO
                                    .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.End_divider) - 1;

            return _spreadsheetIO.Get_rows_as_records<CredCard2InOutRecord>(
                budgetItemListData.Sheet_name,
                first_row_number,
                last_row_number,
                budgetItemListData.First_column_number,
                budgetItemListData.Last_column_number);
        }

        public List<TRecordType> Get_all_annual_budget_items<TRecordType>(BudgetItemListData budgetItemListData)
            where TRecordType : ICSVRecord, new()
        {
            int first_row_number = _spreadsheetIO
                                     .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.Start_divider) + 1;
            int last_row_number = _spreadsheetIO
                                    .Find_row_number_of_last_row_containing_cell(budgetItemListData.Sheet_name, budgetItemListData.End_divider) - 1;

            return _spreadsheetIO.Get_rows_as_records<TRecordType>(
                budgetItemListData.Sheet_name,
                first_row_number,
                last_row_number,
                budgetItemListData.First_column_number,
                budgetItemListData.Last_column_number);
        }

        public double Get_second_child_pocket_money_amount(string shortDateTime)
        {
            const int dateColumn = 1;
            const int amountColumn = 4;
            var sheet_name = PocketMoneySheetNames.Second_child;
            int row = _spreadsheetIO.Find_row_number_of_last_row_containing_cell(sheet_name, shortDateTime, dateColumn);

            return _spreadsheetIO.Get_amount(sheet_name, row, amountColumn);
        }

        public double Get_planning_expenses_already_done()
        {
            var sheet_name = PlanningSheetNames.Expenses;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.Get_amount(sheet_name, row, column);
        }

        public double Get_planning_money_paid_by_guests()
        {
            var sheet_name = PlanningSheetNames.Deposits;
            const int row = 2;
            const int column = 4;
            return _spreadsheetIO.Get_amount(sheet_name, row, column);
        }

        public void Insert_new_row_on_expected_out(double newAmount, string newNotes)
        {
            const int codeColumn = 4;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int new_row_number = _spreadsheetIO.Find_row_number_of_first_row_containing_cell(
                MainSheetNames.Expected_out,
                ReconConsts.ExpectedOutInsertionPoint,
                codeColumn);

            var cell_values = new Dictionary<int, object>()
            {
                { amountColumn, newAmount },
                { notesColumn, newNotes }
            };

            _spreadsheetIO.Insert_new_row(MainSheetNames.Expected_out, new_row_number, cell_values);
        }

        public void Add_new_transaction_to_cred_card3(DateTime newDate, double newAmount, string newDescription)
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

            _spreadsheetIO.Append_new_row(MainSheetNames.Cred_card3, cell_values);
        }

        public void Add_new_transaction_to_savings(DateTime newDate, double newAmount)
        {
            var sheet_name = MainSheetNames.Savings;

            int first_column_number = 1;
            int row_number = _spreadsheetIO.Find_first_empty_row_in_column(sheet_name, first_column_number);

            _spreadsheetIO.Update_date(sheet_name, row_number, first_column_number, newDate);
            _spreadsheetIO.Update_amount(sheet_name, row_number, first_column_number + 1, newAmount);
        }

        public DateTime Get_next_unplanned_month()
        {
            string mortgage_row_description = Get_budget_item_description(Codes.Code042);
            BankRecord bank_record = Get_last_bank_out_record_with_specified_description(mortgage_row_description);

            DateTime next_unplanned_month = bank_record.Date.AddMonths(1);

            return next_unplanned_month;
        }

        private BankRecord Get_last_bank_out_record_with_specified_description(string description)
        {
            var bank_record = new BankRecord();

            var row_number_of_last_relevant_payment = _spreadsheetIO.Find_row_number_of_last_row_containing_cell(
                MainSheetNames.Bank_out,
                description,
                BankRecord.DescriptionIndex + 1);
            var bank_out_row = _spreadsheetIO.Read_specified_row(
                MainSheetNames.Bank_out,
                row_number_of_last_relevant_payment);
            bank_record.Read_from_spreadsheet_row(bank_out_row);

            return bank_record;
        }

        private string Get_budget_item_description(string budgetItemCode)
        {
            var bank_record = new BankRecord();

            int budget_out_code_column = 1;
            const int budgetOutFirstBankRecordColumnNumber = 2;
            const int budgetOutLastBankRecordColumnNumber = 6;

            var row_number_of_budget_item = _spreadsheetIO.Find_row_number_of_last_row_containing_cell(
                MainSheetNames.Budget_out,
                budgetItemCode,
                budget_out_code_column);
            var budget_item_row = _spreadsheetIO.Read_specified_row(
                MainSheetNames.Budget_out,
                row_number_of_budget_item,
                budgetOutFirstBankRecordColumnNumber,
                budgetOutLastBankRecordColumnNumber);
            bank_record.Read_from_spreadsheet_row(budget_item_row);

            return bank_record.Description;
        }

        public void Add_budgeted_bank_in_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = Get_all_monthly_bank_in_budget_items(budgetItemListData);
            Add_records_to_pending_file_for_every_specified_month(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        public void Add_budgeted_bank_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData monthlyBudgetItemListData,
            BudgetItemListData annualBudgetItemListData)
        {
            var monthly_records = Get_all_monthly_bank_out_budget_items(monthlyBudgetItemListData);
            Add_records_to_pending_file_for_every_specified_month(
                monthly_records,
                pendingFile,
                budgetingMonths);

            var annual_records = Get_all_annual_budget_items<BankRecord>(annualBudgetItemListData);
            Add_records_to_pending_file_for_records_that_have_matching_months(
                annual_records,
                pendingFile,
                budgetingMonths);
        }

        public void Add_budgeted_cred_card1_in_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = Get_all_monthly_cred_card1_budget_items(budgetItemListData);
            Add_records_to_pending_file_for_every_specified_month(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        public void Add_budgeted_cred_card2_in_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData)
        {
            var base_records = Get_all_monthly_cred_card2_budget_items(budgetItemListData);
            Add_records_to_pending_file_for_every_specified_month(
                base_records,
                pendingFile,
                budgetingMonths);
        }

        private void Add_records_to_pending_file_for_every_specified_month<TRecordType>(
            IEnumerable<TRecordType> baseRecords, 
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgetingMonths.Last_month_for_budget_planning >= budgetingMonths.Next_unplanned_month
                ? budgetingMonths.Last_month_for_budget_planning
                : budgetingMonths.Last_month_for_budget_planning + 12;
            for (int month = budgetingMonths.Next_unplanned_month; month <= final_month; month++)
            {
                int new_month = month;
                int new_year = budgetingMonths.Start_year;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }
                var new_monthly_records = baseRecords.Select(
                    x => (TRecordType)
                        With_correct_days_per_month(x.Copy(), new_year, new_month));
                pendingFile.Records.AddRange(new_monthly_records);
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private void Add_records_to_pending_file_for_records_that_have_matching_months<TRecordType>(
            IEnumerable<TRecordType> baseRecords,
            ICSVFile<TRecordType> pendingFile,
            BudgetingMonths budgetingMonths) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgetingMonths.Last_month_for_budget_planning >= budgetingMonths.Next_unplanned_month
                ? budgetingMonths.Last_month_for_budget_planning
                : budgetingMonths.Last_month_for_budget_planning + 12;
            for (int month = budgetingMonths.Next_unplanned_month; month <= final_month; month++)
            {
                var new_month = month;
                var new_year = budgetingMonths.Start_year;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }
                var new_annual_records = baseRecords
                    .Where(x => x.Date.Month == new_month)
                    .Select(x => (TRecordType)
                        With_correct_days_per_month(x.Copy(), new_year, x.Date.Month));
                pendingFile.Records.AddRange(new_annual_records.ToList());
            }
            pendingFile.Records = pendingFile.Records.OrderBy(record => record.Date).ToList();
        }

        private ICSVRecord With_correct_days_per_month(ICSVRecord record, int newYear, int newMonth)
        {
            int days_in_month = DateTime.DaysInMonth(newYear, newMonth);
            int day = record.Date.Day > days_in_month
                ? days_in_month
                : record.Date.Day;
            return record.With_date(new DateTime(newYear, newMonth, day));
        }

        public TRecordType Get_most_recent_row_containing_text<TRecordType>(
                string sheetName, 
                string textToSearchFor,
                List<int> expectedColumnNumbers)
            where TRecordType : ICSVRecord, new()
        {
            var row_number = _spreadsheetIO.Find_row_number_of_last_row_with_cell_containing_text(
                sheetName, 
                textToSearchFor,
                expectedColumnNumbers);
            var csv_record = new TRecordType();
            csv_record.Read_from_spreadsheet_row(_spreadsheetIO.Read_specified_row(sheetName, row_number));

            return csv_record;
        }

        public void Update_balance_on_totals_sheet(
            string balanceCode,
            double newBalance,
            string newText,
            int balanceColumn,
            int textColumn,
            int codeColumn)
        {
            _spreadsheetIO.Update_amount_and_text(
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