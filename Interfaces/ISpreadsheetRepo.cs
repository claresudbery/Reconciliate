using System;
using System.Collections.Generic;
using Interfaces.Constants;

namespace Interfaces
{
    public interface ISpreadsheetRepo
    {
        void Dispose();
        int Last_row_number(String sheet_name);
        int Find_first_empty_row_in_column(string sheet_name, int column_number);
        int Find_row_number_of_first_row_containing_cell(
            string sheet_name,
            string target_cell_text,
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn);
        int Find_row_number_of_last_row_containing_cell(
            string sheet_name,
            string target_cell_text,
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn);
        int Find_row_number_of_last_row_with_cell_containing_text(
            string sheet_name,
            string target_sub_text,
            List<int> expected_column_numbers);
        ICellRow Read_specified_row(String sheet_name, int row_number);
        ICellRow Read_specified_row(String sheet_name, int row_number, int start_column, int end_column);
        ICellRow Read_specified_row(int row_number);
        ICellSet Current_cells(String sheet_name);
        List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheet_name,
            int first_row_number,
            int last_row_number,
            int first_column_number,
            int last_column_number) where TRecordType : ICSVRecord, new();
        ICellRow Read_last_row(String sheet_name);
        String Read_last_row_as_csv(String sheet_name, ICSVRecord csv_record);
        void Append_csv_record(String sheet_name, ICSVRecord csv_record);
        void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new();
        void Remove_last_row(String sheet_name);
        void Delete_specified_rows(String sheet_name, int first_row_number, int last_row_number);
        DateTime Get_date(string sheet_name, int row, int column);
        string Get_text(string sheet_name, int row, int column);
        double Get_amount(string sheet_name, int row, int column);
        double Get_amount(string sheet_name, string code, int amount_column = SpreadsheetConsts.DefaultAmountColumn);
        void Update_date(string sheet_name, int date_row, int date_column, DateTime new_date);
        void Update_text(string sheet_name, int text_row, int text_column, string new_text);
        void Update_amount(string sheet_name, int amount_row, int amount_column, double new_amount);
        void Update_amount(
            string sheet_name, 
            string amount_code, 
            double new_amount, 
            int amount_column = SpreadsheetConsts.DefaultAmountColumn, 
            int code_column = SpreadsheetConsts.DefaultCodeColumn);
        void Update_amount_and_text(
            string sheet_name,
            string amount_code,
            double new_amount,
            string new_text,
            int amount_column = SpreadsheetConsts.DefaultAmountColumn,
            int text_column = SpreadsheetConsts.DefaultTextColumn,
            int code_column = SpreadsheetConsts.DefaultCodeColumn);
        void Insert_new_row(string sheet_name, int new_row_number, Dictionary<int, object> cell_values);
        void Append_new_row(string sheet_name, Dictionary<int, object> cell_values);
    }
}