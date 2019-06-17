using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeCellSet : ICellSet
    {
        public void Populate_cell(int row_number, int column_number, object cell_content)
        {
        }

        public object Get_cell(int row_number, int column_number)
        {
            return 0;
        }
    }
}