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
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15 }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.BankOut,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 12).ToOADate(), null, "x", "ABC", "CREDIT CARD 1", null, (double)500 }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 13).ToOADate(), null, "x", "ABC", "CREDIT CARD 2", null, (double)1500 }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.CredCard1,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation" })
                }},
            { MainSheetNames.CredCard2,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { "Headers" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().WithFakeData(new List<object> { null, "divider" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation" })
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
                }}
        };
    }
}