using Interfaces;

namespace ConsoleCatchallTests.Reconciliation.TestUtils
{
    internal class MockSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private ISpreadsheetRepo _mockSpreadsheet;

        public MockSpreadsheetRepoFactory(ISpreadsheetRepo mockSpreadsheet)
        {
            _mockSpreadsheet = mockSpreadsheet;
        }

        public ISpreadsheetRepo CreateSpreadsheetRepo()
        {
            return _mockSpreadsheet;
        }

        public void DisposeOfSpreadsheetRepo()
        {
            // Probably not necessary for mock spreadsheets, but putting it in just in case!
            if (_mockSpreadsheet != null)
            {
                _mockSpreadsheet.Dispose();
            }
        }
    }
}