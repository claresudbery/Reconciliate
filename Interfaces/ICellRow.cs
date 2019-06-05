namespace Interfaces
{
    public interface ICellRow
    {
        void DoAThing(object thing);
        object ReadCell(int columnNumber);
        int Count { get; set; }
    }
}