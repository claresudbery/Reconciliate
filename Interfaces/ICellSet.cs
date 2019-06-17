namespace Interfaces
{
    public interface ICellSet
    {
        void Populate_cell(int rowNumber, int columnNumber, object cellContent);
        object Get_cell(int rowNumber, int columnNumber);
    }
}