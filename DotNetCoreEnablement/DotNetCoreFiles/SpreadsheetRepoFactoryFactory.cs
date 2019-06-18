using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory Get_factory(string spreadsheet_file_name_and_path)
        {
            return new FakeSpreadsheetRepoFactory();
        }
    }
}
