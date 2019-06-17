using System.Collections.Generic;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class LastRowNumbers
    {
        public Dictionary<string, int> Data { get; } = new Dictionary<string, int>
        {
            {MainSheetNames.Bank_in     , 6},
            {MainSheetNames.Bank_out    , 9},
            {MainSheetNames.Cred_card1  , 6},
            {MainSheetNames.Cred_card2  , 6},
            {MainSheetNames.Expected_in , 8},
            {MainSheetNames.Expected_out, 20},
            {MainSheetNames.Cred_card3  , 5},
            {MainSheetNames.Savings    , 4}
        };
    }
}