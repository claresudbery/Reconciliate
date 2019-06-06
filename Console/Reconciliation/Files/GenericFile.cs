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

        public void RefreshFileContents()
        {
            File.PopulateRecordsFromOriginalFileLoad();
        }

        public void FilterForPositiveRecordsOnly()
        {
            File.FilterForPositiveRecordsOnly();
        }

        public void FilterForNegativeRecordsOnly()
        {
            File.FilterForNegativeRecordsOnly();
        }

        public void SwapSignsOfAllAmounts()
        {
            File.SwapSignsOfAllAmounts();
        }
    }
}