using System.Collections.Generic;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class FakeRowNumbersForText
    {
        public Dictionary<string, Dictionary<string, int>> Data { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.Bank_out,
                new Dictionary<string, int> {
                    {ReconConsts.Cred_card1_dd_description, 6},
                    {ReconConsts.Cred_card2_dd_description, 7}
                }}
        };
    }
}