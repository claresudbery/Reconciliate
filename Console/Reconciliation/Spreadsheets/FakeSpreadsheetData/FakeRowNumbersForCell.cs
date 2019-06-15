using System.Collections.Generic;
using Interfaces.Constants;

namespace Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class FakeRowNumbersForCell
    {
        public Dictionary<string, Dictionary<string, int>> Data { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.BankIn,
                new Dictionary<string, int> {
                    {Dividers.DividerText, 5}
                }},
            { MainSheetNames.BankOut,
                new Dictionary<string, int> {
                    {FakeSpreadsheetRepo.FakeMortgageDescription, 5},
                    {Dividers.DividerText, 8},
                }},
            { MainSheetNames.CredCard1,
                new Dictionary<string, int> {
                    {Dividers.DividerText, 5}
                }},
            { MainSheetNames.CredCard2,
                new Dictionary<string, int> {
                    {Dividers.DividerText, 5}
                }},
            { MainSheetNames.ExpectedIn,
                new Dictionary<string, int> {
                    {Dividers.DividerText, 5}
                }},
            { MainSheetNames.BudgetIn,
                new Dictionary<string, int> {
                    {Dividers.Date, 1},
                    {Dividers.Total, 5}
                }},
            { MainSheetNames.BudgetOut,
                new Dictionary<string, int> {
                    {Dividers.SODDs, 8},
                    {Codes.Code042, 12},
                    {Dividers.CredCard1, 13},
                    {Dividers.CredCard2, 17},
                    {Dividers.SODDTotal, 21},
                    {Dividers.AnnualSODDs, 23},
                    {Dividers.AnnualTotal, 27}
                }}
        };
    }
}