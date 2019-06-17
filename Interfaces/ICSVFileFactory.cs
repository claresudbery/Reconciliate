namespace Interfaces
{
    public interface ICSVFileFactory<TRecordType> where TRecordType : ICSVRecord, new()
    {
        ICSVFile<TRecordType> Create_csv_file(
            bool load_file = true,
            char? override_separator = null,
            bool order_on_load = true);
    }
}