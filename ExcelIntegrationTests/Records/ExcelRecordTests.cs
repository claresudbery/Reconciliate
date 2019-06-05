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
            string currentPath = TestContext.CurrentContext.TestDirectory;
            return TestHelper.FullyQualifiedSpreadsheetFilePath(currentPath)
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