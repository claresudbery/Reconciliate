using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests.TestUtils
{
    internal static class TestHelper
    {
        private static ExcelSpreadsheetRepo _excelSpreadsheet;
        private static int _numSpreadsheetClients = 0;
        private const int ExpectedNumSpreadsheetClients = 2;

        public static string CSVFileLocation = "reconciliation-samples";
        public static string SpreadsheetFileLocation = "spreadsheet-samples";

        public static string Relative_project_root(string currentPath)
        {
            // This is a massive hack, but after a ton of googling and heartache this was the only way I could find!
            string relative_project_root = "/../../../";

            if (currentPath.Contains("netcoreapp"))
            {
                relative_project_root = "/.." + relative_project_root;
            }

            return relative_project_root;
        }

        public static string Fully_qualified_folder_path(string currentPath, string folderName)
        {
            return currentPath
                   + Relative_project_root(currentPath)
                   + folderName;
        }

        public static string Fully_qualified_csv_file_path(string currentPath)
        {
            return Fully_qualified_folder_path(currentPath, CSVFileLocation);
        }

        public static string Fully_qualified_spreadsheet_file_path(string currentPath)
        {
            return Fully_qualified_folder_path(currentPath, SpreadsheetFileLocation);
        }

        public static void Set_correct_date_formatting()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }

        private static string Test_spreadsheet_path()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            return Fully_qualified_spreadsheet_file_path(current_path)
                + "/" + "Test-Spreadsheet.xlsx";
        }

        public static ExcelSpreadsheetRepo One_time_spreadsheet_set_up()
        {
            _numSpreadsheetClients++;

            if (_numSpreadsheetClients == 1)
            {
                _excelSpreadsheet = new ExcelSpreadsheetRepo(Test_spreadsheet_path());
            }

            return _excelSpreadsheet;
        }

        // !!! This method only expects to be called twice:
        //      Once by ExcelIntegrationTests.OneTimeTearDown and once by ExcelRecordTests.OneTimeTearDown.
        // If any other clients are added then you need to increment ExpectedNumSpreadsheetClients!!
        public static void One_time_spreadsheet_tear_down()
        {
            if (_numSpreadsheetClients == ExpectedNumSpreadsheetClients)
            {
                _excelSpreadsheet.Dispose();
            }
        }
    }
}
