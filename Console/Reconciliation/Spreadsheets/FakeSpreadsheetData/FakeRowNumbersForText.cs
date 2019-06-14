using System.Collections.Generic;
using Interfaces.Constants;

namespace Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal static class FakeRowNumbersForText
    {
        public static Dictionary<string, Dictionary<string, int>> Data = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.BankOut,
                new Dictionary<string, int> {
                    {"CREDIT CARD 1", 5},
                    {"CREDIT CARD 2", 6}
                }}
        };
    }
}