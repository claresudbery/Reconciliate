using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeCellSet : ICellSet
    {
        public void Populate_cell(int rowNumber, int columnNumber, object cellContent)
        {
        }

        public object Get_cell(int rowNumber, int columnNumber)
        {
            return 0;
        }
    }
}