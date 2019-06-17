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
            ReconConsts.DefaultFilePath,
            "FakeSpreadsheetDataInfo");

        private readonly FakeRowNumbersForCell _fakeRowNumbersForCell = new FakeRowNumbersForCell();
        private readonly FakeRowNumbersForText _fakeRowNumbersForText = new FakeRowNumbersForText();
        private readonly FakeRows _fakeRows = new FakeRows();
        private readonly LastRowNumbers _lastRowNumbers = new LastRowNumbers();

        public static string FakeMortgageDescription = "Mortgage description";

        public FakeSpreadsheetRepo()
        {
            _debugLog.AppendToFileAsSourceLine("*******************************************************************************");
            _debugLog.AppendToFileAsSourceLine("*******************************************************************************");
            _debugLog.AppendToFileAsSourceLine("**                             NEW RUN OF INFO                               **");
            _debugLog.AppendToFileAsSourceLine($"**                            {DateTime.Now}                                 **");
            _debugLog.AppendToFileAsSourceLine("*******************************************************************************");
            _debugLog.AppendToFileAsSourceLine("*******************************************************************************");
        }

        public void Dispose()
        {
        }

        public ICellSet CurrentCells(string sheetName)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return new FakeCellSet();
        }

        public int LastRowNumber(string sheetName)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");

            return _lastRowNumbers.Data.ContainsKey(sheetName)
                ? _lastRowNumbers.Data[sheetName]
                : 2;
        }

        public ICellRow ReadSpecifiedRow(string sheetName, int rowNumber)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, rowNumber {rowNumber}");

            return _fakeRows.Data.ContainsKey(sheetName)
                ? _fakeRows.Data[sheetName][rowNumber - 1] 
                : new FakeCellRow();
        }

        public int FindFirstEmptyRowInColumn(string sheetName, int columnNumber)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, columnNumber {columnNumber}");
            return 2;
        }

        public int FindRowNumberOfLastRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetSubText {targetCellText}");
            return _fakeRowNumbersForCell.Data.ContainsKey(sheetName) 
                ? _fakeRowNumbersForCell.Data[sheetName][targetCellText] 
                : 2;
        }

        public int FindRowNumberOfLastRowWithCellContainingText(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetSubText {targetSubText}");
            return _fakeRowNumbersForText.Data.ContainsKey(sheetName) 
                ? _fakeRowNumbersForText.Data[sheetName][targetSubText] 
                : 2;
        }

        public double GetAmount(string sheetName, string code, int column)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, code {code}, column {column}");
            return 0;
        }

        public double GetAmount(string sheetName, int row, int column)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return 0;
        }

        public DateTime GetDate(string sheetName, int row, int column)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return new DateTime();
        }

        public string GetText(string sheetName, int row, int column)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return "fake";
        }

        public int FindRowNumberOfFirstRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetCellText {targetCellText}");
            return 2;
        }

        public ICellRow ReadLastRow(string sheetName)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return new FakeCellRow();
        }

        public string ReadLastRowAsCsv(string sheetName, ICSVRecord csvRecord)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return "";
        }

        // To do: Stop having this method on this interface - this is common code that could exist somewhere independently.
        public List<TRecordType> GetRowsAsRecords<TRecordType>(
            string sheetName, 
            int firstRowNumber, 
            int lastRowNumber, 
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, firstRowNumber {firstRowNumber}, lastRowNumber {lastRowNumber}, firstColumnNumber {firstColumnNumber}, lastColumnNumber {lastColumnNumber}");

            List<TRecordType> records = new List<TRecordType>();

            for (int row_number = firstRowNumber; row_number <= lastRowNumber; row_number++)
            {
                var csv_record = new TRecordType();
                csv_record.ReadFromSpreadsheetRow(ReadSpecifiedRow(
                    sheetName,
                    row_number,
                    firstColumnNumber,
                    lastColumnNumber));
                records.Add(csv_record);
            }

            return records;
        }

        public ICellRow ReadSpecifiedRow(string sheetName, int rowNumber, int startColumn, int endColumn)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()} with start/end cols: sheetName {sheetName}, rowNumber {rowNumber}, startColumn {startColumn}, endColumn {endColumn}");
            
            var full_row = _fakeRows.Data.ContainsKey(sheetName)
                ? _fakeRows.Data[sheetName][rowNumber - 1] 
                : new FakeCellRow();

            List<object> partial_row = new List<object>(); 
            for (int col_index = startColumn; col_index <= endColumn; col_index++)
            {
                partial_row.Add(full_row.ReadCell(col_index - 1));
            }
            
            var result = new FakeCellRow().WithFakeData(partial_row);
            return result;
        }

        public ICellRow ReadSpecifiedRow(int rowNumber)
        {
            _debugLog.AppendToFileAsSourceLine($"{GetMethodName()} with row number only: rowNumber {rowNumber}");
            return new FakeCellRow();
        }

        /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////
        /// /// ///////////////////////////////////////////////////////////////////////////////

        public void AppendCsvRecord(string sheetName, ICSVRecord csvRecord)
        {
        }

        public void RemoveLastRow(string sheetName)
        {
        }

        public void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
        }

        public void UpdateDate(string sheetName, int dateRow, int dateColumn, DateTime newDate)
        {
        }

        public void UpdateText(string sheetName, int textRow, int textColumn, string newText)
        {
        }

        public void UpdateAmount(string sheetName, int amountRow, int amountColumn, double newAmount)
        {
        }

        public void UpdateAmount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1)
        {
        }

        public void UpdateAmountAndText(string sheetName, string amountCode, double newAmount, string newText, int amountColumn = 2,
            int textColumn = 3, int codeColumn = 1)
        {
        }

        public void InsertNewRow(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
        {
        }

        public void AppendNewRow(string sheetName, Dictionary<int, object> cellValues)
        {
        }

        public void DeleteSpecifiedRows(string sheetName, int firstRowNumber, int lastRowNumber)
        {
        }

        private string GetMethodName()
        {
            return new StackTrace(1).GetFrame(0).GetMethod().Name;
        }
    }
}