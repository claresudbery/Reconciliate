using Interfaces;
using Microsoft.Office.Interop.Excel;

namespace ExcelLibrary
{
    public class ExcelRange : ICellSet
    {
        private Range _excel_cells;

        public ExcelRange(Range excelCells)
        {
            _excel_cells = excelCells;
        }

        public void Populate_cell(int rowNumber, int columnNumber, object cellContent)
        {
            _excel_cells[rowNumber, columnNumber] = cellContent;
        }

        public object Get_cell(int rowNumber, int columnNumber)
        {
            return _excel_cells[rowNumber, columnNumber];
        }
    }
}