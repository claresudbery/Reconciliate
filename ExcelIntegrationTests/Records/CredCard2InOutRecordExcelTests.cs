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
    // These are the tests for CredCard2InOutRecord
    [TestFixture]
    public partial class ExcelRecordTests //(CredCard2InOutRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateCredCard2InOutRecordCells()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 1),
                UnreconciledAmount = 22.48,
                Description = "New description which will overwrite what's normally there.",
                ReconciledAmount = 661234.56
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard2InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(credCard2InOutRecord.Date, DateTime.FromOADate((double)newRow.ReadCell(0)));
            Assert.AreEqual(credCard2InOutRecord.UnreconciledAmount, (Double)newRow.ReadCell(1));
            Assert.AreEqual(credCard2InOutRecord.Description, (String)newRow.ReadCell(3));
            Assert.AreEqual(credCard2InOutRecord.ReconciledAmount, (Double)newRow.ReadCell(4));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenCredCard2InOutRecordIsDivider()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard2InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)newRow.ReadCell(1));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenCredCard2InOutRecordIsMatched()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard2InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual("x", (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenCredCard2InOutRecordIsNotMatched()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard2InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, (String)newRow.ReadCell(2));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyCredCard2InOutRecordCells()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord
            {
                UnreconciledAmount = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var lastRowNumber = _spreadsheet.LastRowNumber("CredCard");
            var previousRecord = new CredCard2InOutRecord();
            previousRecord.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
            var newRow = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, newRow.ReadCell(1));
            Assert.AreEqual(null, newRow.ReadCell(4));

            // Clean up
            previousRecord.PopulateSpreadsheetRow(cells, lastRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard2InOutRecordCells()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 4, day: 27);
            Double expectedUnreconciledAmount = 5.10;
            String expectedDescription = "pintipoplication";
            Double expectedReconciledAmount = 10567.89;
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            credCard2InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard2InOutRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, credCard2InOutRecord.UnreconciledAmount);
            Assert.AreEqual(expectedDescription, credCard2InOutRecord.Description);
            Assert.AreEqual(expectedReconciledAmount, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithCredCard2InOutCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            String expectedDescription = "SOMETHING EXCITING & SOMEWHERE COOL";
            var credCard2InOutRecord = new CredCard2InOutRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                expectedDescription
            };

            // Act 
            credCard2InOutRecord.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expectedDescription, credCard2InOutRecord.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard2InOutRecordCellsWhenThereIsANullReconciledAmount()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 4, day: 25);
            Double expectedUnreconciledAmount = 4.90;
            String expectedDescription = "pintipoplication";
            Double expectedReconciledAmount = 0;
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("CredCard", 8);

            // Act 
            credCard2InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard2InOutRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, credCard2InOutRecord.UnreconciledAmount);
            Assert.AreEqual(expectedDescription, credCard2InOutRecord.Description);
            Assert.AreEqual(expectedReconciledAmount, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromCredCard2InOutRecordCells()
        {
            // Arrange
            String expectedSourceLine = String.Format("27/04/2018^£5.10^^pintipoplication^\"£10,567.89\"^");
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            credCard2InOutRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedSourceLine, credCard2InOutRecord.SourceLine);
        }
    }
}
