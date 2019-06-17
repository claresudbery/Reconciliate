using System.Collections.Generic;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeCellRow : ICellRow
    {
        public int Count {
            get { return _fake_cells.Count; }
            set { }
        }

        private List<object> _fake_cells = new List<object>();

        public FakeCellRow With_fake_data(List<object> fake_data)
        {
            _fake_cells = fake_data;
            return this;
        }

        public void Populate_cell(int row_number, int column_number, object cell_content)
        {
        }

        public object Read_cell(int column_number)
        {
            return _fake_cells[column_number];
        }
    }
}