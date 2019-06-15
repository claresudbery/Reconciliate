using Interfaces;

namespace Console.Reconciliation.Spreadsheets
{
    internal class FakeCellSet : ICellSet
    {
        public void PopulateCell(int rowNumber, int columnNumber, object cellContent)
        {
        }

        public object GetCell(int rowNumber, int columnNumber)
        {
            return 0;
        }
    }
}