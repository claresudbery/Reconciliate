using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal interface ISpreadsheet
    {
        ICellRow Read_last_row(String sheetName);
        int Find_row_number_of_last_divider_row(string sheetName);
        String Read_last_row_as_csv(String sheetName, ICSVRecord csvRecord);
        String Read_specified_row_as_csv(String sheetName, int rowNumber, ICSVRecord csvRecord);
        void Add_unreconciled_rows_to_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        ICSVFile<TRecordType> Read_unreconciled_rows_as_csv_file<TRecordType>(
            ICSVFileFactory<TRecordType> csvFileFactory,
            String sheetName) where TRecordType : ICSVRecord, new();
        void Delete_unreconciled_rows(string sheetName);
        double Get_second_child_pocket_money_amount(string shortDateTime);
        TRecordType Get_most_recent_row_containing_text<TRecordType>(
            string sheetName,
            string textToSearchFor,
            List<int> expectedColumnNumbers)
            where TRecordType : ICSVRecord, new();
        double Get_planning_expenses_already_done();
        double Get_planning_money_paid_by_guests();
        void Insert_new_row_on_expected_out(double newAmount, string newNotes);
        void Add_new_transaction_to_savings(DateTime newDate, double newAmount);
        void Update_balance_on_totals_sheet(
            string balanceCode,
            double newBalance,
            string newText,
            int balanceColumn,
            int textColumn,
            int codeColumn);
        DateTime Get_next_unplanned_month();

        void Add_budgeted_bank_in_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData budgetItemListData);
        void Add_budgeted_bank_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData monthlyBudgetItemListData,
            BudgetItemListData annualBudgetItemListData);
        void Add_budgeted_cred_card1_in_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData);
        void Add_budgeted_cred_card2_in_out_data_to_pending_file(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData);
    }
}