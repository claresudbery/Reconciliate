using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Files
{
    internal class CSVFileFactory<TRecordType> : ICSVFileFactory<TRecordType> where TRecordType : ICSVRecord, new()
    {
        public ICSVFile<TRecordType> Create_csv_file(bool loadFile = true, char? overrideSeparator = null, bool orderOnLoad = true)
        {
            var file_io = new FileIO<TRecordType>(
                new SpreadsheetRepoFactoryFactory()
                    .Get_factory(ReconConsts.Main_spreadsheet_path + "/" + ReconConsts.Main_spreadsheet_file_name), 
                "", "");
            var csv_file = new CSVFile<TRecordType>(file_io);
            csv_file.Load(false);
            return csv_file;
        }
    }
}