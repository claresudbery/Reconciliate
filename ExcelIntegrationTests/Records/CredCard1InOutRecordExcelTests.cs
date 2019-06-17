using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelLibrary;
using Interfaces.Constants;
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
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 1),
                UnreconciledAmount = 22.22,
                Description = "New description which will overwrite what's normally there.",
                ReconciledAmount = 9999.56
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard1InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card1_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(cred_card1_in_out_record.Date, DateTime.FromOADate((double)new_row.ReadCell(0)));
            Assert.AreEqual(cred_card1_in_out_record.UnreconciledAmount, (Double)new_row.ReadCell(1));
            Assert.AreEqual(cred_card1_in_out_record.Description, (String)new_row.ReadCell(3));
            Assert.AreEqual(cred_card1_in_out_record.ReconciledAmount, (Double)new_row.ReadCell(4));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenCredCard1InOutRecordIsDivider()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard1InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card1_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)new_row.ReadCell(1));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenCredCard1InOutRecordIsMatched()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard1InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card1_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual("x", (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenCredCard1InOutRecordIsNotMatched()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard1InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card1_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyCellsForCredCard1InOutRecord()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                UnreconciledAmount = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard1InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card1_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, new_row.ReadCell(1));
            Assert.AreEqual(null, new_row.ReadCell(4));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard1InOutRecordCells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 27);
            Double expected_unreconciled_amount = 5.10;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 10567.89;
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            cred_card1_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card1_in_out_record.UnreconciledAmount);
            Assert.AreEqual(expected_description, cred_card1_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card1_in_out_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithCredCard1InOutCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            String expected_description = "SOMETHING EXCITING & SOMEWHERE COOL";
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                expected_description
            };

            // Act 
            cred_card1_in_out_record.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, cred_card1_in_out_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCellsWhenThereIsANullReconciledAmountOnCredCard1InOutRecord()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 25);
            Double expected_unreconciled_amount = 4.90;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 0;
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("CredCard", 8);

            // Act 
            cred_card1_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card1_in_out_record.UnreconciledAmount);
            Assert.AreEqual(expected_description, cred_card1_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card1_in_out_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromCredCard1InOutRecordCells()
        {
            // Arrange
            String expected_source_line = String.Format("27/04/2018^£5.10^^pintipoplication^\"£10,567.89\"^");
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            cred_card1_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_source_line, cred_card1_in_out_record.SourceLine);
        }
    }
}
