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
            { MainSheetNames.ExpectedIn,
                new List<ICellRow> {
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 1).ToOADate(), (double)60, Codes.Expenses, null, null, null, "Expense 001" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 2).ToOADate(), (double)29.24, Codes.Expenses, null, null, null, "Expense 002" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 3).ToOADate(), (double)100, Codes.Expenses, null, null, null, "Expense 003" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 4).ToOADate(), (double)64.08, Codes.Expenses, null, null, null, "Expense 004" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 5).ToOADate(), (double)214, Codes.Expenses, null, null, null, "Expense 005" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 6).ToOADate(), (double)53, Codes.Expenses, null, null, null, "Expense 006" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 7).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 007" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 8).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 008" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 9).ToOADate(), (double)38.99, Codes.Expenses, null, null, null, "Expense 009" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 10).ToOADate(), (double)45, "Codexxx", null, null, null, "Expense 010" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 11).ToOADate(), (double)84.45, "Code yyy", null, null, null, "Expense 011" }),
                    new FakeCellRow().WithFakeData(new List<object> { new DateTime(2018, 10, 12).ToOADate(), (double)63.32, Codes.Expenses, null, null, null, "Expense 012" })
                }},
        };
    }
}