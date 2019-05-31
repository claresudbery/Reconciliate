using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    [TestFixture]
    public partial class ExcelRecordTests
    {
        private readonly string _spreadsheetFileNameAndPath;
        private ExcelSpreadsheetRepo _spreadsheet;

        public ExcelRecordTests()
        {
            string currentPath = TestContext.CurrentContext.TestDirectory;
            _spreadsheetFileNameAndPath = 
                TestHelper.FullyQualifiedSpreadsheetFilePath(currentPath)
                + "/" + "Test-Spreadsheet.xlsx";
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _spreadsheet = new ExcelSpreadsheetRepo(_spreadsheetFileNameAndPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _spreadsheet.Dispose();
        }
    }
}