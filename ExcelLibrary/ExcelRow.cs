using System.Collections.Generic;
using Interfaces;

namespace ExcelLibrary
{
    public class ExcelRow : ICellRow
    {
        private List<object> _excel_cells;

        public ExcelRow(List<object> excel_cells)
        {
            _excel_cells = excel_cells;
        }

        public object Read_cell(int column_number)
        {
            return _excel_cells[column_number];
        }

        public int Count {
            get { return _excel_cells.Count; }
            set { }
        }
    }
}