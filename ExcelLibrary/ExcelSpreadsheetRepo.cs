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
        private readonly HiddenExcelSpreadsheet _inner_spreadsheet;

        public ExcelSpreadsheetRepo(string spreadsheet_file_name_and_path)
        {
            _inner_spreadsheet = new HiddenExcelSpreadsheet(spreadsheet_file_name_and_path);
        }

        private HiddenExcelSpreadsheet Inner_spreadsheet
        {
            get
            {
                if (!_inner_spreadsheet.File_available)
                {
                    throw new Exception("Spreadsheet not available (do you have it open?)");
                }
                return _inner_spreadsheet;
            }
        }

        public void Dispose()
        {
            _inner_spreadsheet.Dispose();
        }

        public ICellSet Current_cells(String sheet_name)
        {
            return new ExcelRange(Inner_spreadsheet.Current_cells(sheet_name));
        }

        public int Last_row_number(String sheet_name)
        {
            return Inner_spreadsheet.Last_row_number(sheet_name);
        }

        public int Find_first_empty_row_in_column(string sheet_name, int column_number)
        {
            return Inner_spreadsheet.Find_first_empty_row_in_column(sheet_name, column_number);
        }

        public void Append_csv_record(String sheet_name, ICSVRecord csv_record)
        {
            Inner_spreadsheet.Append_csv_record(sheet_name, csv_record);
        }

        public void Remove_last_row(String sheet_name)
        {
            Inner_spreadsheet.Remove_last_row(sheet_name);
        }

        public ICellRow Read_specified_row(String sheet_name, int row_number)
        {
            return Inner_spreadsheet.Read_specified_row(sheet_name, row_number);
        }

        public ICellRow Read_specified_row(String sheet_name, int row_number, int start_column, int end_column)
        {
            return Inner_spreadsheet.Read_specified_row(sheet_name, row_number, start_column, end_column);
        }

        public ICellRow Read_specified_row(int row_number)
        {
            return Inner_spreadsheet.Read_specified_row(row_number);
        }

        public void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file) where TRecordType : ICSVRecord, new()
        {
            Inner_spreadsheet.Append_csv_file(sheet_name, csv_file);
        }

        public void Delete_specified_rows(String sheet_name, int first_row_number, int last_row_number)
        {
            Inner_spreadsheet.Delete_specified_rows(sheet_name, first_row_number, last_row_number);
        }

        public List<TRecordType> Get_rows_as_records<TRecordType>(
            string sheet_name, 
            int first_row_number, 
            int last_row_number, 
            int first_column_number,
            int last_column_number) where TRecordType : ICSVRecord, new()
        {
            return Inner_spreadsheet.Get_rows_as_records<TRecordType>(
                sheet_name,
                first_row_number,
                last_row_number,
                first_column_number,
                last_column_number);
        }

        public int Find_row_number_of_last_row_containing_cell(
            string sheet_name, 
            string target_cell_text, 
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
        {
            return Inner_spreadsheet.Find_row_number_of_last_row_containing_cell(
                sheet_name,
                target_cell_text,
                expected_column_number);
        }

        public int Find_row_number_of_last_row_with_cell_containing_text(string sheet_name, string target_sub_text, List<int> expected_column_numbers)
        {
            return Inner_spreadsheet.Find_row_number_of_last_row_with_cell_containing_text(
                sheet_name,
                target_sub_text,
                expected_column_numbers);
        }

        public double Get_amount(string sheet_name, string amount_code, int amount_column = SpreadsheetConsts.DefaultAmountColumn)
        {
            return Inner_spreadsheet.Get_amount(sheet_name, amount_code, amount_column);
        }

        public void Update_date(string sheet_name, int date_row, int date_column, DateTime new_date)
        {
            Inner_spreadsheet.Update_date(sheet_name, date_row, date_column, new_date);
        }

        public void Update_text(string sheet_name, int text_row, int text_column, string new_text)
        {
            Inner_spreadsheet.Update_text(sheet_name, text_row, text_column, new_text);
        }

        public void Update_amount(string sheet_name, int amount_row, int amount_column, double new_amount)
        {
            Inner_spreadsheet.Update_amount(sheet_name, amount_row, amount_column, new_amount);
        }

        public void Update_amount(
            string sheet_name, 
            string amount_code, 
            double new_amount, 
            int amount_column = SpreadsheetConsts.DefaultAmountColumn, 
            int code_column = SpreadsheetConsts.DefaultCodeColumn)
        {
            Inner_spreadsheet.Update_amount(sheet_name, amount_code, new_amount, amount_column, code_column);
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
            Inner_spreadsheet.Update_amount_and_text(sheet_name, amount_code, new_amount, new_text, amount_column, text_column, code_column);
        }

        public DateTime Get_date(string sheet_name, int row, int column)
        {
            return Inner_spreadsheet.Get_date(sheet_name, row, column);
        }

        public double Get_amount(string sheet_name, int row, int column)
        {
            return Inner_spreadsheet.Get_amount(sheet_name, row, column);
        }

        public string Get_text(string sheet_name, int row, int column)
        {
            return Inner_spreadsheet.Get_text(sheet_name, row, column);
        }

        public int Find_row_number_of_first_row_containing_cell(
            string sheet_name, 
            string target_cell_text, 
            int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
        {
            return Inner_spreadsheet.Find_row_number_of_first_row_containing_cell(
                sheet_name, 
                target_cell_text,
                expected_column_number);
        }

        public void Insert_new_row(string sheet_name, int new_row_number, Dictionary<int, object> cell_values)
        {
            Inner_spreadsheet.Insert_new_row(sheet_name, new_row_number, cell_values);
        }

        public void Append_new_row(string sheet_name, Dictionary<int, object> cell_values)
        {
            Inner_spreadsheet.Append_new_row(sheet_name, cell_values);
        }

        public ICellRow Read_last_row(string sheet_name)
        {
            return Inner_spreadsheet.Read_last_row(sheet_name);
        }

        public string Read_last_row_as_csv(string sheet_name, ICSVRecord csv_record)
        {
            return Inner_spreadsheet.Read_last_row_as_csv(sheet_name, csv_record);
        }

        private class HiddenExcelSpreadsheet : IDisposable
        {
            private String _spreadsheet_file_name_and_path;
            private Workbooks _workbooks = null;
            private Workbook _workbook = null;
            private Sheets _worksheets = null;
            private List<Worksheet> _opened_worksheets = new List<Worksheet>();
            private Worksheet _current_worksheet = null;
            private Application _application = null;
            private String _current_sheet_name = String.Empty;
            private bool _file_available = false;

            public bool File_available
            {
                get { return _file_available; }
            }

            public HiddenExcelSpreadsheet(String spreadsheet_file_name_and_path)
            {
                _spreadsheet_file_name_and_path = spreadsheet_file_name_and_path;
                _file_available = false;
                if (Is_file_available(_spreadsheet_file_name_and_path))
                {
                    _file_available = true;
                    _application = new Application();
                    _application.Visible = false;
                    _workbooks = _application.Workbooks;
                    _workbook = _workbooks.Open(_spreadsheet_file_name_and_path);
                    _worksheets = _workbook.Sheets;
                }
            }

            public void Dispose()
            {
                if (_file_available)
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
                    foreach (var sheet in _opened_worksheets)
                    {
                        while (Marshal.ReleaseComObject(sheet) != 0)
                        {
                        }
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            private bool Is_file_available(String file_name_and_path)
            {
                bool result = false;
                try
                {
                    using (Stream stream = new FileStream(file_name_and_path, FileMode.Open))
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

            public Range Current_cells(String sheet_name)
            {
                Open_sheet(sheet_name);
                return _current_worksheet.Cells;
            }

            private void Open_sheet(String sheet_name)
            {
                _current_worksheet = (Worksheet) _worksheets[sheet_name];
                if (!_opened_worksheets.Contains(_current_worksheet))
                {
                    _opened_worksheets.Add(_current_worksheet);
                }
            }

            public void Append_csv_record(String sheet_name, ICSVRecord csv_record)
            {
                Open_sheet(sheet_name);
                var new_row_number = Last_row_number(sheet_name) + 1;
                csv_record.Populate_spreadsheet_row(new ExcelRange(_current_worksheet.Cells), new_row_number);

                _workbook.Save();
            }

            public void Append_new_row(string sheet_name, Dictionary<int, object> cell_values)
            {
                Open_sheet(sheet_name);
                var new_row_number = Last_row_number(sheet_name) + 1;

                foreach (var cell_value in cell_values)
                {
                    _current_worksheet.Cells[new_row_number, cell_value.Key] = cell_value.Value;
                }

                _workbook.Save();
            }

            public void Remove_last_row(String sheet_name)
            {
                Open_sheet(sheet_name);
                var last_row_number = Last_row_number(sheet_name);

                var used_range = _current_worksheet.UsedRange;
                var used_columns = used_range.Columns;
                var num_used_columns = used_columns.Count;

                for (int index = 1; index <= num_used_columns; index++)
                {
                    _current_worksheet.Cells[last_row_number, index] = String.Empty;
                }

                _workbook.Save();
            }

            public int Last_row_number(String sheet_name)
            {
                Open_sheet(sheet_name);
                var cells = _current_worksheet.Cells;
                var special_cells = cells.SpecialCells(XlCellType.xlCellTypeLastCell);
                return special_cells.Row;
            }

            public int Find_first_empty_row_in_column(string sheet_name, int column_number)
            {
                Open_sheet(sheet_name);

                var used_range = _current_worksheet.UsedRange;
                Range first_cell_in_column = used_range.Cells[1, column_number] as Range;
                Range last_full_cell_in_column = first_cell_in_column.End[XlDirection.xlDown];

                return last_full_cell_in_column.Row + 1;
            }

            public ICellRow Read_specified_row(String sheet_name, int row_number)
            {
                Open_sheet(sheet_name);
                return Read_specified_row(row_number);
            }

            public ICellRow Read_specified_row(int row_number)
            {
                var values = new List<object>();
                var used_range = _current_worksheet.UsedRange;
                var used_columns = used_range.Columns;
                var num_used_columns = used_columns.Count;

                for (int column_count = 1; column_count <= num_used_columns; column_count++)
                {
                    values.Add((used_range.Cells[row_number, column_count] as Range).Value2);
                }

                return new ExcelRow(values);
            }

            public ICellRow Read_specified_row(String sheet_name, int row_number, int start_column, int end_column)
            {
                Open_sheet(sheet_name);
                return Read_specified_row(row_number, start_column, end_column);
            }

            private ICellRow Read_specified_row(int row_number, int start_column, int end_column)
            {
                var values = new List<object>();
                var used_range = _current_worksheet.UsedRange;

                for (int column_count = start_column; column_count <= end_column; column_count++)
                {
                    values.Add((used_range.Cells[row_number, column_count] as Range).Value2);
                }

                return new ExcelRow(values);
            }

            private Object Read_specified_cell(String sheet_name, int row, int column)
            {
                Open_sheet(sheet_name);
                var used_range = _current_worksheet.UsedRange;
                var cell_value = (used_range.Cells[row, column] as Range)?.Value2;
                return cell_value;
            }

            public void Append_csv_file<TRecordType>(string sheet_name, ICSVFile<TRecordType> csv_file)
                where TRecordType : ICSVRecord, new()
            {
                Open_sheet(sheet_name);
                var new_row_number = Last_row_number(sheet_name) + 1;
                List<TRecordType> ordered_records = csv_file.Records_ordered_for_spreadsheet();

                foreach (var csv_record in ordered_records)
                {
                    csv_record.Populate_spreadsheet_row(new ExcelRange(_current_worksheet.Cells), new_row_number);
                    new_row_number++;
                }

                _workbook.Save();
            }

            public int Find_row_number_of_last_row_containing_cell(
                string sheet_name,
                string target_cell_text,
                int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
            {
                Open_sheet(sheet_name);
                Range cell_containing_last_target_text = Find_last_cell_containing_text(target_cell_text);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, target_cell_text,
                        _current_worksheet.Name));
                }
                if (cell_containing_last_target_text.Column != expected_column_number)
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, target_cell_text,
                        _current_worksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            public int Find_row_number_of_last_row_with_cell_containing_text(
                string sheet_name, 
                string target_sub_text, 
                List<int> expected_column_numbers)
            {
                Open_sheet(sheet_name);
                Range cell_containing_last_target_text = Find_last_cell_containing_text(target_sub_text, false);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, target_sub_text,
                        _current_worksheet.Name));
                }
                if (!expected_column_numbers.Contains(cell_containing_last_target_text.Column))
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, target_sub_text,
                        _current_worksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            public int Find_row_number_of_first_row_containing_cell(
                string sheet_name,
                string target_cell_text,
                int expected_column_number = SpreadsheetConsts.DefaultDividerColumn)
            {
                Open_sheet(sheet_name);
                Range cell_containing_last_target_text = Find_first_cell_containing_text(target_cell_text);

                if (cell_containing_last_target_text == null)
                {
                    throw new Exception(String.Format(ReconConsts.MissingCodeInWorksheet, target_cell_text,
                        _current_worksheet.Name));
                }
                if (cell_containing_last_target_text.Column != expected_column_number)
                {
                    throw new Exception(String.Format(ReconConsts.CodeInWrongPlace, target_cell_text,
                        _current_worksheet.Name));
                }

                return cell_containing_last_target_text.Row;
            }

            private Range Find_first_cell_containing_text(string text_to_search_for)
            {
                var used_range = _current_worksheet.UsedRange;
                return used_range.Find(text_to_search_for, Type.Missing,
                    XlFindLookIn.xlValues, XlLookAt.xlWhole,
                    XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                    Type.Missing, Type.Missing);
            }

            private Range Find_last_cell_containing_text(string text_to_search_for, bool full_text_match = true)
            {
                Range current_find = null;
                Range first_find = null;
                Range last_find = null;
                var used_range = _current_worksheet.UsedRange;

                if (full_text_match)
                {
                    current_find = used_range.Find(text_to_search_for, Type.Missing,
                        XlFindLookIn.xlValues, XlLookAt.xlWhole,
                        XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false,
                        Type.Missing, Type.Missing);
                }
                else
                {
                    current_find = used_range.Find(text_to_search_for, Type.Missing,
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

            public void Delete_specified_rows(string sheet_name, int first_row_number, int last_row_number)
            {
                Open_sheet(sheet_name);

                var used_range = _current_worksheet.UsedRange;
                for (int row_count = last_row_number; row_count >= first_row_number; row_count--)
                {
                    var first_cell_in_row = (Range)used_range.Cells[row_count, 1];
                    var row_to_delete = first_cell_in_row.EntireRow;
                    row_to_delete.Delete(Type.Missing);
                }

                _workbook.Save();
            }

            public List<TRecordType> Get_rows_as_records<TRecordType>(
                string sheet_name,
                int first_row_number,
                int last_row_number,
                int first_column_number,
                int last_column_number) where TRecordType : ICSVRecord, new()
            {
                List<TRecordType> monthly_budget_items = new List<TRecordType>();

                for (int row_number = first_row_number; row_number <= last_row_number; row_number++)
                {
                    var csv_record = new TRecordType();
                    csv_record.Read_from_spreadsheet_row(Read_specified_row(
                        sheet_name,
                        row_number,
                        first_column_number,
                        last_column_number));
                    monthly_budget_items.Add(csv_record);
                }

                return monthly_budget_items;
            }

            public double Get_amount(string sheet_name, string amount_code, int amount_column)
            {
                const int codeColumn = 1;
                int row = Find_row_number_of_last_row_containing_cell(sheet_name, amount_code, codeColumn);

                var excel_cell = Read_specified_cell(sheet_name, row, amount_column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheet_name}, amountCode {amount_code}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            private void Update_cell(string sheet_name, int row, int column, Object new_value)
            {
                Open_sheet(sheet_name);
                _current_worksheet.Cells[row, column] = new_value;
                _workbook.Save();
            }

            public void Update_date(string sheet_name, int date_row, int date_column, DateTime new_date)
            {
                Update_cell(sheet_name, date_row, date_column, new_date.ToOADate());
            }

            public void Update_text(string sheet_name, int text_row, int text_column, string new_text)
            {
                Update_cell(sheet_name, text_row, text_column, new_text);
            }

            public void Update_amount(string sheet_name, int amount_row, int amount_column, double new_amount)
            {
                Update_cell(sheet_name, amount_row, amount_column, new_amount);
            }

            public void Update_amount(string sheet_name, string amount_code, double new_amount, int amount_column, int code_column = SpreadsheetConsts.DefaultCodeColumn)
            {
                int row = Find_row_number_of_last_row_containing_cell(sheet_name, amount_code, code_column);
                Update_amount(sheet_name, row, amount_column, new_amount);
            }

            public void Update_amount_and_text(
                string sheet_name, 
                string amount_code, 
                double new_amount, 
                string new_text, 
                int amount_column, 
                int text_column,
                int code_column = SpreadsheetConsts.DefaultCodeColumn)
            {
                int row = Find_row_number_of_last_row_containing_cell(sheet_name, amount_code, code_column);

                Open_sheet(sheet_name);
                _current_worksheet.Cells[row, amount_column] = new_amount;
                _current_worksheet.Cells[row, text_column] = new_text;
                _workbook.Save();
            }

            internal DateTime Get_date(string sheet_name, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheet_name, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheet_name}, row {row}, column {column}.");
                }

                return DateTime.FromOADate((double)excel_cell);
            }

            internal double Get_amount(string sheet_name, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheet_name, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheet_name}, row {row}, column {column}.");
                }

                return Math.Round((Double)excel_cell, 2);
            }

            internal string Get_text(string sheet_name, int row, int column)
            {
                var excel_cell = Read_specified_cell(sheet_name, row, column);

                if (null == excel_cell)
                {
                    throw new Exception(ReconConsts.MissingCell + $"{sheet_name}, row {row}, column {column}.");
                }

                return (string)excel_cell;
            }

            public void Insert_new_row(string sheet_name, int new_row_number, Dictionary<int, object> cell_values)
            {
                Open_sheet(sheet_name);

                var used_range = _current_worksheet.UsedRange;
                var first_cell_in_row = (Range)used_range.Cells[new_row_number, 1];
                var row_to_insert_before = first_cell_in_row.EntireRow;
                row_to_insert_before.Insert(XlInsertShiftDirection.xlShiftDown);

                foreach (var cell_value in cell_values)
                {
                    _current_worksheet.Cells[new_row_number, cell_value.Key] = cell_value.Value;
                }

                _workbook.Save();
            }

            public ICellRow Read_last_row(String sheet_name)
            {
                int last_row_number = Last_row_number(sheet_name);
                return Read_specified_row(last_row_number);
            }

            public String Read_last_row_as_csv(String sheet_name, ICSVRecord csv_record)
            {
                csv_record.Read_from_spreadsheet_row(Read_last_row(sheet_name));
                return csv_record.To_csv();
            }
        }
    }
}
