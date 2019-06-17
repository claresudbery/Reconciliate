namespace Interfaces
{
    public interface ICellSet
    {
        void Populate_cell(int row_number, int column_number, object cell_content);
        object Get_cell(int row_number, int column_number);
    }
}