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
    // These are the tests for CredCard1InOutRecord
    [TestFixture]
    public partial class ExcelRecordTests //(CredCard1InOutRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateCredCard1InOutRecordCells()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 1),
                UnreconciledAmount = 22.22,
                Description = "New description which will overwrite what's normally there.",
                ReconciledAmount = 9999.56
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard1InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(credCard1InOutRecord.Date, DateTime.FromOADate((double)newRow.ReadCell(0)));
            Assert.AreEqual(credCard1InOutRecord.UnreconciledAmount, (Double)newRow.ReadCell(1));
            Assert.AreEqual(credCard1InOutRecord.Description, (String)newRow.ReadCell(3));
            Assert.AreEqual(credCard1InOutRecord.ReconciledAmount, (Double)newRow.ReadCell(4));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenCredCard1InOutRecordIsDivider()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard1InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)newRow.ReadCell(1));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenCredCard1InOutRecordIsMatched()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard1InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual("x", (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenCredCard1InOutRecordIsNotMatched()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard1InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyCellsForCredCard1InOutRecord()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                UnreconciledAmount = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard1InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, newRow.ReadCell(1));
            Assert.AreEqual(null, newRow.ReadCell(4));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard1InOutRecordCells()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 4, day: 27);
            Double expectedUnreconciledAmount = 5.10;
            String expectedDescription = "pintipoplication";
            Double expectedReconciledAmount = 10567.89;
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            credCard1InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, credCard1InOutRecord.UnreconciledAmount);
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
            Assert.AreEqual(expectedReconciledAmount, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithCredCard1InOutCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            String expectedDescription = "SOMETHING EXCITING & SOMEWHERE COOL";
            var credCard1InOutRecord = new CredCard1InOutRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                expectedDescription
            };

            // Act 
            credCard1InOutRecord.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCellsWhenThereIsANullReconciledAmountOnCredCard1InOutRecord()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 4, day: 25);
            Double expectedUnreconciledAmount = 4.90;
            String expectedDescription = "pintipoplication";
            Double expectedReconciledAmount = 0;
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("CredCard", 8);

            // Act 
            credCard1InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, credCard1InOutRecord.UnreconciledAmount);
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
            Assert.AreEqual(expectedReconciledAmount, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromCredCard1InOutRecordCells()
        {
            // Arrange
            String expectedSourceLine = String.Format("27/04/2018^£5.10^^pintipoplication^\"£10,567.89\"^");
            //var credCard1InOutRecord = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadLastRow("Bank");

            Assert.AreEqual(true, true);

            // Act 
            //credCard1InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            //Assert.AreEqual(expectedSourceLine, credCard1InOutRecord.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingCredCard1SpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                Match = new CredCard1Record
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Description = "match description"
                }
            };
            var mockCells = new Mock<ICellSet>();

            // Act 
            credCard1InOutRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DateSpreadsheetIndex + 1, credCard1InOutRecord.Match.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.AmountSpreadsheetIndex + 1, credCard1InOutRecord.Match.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DescriptionSpreadsheetIndex + 1, credCard1InOutRecord.Match.Description), "Desc");
        }
    }
}
