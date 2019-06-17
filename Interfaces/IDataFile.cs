namespace Interfaces
{
    public interface IDataFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        ICSVFile<TRecordType> File { get; set; }

        void Load(bool load_file = true,
            char? override_separator = null,
            bool order_on_load = true);

        void Refresh_file_contents();
    }
}