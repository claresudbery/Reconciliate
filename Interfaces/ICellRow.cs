namespace Interfaces
{
    public interface ICellRow
    {
        object Read_cell(int columnNumber);
        int Count { get; set; }
    }
}