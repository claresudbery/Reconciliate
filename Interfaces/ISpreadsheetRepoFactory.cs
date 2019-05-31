namespace Interfaces
{
    public interface ISpreadsheetRepoFactory
    {
        ISpreadsheetRepo CreateSpreadsheetRepo();
        void DisposeOfSpreadsheetRepo();
    }
}