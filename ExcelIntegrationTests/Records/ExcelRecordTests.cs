using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    [TestFixture]
    public partial class ExcelRecordTests
    {
        private ExcelSpreadsheetRepo _spreadsheet;

        private static string Test_spreadsheet_path()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            return TestHelper.Fully_qualified_spreadsheet_file_path(current_path)
                   + "/" + "Test-Spreadsheet.xlsx";
        }

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            _spreadsheet = new ExcelSpreadsheetRepo(Test_spreadsheet_path());
        }

        [OneTimeTearDown]
        public void One_time_tear_down()
        {
            _spreadsheet.Dispose();
        }
    }
}