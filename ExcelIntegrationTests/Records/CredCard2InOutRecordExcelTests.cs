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
        public void Will_populate_cred_card2_in_out_record_cells()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 1),
                Unreconciled_amount = 22.48,
                Description = "New description which will overwrite what's normally there.",
                Reconciled_amount = 661234.56
            };
            var cells = _spreadsheet.Current_cells("CredCard");
            var last_row_number = _spreadsheet.Last_row_number("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row("CredCard"));

            // Act 
            cred_card2_in_out_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row("CredCard");

            // Assert
            Assert.AreEqual(cred_card2_in_out_record.Date, DateTime.FromOADate((double)new_row.Read_cell(0)));
            Assert.AreEqual(cred_card2_in_out_record.Unreconciled_amount, (Double)new_row.Read_cell(1));
            Assert.AreEqual(cred_card2_in_out_record.Description, (String)new_row.Read_cell(3));
            Assert.AreEqual(cred_card2_in_out_record.Reconciled_amount, (Double)new_row.Read_cell(4));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_divider_text_in_second_cell_when_cred_card2_in_out_record_is_divider()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Divider = true};
            var cells = _spreadsheet.Current_cells("CredCard");
            var last_row_number = _spreadsheet.Last_row_number("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row("CredCard"));

            // Act 
            cred_card2_in_out_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row("CredCard");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)new_row.Read_cell(1));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_x_in_cell_when_cred_card2_in_out_record_is_matched()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Matched = true};
            var cells = _spreadsheet.Current_cells("CredCard");
            var last_row_number = _spreadsheet.Last_row_number("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row("CredCard"));

            // Act 
            cred_card2_in_out_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row("CredCard");

            // Assert
            Assert.AreEqual("x", (String)new_row.Read_cell(2));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_null_in_relevant_cell_when_cred_card2_in_out_record_is_not_matched()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord {Matched = false};
            var cells = _spreadsheet.Current_cells("CredCard");
            var last_row_number = _spreadsheet.Last_row_number("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row("CredCard"));

            // Act 
            cred_card2_in_out_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row("CredCard");

            // Assert
            Assert.AreEqual(null, (String)new_row.Read_cell(2));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_zero_amounts_as_empty_cred_card2_in_out_record_cells()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Unreconciled_amount = 0,
                Reconciled_amount = 0
            };
            var cells = _spreadsheet.Current_cells("CredCard");
            var last_row_number = _spreadsheet.Last_row_number("CredCard");
            var previous_record = new CredCard2InOutRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row("CredCard"));

            // Act 
            cred_card2_in_out_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row("CredCard");

            // Assert
            Assert.AreEqual(null, new_row.Read_cell(1));
            Assert.AreEqual(null, new_row.Read_cell(4));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_cred_card2_in_out_record_cells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 27);
            Double expected_unreconciled_amount = 5.10;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 10567.89;
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.Read_last_row("CredCard");

            // Act 
            cred_card2_in_out_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card2_in_out_record.Unreconciled_amount);
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card2_in_out_record.Reconciled_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_cope_with_cred_card2_in_out_cells_when_not_all_cells_are_populated()
        {
            // Arrange
            String expected_description = "SOMETHING EXCITING & SOMEWHERE COOL";
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                null,
                null,
                expected_description
            };

            // Act 
            cred_card2_in_out_record.Read_from_spreadsheet_row(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_cred_card2_in_out_record_cells_when_there_is_a_null_reconciled_amount()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 4, day: 25);
            Double expected_unreconciled_amount = 4.90;
            String expected_description = "pintipoplication";
            Double expected_reconciled_amount = 0;
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.Read_specified_row("CredCard", 8);

            // Act 
            cred_card2_in_out_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, cred_card2_in_out_record.Unreconciled_amount);
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
            Assert.AreEqual(expected_reconciled_amount, cred_card2_in_out_record.Reconciled_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_source_line_when_reading_from_cred_card2_in_out_record_cells()
        {
            // Arrange
            String expected_source_line = String.Format("27/04/2018^£5.10^^pintipoplication^\"£10,567.89\"^");
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var cells = _spreadsheet.Read_last_row("CredCard");

            // Act 
            cred_card2_in_out_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_source_line, cred_card2_in_out_record.Source_line);
        }
    }
}
