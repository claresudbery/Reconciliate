using Interfaces;

namespace Console.Reconciliation.Spreadsheets
{
    internal interface ISpreadsheetRepoFactoryFactory
    {
        ISpreadsheetRepoFactory GetFactory(string spreadsheetFileNameAndPath);
    }
}