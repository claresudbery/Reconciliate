using System.Collections.Generic;
using Interfaces;

namespace ExcelLibrary
{
    public class ExcelRow : ICellRow
    {
        private List<object> _excelCells;

        public ExcelRow(List<object> excelCells)
        {
            _excelCells = excelCells;
        }

        public object ReadCell(int columnNumber)
        {
            return _excelCells[columnNumber];
        }

        public int Count {
            get { return _excelCells.Count; }
            set { }
        }
    }
}