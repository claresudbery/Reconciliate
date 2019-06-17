using Interfaces;

namespace ConsoleCatchallTests.Reconciliation.TestUtils
{
    internal class MockSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private ISpreadsheetRepo _mock_spreadsheet;

        public MockSpreadsheetRepoFactory(ISpreadsheetRepo mock_spreadsheet)
        {
            _mock_spreadsheet = mock_spreadsheet;
        }

        public ISpreadsheetRepo Create_spreadsheet_repo()
        {
            return _mock_spreadsheet;
        }

        public void Dispose_of_spreadsheet_repo()
        {
            // Probably not necessary for mock spreadsheets, but putting it in just in case!
            if (_mock_spreadsheet != null)
            {
                _mock_spreadsheet.Dispose();
            }
        }
    }
}