using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets.FakeSpreadsheetData
{
    internal class FakeRows
    {
        public Dictionary<string, List<ICellRow>> Data { get; } = new Dictionary<string, List<ICellRow>>
        {
            { MainSheetNames.Bank_in,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5, null, null, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10, null, null, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15, null, null, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Divider_text }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.Bank_out,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)5, null, null, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "ABC", "already reconciled", null, (double)10, null, null, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "ABC", "already reconciled", (double)12345, (double)15, null, null, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "BAC", "\"already reconciled\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 4).ToOADate(), null, "x", "ABC", FakeSpreadsheetRepo.FakeMortgageDescription, null, (double)1000, null, null, null, null, new DateTime(2019, 5, 4).ToOADate(), (double)1000, "BAC", $"\"{FakeSpreadsheetRepo.FakeMortgageDescription}\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 12).ToOADate(), null, "x", "ABC", ReconConsts.Cred_card1_dd_description, null, (double)500, null, null, null, null, new DateTime(2019, 5, 12).ToOADate(), (double)500, "BAC", $"\"{ReconConsts.Cred_card1_dd_description}\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 13).ToOADate(), null, "x", "ABC", ReconConsts.Cred_card2_dd_description, null, (double)1500, null, null, null, null, new DateTime(2019, 5, 13).ToOADate(), (double)1500, "BAC", $"\"{ReconConsts.Cred_card2_dd_description}\"" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Divider_text }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)5.05, null, "ABC", "left over from previous reconciliation" })
                }},
            { MainSheetNames.Cred_card1,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)0, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)0, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)0, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Divider_text }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation", (double)0 })
                }},
            { MainSheetNames.Cred_card2,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)0, "x", "already reconciled", (double)5, null, null, new DateTime(2019, 5, 1).ToOADate(), (double)5, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)0, "x", "already reconciled", (double)10, null, null, new DateTime(2019, 5, 2).ToOADate(), (double)10, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)0, "x", "already reconciled", (double)15, null, null, new DateTime(2019, 5, 3).ToOADate(), (double)15, "ALREADY RECONCILED" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Divider_text }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 31).ToOADate(), (double)10.00, null, "left over from previous reconciliation", (double)0 })
                }},
            { MainSheetNames.Expected_in,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, Codes.Code001, (double)05, new DateTime(2019, 5, 1).ToOADate(), null, "already reconciled" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, Codes.Code002, (double)10, new DateTime(2019, 5, 2).ToOADate(), (double)25, "already reconciled" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, Codes.Code003, (double)15, new DateTime(2019, 5, 3).ToOADate(), (double)25, "already reconciled" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Divider_text }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 6, 1).ToOADate(), (double)20, Codes.Expenses, null, null, null, "first expense" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 6, 2).ToOADate(), (double)25, Codes.Expenses, null, null, null, "second expense" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 6, 3).ToOADate(), (double)30, Codes.Expenses, null, null, null, "third expense" })
                }},
            { MainSheetNames.Expected_out,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code001, (double)686.72, null, "DESCRIPTION 001" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, null, null, "words words words" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code002, (double)72.62, null, "DESCRIPTION 002" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code003, (double)47.26, null, "NOTES 003" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, null, null, "words words words" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, (double)534.00, null, "NOTES 004" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, (double)70.31, null, "NOTES 005" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code004, (double)1348.16, null, "NOTES 006" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, (double)168.21, null, "NOTES 007" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code005, (double)428.02, null, "NOTES 008" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, "*****", "*****", "***** WORDS WORDS WORDS" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code006, (double)10, null, "NOTES 010" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code007, (double)102.88, null, "NOTES 009" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, "*****", "*****", "***** WORDS WORDS WORDS" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code008, (double)1799.22, null, "NOTES 011" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code009, (double)499.82, null, "NOTES 012" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code010, (double)1265.22, null, "NOTES 013" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code011, (double)2633.33, null, "NOTES 014" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, null, (double)2633.33, "some more writing about some stuff" })
                }},
            { MainSheetNames.Cred_card3,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), null, "x", "already reconciled", (double)5 }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), null, "x", "already reconciled", (double)10 }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), null, "x", "already reconciled", (double)15 }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 6, 1).ToOADate(), (double)20.00, null, "new transaction" })
                }},
            { MainSheetNames.Savings,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 1).ToOADate(), (double)100, (double)100 }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 2).ToOADate(), (double)100, (double)200 }),
                    new FakeCellRow().With_fake_data(new List<object> { new DateTime(2019, 5, 3).ToOADate(), (double)100, (double)300 }),
                }},
            { MainSheetNames.Budget_in,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code001, new DateTime(2019, 6, 1).ToOADate(), (double)21.76, null, "POS", "Bank monthly incoming transaction 01" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code002, new DateTime(2019, 6, 2).ToOADate(), (double)15.60, null, "PCL", "Bank monthly incoming transaction 02" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code003, new DateTime(2019, 6, 3).ToOADate(), (double)54.97, null, "POS", "Bank monthly incoming transaction 03" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Total })
                }},
            { MainSheetNames.Budget_out,
                new List<ICellRow> {
                    new FakeCellRow().With_fake_data(new List<object> { "Headers" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Expenses }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code010, new DateTime(1900, 1, 1).ToOADate(), (double)5, null, "POS",  "Monthly expense 001" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code011, new DateTime(1900, 1, 1).ToOADate(), (double)10, null, "PCL", "Monthly expense 002" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code012, new DateTime(1900, 1, 1).ToOADate(), (double)15, null, "POS", "Monthly expense 003" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Expenses_total, (double)45 }),
                    new FakeCellRow().With_fake_data(new List<object> { null }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Sod_ds }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code013, new DateTime(2019, 6, 1).ToOADate(), (double)35, null, "POS", "Bank monthly outgoing transaction 01" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code014, new DateTime(2019, 6, 2).ToOADate(), (double)45, null, "PCL", "Bank monthly outgoing transaction 02" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code015, new DateTime(2019, 6, 3).ToOADate(), (double)55, null, "POS", "Bank monthly outgoing transaction 03" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code042, new DateTime(2019, 6, 4).ToOADate(), (double)1000, null, "POS", FakeSpreadsheetRepo.FakeMortgageDescription }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Cred_card1 }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code016, new DateTime(2019, 6, 1).ToOADate(), (double)65, null, "CredCard1 monthly transaction 01" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code017, new DateTime(2019, 6, 2).ToOADate(), (double)75, null, "CredCard1 monthly transaction 02" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code018, new DateTime(2019, 6, 3).ToOADate(), (double)85, null, "CredCard1 monthly transaction 03" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Cred_card2 }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code019, new DateTime(2019, 6, 1).ToOADate(), (double)95, null,  "CredCard2 monthly transaction 01" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code020, new DateTime(2019, 6, 2).ToOADate(), (double)105, null, "CredCard2 monthly transaction 02" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code021, new DateTime(2019, 6, 3).ToOADate(), (double)115, null, "CredCard2 monthly transaction 03" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Sodd_total, (double)1675 }),
                    new FakeCellRow().With_fake_data(new List<object> { null }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Annual_sod_ds }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code022, new DateTime(2019, 1, 1).ToOADate(), (double)125, null, "POS", "Annual budgeted amount 001" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code023, new DateTime(2019, 7, 1).ToOADate(), (double)135, null, "PCL", "Annual budgeted amount 002" }),
                    new FakeCellRow().With_fake_data(new List<object> { Codes.Code024, new DateTime(2019, 11, 1).ToOADate(), (double)145, null, "POS","Annual budgeted amount 003" }),
                    new FakeCellRow().With_fake_data(new List<object> { null, Dividers.Annual_total, (double)405 })
                }}
        };
    }
}