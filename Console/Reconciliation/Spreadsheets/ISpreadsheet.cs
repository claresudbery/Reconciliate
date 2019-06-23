using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal interface ISpreadsheet
    {
        ICellRow Read_last_row(String sheet_name);
        int Find_row_number_of_last_divider_row(string sheet_name);
        String Read_last_row_as_csv(String sheet_name, ICSVRecord csv_record);
        String Read_specified_row_as_csv(String sheet_name, int row_number, ICSVRecord csv_record);
        void Add_unreconciled_rows_to_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new();
        void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new();
        ICSVFile<TRecordType> Read_unreconciled_rows_as_csv_file<TRecordType>(
            ICSVFileFactory<TRecordType> csv_file_factory,
            String sheet_name) where TRecordType : ICSVRecord, new();
        void Delete_unreconciled_rows(string sheet_name);
        double Get_second_child_pocket_money_amount(string short_date_time);
        TRecordType Get_most_recent_row_containing_text<TRecordType>(
            string sheet_name,
            string text_to_search_for,
            List<int> expected_column_numbers)
            where TRecordType : ICSVRecord, new();
        double Get_planning_expenses_already_done();
        double Get_planning_money_paid_by_guests();
        void Insert_new_row_on_expected_out(double new_amount, string new_notes);
        void Add_new_transaction_to_savings(DateTime new_date, double new_amount);
        void Update_balance_on_totals_sheet(
            string balance_code,
            double new_balance,
            string new_text,
            int balance_column,
            int text_column,
            int code_column);
        DateTime Get_next_unplanned_month();

        void Add_budgeted_monthly_data_to_pending_file<TOwnedType>(
                BudgetingMonths budgeting_months,
                ICSVFile<TOwnedType> pending_file,
                BudgetItemListData monthly_budget_item_list_data)
            where TOwnedType : ICSVRecord, new();
        void Add_budgeted_annual_data_to_pending_file<TOwnedType>(
                BudgetingMonths budgeting_months,
                ICSVFile<TOwnedType> pending_file,
                BudgetItemListData annual_budget_item_list_data)
            where TOwnedType : ICSVRecord, new();
    }
}