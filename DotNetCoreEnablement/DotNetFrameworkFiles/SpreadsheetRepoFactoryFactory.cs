using ExcelLibrary;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // In the .Net Core version of this file, the factory-factory returns a FakeSpreadsheetFactory.
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory Get_factory(string spreadsheet_file_name_and_path)
        {
            return new ExcelSpreadsheetRepoFactory(spreadsheet_file_name_and_path);
        }
    }
}