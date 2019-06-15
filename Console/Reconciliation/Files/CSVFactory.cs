using Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CSVFileFactory<TRecordType> : ICSVFileFactory<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public ICSVFile<TRecordType> CreateCSVFile(bool loadFile = true, char? overrideSeparator = null, bool orderOnLoad = true)
        {
            var fileIO = new FileIO<TRecordType>(
                new SpreadsheetRepoFactoryFactory()
                    .GetFactory(ReconConsts.MainSpreadsheetPath + "/" + ReconConsts.MainSpreadsheetFileName), 
                "", "");
            var csvFile = new CSVFile<TRecordType>(fileIO);
            csvFile.Load(false);
            return csvFile;
        }
    }
}