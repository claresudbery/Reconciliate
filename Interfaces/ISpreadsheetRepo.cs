using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface ISpreadsheetRepo
    {
        void Dispose();
        int LastRowNumber(String sheetName);
        int FindFirstEmptyRowInColumn(string sheetName, int columnNumber);
        int FindRowNumberOfFirstRowContainingCell(
            string sheetName,
            string targetCellText,
            int expectedColumnNumber = 2);
        int FindRowNumberOfLastRowContainingCell(
            string sheetName,
            string targetCellText,
            int expectedColumnNumber = 2);
        int FindRowNumberOfLastRowWithCellContainingText(
            string sheetName,
            string targetSubText,
            List<int> expectedColumnNumbers);
        ICellRow ReadSpecifiedRow(String sheetName, int rowNumber);
        ICellRow ReadSpecifiedRow(String sheetName, int rowNumber, int startColumn, int endColumn);
        ICellRow ReadSpecifiedRow(int rowNumber);
        ICellSet CurrentCells(String sheetName);
        List<TRecordType> GetRowsAsRecords<TRecordType>(
            string sheetName,
            int firstRowNumber,
            int lastRowNumber,
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new();
        ICellRow ReadLastRow(String sheetName);
        String ReadLastRowAsCsv(String sheetName, ICSVRecord csvRecord);
        void AppendCsvRecord(String sheetName, ICSVRecord csvRecord);
        void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        void RemoveLastRow(String sheetName);
        void DeleteSpecifiedRows(String sheetName, int firstRowNumber, int lastRowNumber);
        DateTime GetDate(string sheetName, int row, int column);
        string GetText(string sheetName, int row, int column);
        double GetAmount(string sheetName, int row, int column);
        double GetAmount(string sheetName, string code, int amountColumn = 3);
        void UpdateDate(string sheetName, int dateRow, int dateColumn, DateTime newDate);
        void UpdateText(string sheetName, int textRow, int textColumn, string newText);
        void UpdateAmount(string sheetName, int amountRow, int amountColumn, double newAmount);
        void UpdateAmount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1);
        void UpdateAmountAndText(
            string sheetName,
            string amountCode,
            double newAmount,
            string newText,
            int amountColumn = 2,
            int textColumn = 3,
            int codeColumn = 1);
        void InsertNewRow(string sheetName, int newRowNumber, Dictionary<int, object> cellValues);
        void AppendNewRow(string sheetName, Dictionary<int, object> cellValues);
    }
}