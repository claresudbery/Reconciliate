using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;

namespace Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal static class FakeRows
    {
        public static Dictionary<string, List<ICellRow>> Data = new Dictionary<string, List<ICellRow>>
        {
            { MainSheetNames.BankIn,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5, null, null, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10, null, null, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15, null, null, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.BankOut,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5, null, null, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10, null, null, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15, null, null, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 4).ToOADate(), null, "x", "ABC", "Mortgage description", null, (double)1000, null, null, null, null, new DateTime(2019, 5, 4).ToOADate(), (double)1000, "BAC", "\"Mortgage description\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 12).ToOADate(), null, "x", "ABC", "CREDIT CARD 1", null, (double)500, null, null, null, null, new DateTime(2019, 5, 12).ToOADate(), (double)500, "BAC", "\"CRED CARD 1\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 13).ToOADate(), null, "x", "ABC", "CREDIT CARD 2", null, (double)1500, null, null, null, null, new DateTime(2019, 5, 13).ToOADate(), (double)1500, "BAC", "\"CRED CARD 2\"" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.CredCard1,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)0, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)0, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)0, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation", (double)0 })
                }},
            { MainSheetNames.CredCard2,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)0, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)0, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)0, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation", (double)0 })
                }},
            { MainSheetNames.ExpectedIn,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "Code001", (double)05, new DateTime(2019, 5, 1).ToOADate(), null, "already reconciled" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "Code002", (double)10, new DateTime(2019, 5, 2).ToOADate(), (double)25, "already reconciled" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "Code003", (double)15, new DateTime(2019, 5, 3).ToOADate(), (double)25, "already reconciled" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 6, 1).ToOADate(), (double)20, Codes.Expenses, null, null, null, "first expense" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 6, 2).ToOADate(), (double)25, Codes.Expenses, null, null, null, "second expense" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 6, 3).ToOADate(), (double)30, Codes.Expenses, null, null, null, "third expense" })
                }},
            { MainSheetNames.ExpectedOut,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code001", (double)686.72, null, "DESCRIPTION 001" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, null, null, "words words words" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code002", (double)72.62, null, "DESCRIPTION 002" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code003", (double)47.26, null, "NOTES 003" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, null, null, "words words words" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, (double)534.00, null, "NOTES 004" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, (double)70.31, null, "NOTES 005" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code004", (double)1348.16, null, "NOTES 006" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, (double)168.21, null, "NOTES 007" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code005", (double)428.02, null, "NOTES 008" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "*****", "*****", "***** WORDS WORDS WORDS" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code006", (double)10, null, "NOTES 010" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code007", (double)102.88, null, "NOTES 009" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "*****", "*****", "***** WORDS WORDS WORDS" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code008", (double)1799.22, null, "NOTES 011" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code009", (double)499.82, null, "NOTES 012" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code010", (double)1265.22, null, "NOTES 013" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code011", (double)2633.33, null, "NOTES 014" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, null, (double)2633.33, "some more writing about some stuff" })
                }},
            { MainSheetNames.CredCard3,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "already reconciled", (double)5 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "already reconciled", (double)10 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "already reconciled", (double)15 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 6, 1).ToOADate(), (double)20.00, null, "new transaction" })
                }},
            { MainSheetNames.Savings,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)100, (double)100 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)100, (double)200 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)100, (double)300 }),
                }},
            { MainSheetNames.BudgetIn,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code001", new DateTime(2019, 6, 1).ToOADate(), (double)21.76, null, "POS", "Bank monthly incoming transaction 01" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code002", new DateTime(2019, 6, 2).ToOADate(), (double)15.60, null, "PCL", "Bank monthly incoming transaction 02" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code003", new DateTime(2019, 6, 3).ToOADate(), (double)54.97, null, "POS", "Bank monthly incoming transaction 03" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "Total" })
                }},
            { MainSheetNames.BudgetOut,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "Expenses" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code010", new DateTime(1900, 1, 1).ToOADate(), (double)5, null, "POS",  "Monthly expense 001" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code011", new DateTime(1900, 1, 1).ToOADate(), (double)10, null, "PCL", "Monthly expense 002" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code012", new DateTime(1900, 1, 1).ToOADate(), (double)15, null, "POS", "Monthly expense 003" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "Expenses Total", (double)45 }),
                    new FakeCellRow().WithFakeData(new List<object> { null }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "SODDs" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code013", new DateTime(2019, 6, 1).ToOADate(), (double)35, null, "POS", "Bank monthly outgoing transaction 01" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code014", new DateTime(2019, 6, 2).ToOADate(), (double)45, null, "PCL", "Bank monthly outgoing transaction 02" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code015", new DateTime(2019, 6, 3).ToOADate(), (double)55, null, "POS", "Bank monthly outgoing transaction 03" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code042", new DateTime(2019, 6, 4).ToOADate(), (double)1000, null, "POS", "Mortgage description" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "CredCard1 cred card" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code016", new DateTime(2019, 6, 1).ToOADate(), (double)65, null, "CredCard1 monthly transaction 01" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code017", new DateTime(2019, 6, 2).ToOADate(), (double)75, null, "CredCard1 monthly transaction 02" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code018", new DateTime(2019, 6, 3).ToOADate(), (double)85, null, "CredCard1 monthly transaction 03" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "CredCard2 cred card" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code019", new DateTime(2019, 6, 1).ToOADate(), (double)95, null,  "CredCard2 monthly transaction 01" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code020", new DateTime(2019, 6, 2).ToOADate(), (double)105, null, "CredCard2 monthly transaction 02" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code021", new DateTime(2019, 6, 3).ToOADate(), (double)115, null, "CredCard2 monthly transaction 03" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "SODDTotal", (double)1675 }),
                    new FakeCellRow().WithFakeData(new List<object> { null }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "AnnualSODDs" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code022", new DateTime(2019, 1, 1).ToOADate(), (double)125, null, "POS", "Annual budgeted amount 001" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code023", new DateTime(2019, 7, 1).ToOADate(), (double)135, null, "PCL", "Annual budgeted amount 002" }),
                    new FakeCellRow().WithFakeData(new List<object> { "Code024", new DateTime(2019, 11, 1).ToOADate(), (double)145, null, "POS","Annual budgeted amount 003" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "AnnualTotal", (double)405 })
                }}
        };
    }
}