using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    [TestFixture]
    public partial class ExcelRecordTests
    {
        private ExcelSpreadsheetRepo _spreadsheet;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _spreadsheet = TestHelper.OneTimeSpreadsheetSetUp();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestHelper.OneTimeSpreadsheetTearDown();
        }
    }
}