using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface ISpreadsheetRepo
    {
        void Dispose();
        int Last_row_number(String sheetName);
        int Find_first_empty_row_in_column(string sheetName, int columnNumber);
        int Find_row_number_of_first_row_containing_cell(
            string sheetName,
            string targetCellText,
            int expectedColumnNumber = 2);
        int Find_row_number_of_last_row_containing_cell(
            string sheetName,
            string targetCellText,
            int expectedColumnNumber = 2);
        int Find_row_number_of_last_row_with_cell_containing_text(
            string sheetName,
            string targetSubText,
            List<int> expectedColumnNumbers);
        ICellRow Read_specified_row(String sheetName, int rowNumber);
        ICellRow Read_specified_row(String sheetName, int rowNumber, int startColumn, int endColumn);
        ICellRow Read_specified_row(int rowNumber);
        ICellSet Current_cells(String sheetName);
        List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheetName,
            int firstRowNumber,
            int lastRowNumber,
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new();
        ICellRow Read_last_row(String sheetName);
        String Read_last_row_as_csv(String sheetName, ICSVRecord csvRecord);
        void Append_csv_record(String sheetName, ICSVRecord csvRecord);
        void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        void Remove_last_row(String sheetName);
        void Delete_specified_rows(String sheetName, int firstRowNumber, int lastRowNumber);
        DateTime Get_date(string sheetName, int row, int column);
        string Get_text(string sheetName, int row, int column);
        double Get_amount(string sheetName, int row, int column);
        double Get_amount(string sheetName, string code, int amountColumn = 3);
        void Update_date(string sheetName, int dateRow, int dateColumn, DateTime newDate);
        void Update_text(string sheetName, int textRow, int textColumn, string newText);
        void Update_amount(string sheetName, int amountRow, int amountColumn, double newAmount);
        void Update_amount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1);
        void Update_amount_and_text(
            string sheetName,
            string amountCode,
            double newAmount,
            string newText,
            int amountColumn = 2,
            int textColumn = 3,
            int codeColumn = 1);
        void Insert_new_row(string sheetName, int newRowNumber, Dictionary<int, object> cellValues);
        void Append_new_row(string sheetName, Dictionary<int, object> cellValues);
    }
}