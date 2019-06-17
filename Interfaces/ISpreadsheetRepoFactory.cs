namespace Interfaces
{
    public interface ISpreadsheetRepoFactory
    {
        ISpreadsheetRepo Create_spreadsheet_repo();
        void Dispose_of_spreadsheet_repo();
    }
}