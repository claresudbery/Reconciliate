using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private FakeSpreadsheetRepo _fake_spreadsheet;

        public ISpreadsheetRepo Create_spreadsheet_repo()
        {
            _fake_spreadsheet = new FakeSpreadsheetRepo();
            return _fake_spreadsheet;
        }

        public void Dispose_of_spreadsheet_repo()
        {
            // Not really necessary at the moment as Dispose() does nothing anyway,
            // but leaving this here just in case it ever does do something!
            if (_fake_spreadsheet != null)
            {
                _fake_spreadsheet.Dispose();
            }
        }
    }
}