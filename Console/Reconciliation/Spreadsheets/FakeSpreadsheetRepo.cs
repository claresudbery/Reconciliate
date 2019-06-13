using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // There is a distinction between FakeSpreadsheetRepo and DebugSpreadsheetRepo:
    // FakeSpreadsheetRepo does no Excel file access at all, and is used in .Net.
    // DebugSpreadsheetRepo does real Excel file access but only reads from the live spreadsheet: It writes to a backed-up copy.
    internal class FakeSpreadsheetRepo : ISpreadsheetRepo
    {
        public void Dispose()
        {
        }

        public ICellSet CurrentCells(string sheetName)
        {
            return new FakeCellSet();
        }

        public int LastRowNumber(string sheetName)
        {
            int result = 0;

            if (sheetName == MainSheetNames.BankIn) {result = 6;}
            else if (sheetName == MainSheetNames.BankOut) {result = 8;}
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
            if (sheetName == MainSheetNames.ExpectedIn)
            {
                switch (rowNumber)
                {
                    case 2: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 1).ToOADate(), (double)60, Codes.Expenses, null, null, null, "Expense 001" }); 
                    case 3: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 2).ToOADate(), (double)29.24, Codes.Expenses, null, null, null, "Expense 002" }); 
                    case 4: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 3).ToOADate(), (double)100, Codes.Expenses, null, null, null, "Expense 003" }); 
                    case 5: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 4).ToOADate(), (double)64.08, Codes.Expenses, null, null, null, "Expense 004" }); 
                    case 6: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 5).ToOADate(), (double)214, Codes.Expenses, null, null, null, "Expense 005" }); 
                    case 7: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 6).ToOADate(), (double)53, Codes.Expenses, null, null, null, "Expense 006" }); 
                    case 8: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 7).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 007" }); 
                    case 9: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 8).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 008" }); 
                    case 10: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 9).ToOADate(), (double)38.99, Codes.Expenses, null, null, null, "Expense 009" }); 
                    case 11: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 10).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 010" }); 
                    case 12: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 11).ToOADate(), (double)84.45, "Code yyy", null, null, null, "Expense 011" }); 
                    case 13: return new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 12).ToOADate(), (double)63.32, Codes.Expenses, null, null, null, "Expense 012" }); 
                }
            }
            return new FakeCellRow();
        }

        public int FindFirstEmptyRowInColumn(string sheetName, int columnNumber)
        {
            return 0;
        }

        public int FindRowNumberOfLastRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            if (sheetName == MainSheetNames.ExpectedIn && targetCellText == Dividers.DividerText)
            {
                return 1;
            }
            return 0;
        }

        public int FindRowNumberOfLastRowWithCellContainingText(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            return 0;
        }

        public double GetAmount(string sheetName, string code, int column)
        {
            return 0;
        }

        public double GetAmount(string sheetName, int row, int column)
        {
            return 0;
        }

        public DateTime GetDate(string sheetName, int row, int column)
        {
            return new DateTime();
        }

        public string GetText(string sheetName, int row, int column)
        {
            return "fake";
        }

        public int FindRowNumberOfFirstRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            return 0;
        }

        public ICellRow ReadLastRow(string sheetName)
        {
            return new FakeCellRow();
        }

        public string ReadLastRowAsCsv(string sheetName, ICSVRecord csvRecord)
        {
            return "";
        }

        public List<TRecordType> GetRowsAsRecords<TRecordType>(string sheetName, int firstRowNumber, int lastRowNumber, int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            return new List<TRecordType>();
        }

        public ICellRow ReadSpecifiedRow(string sheetName, int rowNumber, int startColumn, int endColumn)
        {
            return new FakeCellRow();
        }

        public ICellRow ReadSpecifiedRow(int rowNumber)
        {
            return new FakeCellRow();
        }

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
    }
}