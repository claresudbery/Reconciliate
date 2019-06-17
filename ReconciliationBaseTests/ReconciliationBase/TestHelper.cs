namespace ReconciliationBaseTests.ReconciliationBase
{
    internal static class TestHelper
    {
        public static string CSVFileLocation = "reconciliation-samples";
        public static string SpreadsheetFileLocation = "spreadsheet-samples";

        public static string RelativeProjectRoot(string currentPath)
        {
            // This is a massive hack, but after a ton of googling and heartache this was the only way I could find!
            string relative_project_root = "/../../../";

            if (currentPath.Contains("netcoreapp"))
            {
                relative_project_root = "/.." + relative_project_root;
            }

            return relative_project_root;
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
    }
}
