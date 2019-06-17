namespace ReconciliationBaseTests.ReconciliationBase
{
    internal static class TestHelper
    {
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
    }
}
