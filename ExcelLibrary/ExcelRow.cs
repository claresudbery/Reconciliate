using System.Collections.Generic;
using Interfaces;

namespace ExcelLibrary
{
    public class ExcelRow : ICellRow
    {
        private List<object> _excel_cells;

        public ExcelRow(List<object> excelCells)
        {
            _excel_cells = excelCells;
        }

        public object Read_cell(int columnNumber)
        {
            return _excel_cells[columnNumber];
        }

        public int Count {
            get { return _excel_cells.Count; }
            set { }
        }
    }
}