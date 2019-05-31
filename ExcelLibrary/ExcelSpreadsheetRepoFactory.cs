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

        public ISpreadsheetRepo CreateSpreadsheetRepo()
        {
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_spreadsheetFileNameAndPath);
            return _excelSpreadsheet;
        }

        public void DisposeOfSpreadsheetRepo()
        {
            if (_excelSpreadsheet != null)
            {
                _excelSpreadsheet.Dispose();
            }
        }
    }
}