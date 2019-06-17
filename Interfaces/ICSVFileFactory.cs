namespace Interfaces
{
    public interface ICSVFileFactory<TRecordType> where TRecordType : ICSVRecord, new()
    {
        ICSVFile<TRecordType> Create_csv_file(
            bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true);
    }
}