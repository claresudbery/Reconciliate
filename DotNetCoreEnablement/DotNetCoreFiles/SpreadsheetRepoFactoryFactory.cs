using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory GetFactory(string spreadsheetFileNameAndPath)
        {
            return new FakeSpreadsheetRepoFactory();
        }
    }
}