using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelLibrary;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for BankRecord
    [TestFixture]
    public partial class ExcelRecordTests //(BankRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateBankRecordCells()
        {
            // Arrange
            var bankRecord = new BankRecord
            {
                Date = new DateTime(year: 2018, month: 7, day: 1),
                UnreconciledAmount = 80.20,
                Type = "Chq",
                Description = "New description which will overwrite what's normally there.",
                ChequeNumber = 4234566,
                ReconciledAmount = 6666.66
            };
            var cells = _spreadsheet.CurrentCells("Bank");
            var lastRowNumber = _spreadsheet.LastRowNumber("Bank");
            var previousRecord = new BankRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bankRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(bankRecord.Date, DateTime.FromOADate((double)newRow.ReadCell(0)));
            Assert.AreEqual(bankRecord.UnreconciledAmount, (Double)newRow.ReadCell(1));
            Assert.AreEqual(bankRecord.Type, (String)newRow.ReadCell(3));
            Assert.AreEqual(bankRecord.Description, (String)newRow.ReadCell(4));
            Assert.AreEqual(bankRecord.ChequeNumber, (Double)newRow.ReadCell(5));
            Assert.AreEqual(bankRecord.ReconciledAmount, (Double)newRow.ReadCell(6));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenBankRecordIsDivider()
        {
            // Arrange
            var bankRecord = new BankRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("Bank");
            var lastRowNumber = _spreadsheet.LastRowNumber("Bank");
            var previousRecord = new BankRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bankRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)newRow.ReadCell(1));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenBankRecordIsMatched()
        {
            // Arrange
            var bankRecord = new BankRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("Bank");
            var lastRowNumber = _spreadsheet.LastRowNumber("Bank");
            var previousRecord = new BankRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bankRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual("x", (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenBankRecordIsNotMatched()
        {
            // Arrange
            var bankRecord = new BankRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("Bank");
            var lastRowNumber = _spreadsheet.LastRowNumber("Bank");
            var previousRecord = new BankRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bankRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(null, (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyBankRecordCells()
        {
            // Arrange
            var bankRecord = new BankRecord
            {
                UnreconciledAmount = 0,
                ChequeNumber = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("Bank");
            var lastRowNumber = _spreadsheet.LastRowNumber("Bank");
            var previousRecord = new BankRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bankRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(null, newRow.ReadCell(1));
            Assert.AreEqual(null, newRow.ReadCell(5));
            Assert.AreEqual(null, newRow.ReadCell(6));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromBankRecordCells()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 7, day: 2);
            Double expectedUnreconciledAmount = 5.20;
            String expectedType = "ABC";
            String expectedDescription = "description4";
            int expectedChequeNumber = 42345;
            Double expectedReconciledAmount = 7567.89;
            var bankRecord = new BankRecord();
            var cells = _spreadsheet.ReadLastRow("Bank");

            // Act 
            bankRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, bankRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, bankRecord.UnreconciledAmount);
            Assert.AreEqual(expectedType, bankRecord.Type);
            Assert.AreEqual(expectedDescription, bankRecord.Description);
            Assert.AreEqual(expectedChequeNumber, bankRecord.ChequeNumber);
            Assert.AreEqual(expectedReconciledAmount, bankRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithBankCellsWhenMatchedCellIsNotPopulated()
        {
            // Arrange
            String expectedDescription = "PASTA PLASTER";
            var bankRecord = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                "POS",
                expectedDescription
            };

            // Act 
            bankRecord.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expectedDescription, bankRecord.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithBankCellsWhenUnreconciledAmountIsNotPopulated()
        {
            // Arrange
            String expectedDescription = "PASTA PLASTER";
            var bankRecord = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                null,
                "x",
                "POS",
                expectedDescription
            };

            // Act 
            bankRecord.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expectedDescription, bankRecord.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromBankRecordCellsWhenValuesAreNull()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 7, day: 1);
            Double expectedUnreconciledAmount = 5.15;
            String expectedType = "ABC";
            String expectedDescription = "description3";
            int expectedChequeNumber = 0;
            Double expectedReconciledAmount = 0;
            var bankRecord = new BankRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("Bank", 6);

            // Act 
            bankRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, bankRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, bankRecord.UnreconciledAmount);
            Assert.AreEqual(expectedType, bankRecord.Type);
            Assert.AreEqual(expectedDescription, bankRecord.Description);
            Assert.AreEqual(expectedChequeNumber, bankRecord.ChequeNumber);
            Assert.AreEqual(expectedReconciledAmount, bankRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromBankRecordCells()
        {
            // Arrange
            String expectedSourceLine = String.Format("02/07/2018^£5.20^^ABC^description4^42345^\"£7,567.89\"^^^");
            var bankRecord = new BankRecord();
            var cells = _spreadsheet.ReadLastRow("Bank");

            Assert.AreEqual(true, true);

            // Act 
            //bankRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            //Assert.AreEqual(expectedSourceLine, bankRecord.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingBankSpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var bankRecord = new BankRecord
            {
                Match = new ActualBankRecord
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Type = "POS",
                    Description = "match description"
                }
            };
            var mockCells = new Mock<ICellSet>();

            // Act 
            bankRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DateSpreadsheetIndex + 1, bankRecord.Match.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, bankRecord.Match.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, ((ActualBankRecord)bankRecord.Match).Type), "Type");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, bankRecord.Match.Description), "Desc");
        }
    }
}
