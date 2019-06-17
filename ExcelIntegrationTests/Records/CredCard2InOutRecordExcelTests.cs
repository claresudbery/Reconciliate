using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelLibrary;
using Interfaces.Constants;
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
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 1),
                UnreconciledAmount = 22.48,
                Description = "New description which will overwrite what's normally there.",
                ReconciledAmount = 661234.56
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(cred_card2_in_out_record.Date, DateTime.FromOADate((double)new_row.ReadCell(0)));
            Assert.AreEqual(cred_card2_in_out_record.UnreconciledAmount, (Double)new_row.ReadCell(1));
            Assert.AreEqual(cred_card2_in_out_record.Description, (String)new_row.ReadCell(3));
            Assert.AreEqual(cred_card2_in_out_record.ReconciledAmount, (Double)new_row.ReadCell(4));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenCredCard2InOutRecordIsDivider()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)new_row.ReadCell(1));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenCredCard2InOutRecordIsMatched()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual("x", (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenCredCard2InOutRecordIsNotMatched()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyCredCard2InOutRecordCells()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                UnreconciledAmount = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("CredCard");
            var last_row_number = _spreadsheet.LastRowNumber("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("CredCard"));

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("CredCard");

            // Assert
            Assert.AreEqual(null, new_row.ReadCell(1));
            Assert.AreEqual(null, new_row.ReadCell(4));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard2InOutRecordCells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 27);
            Double expected_unreconciled_amount = 5.10;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 10567.89;
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            cred_card2_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card2_in_out_record.UnreconciledAmount);
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithCredCard2InOutCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            String expected_description = "SOMETHING EXCITING & SOMEWHERE COOL";
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                expected_description
            };

            // Act 
            cred_card2_in_out_record.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard2InOutRecordCellsWhenThereIsANullReconciledAmount()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 25);
            Double expected_unreconciled_amount = 4.90;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 0;
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("CredCard", 8);

            // Act 
            cred_card2_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card2_in_out_record.UnreconciledAmount);
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromCredCard2InOutRecordCells()
        {
            // Arrange
            String expected_source_line = String.Format("27/04/2018^£5.10^^pintipoplication^\"£10,567.89\"^");
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.ReadLastRow("CredCard");

            // Act 
            cred_card2_in_out_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_source_line, cred_card2_in_out_record.SourceLine);
        }
    }
}
