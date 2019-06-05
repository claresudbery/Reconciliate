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

        public static string RelativeProjectRoot(string currentPath)
        {
            // This is a massive hack, but after a ton of googling and heartache this was the only way I could find!
            string relativeProjectRoot = "/../../../";

            if (currentPath.Contains("netcoreapp"))
            {
                relativeProjectRoot = "/.." + relativeProjectRoot;
            }

            return relativeProjectRoot;
        }

        public static string FullyQualifiedFolderPath(string currentPath, string folderName)
        {
            return currentPath
                   + RelativeProjectRoot(currentPath)
                   + folderName;
        }

        public static string FullyQualifiedCSVFilePath(string currentPath)
        {
            return FullyQualifiedFolderPath(currentPath, CSVFileLocation);
        }

        public static string FullyQualifiedSpreadsheetFilePath(string currentPath)
        {
            return FullyQualifiedFolderPath(currentPath, SpreadsheetFileLocation);
        }

        public static void SetCorrectDateFormatting()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }

        private static string TestSpreadsheetPath()
        {
            string currentPath = TestContext.CurrentContext.TestDirectory;
            return FullyQualifiedSpreadsheetFilePath(currentPath)
                + "/" + "Test-Spreadsheet.xlsx";
        }

        public static ExcelSpreadsheetRepo OneTimeSpreadsheetSetUp()
        {
            _numSpreadsheetClients++;

            if (_numSpreadsheetClients == 1)
            {
                _excelSpreadsheet = new ExcelSpreadsheetRepo(TestSpreadsheetPath());
            }

            return _excelSpreadsheet;
        }

        // !!! This method only expects to be called twice:
        //      Once by ExcelIntegrationTests.OneTimeTearDown and once by ExcelRecordTests.OneTimeTearDown.
        // If any other clients are added then you need to increment ExpectedNumSpreadsheetClients!!
        public static void OneTimeSpreadsheetTearDown()
        {
            if (_numSpreadsheetClients == ExpectedNumSpreadsheetClients)
            {
                _excelSpreadsheet.Dispose();
            }
        }
    }
}
