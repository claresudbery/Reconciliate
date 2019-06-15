using System.Collections.Generic;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class LastRowNumbers
    {
        public Dictionary<string, int> Data { get; } = new Dictionary<string, int>
        {
            {MainSheetNames.BankIn     , 6},
            {MainSheetNames.BankOut    , 9},
            {MainSheetNames.CredCard1  , 6},
            {MainSheetNames.CredCard2  , 6},
            {MainSheetNames.ExpectedIn , 8},
            {MainSheetNames.ExpectedOut, 20},
            {MainSheetNames.CredCard3  , 5},
            {MainSheetNames.Savings    , 4}
        };
    }
}