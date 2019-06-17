using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    [TestFixture]
    public partial class ExcelRecordTests
    {
        private ExcelSpreadsheetRepo _spreadsheet;

        private static string TestSpreadsheetPath()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            return TestHelper.FullyQualifiedSpreadsheetFilePath(current_path)
                   + "/" + "Test-Spreadsheet.xlsx";
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _spreadsheet = new ExcelSpreadsheetRepo(TestSpreadsheetPath());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _spreadsheet.Dispose();
        }
    }
}