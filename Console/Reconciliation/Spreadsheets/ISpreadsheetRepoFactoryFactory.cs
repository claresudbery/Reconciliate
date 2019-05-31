using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal interface ISpreadsheetRepoFactoryFactory
    {
        ISpreadsheetRepoFactory GetFactory(string spreadsheetFileNameAndPath);
    }
}