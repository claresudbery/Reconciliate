using System.Collections.Generic;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeCellRow : ICellRow
    {
        public int Count {
            get { return _fakeCells.Count; }
            set { }
        }

        private List<object> _fakeCells = new List<object>();

        public FakeCellRow With_fake_data(List<object> fakeData)
        {
            _fakeCells = fakeData;
            return this;
        }

        public void Populate_cell(int rowNumber, int columnNumber, object cellContent)
        {
        }

        public object Read_cell(int columnNumber)
        {
            return _fakeCells[columnNumber];
        }
    }
}