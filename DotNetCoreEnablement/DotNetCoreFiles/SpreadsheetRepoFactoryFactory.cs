using Interfaces;

namespace Console.Reconciliation.Spreadsheets
{
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory GetFactory(string spreadsheetFileNameAndPath)
        {
            return new FakeSpreadsheetRepoFactory();
        }
    }
}