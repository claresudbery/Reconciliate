using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class ActualBankOutFile : IDataFile<ActualBankRecord>
    {
        public ICSVFile<ActualBankRecord> File { get; set; }

        public ActualBankOutFile(ICSVFile<ActualBankRecord> csvFile)
        {
            File = csvFile;
        }

        public void Load(bool loadFile = true,
            char? overrideSeparator = null,
            bool orderOnLoad = true)
        {
            File.Load(loadFile, overrideSeparator, orderOnLoad);
            RefreshFileContents();
        }

        public void RefreshFileContents()
        {
            File.FilterForNegativeRecordsOnly();
        }
    }
}