using Interfaces;

namespace ExcelLibrary
{
    public class ExcelSpreadsheetRepoFactory : ISpreadsheetRepoFactory
    {
        private ExcelSpreadsheetRepo _excelSpreadsheet;
        private readonly string _spreadsheetFileNameAndPath;

        public ExcelSpreadsheetRepoFactory(string spreadsheetFileNameAndPath)
        {
            _spreadsheetFileNameAndPath = spreadsheetFileNameAndPath;
        }

        public ISpreadsheetRepo Create_spreadsheet_repo()
        {
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_spreadsheetFileNameAndPath);
            return _excelSpreadsheet;
        }

        public void Dispose_of_spreadsheet_repo()
        {
            if (_excelSpreadsheet != null)
            {
                _excelSpreadsheet.Dispose();
            }
        }
    }
}