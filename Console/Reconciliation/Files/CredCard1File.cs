using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CredCard1File : IDataFile<CredCard1Record>
    {
        public ICSVFile<CredCard1Record> File { get; set; }

        public CredCard1File(ICSVFile<CredCard1Record> csvFile)
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
            File.SwapSignsOfAllAmounts();
        }
    }
}