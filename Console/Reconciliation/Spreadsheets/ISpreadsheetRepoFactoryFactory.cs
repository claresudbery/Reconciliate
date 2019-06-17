using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal interface ISpreadsheetRepoFactoryFactory
    {
        ISpreadsheetRepoFactory Get_factory(string spreadsheetFileNameAndPath);
    }
}