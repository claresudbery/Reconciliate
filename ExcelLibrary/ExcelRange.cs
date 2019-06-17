using Interfaces;
using Microsoft.Office.Interop.Excel;

namespace ExcelLibrary
{
    public class ExcelRange : ICellSet
    {
        private Range _excel_cells;

        public ExcelRange(Range excel_cells)
        {
            _excel_cells = excel_cells;
        }

        public void Populate_cell(int row_number, int column_number, object cell_content)
        {
            _excel_cells[row_number, column_number] = cell_content;
        }

        public object Get_cell(int row_number, int column_number)
        {
            return _excel_cells[row_number, column_number];
        }
    }
}