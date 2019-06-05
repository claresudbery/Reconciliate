namespace Interfaces
{
    public interface ICellRow
    {
        object ReadCell(int columnNumber);
        int Count { get; set; }
    }
}