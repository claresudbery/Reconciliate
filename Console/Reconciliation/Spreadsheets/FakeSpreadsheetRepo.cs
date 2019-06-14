using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Console.Reconciliation.Spreadsheets.FakeSpreadsheetData;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // There is a distinction between FakeSpreadsheetRepo and DebugSpreadsheetRepo:
    // FakeSpreadsheetRepo does no Excel file access at all, and is used in .Net.
    // DebugSpreadsheetRepo does real Excel file access but only reads from the live spreadsheet: It writes to a backed-up copy.
    internal class FakeSpreadsheetRepo : ISpreadsheetRepo
    {
        FileIO<BankRecord> DebugLog = new FileIO<BankRecord>(
            new FakeSpreadsheetRepoFactory(), 
            ReconConsts.DefaultFilePath,
            "FakeSpreadsheetDataInfo");

        public FakeSpreadsheetRepo()
        {
            DebugLog.AppendToFileAsSourceLine("*******************************************************************************");
            DebugLog.AppendToFileAsSourceLine("*******************************************************************************");
            DebugLog.AppendToFileAsSourceLine("**                             NEW RUN OF INFO                               **");
            DebugLog.AppendToFileAsSourceLine($"**                            {DateTime.Now}                                 **");
            DebugLog.AppendToFileAsSourceLine("*******************************************************************************");
            DebugLog.AppendToFileAsSourceLine("*******************************************************************************");
        }

        public void Dispose()
        {
        }

        public ICellSet CurrentCells(string sheetName)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return new FakeCellSet();
        }

        public int LastRowNumber(string sheetName)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");

            int result = 0;

            if (sheetName == MainSheetNames.BankIn) {result = 6;}
            else if (sheetName == MainSheetNames.BankOut) {result = 9;}
            else if (sheetName == MainSheetNames.CredCard1) {result = 6;}
            else if (sheetName == MainSheetNames.CredCard2) {result = 6;}
            else if (sheetName == MainSheetNames.ExpectedIn) {result = 8;}
            else if (sheetName == MainSheetNames.ExpectedOut) {result = 20;}
            else if (sheetName == MainSheetNames.CredCard3) {result = 5;}
            else if (sheetName == MainSheetNames.Savings) {result = 4;}

            return result;
        }

        public ICellRow ReadSpecifiedRow(string sheetName, int rowNumber)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, rowNumber {rowNumber}");
            return FakeRows.Data.ContainsKey(sheetName)? FakeRows.Data[sheetName][rowNumber - 1] : new FakeCellRow();
        }

        public int FindFirstEmptyRowInColumn(string sheetName, int columnNumber)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, columnNumber {columnNumber}");
            return 2;
        }

        public int FindRowNumberOfLastRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetSubText {targetCellText}");
            return FakeRowNumbersForCell.Data.ContainsKey(sheetName) 
                ? FakeRowNumbersForCell.Data[sheetName][targetCellText] 
                : 2;
        }

        public int FindRowNumberOfLastRowWithCellContainingText(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetSubText {targetSubText}");
            return FakeRowNumbersForText.Data.ContainsKey(sheetName) 
                ? FakeRowNumbersForText.Data[sheetName][targetSubText] 
                : 2;
        }

        public double GetAmount(string sheetName, string code, int column)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, code {code}, column {column}");
            return 0;
        }

        public double GetAmount(string sheetName, int row, int column)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return 0;
        }

        public DateTime GetDate(string sheetName, int row, int column)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return new DateTime();
        }

        public string GetText(string sheetName, int row, int column)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, row {row}, column {column}");
            return "fake";
        }

        public int FindRowNumberOfFirstRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, targetCellText {targetCellText}");
            return 2;
        }

        public ICellRow ReadLastRow(string sheetName)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return new FakeCellRow();
        }

        public string ReadLastRowAsCsv(string sheetName, ICSVRecord csvRecord)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}");
            return "";
        }

        public List<TRecordType> GetRowsAsRecords<TRecordType>(string sheetName, int firstRowNumber, int lastRowNumber, int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()}: sheetName {sheetName}, firstRowNumber {firstRowNumber}, lastRowNumber {lastRowNumber}, firstColumnNumber {firstColumnNumber}");
            return new List<TRecordType>();
        }

        public ICellRow ReadSpecifiedRow(string sheetName, int rowNumber, int startColumn, int endColumn)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()} with start/end cols: sheetName {sheetName}, rowNumber {rowNumber}, startColumn {startColumn}, endColumn {endColumn}");
            
            var fullRow = FakeRows.Data.ContainsKey(sheetName)
                ? FakeRows.Data[sheetName][rowNumber - 1] 
                : new FakeCellRow();

            List<object> partialRow = new List<object>(); 
            for (int colIndex = startColumn; colIndex <= endColumn; colIndex++)
            {
                partialRow.Add(fullRow.ReadCell(colIndex - 1));
            }
            
            var result = new FakeCellRow().WithFakeData(partialRow);
            return result;
        }

        public ICellRow ReadSpecifiedRow(int rowNumber)
        {
            DebugLog.AppendToFileAsSourceLine($"{GetMethodName()} with row number only: rowNumber {rowNumber}");
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