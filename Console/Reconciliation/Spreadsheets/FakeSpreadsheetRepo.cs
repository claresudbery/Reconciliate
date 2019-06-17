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
        FileIO<BankRecord> _debugLog = new FileIO<BankRecord>(
            new FakeSpreadsheetRepoFactory(), 
            ReconConsts.Default_file_path,
            "FakeSpreadsheetDataInfo");

        private readonly FakeRowNumbersForCell _fakeRowNumbersForCell = new FakeRowNumbersForCell();
        private readonly FakeRowNumbersForText _fakeRowNumbersForText = new FakeRowNumbersForText();
        private readonly FakeRows _fakeRows = new FakeRows();
        private readonly LastRowNumbers _lastRowNumbers = new LastRowNumbers();

        public static string FakeMortgageDescription = "Mortgage description";

        public FakeSpreadsheetRepo()
        {
            _debugLog.Append_to_file_as_source_line("*******************************************************************************");
            _debugLog.Append_to_file_as_source_line("*******************************************************************************");
            _debugLog.Append_to_file_as_source_line("**                             NEW RUN OF INFO                               **");
            _debugLog.Append_to_file_as_source_line($"**                            {DateTime.Now}                                 **");
            _debugLog.Append_to_file_as_source_line("*******************************************************************************");
            _debugLog.Append_to_file_as_source_line("*******************************************************************************");
        }

        public void Dispose()
        {
        }

        public ICellSet Current_cells(string sheetName)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}");
            return new FakeCellSet();
        }

        public int Last_row_number(string sheetName)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}");

            return _lastRowNumbers.Data.ContainsKey(sheetName)
                ? _lastRowNumbers.Data[sheetName]
                : 2;
        }

        public ICellRow Read_specified_row(string sheetName, int rowNumber)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, rowNumber {rowNumber}");

            return _fakeRows.Data.ContainsKey(sheetName)
                ? _fakeRows.Data[sheetName][rowNumber - 1] 
                : new FakeCellRow();
        }

        public int Find_first_empty_row_in_column(string sheetName, int columnNumber)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, columnNumber {columnNumber}");
            return 2;
        }

        public int Find_row_number_of_last_row_containing_cell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, targetSubText {targetCellText}");
            return _fakeRowNumbersForCell.Data.ContainsKey(sheetName) 
                ? _fakeRowNumbersForCell.Data[sheetName][targetCellText] 
                : 2;
        }

        public int Find_row_number_of_last_row_with_cell_containing_text(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, targetSubText {targetSubText}");
            return _fakeRowNumbersForText.Data.ContainsKey(sheetName) 
                ? _fakeRowNumbersForText.Data[sheetName][targetSubText] 
                : 2;
        }

        public double Get_amount(string sheetName, string code, int column)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, code {code}, column {column}");
            return 0;
        }

        public double Get_amount(string sheetName, int row, int column)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, row {row}, column {column}");
            return 0;
        }

        public DateTime Get_date(string sheetName, int row, int column)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, row {row}, column {column}");
            return new DateTime();
        }

        public string Get_text(string sheetName, int row, int column)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, row {row}, column {column}");
            return "fake";
        }

        public int Find_row_number_of_first_row_containing_cell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, targetCellText {targetCellText}");
            return 2;
        }

        public ICellRow Read_last_row(string sheetName)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}");
            return new FakeCellRow();
        }

        public string Read_last_row_as_csv(string sheetName, ICSVRecord csvRecord)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}");
            return "";
        }

        // To do: Stop having this method on this interface - this is common code that could exist somewhere independently.
        public List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheetName, 
            int firstRowNumber, 
            int lastRowNumber, 
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()}: sheetName {sheetName}, firstRowNumber {firstRowNumber}, lastRowNumber {lastRowNumber}, firstColumnNumber {firstColumnNumber}, lastColumnNumber {lastColumnNumber}");

            List<TRecordType> records = new List<TRecordType>();

            for (int row_number = firstRowNumber; row_number <= lastRowNumber; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.Read_from_spreadsheet_row(Read_specified_row(
                    sheetName,
                    row_number,
                    firstColumnNumber,
                    lastColumnNumber));
                records.Add(csv_record);
            }

            return records;
        }

        public ICellRow Read_specified_row(string sheetName, int rowNumber, int startColumn, int endColumn)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()} with start/end cols: sheetName {sheetName}, rowNumber {rowNumber}, startColumn {startColumn}, endColumn {endColumn}");
            
            var full_row = _fakeRows.Data.ContainsKey(sheetName)
                ? _fakeRows.Data[sheetName][rowNumber - 1] 
                : new FakeCellRow();

            List<object> partial_row = new List<object>(); 
            for (int col_index = startColumn; col_index <= endColumn; col_index++)
            {
                partial_row.Add(full_row.Read_cell(col_index - 1));
            }
            
            var result = new FakeCellRow().With_fake_data(partial_row);
            return result;
        }

        public ICellRow Read_specified_row(int rowNumber)
        {
            _debugLog.Append_to_file_as_source_line($"{Get_method_name()} with row number only: rowNumber {rowNumber}");
            return new FakeCellRow();
        }

        /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////

        public void Append_csv_record(string sheetName, ICSVRecord csvRecord)
        {
        }

        public void Remove_last_row(string sheetName)
        {
        }

        public void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
        }

        public void Update_date(string sheetName, int dateRow, int dateColumn, DateTime newDate)
        {
        }

        public void Update_text(string sheetName, int textRow, int textColumn, string newText)
        {
        }

        public void Update_amount(string sheetName, int amountRow, int amountColumn, double newAmount)
        {
        }

        public void Update_amount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1)
        {
        }

        public void Update_amount_and_text(string sheetName, string amountCode, double newAmount, string newText, int amountColumn = 2,
            int textColumn = 3, int codeColumn = 1)
        {
        }

        public void Insert_new_row(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
        {
        }

        public void Append_new_row(string sheetName, Dictionary<int, object> cellValues)
        {
        }

        public void Delete_specified_rows(string sheetName, int firstRowNumber, int lastRowNumber)
        {
        }

        private string Get_method_name()
        {
            return new StackTrace(1).GetFrame(0).GetMethod().Name;
        }
    }
}