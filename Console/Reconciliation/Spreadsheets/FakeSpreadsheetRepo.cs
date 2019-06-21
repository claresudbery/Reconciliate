using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // There is a distinction between FakeSpreadsheetRepo and DebugSpreadsheetRepo:
    // FakeSpreadsheetRepo does no Excel file access at all, and is used in .Net.
    // DebugSpreadsheetRepo does real Excel file access but only reads from the live spreadsheet: It writes to a backed-up copy.
    internal class FakeSpreadsheetRepo : ISpreadsheetRepo
    {
        FileIO<BankRecord> _debug_log = new FileIO<BankRecord>(
            new FakeSpreadsheetRepoFactory(), 
            ReconConsts.Default_file_path,
            "FakeSpreadsheetDataInfo");

        private readonly FakeRowNumbersForCell _fake_row_numbers_for_cell = new FakeRowNumbersForCell();
        private readonly FakeRowNumbersForText _fake_row_numbers_for_text = new FakeRowNumbersForText();
        private readonly FakeRows _fake_rows = new FakeRows();
        private readonly LastRowNumbers _last_row_numbers = new LastRowNumbers();

        public static string FakeMortgageDescription = "Mortgage description";

        public FakeSpreadsheetRepo()
        {
            _debug_log.Append_to_file_as_source_line("*******************************************************************************");
            _debug_log.Append_to_file_as_source_line("*******************************************************************************");
            _debug_log.Append_to_file_as_source_line("**                             NEW RUN OF INFO                               **");
            _debug_log.Append_to_file_as_source_line($"**                            {DateTime.Now}                                 **");
            _debug_log.Append_to_file_as_source_line("*******************************************************************************");
            _debug_log.Append_to_file_as_source_line("*******************************************************************************");
        }

        public void Dispose()
        {
        }

        public ICellSet Current_cells(string sheet_name)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}");
            return new FakeCellSet();
        }

        public int Last_row_number(string sheet_name)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}");

            return _last_row_numbers.Data.ContainsKey(sheet_name)
                ? _last_row_numbers.Data[sheet_name]
                : 2;
        }

        public ICellRow Read_specified_row(string sheet_name, int row_number)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, rowNumber {row_number}");

            return _fake_rows.Data.ContainsKey(sheet_name)
                ? _fake_rows.Data[sheet_name][row_number - 1] 
                : new FakeCellRow();
        }

        public int Find_first_empty_row_in_column(string sheet_name, int column_number)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, columnNumber {column_number}");
            return 2;
        }

        public int Find_row_number_of_last_row_containing_cell(
            string sheet_name, 
            string target_cell_text, 
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, targetSubText {target_cell_text}");
            return _fake_row_numbers_for_cell.Data.ContainsKey(sheet_name) 
                ? _fake_row_numbers_for_cell.Data[sheet_name][target_cell_text] 
                : 2;
        }

        public int Find_row_number_of_last_row_with_cell_containing_text(string sheet_name, string target_sub_text, List<int> expected_column_numbers)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, targetSubText {target_sub_text}");
            return _fake_row_numbers_for_text.Data.ContainsKey(sheet_name) 
                ? _fake_row_numbers_for_text.Data[sheet_name][target_sub_text] 
                : 2;
        }

        public double Get_amount(string sheet_name, string code, int column)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, code {code}, column {column}");
            return 0;
        }

        public double Get_amount(string sheet_name, int row, int column)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, row {row}, column {column}");
            return 0;
        }

        public DateTime Get_date(string sheet_name, int row, int column)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, row {row}, column {column}");
            return new DateTime();
        }

        public string Get_text(string sheet_name, int row, int column)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, row {row}, column {column}");
            return "fake";
        }

        public int Find_row_number_of_first_row_containing_cell(
            string sheet_name, 
            string target_cell_text, 
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, targetCellText {target_cell_text}");
            return 2;
        }

        public ICellRow Read_last_row(string sheet_name)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}");
            return new FakeCellRow();
        }

        public string Read_last_row_as_csv(string sheet_name, ICSVRecord csv_record)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}");
            return "";
        }

        // To do: Stop having this method on this interface - this is common code that could exist somewhere independently.
        public List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheet_name, 
            int first_row_number, 
            int last_row_number, 
            int first_column_number,
            int last_column_number) where TRecordType : ICSVRecord, new()
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheet_name}, firstRowNumber {first_row_number}, lastRowNumber {last_row_number}, firstColumnNumber {first_column_number}, lastColumnNumber {last_column_number}");

            List<TRecordType> records = new List<TRecordType>();

            for (int row_number = first_row_number; row_number <= last_row_number; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.Read_from_spreadsheet_row(Read_specified_row(
                    sheet_name,
                    row_number,
                    first_column_number,
                    last_column_number));
                records.Add(csv_record);
            }

            return records;
        }

        public ICellRow Read_specified_row(string sheet_name, int row_number, int start_column, int end_column)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()} with start/end cols: sheetName {sheet_name}, rowNumber {row_number}, startColumn {start_column}, endColumn {end_column}");
            
            var full_row = _fake_rows.Data.ContainsKey(sheet_name)
                ? _fake_rows.Data[sheet_name][row_number - 1] 
                : new FakeCellRow();

            List<object> partial_row = new List<object>(); 
            for (int col_index = start_column; col_index <= end_column; col_index++)
            {
                partial_row.Add(full_row.Read_cell(col_index - 1));
            }
            
            var result = new FakeCellRow().With_fake_data(partial_row);
            return result;
        }

        public ICellRow Read_specified_row(int row_number)
        {
            _debug_log.Append_to_file_as_source_line($"{Get_method_name()} with row number only: rowNumber {row_number}");
            return new FakeCellRow();
        }

        /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////

        public void Append_csv_record(string sheet_name, ICSVRecord csv_record)
        {
        }

        public void Remove_last_row(string sheet_name)
        {
        }

        public void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new()
        {
        }

        public void Update_date(string sheet_name, int date_row, int date_column, DateTime new_date)
        {
        }

        public void Update_text(string sheet_name, int text_row, int text_column, string new_text)
        {
        }

        public void Update_amount(string sheet_name, int amount_row, int amount_column, double new_amount)
        {
        }

        public void Update_amount(
            string sheet_name, 
            string amount_code, 
            double new_amount, 
            int amount_column = SpreadsheetConsts.DefaultAmountColumn, 
            int code_column = SpreadsheetConsts.DefaultCodeColumn)
        {
        }

        public void Update_amount_and_text(
            string sheet_name, 
            string amount_code, 
            double new_amount, 
            string new_text, 
            int amount_column = SpreadsheetConsts.DefaultAmountColumn,
            int text_column = SpreadsheetConsts.DefaultTextColumn, 
            int code_column = SpreadsheetConsts.DefaultCodeColumn)
        {
        }

        public void Insert_new_row(string sheet_name, int new_row_number, Dictionary<int, object> cell_values)
        {
        }

        public void Append_new_row(string sheet_name, Dictionary<int, object> cell_values)
        {
        }

        public void Delete_specified_rows(string sheet_name, int first_row_number, int last_row_number)
        {
        }

        private string Get_method_name()
        {
            return new StackTrace(1).GetFrame(0).GetMethod().Name;
        }
    }
}