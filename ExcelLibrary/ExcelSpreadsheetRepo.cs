using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Interfaces;
using Interfaces.Constants;
using Microsoft.Office.Interop.Excel;

namespace ExcelLibrary
{
    public class ExcelSpreadsheetRepo : ISpreadsheetRepo, IDisposable
    {
        // !! Don't use this variable, always use the InnerSpreadsheet property, so you get 
        // the protection re file availability.
        private readonly HiddenExcelSpreadsheet _innerSpreadsheet;

        public ExcelSpreadsheetRepo(string spreadsheetFileNameAndPath)
        {
            _innerSpreadsheet = new HiddenExcelSpreadsheet(spreadsheetFileNameAndPath);
        }

        private HiddenExcelSpreadsheet InnerSpreadsheet
        {
            get
            {
                if (!_innerSpreadsheet.FileAvailable)
                {
                    throw new Exception("Spreadsheet not available (do you have it open?)");
                }
                return _innerSpreadsheet;
            }
        }

        public void Dispose()
        {
            _innerSpreadsheet.Dispose();
        }

        public ICellSet CurrentCells(String sheetName)
        {
            return new ExcelRange(InnerSpreadsheet.CurrentCells(sheetName));
        }

        public int LastRowNumber(String sheetName)
        {
            return InnerSpreadsheet.LastRowNumber(sheetName);
        }

        public int FindFirstEmptyRowInColumn(string sheetName, int columnNumber)
        {
            return InnerSpreadsheet.FindFirstEmptyRowInColumn(sheetName, columnNumber);
        }

        public void AppendCsvRecord(String sheetName, ICSVRecord csvRecord)
        {
            InnerSpreadsheet.AppendCsvRecord(sheetName, csvRecord);
        }

        public void RemoveLastRow(String sheetName)
        {
            InnerSpreadsheet.RemoveLastRow(sheetName);
        }

        public ICellRow ReadSpecifiedRow(String sheetName, int rowNumber)
        {
            return InnerSpreadsheet.ReadSpecifiedRow(sheetName, rowNumber);
        }

        public ICellRow ReadSpecifiedRow(String sheetName, int rowNumber, int startColumn, int endColumn)
        {
            return InnerSpreadsheet.ReadSpecifiedRow(sheetName, rowNumber, startColumn, endColumn);
        }

        public ICellRow ReadSpecifiedRow(int rowNumber)
        {
            return InnerSpreadsheet.ReadSpecifiedRow(rowNumber);
        }

        public void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            InnerSpreadsheet.AppendCsvFile(sheetName, csvFile);
        }

        public void DeleteSpecifiedRows(String sheetName, int firstRowNumber, int lastRowNumber)
        {
            InnerSpreadsheet.DeleteSpecifiedRows(sheetName, firstRowNumber, lastRowNumber);
        }

        public List<TRecordType> GetRowsAsRecords<TRecordType>(
            string sheetName, 
            int firstRowNumber, 
            int lastRowNumber, 
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            return InnerSpreadsheet.GetRowsAsRecords<TRecordType>(
                sheetName,
                firstRowNumber,
                lastRowNumber,
                firstColumnNumber,
                lastColumnNumber);
        }

        public int FindRowNumberOfLastRowContainingCell(
            string sheetName, 
            string targetCellText, 
            int expectedColumnNumber = 2)
        {
            return InnerSpreadsheet.FindRowNumberOfLastRowContainingCell(
                sheetName,
                targetCellText,
                expectedColumnNumber);
        }

        public int FindRowNumberOfLastRowWithCellContainingText(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            return InnerSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                sheetName,
                targetSubText,
                expectedColumnNumbers);
        }

        public double GetAmount(string sheetName, string amountCode, int amountColumn = 3)
        {
            return InnerSpreadsheet.GetAmount(sheetName, amountCode, amountColumn);
        }

        public void UpdateDate(string sheetName, int dateRow, int dateColumn, DateTime newDate)
        {
            InnerSpreadsheet.UpdateDate(sheetName, dateRow, dateColumn, newDate);
        }

        public void UpdateText(string sheetName, int textRow, int textColumn, string newText)
        {
            InnerSpreadsheet.UpdateText(sheetName, textRow, textColumn, newText);
        }

        public void UpdateAmount(string sheetName, int amountRow, int amountColumn, double newAmount)
        {
            InnerSpreadsheet.UpdateAmount(sheetName, amountRow, amountColumn, newAmount);
        }

        public void UpdateAmount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1)
        {
            InnerSpreadsheet.UpdateAmount(sheetName, amountCode, newAmount, amountColumn, codeColumn);
        }

        public void UpdateAmountAndText(
            string sheetName, 
            string amountCode, 
            double newAmount,
            string newText,
            int amountColumn = 2,
            int textColumn = 3,
            int codeColumn = 1)
        {
            InnerSpreadsheet.UpdateAmountAndText(sheetName, amountCode, newAmount, newText, amountColumn, textColumn, codeColumn);
        }

        public DateTime GetDate(string sheetName, int row, int column)
        {
            return InnerSpreadsheet.GetDate(sheetName, row, column);
        }

        public double GetAmount(string sheetName, int row, int column)
        {
            return InnerSpreadsheet.GetAmount(sheetName, row, column);
        }

        public string GetText(string sheetName, int row, int column)
        {
            return InnerSpreadsheet.GetText(sheetName, row, column);
        }

        public int FindRowNumberOfFirstRowContainingCell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            return InnerSpreadsheet.FindRowNumberOfFirstRowContainingCell(
                sheetName, 
                targetCellText,
                expectedColumnNumber);
        }

        public void InsertNewRow(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
        {
            InnerSpreadsheet.InsertNewRow(sheetName, newRowNumber, cellValues);
        }

        public void AppendNewRow(string sheetName, Dictionary<int, object> cellValues)
        {
            InnerSpreadsheet.AppendNewRow(sheetName, cellValues);
        }

        public ICellRow ReadLastRow(string sheetName)
        {
            return InnerSpreadsheet.ReadLastRow(sheetName);
        }

        public string ReadLastRowAsCsv(string sheetName, ICSVRecord csvRecord)
        {
            return InnerSpreadsheet.ReadLastRowAsCsv(sheetName, csvRecord);
        }

        private class HiddenExcelSpreadsheet : IDisposable
        {
            private String _spreadsheetFileNameAndPath;
            private Workbooks _workbooks = null;
            private Workbook _workbook = null;
            private Sheets _worksheets = null;
            private List<Worksheet> _openedWorksheets = new List<Worksheet>();
            private Worksheet _currentWorksheet = null;
            private Application _application = null;
            private String _currentSheetName = String.Empty;
            private bool _fileAvailable = false;

            public bool FileAvailable
            {
                get { return _fileAvailable; }
            }

            public HiddenExcelSpreadsheet(String spreadsheetFileNameAndPath)
            {
                _spreadsheetFileNameAndPath = spreadsheetFileNameAndPath;
                _fileAvailable = false;
                if (IsFileAvailable(_spreadsheetFileNameAndPath))
                {
                    _fileAvailable = true;
                    _application = new Application();
                    _application.Visible = false;
                    _workbooks = _application.Workbooks;
                    _workbook = _workbooks.Open(_spreadsheetFileNameAndPath);
                    _worksheets = _workbook.Sheets;
                }
            }

            public void Dispose()
            {
                if (_fileAvailable)
                {
                    // Cleanup
                    _workbook.Close(false);
                    _application.Quit();

                    // Manual disposal because of COM
                    while (Marshal.ReleaseComObject(_application) != 0)
                    {
                    }
                    while (Marshal.ReleaseComObject(_workbook) != 0)
                    {
                    }
                    while (Marshal.ReleaseComObject(_workbooks) != 0)
                    {
                    }
                    while (Marshal.ReleaseComObject(_worksheets) != 0)
                    {
                    }
                    foreach (var sheet in _openedWorksheets)
                    {
                        while (Marshal.ReleaseComObject(sheet) != 0)
                        {
                        }
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            private bool IsFileAvailable(String fileNameAndPath)
            {
                bool result = false;
                try
                {
                    using (Stream stream = new FileStream(fileNameAndPath, FileMode.Open))
                    {
                        result = true;
                    }
                }
                catch
                {
                    result = false;
                }

                return result;
            }

            public Range CurrentCells(String sheetName)
            {
                OpenSheet(sheetName);
                return _currentWorksheet.Cells;
            }

            private void OpenSheet(String sheetName)
            {
                _currentWorksheet = (Worksheet) _worksheets[sheetName];
                if (!_openedWorksheets.Contains(_currentWorksheet))
                {
                    _openedWorksheets.Add(_currentWorksheet);
                }
            }

            public void AppendCsvRecord(String sheetName, ICSVRecord csvRecord)
            {
                OpenSheet(sheetName);
                var new_row_number = LastRowNumber(sheetName) + 1;
                csvRecord.PopulateSpreadsheetRow(new ExcelRange(_currentWorksheet.Cells), new_row_number);

                _workbook.Save();
            }

            public void AppendNewRow(string sheetName, Dictionary<int, object> cellValues)
            {
                OpenSheet(sheetName);
                var new_row_number = LastRowNumber(sheetName) + 1;

                foreach (var cell_value in cellValues)
                {
                    _currentWorksheet.Cells[new_row_number, cell_value.Key] = cell_value.Value;
                }

                _workbook.Save();
            }

            public void RemoveLastRow(String sheetName)
            {
                OpenSheet(sheetName);
                var last_row_number = LastRowNumber(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                var used_columns = used_range.Columns;
                var num_used_columns = used_columns.Count;

                for (int index = 1; index <= num_used_columns; index++)
                {
                    _currentWorksheet.Cells[last_row_number, index] = String.Empty;
                }

                _workbook.Save();
            }

            public int LastRowNumber(String sheetName)
            {
                OpenSheet(sheetName);
                var cells = _currentWorksheet.Cells;
                var special_cells = cells.SpecialCells(XlCellType.xlCellTypeLastCell);
                return special_cells.Row;
            }

            public int FindFirstEmptyRowInColumn(string sheetName, int columnNumber)
            {
                OpenSheet(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                Range first_cell_in_column = used_range.Cells[1, columnNumber] as Range;
                Range last_full_cell_in_column = first_cell_in_column.End[XlDirection.xlDown];

                return last_full_cell_in_column.Row + 1;
            }

            public ICellRow ReadSpecifiedRow(String sheetName, int rowNumber)
            {
                OpenSheet(sheetName);
                return ReadSpecifiedRow(rowNumber);
            }

            public ICellRow ReadSpecifiedRow(int rowNumber)
            {
                var values = new List<object>();
                var used_range = _currentWorksheet.UsedRange;
                var used_columns = used_range.Columns;
                var num_used_columns = used_columns.Count;

                for (int column_count = 1; column_count <= num_used_columns; column_count++)
                {
                    values.Add((used_range.Cells[rowNumber, column_count] as Range).Value2);
                }

                return new ExcelRow(values);
            }

            public ICellRow ReadSpecifiedRow(String sheetName, int rowNumber, int startColumn, int endColumn)
            {
                OpenSheet(sheetName);
                return ReadSpecifiedRow(rowNumber, startColumn, endColumn);
            }

            private ICellRow ReadSpecifiedRow(int rowNumber, int startColumn, int endColumn)
            {
                var values = new List<object>();
                var used_range = _currentWorksheet.UsedRange;

                for (int column_count = startColumn; column_count <= endColumn; column_count++)
                {
                    values.Add((used_range.Cells[rowNumber, column_count] as Range).Value2);
                }

                return new ExcelRow(values);
            }

            private Object ReadSpecifiedCell(String sheetName, int row, int column)
            {
                OpenSheet(sheetName);
                var used_range = _currentWorksheet.UsedRange;
                var cell_value = (used_range.Cells[row, column] as Range)?.Value2;
                return cell_value;
            }

            public void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile)
                where TRecordType : ICSVRecord, new()
            {
                OpenSheet(sheetName);
                var new_row_number = LastRowNumber(sheetName) + 1;
                List<TRecordType> ordered_records = csvFile.RecordsOrderedForSpreadsheet();

                foreach (var csv_record in ordered_records)
                {
                    csv_record.PopulateSpreadsheetRow(new ExcelRange(_currentWorksheet.Cells), new_row_number);
                    new_row_number++;
                }

                _workbook.Save();
            }

            public int FindRowNumberOfLastRowContainingCell(
                string sheetName,
                string targetCellText,
                int expectedColumnNumber = 2)
            {
                OpenSheet(sheetName);
                Range cell_containing_last_target_text = FindLastCellContainingText(targetCellText);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, targetCellText,
                        _currentWorksheet.Name));
                }
                if (cell_containing_last_target_text.Column != expectedColumnNumber)
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, targetCellText,
                        _currentWorksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            public int FindRowNumberOfLastRowWithCellContainingText(
                string sheetName, 
                string targetSubText, 
                List<int> expectedColumnNumbers)
            {
                OpenSheet(sheetName);
                Range cell_containing_last_target_text = FindLastCellContainingText(targetSubText, false);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, targetSubText,
                        _currentWorksheet.Name));
                }
                if (!expectedColumnNumbers.Contains(cell_containing_last_target_text.Column))
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, targetSubText,
                        _currentWorksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            public int FindRowNumberOfFirstRowContainingCell(
                string sheetName,
                string targetCellText,
                int expectedColumnNumber = 2)
            {
                OpenSheet(sheetName);
                Range cell_containing_last_target_text = FindFirstCellContainingText(targetCellText);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, targetCellText,
                        _currentWorksheet.Name));
                }
                if (cell_containing_last_target_text.Column != expectedColumnNumber)
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, targetCellText,
                        _currentWorksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            private Range FindFirstCellContainingText(string textToSearchFor)
            {
                var used_range = _currentWorksheet.UsedRange;
                return used_range.Find(textToSearchFor, Type.Missing,
                    XlFindLookIn.xlValues, XlLookAt.xlWhole,
                    XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                    Type.Missing, Type.Missing);
            }

            private Range FindLastCellContainingText(string textToSearchFor, bool fullTextMatch = true)
            {
                Range current_find = null;
                Range first_find = null;
                Range last_find = null;
                var used_range = _currentWorksheet.UsedRange;

                if (fullTextMatch)
                {
                    current_find = used_range.Find(textToSearchFor, Type.Missing,
                        XlFindLookIn.xlValues, XlLookAt.xlWhole,
                        XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                        Type.Missing, Type.Missing);
                }
                else
                {
                    current_find = used_range.Find(textToSearchFor, Type.Missing,
                        XlFindLookIn.xlValues, XlLookAt.xlPart,
                        XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                        Type.Missing, Type.Missing);
                }

                while (current_find != null)
                {
                    // Keep track of the first range you find. 
                    if (first_find == null)
                    {
                        first_find = current_find;
                    }
                    // If you didn't move to a new range, you are done.
                    else if (current_find.get_Address(XlReferenceStyle.xlA1)
                          == first_find.get_Address(XlReferenceStyle.xlA1))
                    {
                        break;
                    }

                    last_find = current_find;
                    current_find = used_range.FindNext(current_find);
                }

                return last_find;
            }

            public void DeleteSpecifiedRows(string sheetName, int firstRowNumber, int lastRowNumber)
            {
                OpenSheet(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                for (int row_count = lastRowNumber; row_count >= firstRowNumber; row_count--)
                {
                    var first_cell_in_row = (Range)used_range.Cells[row_count, 1];
                    var row_to_delete = first_cell_in_row.EntireRow;
                    row_to_delete.Delete(Type.Missing);
                }

                _workbook.Save();
            }

            public List<TRecordType> GetRowsAsRecords<TRecordType>(
                string sheetName,
                int firstRowNumber,
                int lastRowNumber,
                int firstColumnNumber,
                int lastColumnNumber) where TRecordType : ICSVRecord, new()
            {
                List<TRecordType> monthly_budget_items = new List<TRecordType>();

                for (int row_number = firstRowNumber; row_number <= lastRowNumber; row_number++)
                {
                    var csv_record = new TRecordType();
                    csv_record.ReadFromSpreadsheetRow(ReadSpecifiedRow(
                        sheetName,
                        row_number,
                        firstColumnNumber,
                        lastColumnNumber));
                    monthly_budget_items.Add(csv_record);
                }

                return monthly_budget_items;
            }

            public double GetAmount(string sheetName, string amountCode, int amountColumn)
            {
                const int codeColumn = 1;
                int row = FindRowNumberOfLastRowContainingCell(sheetName, amountCode, codeColumn);

                var excel_cell = ReadSpecifiedCell(sheetName, row, amountColumn);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, amountCode {amountCode}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            private void UpdateCell(string sheetName, int row, int column, Object newValue)
            {
                OpenSheet(sheetName);
                _currentWorksheet.Cells[row, column] = newValue;
                _workbook.Save();
            }

            public void UpdateDate(string sheetName, int dateRow, int dateColumn, DateTime newDate)
            {
                UpdateCell(sheetName, dateRow, dateColumn, newDate.ToOADate());
            }

            public void UpdateText(string sheetName, int textRow, int textColumn, string newText)
            {
                UpdateCell(sheetName, textRow, textColumn, newText);
            }

            public void UpdateAmount(string sheetName, int amountRow, int amountColumn, double newAmount)
            {
                UpdateCell(sheetName, amountRow, amountColumn, newAmount);
            }

            public void UpdateAmount(string sheetName, string amountCode, double newAmount, int amountColumn, int codeColumn = 1)
            {
                int row = FindRowNumberOfLastRowContainingCell(sheetName, amountCode, codeColumn);
                UpdateAmount(sheetName, row, amountColumn, newAmount);
            }

            public void UpdateAmountAndText(
                string sheetName, 
                string amountCode, 
                double newAmount, 
                string newText, 
                int amountColumn, 
                int textColumn,
                int codeColumn = 1)
            {
                int row = FindRowNumberOfLastRowContainingCell(sheetName, amountCode, codeColumn);

                OpenSheet(sheetName);
                _currentWorksheet.Cells[row, amountColumn] = newAmount;
                _currentWorksheet.Cells[row, textColumn] = newText;
                _workbook.Save();
            }

            internal DateTime GetDate(string sheetName, int row, int column)
            {
                var excel_cell = ReadSpecifiedCell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return DateTime.FromOADate((double)excel_cell);
            }

            internal double GetAmount(string sheetName, int row, int column)
            {
                var excel_cell = ReadSpecifiedCell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            internal string GetText(string sheetName, int row, int column)
            {
                var excel_cell = ReadSpecifiedCell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return (string)excel_cell;
            }

            public void InsertNewRow(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
            {
                OpenSheet(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                var first_cell_in_row = (Range)used_range.Cells[newRowNumber, 1];
                var row_to_insert_before = first_cell_in_row.EntireRow;
                row_to_insert_before.Insert(XlInsertShiftDirection.xlShiftDown);

                foreach (var cell_value in cellValues)
                {
                    _currentWorksheet.Cells[newRowNumber, cell_value.Key] = cell_value.Value;
                }

                _workbook.Save();
            }

            public ICellRow ReadLastRow(String sheetName)
            {
                int last_row_number = LastRowNumber(sheetName);
                return ReadSpecifiedRow(last_row_number);
            }

            public String ReadLastRowAsCsv(String sheetName, ICSVRecord csvRecord)
            {
                csvRecord.ReadFromSpreadsheetRow(ReadLastRow(sheetName));
                return csvRecord.ToCsv();
            }
        }
    }
}
