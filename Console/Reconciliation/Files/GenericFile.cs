using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class GenericFile<TRecordType> : IDataFile<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public ICSVFile<TRecordType> File { get; set; }

        public GenericFile(ICSVFile<TRecordType> csvFile)
        {
            File = csvFile;
        }

        public void Load(bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true)
        {
            File.Load(loadFile, overrideSeparator, orderOnLoad);
        }

        public void Refresh_file_contents()
        {
            File.Populate_records_from_original_file_load();
        }
    }
}