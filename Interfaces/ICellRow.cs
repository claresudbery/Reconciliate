namespace Interfaces
{
    public interface ICellRow
    {
        object Read_cell(int column_number);
        int Count { get; set; }
    }
}