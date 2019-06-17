using Interfaces;

namespace ExcelLibrary
{
    public class ExcelSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private ExcelSpreadsheetRepo _excel_spreadsheet;
        private readonly string _spreadsheet_file_name_and_path;

        public ExcelSpreadsheetRepoFactory(string spreadsheet_file_name_and_path)
        {
            _spreadsheet_file_name_and_path = spreadsheet_file_name_and_path;
        }

        public ISpreadsheetRepo Create_spreadsheet_repo()
        {
            _excel_spreadsheet = new ExcelSpreadsheetRepo(_spreadsheet_file_name_and_path);
            return _excel_spreadsheet;
        }

        public void Dispose_of_spreadsheet_repo()
        {
            if (_excel_spreadsheet != null)
            {
                _excel_spreadsheet.Dispose();
            }
        }
    }
}