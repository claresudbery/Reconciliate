using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using Interfaces.Constants;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for BankRecord
    [TestFixture]
    public partial class ExcelRecordTests //(BankRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_bank_record_cells()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                Date = new DateTime(year: 2018, month: 7, day: 1),
                Unreconciled_amount = 80.20,
                Type = "Chq",
                Description = "New description which will overwrite what's normally there.",
                Cheque_number = 4234566,
                Reconciled_amount = 6666.66,
            };
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new BankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual(bank_record.Date, DateTime.FromOADate((double)new_row.Read_cell(0)));
            Assert.AreEqual(bank_record.Unreconciled_amount, (Double)new_row.Read_cell(1));
            Assert.AreEqual(bank_record.Type, (String)new_row.Read_cell(3));
            Assert.AreEqual(bank_record.Description, (String)new_row.Read_cell(4));
            Assert.AreEqual(bank_record.Cheque_number, (Double)new_row.Read_cell(5));
            Assert.AreEqual(bank_record.Reconciled_amount, (Double)new_row.Read_cell(6));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_divider_text_in_second_cell_when_bank_record_is_divider()
        {
            // Arrange
            var bank_record = new BankRecord {Divider = true};
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new BankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)new_row.Read_cell(1));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_x_in_cell_when_bank_record_is_matched()
        {
            // Arrange
            var bank_record = new BankRecord {Matched = true};
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new BankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual("x", (String)new_row.Read_cell(2));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_null_in_relevant_cell_when_bank_record_is_not_matched()
        {
            // Arrange
            var bank_record = new BankRecord {Matched = false};
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new BankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual(null, (String)new_row.Read_cell(2));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_zero_amounts_as_empty_bank_record_cells()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                Unreconciled_amount = 0,
                Cheque_number = 0,
                Reconciled_amount = 0
            };
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new BankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual(null, new_row.Read_cell(1));
            Assert.AreEqual(null, new_row.Read_cell(5));
            Assert.AreEqual(null, new_row.Read_cell(6));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_bank_record_cells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 7, day: 2);
            Double expected_unreconciled_amount = 5.20;
            String expected_type = "ABC";
            String expected_description = "description4";
            int expected_cheque_number = 42345;
            Double expected_reconciled_amount = 7567.89;
            var bank_record = new BankRecord();
            var cells = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Act 
            bank_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, bank_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, bank_record.Unreconciled_amount);
            Assert.AreEqual(expected_type, bank_record.Type);
            Assert.AreEqual(expected_description, bank_record.Description);
            Assert.AreEqual(expected_cheque_number, bank_record.Cheque_number);
            Assert.AreEqual(expected_reconciled_amount, bank_record.Reconciled_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_cope_with_bank_cells_when_matched_cell_is_not_populated()
        {
            // Arrange
            String expected_description = "PASTA PLASTER";
            var bank_record = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                "POS",
                expected_description
            };

            // Act 
            bank_record.Read_from_spreadsheet_row(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_cope_with_bank_cells_when_unreconciled_amount_is_not_populated()
        {
            // Arrange
            String expected_description = "PASTA PLASTER";
            var bank_record = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                null,
                "x",
                "POS",
                expected_description
            };

            // Act 
            bank_record.Read_from_spreadsheet_row(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_bank_record_cells_when_values_are_null()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 7, day: 1);
            Double expected_unreconciled_amount = 5.15;
            String expected_type = "ABC";
            String expected_description = "description3";
            int expected_cheque_number = 0;
            Double expected_reconciled_amount = 0;
            var bank_record = new BankRecord();
            var cells = _spreadsheet.Read_specified_row(TestSheetNames.Bank, 6);

            // Act 
            bank_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, bank_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, bank_record.Unreconciled_amount);
            Assert.AreEqual(expected_type, bank_record.Type);
            Assert.AreEqual(expected_description, bank_record.Description);
            Assert.AreEqual(expected_cheque_number, bank_record.Cheque_number);
            Assert.AreEqual(expected_reconciled_amount, bank_record.Reconciled_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_source_line_when_reading_from_bank_record_cells()
        {
            // Arrange
            String expected_source_line = String.Format("02/07/2018^£5.20^^ABC^description4^42345^\"£7,567.89\"^^^");
            var bank_record = new BankRecord();
            var cells = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Act 
            bank_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_source_line, bank_record.OutputSourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_cope_with_bank_record_cells_when_not_all_cells_are_populated()
        {
            // Arrange
            String expected_description = "SOMETHING EXCITING & SOMEWHERE COOL";
            var bank_record = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                null,
                null,
                "POS",
                expected_description
            };

            // Act 
            bank_record.Read_from_spreadsheet_row(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }
    }
}
