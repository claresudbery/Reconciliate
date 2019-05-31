namespace Interfaces
{
    public interface ICellSet
    {
        void PopulateCell(int rowNumber, int columnNumber, object cellContent);
        object GetCell(int rowNumber, int columnNumber);
    }
}