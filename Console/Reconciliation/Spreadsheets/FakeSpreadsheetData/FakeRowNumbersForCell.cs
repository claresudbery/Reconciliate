using System.Collections.Generic;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class FakeRowNumbersForCell
    {
        public Dictionary<string, Dictionary<string, int>> Data { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            { MainSheetNames.Bank_in,
                new Dictionary<string, int> {
                    {Dividers.Divider_text, 5}
                }},
            { MainSheetNames.Bank_out,
                new Dictionary<string, int> {
                    {FakeSpreadsheetRepo.FakeMortgageDescription, 5},
                    {Dividers.Divider_text, 8},
                }},
            { MainSheetNames.Cred_card1,
                new Dictionary<string, int> {
                    {Dividers.Divider_text, 5}
                }},
            { MainSheetNames.Cred_card2,
                new Dictionary<string, int> {
                    {Dividers.Divider_text, 5}
                }},
            { MainSheetNames.Expected_in,
                new Dictionary<string, int> {
                    {Dividers.Divider_text, 5}
                }},
            { MainSheetNames.Budget_in,
                new Dictionary<string, int> {
                    {Dividers.Date, 1},
                    {Dividers.Total, 5}
                }},
            { MainSheetNames.Budget_out,
                new Dictionary<string, int> {
                    {Dividers.SODDS, 8},
                    {Codes.Code042, 12},
                    {Dividers.Cred_card1, 13},
                    {Dividers.Cred_card2, 17},
                    {Dividers.SODD_total, 21},
                    {Dividers.Annual_SODDS, 23},
                    {Dividers.Annual_total, 27}
                }}
        };
    }
}