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

        private HiddenExcelSpreadsheet Inner_spreadsheet
        {
            get
            {
                if (!_innerSpreadsheet.File_available)
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

        public ICellSet Current_cells(String sheetName)
        {
            return new ExcelRange(Inner_spreadsheet.Current_cells(sheetName));
        }

        public int Last_row_number(String sheetName)
        {
            return Inner_spreadsheet.Last_row_number(sheetName);
        }

        public int Find_first_empty_row_in_column(string sheetName, int columnNumber)
        {
            return Inner_spreadsheet.Find_first_empty_row_in_column(sheetName, columnNumber);
        }

        public void Append_csv_record(String sheetName, ICSVRecord csvRecord)
        {
            Inner_spreadsheet.Append_csv_record(sheetName, csvRecord);
        }

        public void Remove_last_row(String sheetName)
        {
            Inner_spreadsheet.Remove_last_row(sheetName);
        }

        public ICellRow Read_specified_row(String sheetName, int rowNumber)
        {
            return Inner_spreadsheet.Read_specified_row(sheetName, rowNumber);
        }

        public ICellRow Read_specified_row(String sheetName, int rowNumber, int startColumn, int endColumn)
        {
            return Inner_spreadsheet.Read_specified_row(sheetName, rowNumber, startColumn, endColumn);
        }

        public ICellRow Read_specified_row(int rowNumber)
        {
            return Inner_spreadsheet.Read_specified_row(rowNumber);
        }

        public void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new()
        {
            Inner_spreadsheet.Append_csv_file(sheetName, csvFile);
        }

        public void Delete_specified_rows(String sheetName, int firstRowNumber, int lastRowNumber)
        {
            Inner_spreadsheet.Delete_specified_rows(sheetName, firstRowNumber, lastRowNumber);
        }

        public List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheetName, 
            int firstRowNumber, 
            int lastRowNumber, 
            int firstColumnNumber,
            int lastColumnNumber) where TRecordType : ICSVRecord, new()
        {
            return Inner_spreadsheet.Get_rows_as_records<TRecordType>(
                sheetName,
                firstRowNumber,
                lastRowNumber,
                firstColumnNumber,
                lastColumnNumber);
        }

        public int Find_row_number_of_last_row_containing_cell(
            string sheetName, 
            string targetCellText, 
            int expectedColumnNumber = 2)
        {
            return Inner_spreadsheet.Find_row_number_of_last_row_containing_cell(
                sheetName,
                targetCellText,
                expectedColumnNumber);
        }

        public int Find_row_number_of_last_row_with_cell_containing_text(string sheetName, string targetSubText, List<int> expectedColumnNumbers)
        {
            return Inner_spreadsheet.Find_row_number_of_last_row_with_cell_containing_text(
                sheetName,
                targetSubText,
                expectedColumnNumbers);
        }

        public double Get_amount(string sheetName, string amountCode, int amountColumn = 3)
        {
            return Inner_spreadsheet.Get_amount(sheetName, amountCode, amountColumn);
        }

        public void Update_date(string sheetName, int dateRow, int dateColumn, DateTime newDate)
        {
            Inner_spreadsheet.Update_date(sheetName, dateRow, dateColumn, newDate);
        }

        public void Update_text(string sheetName, int textRow, int textColumn, string newText)
        {
            Inner_spreadsheet.Update_text(sheetName, textRow, textColumn, newText);
        }

        public void Update_amount(string sheetName, int amountRow, int amountColumn, double newAmount)
        {
            Inner_spreadsheet.Update_amount(sheetName, amountRow, amountColumn, newAmount);
        }

        public void Update_amount(string sheetName, string amountCode, double newAmount, int amountColumn = 2, int codeColumn = 1)
        {
            Inner_spreadsheet.Update_amount(sheetName, amountCode, newAmount, amountColumn, codeColumn);
        }

        public void Update_amount_and_text(
            string sheetName, 
            string amountCode, 
            double newAmount,
            string newText,
            int amountColumn = 2,
            int textColumn = 3,
            int codeColumn = 1)
        {
            Inner_spreadsheet.Update_amount_and_text(sheetName, amountCode, newAmount, newText, amountColumn, textColumn, codeColumn);
        }

        public DateTime Get_date(string sheetName, int row, int column)
        {
            return Inner_spreadsheet.Get_date(sheetName, row, column);
        }

        public double Get_amount(string sheetName, int row, int column)
        {
            return Inner_spreadsheet.Get_amount(sheetName, row, column);
        }

        public string Get_text(string sheetName, int row, int column)
        {
            return Inner_spreadsheet.Get_text(sheetName, row, column);
        }

        public int Find_row_number_of_first_row_containing_cell(string sheetName, string targetCellText, int expectedColumnNumber = 2)
        {
            return Inner_spreadsheet.Find_row_number_of_first_row_containing_cell(
                sheetName, 
                targetCellText,
                expectedColumnNumber);
        }

        public void Insert_new_row(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
        {
            Inner_spreadsheet.Insert_new_row(sheetName, newRowNumber, cellValues);
        }

        public void Append_new_row(string sheetName, Dictionary<int, object> cellValues)
        {
            Inner_spreadsheet.Append_new_row(sheetName, cellValues);
        }

        public ICellRow Read_last_row(string sheetName)
        {
            return Inner_spreadsheet.Read_last_row(sheetName);
        }

        public string Read_last_row_as_csv(string sheetName, ICSVRecord csvRecord)
        {
            return Inner_spreadsheet.Read_last_row_as_csv(sheetName, csvRecord);
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

            public bool File_available
            {
                get { return _fileAvailable; }
            }

            public HiddenExcelSpreadsheet(String spreadsheetFileNameAndPath)
            {
                _spreadsheetFileNameAndPath = spreadsheetFileNameAndPath;
                _fileAvailable = false;
                if (Is_file_available(_spreadsheetFileNameAndPath))
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

            private bool Is_file_available(String fileNameAndPath)
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

            public Range Current_cells(String sheetName)
            {
                Open_sheet(sheetName);
                return _currentWorksheet.Cells;
            }

            private void Open_sheet(String sheetName)
            {
                _currentWorksheet = (Worksheet) _worksheets[sheetName];
                if (!_openedWorksheets.Contains(_currentWorksheet))
                {
                    _openedWorksheets.Add(_currentWorksheet);
                }
            }

            public void Append_csv_record(String sheetName, ICSVRecord csvRecord)
            {
                Open_sheet(sheetName);
                var new_row_number = Last_row_number(sheetName) + 1;
                csvRecord.Populate_spreadsheet_row(new ExcelRange(_currentWorksheet.Cells), new_row_number);

                _workbook.Save();
            }

            public void Append_new_row(string sheetName, Dictionary<int, object> cellValues)
            {
                Open_sheet(sheetName);
                var new_row_number = Last_row_number(sheetName) + 1;

                foreach (var cell_value in cellValues)
                {
                    _currentWorksheet.Cells[new_row_number, cell_value.Key] = cell_value.Value;
                }

                _workbook.Save();
            }

            public void Remove_last_row(String sheetName)
            {
                Open_sheet(sheetName);
                var last_row_number = Last_row_number(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                var used_columns = used_range.Columns;
                var num_used_columns = used_columns.Count;

                for (int index = 1; index <= num_used_columns; index++)
                {
                    _currentWorksheet.Cells[last_row_number, index] = String.Empty;
                }

                _workbook.Save();
            }

            public int Last_row_number(String sheetName)
            {
                Open_sheet(sheetName);
                var cells = _currentWorksheet.Cells;
                var special_cells = cells.SpecialCells(XlCellType.xlCellTypeLastCell);
                return special_cells.Row;
            }

            public int Find_first_empty_row_in_column(string sheetName, int columnNumber)
            {
                Open_sheet(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                Range first_cell_in_column = used_range.Cells[1, columnNumber] as Range;
                Range last_full_cell_in_column = first_cell_in_column.End[XlDirection.xlDown];

                return last_full_cell_in_column.Row + 1;
            }

            public ICellRow Read_specified_row(String sheetName, int rowNumber)
            {
                Open_sheet(sheetName);
                return Read_specified_row(rowNumber);
            }

            public ICellRow Read_specified_row(int rowNumber)
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

            public ICellRow Read_specified_row(String sheetName, int rowNumber, int startColumn, int endColumn)
            {
                Open_sheet(sheetName);
                return Read_specified_row(rowNumber, startColumn, endColumn);
            }

            private ICellRow Read_specified_row(int rowNumber, int startColumn, int endColumn)
            {
                var values = new List<object>();
                var used_range = _currentWorksheet.UsedRange;

                for (int column_count = startColumn; column_count <= endColumn; column_count++)
                {
                    values.Add((used_range.Cells[rowNumber, column_count] as Range).Value2);
                }

                return new ExcelRow(values);
            }

            private Object Read_specified_cell(String sheetName, int row, int column)
            {
                Open_sheet(sheetName);
                var used_range = _currentWorksheet.UsedRange;
                var cell_value = (used_range.Cells[row, column] as Range)?.Value2;
                return cell_value;
            }

            public void Append_csv_file<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile)
                where TRecordType : ICSVRecord, new()
            {
                Open_sheet(sheetName);
                var new_row_number = Last_row_number(sheetName) + 1;
                List<TRecordType> ordered_records = csvFile.Records_ordered_for_spreadsheet();

                foreach (var csv_record in ordered_records)
                {
                    csv_record.Populate_spreadsheet_row(new ExcelRange(_currentWorksheet.Cells), new_row_number);
                    new_row_number++;
                }

                _workbook.Save();
            }

            public int Find_row_number_of_last_row_containing_cell(
                string sheetName,
                string targetCellText,
                int expectedColumnNumber = 2)
            {
                Open_sheet(sheetName);
                Range cell_containing_last_target_text = Find_last_cell_containing_text(targetCellText);

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

            public int Find_row_number_of_last_row_with_cell_containing_text(
                string sheetName, 
                string targetSubText, 
                List<int> expectedColumnNumbers)
            {
                Open_sheet(sheetName);
                Range cell_containing_last_target_text = Find_last_cell_containing_text(targetSubText, false);

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

            public int Find_row_number_of_first_row_containing_cell(
                string sheetName,
                string targetCellText,
                int expectedColumnNumber = 2)
            {
                Open_sheet(sheetName);
                Range cell_containing_last_target_text = Find_first_cell_containing_text(targetCellText);

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

            private Range Find_first_cell_containing_text(string textToSearchFor)
            {
                var used_range = _currentWorksheet.UsedRange;
                return used_range.Find(textToSearchFor, Type.Missing,
                    XlFindLookIn.xlValues, XlLookAt.xlWhole,
                    XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                    Type.Missing, Type.Missing);
            }

            private Range Find_last_cell_containing_text(string textToSearchFor, bool fullTextMatch = true)
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

            public void Delete_specified_rows(string sheetName, int firstRowNumber, int lastRowNumber)
            {
                Open_sheet(sheetName);

                var used_range = _currentWorksheet.UsedRange;
                for (int row_count = lastRowNumber; row_count >= firstRowNumber; row_count--)
                {
                    var first_cell_in_row = (Range)used_range.Cells[row_count, 1];
                    var row_to_delete = first_cell_in_row.EntireRow;
                    row_to_delete.Delete(Type.Missing);
                }

                _workbook.Save();
            }

            public List<TRecordType> Get_rows_as_records<TRecordType>(
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
                    csv_record.Read_from_spreadsheet_row(Read_specified_row(
                        sheetName,
                        row_number,
                        firstColumnNumber,
                        lastColumnNumber));
                    monthly_budget_items.Add(csv_record);
                }

                return monthly_budget_items;
            }

            public double Get_amount(string sheetName, string amountCode, int amountColumn)
            {
                const int codeColumn = 1;
                int row = Find_row_number_of_last_row_containing_cell(sheetName, amountCode, codeColumn);

                var excel_cell = Read_specified_cell(sheetName, row, amountColumn);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, amountCode {amountCode}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            private void Update_cell(string sheetName, int row, int column, Object newValue)
            {
                Open_sheet(sheetName);
                _currentWorksheet.Cells[row, column] = newValue;
                _workbook.Save();
            }

            public void Update_date(string sheetName, int dateRow, int dateColumn, DateTime newDate)
            {
                Update_cell(sheetName, dateRow, dateColumn, newDate.ToOADate());
            }

            public void Update_text(string sheetName, int textRow, int textColumn, string newText)
            {
                Update_cell(sheetName, textRow, textColumn, newText);
            }

            public void Update_amount(string sheetName, int amountRow, int amountColumn, double newAmount)
            {
                Update_cell(sheetName, amountRow, amountColumn, newAmount);
            }

            public void Update_amount(string sheetName, string amountCode, double newAmount, int amountColumn, int codeColumn = 1)
            {
                int row = Find_row_number_of_last_row_containing_cell(sheetName, amountCode, codeColumn);
                Update_amount(sheetName, row, amountColumn, newAmount);
            }

            public void Update_amount_and_text(
                string sheetName, 
                string amountCode, 
                double newAmount, 
                string newText, 
                int amountColumn, 
                int textColumn,
                int codeColumn = 1)
            {
                int row = Find_row_number_of_last_row_containing_cell(sheetName, amountCode, codeColumn);

                Open_sheet(sheetName);
                _currentWorksheet.Cells[row, amountColumn] = newAmount;
                _currentWorksheet.Cells[row, textColumn] = newText;
                _workbook.Save();
            }

            internal DateTime Get_date(string sheetName, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return DateTime.FromOADate((double)excel_cell);
            }

            internal double Get_amount(string sheetName, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            internal string Get_text(string sheetName, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheetName, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheetName}, row {row}, column {column}.");
                }

                return (string)excel_cell;
            }

            public void Insert_new_row(string sheetName, int newRowNumber, Dictionary<int, object> cellValues)
            {
                Open_sheet(sheetName);

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

            public ICellRow Read_last_row(String sheetName)
            {
                int last_row_number = Last_row_number(sheetName);
                return Read_specified_row(last_row_number);
            }

            public String Read_last_row_as_csv(String sheetName, ICSVRecord csvRecord)
            {
                csvRecord.Read_from_spreadsheet_row(Read_last_row(sheetName));
                return csvRecord.To_csv();
            }
        }
    }
}
