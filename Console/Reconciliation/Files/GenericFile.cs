using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class GenericFile<TRecordType> : IDataFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public ICSVFile<TRecordType> File { get; set; }

        public GenericFile(ICSVFile<TRecordType> csv_file)
        {
            File = csv_file;
        }

        public void Load(bool load_file = true,
            char? override_separator = null,
            bool order_on_load = true)
        {
            File.Load(load_file, override_separator, order_on_load);
        }

        public void Refresh_file_contents()
        {
            File.Populate_records_from_original_file_load();
        }
    }
}