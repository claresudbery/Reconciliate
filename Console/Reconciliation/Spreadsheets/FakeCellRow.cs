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

        public FakeCellRow WithFakeData(List<object> fakeData)
        {
            _fakeCells = fakeData;
            return this;
        }

        public void PopulateCell(int rowNumber, int columnNumber, object cellContent)
        {
        }

        public void DoAThing(object thing)
        {
            throw new System.NotImplementedException();
        }

        public object ReadCell(int columnNumber)
        {
            return _fakeCells[columnNumber];
        }
    }
}