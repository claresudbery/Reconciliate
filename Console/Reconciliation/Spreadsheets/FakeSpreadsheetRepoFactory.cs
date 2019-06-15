using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    internal class FakeSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private FakeSpreadsheetRepo _fakeSpreadsheet;

        public ISpreadsheetRepo CreateSpreadsheetRepo()
        {
            _fakeSpreadsheet = new FakeSpreadsheetRepo();
            return _fakeSpreadsheet;
        }

        public void DisposeOfSpreadsheetRepo()
        {
            // Not really necessary at the moment as Dispose() does nothing anyway,
            // but leaving this here just in case it ever does do something!
            if (_fakeSpreadsheet != null)
            {
                _fakeSpreadsheet.Dispose();
            }
        }
    }
}