namespace ConsoleCatchallTests.Reconciliation.TestUtils
{
    internal static class TestHelper
    {
        public static string CSVFileLocation = "reconciliation-samples";
        public static string SpreadsheetFileLocation = "spreadsheet-samples";

        public static string Relative_project_root(string current_path)
        {
            // This is a massive hack, but after a ton of googling and heartache this was the only way I could find!
            string relative_project_root = "/../../../";

            if (current_path.Contains("netcoreapp"))
            {
                relative_project_root = "/.." + relative_project_root;
            }

            return relative_project_root;
        }

        public static string Fully_qualified_folder_path(string current_path, string folder_name)
        {
            return current_path
                   + Relative_project_root(current_path)
                   + folder_name;
        }

        public static string Fully_qualified_csv_file_path(string current_path)
        {
            return Fully_qualified_folder_path(current_path, CSVFileLocation);
        }

        public static string Fully_qualified_spreadsheet_file_path(string current_path)
        {
            return Fully_qualified_folder_path(current_path, SpreadsheetFileLocation);
        }

        public static void Set_correct_date_formatting()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
