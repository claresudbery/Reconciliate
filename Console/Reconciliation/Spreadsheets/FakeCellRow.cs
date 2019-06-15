using System.Collections.Generic;
using Interfaces;

namespace Console.Reconciliation.Spreadsheets
{
    internal class FakeCellRow : ICellRow
    {
        public int Count {
            get { return _fakeCells.Count; }
            set { }
        }

        private List<object> _fakeCells = new List<object>();

        public FakeCellRow WithFakeData(List<object> fakeData)
        {
            _fakeCells = fakeData;
            return this;
        }

        public void PopulateCell(int rowNumber, int columnNumber, object cellContent)
        {
        }

        public object ReadCell(int columnNumber)
        {
            return _fakeCells[columnNumber];
        }
    }
}