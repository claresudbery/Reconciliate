using System.Collections.Generic;
using Interfaces.Constants;

namespace Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal static class FakeRowNumbersForCell
    {
        public static Dictionary<string, Dictionary<string, int>> Data = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.BankIn,
                new Dictionary<string, int> {
                    {"divider", 5}
                }},
            { MainSheetNames.BankOut,
                new Dictionary<string, int> {
                    {"Mortgage description", 5},
                    {"divider", 8},
                }},
            { MainSheetNames.CredCard1,
                new Dictionary<string, int> {
                    {"divider", 5}
                }},
            { MainSheetNames.CredCard2,
                new Dictionary<string, int> {
                    {"divider", 5}
                }},
            { MainSheetNames.ExpectedIn,
                new Dictionary<string, int> {
                    {"divider", 5}
                }},
            { MainSheetNames.BudgetIn,
                new Dictionary<string, int> {
                    {"Total", 5}
                }},
            { MainSheetNames.BudgetOut,
                new Dictionary<string, int> {
                    {"Code042", 12},
                    {"CredCard1 cred card", 13},
                    {"CredCard2 cred card", 17}
                }}
        };
    }
}