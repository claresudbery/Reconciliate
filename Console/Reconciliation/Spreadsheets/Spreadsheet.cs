using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Exceptions;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class Spreadsheet : ISpreadsheet
    {
        private readonly ISpreadsheetRepo _spreadsheet_io;

        public Spreadsheet(ISpreadsheetRepo spreadsheet_io)
        {
            _spreadsheet_io = spreadsheet_io;
        }

        public ICellRow Read_last_row(String sheet_name)
        {
            return _spreadsheet_io.Read_last_row(sheet_name);
        }

        public String Read_last_row_as_csv(String sheet_name, ICSVRecord csv_record)
        {
            return _spreadsheet_io.Read_last_row_as_csv(sheet_name, csv_record);
        }

        public String Read_specified_row_as_csv(String sheet_name, int row_number, ICSVRecord csv_record)
        {
            csv_record.Read_from_spreadsheet_row(_spreadsheet_io.Read_specified_row(sheet_name, row_number));
            return csv_record.To_csv();
        }

        public int Find_row_number_of_last_divider_row(string sheet_name)
        {
            return _spreadsheet_io.Find_row_number_of_last_row_containing_cell(
                sheet_name,
                Dividers.Divider_text,
                new List<int> {2});
        }

        public void Add_unreconciled_rows_to_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new()
        {
            int divider_row_number = Find_row_number_of_last_divider_row(sheet_name);
            int last_row_number = _spreadsheet_io.Last_row_number(sheet_name);

            for (int row_number = divider_row_number + 1; row_number <= last_row_number; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.Read_from_spreadsheet_row(_spreadsheet_io.Read_specified_row(sheet_name, row_number));
                csv_file.Records.Add(csv_record);
            }

            csv_file.Records = csv_file.Records.OrderBy(record => record.Date).ToList();
        }

        public void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new()
        {
            _spreadsheet_io.Append_csv_file(sheet_name, csv_file);
        }

        public void Delete_unreconciled_rows(string sheet_name)
        {
            int last_row_number = _spreadsheet_io.Last_row_number(sheet_name);
            int divider_row_number = Find_row_number_of_last_divider_row(sheet_name);
            _spreadsheet_io.Delete_specified_rows(sheet_name, divider_row_number + 1, last_row_number);
        }

        public ICSVFile<TRecordType> Read_unreconciled_rows_as_csv_file<TRecordType>(
            ICSVFileFactory<TRecordType> csv_file_factory,
            String sheet_name) where TRecordType : ICSVRecord, new()
        {
            var csv_file = csv_file_factory.Create_csv_file(false);

            Add_unreconciled_rows_to_csv_file(sheet_name, csv_file);

            return csv_file;
        }

        public List<TRecordType> Get_all_budget_items<TRecordType>(BudgetItemListData budget_item_list_data)
            where TRecordType : ICSVRecord, new()
        {
            int first_row_number = _spreadsheet_io
                                     .Find_row_number_of_last_row_containing_cell(budget_item_list_data.Budget_sheet_name, budget_item_list_data.Start_divider, new List<int> { ReconConsts.BudgetDividerColumn }) + 1;
            int last_row_number = _spreadsheet_io
                                    .Find_row_number_of_last_row_containing_cell(budget_item_list_data.Budget_sheet_name, budget_item_list_data.End_divider, new List<int> { ReconConsts.BudgetDividerColumn }) - 1;

            return _spreadsheet_io.Get_rows_as_records<TRecordType>(
                budget_item_list_data.Budget_sheet_name,
                first_row_number,
                last_row_number,
                budget_item_list_data.First_column_number,
                budget_item_list_data.Last_column_number);
        }

        public double Get_second_child_pocket_money_amount(string short_date_time)
        {
            const int dateColumn = 1;
            const int amountColumn = 4;
            var sheet_name = PocketMoneySheetNames.Second_child;
            int row = _spreadsheet_io.Find_row_number_of_last_row_containing_cell(sheet_name, short_date_time, new List<int> { dateColumn });

            return _spreadsheet_io.Get_amount(sheet_name, row, amountColumn);
        }

        public double Get_planning_expenses_already_done()
        {
            var sheet_name = PlanningSheetNames.Expenses;
            const int row = 2;
            const int column = 4;
            return _spreadsheet_io.Get_amount(sheet_name, row, column);
        }

        public double Get_planning_money_paid_by_guests()
        {
            var sheet_name = PlanningSheetNames.Deposits;
            const int row = 2;
            const int column = 4;
            return _spreadsheet_io.Get_amount(sheet_name, row, column);
        }

        public void Insert_new_row_on_expected_out(double new_amount, string new_notes)
        {
            const int codeColumn = 4;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int new_row_number = _spreadsheet_io.Find_row_number_of_first_row_containing_cell(
                MainSheetNames.Expected_out,
                ReconConsts.ExpectedOutInsertionPoint,
                codeColumn);

            var cell_values = new Dictionary<int, object>()
            {
                { amountColumn, new_amount },
                { notesColumn, new_notes }
            };

            _spreadsheet_io.Insert_new_row(MainSheetNames.Expected_out, new_row_number, cell_values);
        }

        public void Add_new_transaction_to_cred_card3(DateTime new_date, double new_amount, string new_description)
        {
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;

            var cell_values = new Dictionary<int, object>()
            {
                { dateColumn, new_date.ToOADate() },
                { amountColumn, new_amount },
                { descriptionColumn, new_description }
            };

            _spreadsheet_io.Append_new_row(MainSheetNames.Cred_card3, cell_values);
        }

        public void Add_new_transaction_to_savings(DateTime new_date, double new_amount)
        {
            var sheet_name = MainSheetNames.Savings;

            int first_column_number = 1;
            int row_number = _spreadsheet_io.Find_first_empty_row_in_column(sheet_name, first_column_number);

            _spreadsheet_io.Update_date(sheet_name, row_number, first_column_number, new_date);
            _spreadsheet_io.Update_amount(sheet_name, row_number, first_column_number + 1, new_amount);
        }
        
        public void Update_expected_out(int multiplier, string budget_code, string expected_out_code)
        {
            var base_amount = _spreadsheet_io.Get_amount(MainSheetNames.Budget_out, budget_code);
            var new_amount = multiplier * base_amount;
            _spreadsheet_io.Update_amount(MainSheetNames.Expected_out, expected_out_code, new_amount);
        }

        public void Update_owed_CHB(BudgetingMonths budgeting_months)
        {
            var num_months = budgeting_months.Num_budgeting_months();
            var base_amount = _spreadsheet_io.Get_amount(MainSheetNames.Budget_out, Codes.Code003);
            var current_amount = _spreadsheet_io.Get_amount(MainSheetNames.Expected_out, Codes.Code003, 2);
            var new_amount = current_amount + (base_amount * num_months);
            _spreadsheet_io.Update_amount(MainSheetNames.Expected_out, Codes.Code003, new_amount);

            var text = _spreadsheet_io.Get_text(MainSheetNames.Expected_out, Codes.Code003);
            if (!String.IsNullOrEmpty(text))
            {
                text = text.Substring(0, text.Length - 9)
                       + budgeting_months.Budgeting_end_date().ToString("MMM", CultureInfo.CurrentCulture)
                       + $" {budgeting_months.Budgeting_end_date().Year})";
                _spreadsheet_io.Update_text(MainSheetNames.Expected_out, Codes.Code003, text);
            }
        }

        public DateTime Get_next_unplanned_month<TRecordType>(BudgetItemListData budget_item_list_data = null)
            where TRecordType : ICSVRecord, new()
        {
            string budget_item_description = $"Couldn't find budget item on {budget_item_list_data.Budget_sheet_name} between {budget_item_list_data.Start_divider} and {budget_item_list_data.End_divider}";
            DateTime next_unplanned_month = DateTime.Today;

            try
            {
                budget_item_description = Get_budget_item_description(budget_item_list_data);
                TRecordType record = Get_last_record_with_specified_description<TRecordType>(budget_item_description, budget_item_list_data);

                next_unplanned_month = record.Date.AddMonths(1);
            }
            catch (Exception)
            {
                throw new MonthlyBudgetedRowNotFoundException(budget_item_description);
            }

            return next_unplanned_month;
        }

        private TRecordType Get_last_record_with_specified_description<TRecordType>(
                string description,
                BudgetItemListData budget_item_list_data) 
            where TRecordType : ICSVRecord, new()
        {
            var record = new TRecordType();

            var row_number_of_last_relevant_payment = _spreadsheet_io.Find_row_number_of_last_row_containing_cell(
                budget_item_list_data.Owned_sheet_name,
                description,
                new List<int> { budget_item_list_data.Last_column_number - 1, budget_item_list_data.Third_party_desc_col },
                false);
            var spreadsheet_row = _spreadsheet_io.Read_specified_row(
                budget_item_list_data.Owned_sheet_name,
                row_number_of_last_relevant_payment);
            record.Read_from_spreadsheet_row(spreadsheet_row);

            return record;
        }

        private string Get_budget_item_description(BudgetItemListData budget_item_list_data)
        {
            string result = null;

            int row_before_first_budget_item = _spreadsheet_io.Find_row_number_of_last_row_containing_cell(
                budget_item_list_data.Budget_sheet_name,
                budget_item_list_data.Start_divider,
                new List<int> { ReconConsts.BudgetDividerColumn });
            int row_after_last_budget_item = _spreadsheet_io.Find_row_number_of_last_row_containing_cell(
                budget_item_list_data.Budget_sheet_name,
                budget_item_list_data.End_divider,
                new List<int> { ReconConsts.BudgetDividerColumn });

            if (row_after_last_budget_item - row_before_first_budget_item > 1)
            {
                result = _spreadsheet_io.Get_text(
                    budget_item_list_data.Budget_sheet_name,
                    row_before_first_budget_item + 1,
                    budget_item_list_data.Last_column_number);
            }

            return result;
        }

        private string Get_budget_item_description(string budget_item_code)
        {
            var bank_record = new BankRecord();

            int budget_out_code_column = 1;
            const int budgetOutFirstBankRecordColumnNumber = 2;
            const int budgetOutLastBankRecordColumnNumber = 6;

            var row_number_of_budget_item = _spreadsheet_io.Find_row_number_of_last_row_containing_cell(
                MainSheetNames.Budget_out,
                budget_item_code,
                new List<int> { budget_out_code_column });
            var budget_item_row = _spreadsheet_io.Read_specified_row(
                MainSheetNames.Budget_out,
                row_number_of_budget_item,
                budgetOutFirstBankRecordColumnNumber,
                budgetOutLastBankRecordColumnNumber);
            bank_record.Read_from_spreadsheet_row(budget_item_row);

            return bank_record.Description;
        }

        public void Add_budgeted_annual_data_to_pending_file<TRecordType>(
            BudgetingMonths budgeting_months,
            ICSVFile<TRecordType> pending_file,
            BudgetItemListData budget_item_list_data) where TRecordType : ICSVRecord, new()
        {
            var annual_records = Get_all_budget_items<TRecordType>(budget_item_list_data);
            Add_records_to_pending_file_for_records_that_have_matching_months<TRecordType>(
                annual_records,
                (ICSVFile<TRecordType>)pending_file,
                budgeting_months);
        }

        public void Add_budgeted_monthly_data_to_pending_file<TRecordType>(
            BudgetingMonths budgeting_months,
            ICSVFile<TRecordType> pending_file,
            BudgetItemListData budget_item_list_data) where TRecordType : ICSVRecord, new()
        {
            var base_records = Get_all_budget_items<TRecordType>(budget_item_list_data);
            Add_records_to_pending_file_for_every_specified_month<TRecordType>(
                base_records,
                (ICSVFile<TRecordType>)pending_file,
                budgeting_months);
        }

        private void Add_records_to_pending_file_for_every_specified_month<TRecordType>(
            IEnumerable<TRecordType> base_records, 
            ICSVFile<TRecordType> pending_file,
            BudgetingMonths budgeting_months) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgeting_months.Last_month_for_budget_planning >=
                              budgeting_months.Next_unplanned_month
                ? budgeting_months.Last_month_for_budget_planning
                : budgeting_months.Last_month_for_budget_planning + 12;
            for (int month = budgeting_months.Next_unplanned_month; month <= final_month; month++)
            {
                int new_month = month;
                int new_year = budgeting_months.Start_year;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }

                var new_monthly_records = base_records.Select(
                    x => (TRecordType)
                        With_correct_days_per_month(x.Copy(), new_year, new_month));
                pending_file.Records.AddRange(new_monthly_records);
            }

            pending_file.Records = pending_file.Records.OrderBy(record => record.Date).ToList();
        }

        private void Add_records_to_pending_file_for_records_that_have_matching_months<TRecordType>(
            IEnumerable<TRecordType> base_records,
            ICSVFile<TRecordType> pending_file,
            BudgetingMonths budgeting_months) where TRecordType : ICSVRecord, new()
        {
            var final_month = budgeting_months.Last_month_for_budget_planning >= budgeting_months.Next_unplanned_month
                ? budgeting_months.Last_month_for_budget_planning
                : budgeting_months.Last_month_for_budget_planning + 12;
            for (int month = budgeting_months.Next_unplanned_month; month <= final_month; month++)
            {
                var new_month = month;
                var new_year = budgeting_months.Start_year;
                if (month > 12)
                {
                    new_month = month - 12;
                    new_year = new_year + 1;
                }
                var new_annual_records = base_records
                    .Where(x => x.Date.Month == new_month)
                    .Select(x => (TRecordType)
                        With_correct_days_per_month(x.Copy(), new_year, x.Date.Month));
                pending_file.Records.AddRange(new_annual_records.ToList());
            }
            pending_file.Records = pending_file.Records.OrderBy(record => record.Date).ToList();
        }

        private ICSVRecord With_correct_days_per_month(ICSVRecord record, int new_year, int new_month)
        {
            int days_in_month = DateTime.DaysInMonth(new_year, new_month);
            int day = record.Date.Day > days_in_month
                ? days_in_month
                : record.Date.Day;
            return record.With_date(new DateTime(new_year, new_month, day));
        }

        public TRecordType Get_most_recent_row_containing_text<TRecordType>(
                string sheet_name, 
                string text_to_search_for,
                List<int> expected_column_numbers)
            where TRecordType : ICSVRecord, new()
        {
            var row_number = _spreadsheet_io.Find_row_number_of_last_row_with_cell_containing_text(
                sheet_name, 
                text_to_search_for,
                expected_column_numbers);
            var csv_record = new TRecordType();
            csv_record.Read_from_spreadsheet_row(_spreadsheet_io.Read_specified_row(sheet_name, row_number));

            return csv_record;
        }

        public void Update_balance_on_totals_sheet(
            string balance_code,
            double new_balance,
            string new_text,
            int balance_column,
            int text_column,
            int code_column,
            IInputOutput input_output)
        {
            input_output.Output_line($"Updating {MainSheetNames.Totals} with {balance_code} {new_balance} '{new_text}'.");
            
            _spreadsheet_io.Update_amount_and_text(
                MainSheetNames.Totals,
                balance_code,
                new_balance,
                new_text,
                balance_column,
                text_column,
                code_column);
        }
    }
}