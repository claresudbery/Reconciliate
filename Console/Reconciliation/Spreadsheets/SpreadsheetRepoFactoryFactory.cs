using ExcelLibrary;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // In the .Net Core version of this file, the factory-factory returns a FakeSpreadsheetFactory.
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory Get_factory(string spreadsheetFileNameAndPath)
        {
            return new ExcelSpreadsheetRepoFactory(spreadsheetFileNameAndPath);
        }
    }
}