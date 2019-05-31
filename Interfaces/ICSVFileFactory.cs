namespace Interfaces
{
    public interface ICSVFileFactory<TRecordType> where TRecordType : ICSVRecord, new()
    {
        ICSVFile<TRecordType> CreateCSVFile(
            bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true);
    }
}