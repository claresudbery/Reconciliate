using Interfaces;
using Microsoft.Office.Interop.Excel;

namespace ExcelLibrary
{
    public class ExcelRange : ICellSet
    {
        private Range _excelCells;

        public ExcelRange(Range excelCells)
        {
            _excelCells = excelCells;
        }

        public void PopulateCell(int rowNumber, int columnNumber, object cellContent)
        {
            _excelCells[rowNumber, columnNumber] = cellContent;
        }

        public object GetCell(int rowNumber, int columnNumber)
        {
            return _excelCells[rowNumber, columnNumber];
        }
    }
}