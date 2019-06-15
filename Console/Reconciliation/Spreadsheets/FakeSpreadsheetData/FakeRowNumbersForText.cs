using System.Collections.Generic;
using Interfaces.Constants;

namespace Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class FakeRowNumbersForText
    {
        public Dictionary<string, Dictionary<string, int>> Data { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.BankOut,
                new Dictionary<string, int> {
                    {ReconConsts.CredCard1DdDescription, 6},
                    {ReconConsts.CredCard2DdDescription, 7}
                }}
        };
    }
}