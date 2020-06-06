using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class ExpectedIncomeRecordTests
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_expected_income_record_cells()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                Unreconciled_amount = 80.20,
                Code = "Acme Expenses",
                Reconciled_amount = 6666.66,
                Date_paid = DateTime.Today.AddDays(1),
                Total_paid = 42.34,
                Description = "description"
            };
            var mock_cells = new Mock<ICellSet>();
            var row = 10;

            // Act 
            expected_income_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.DateIndex + 1, expected_income_record.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.UnreconciledAmountIndex + 1, expected_income_record.Unreconciled_amount), "UnreconciledAmount");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.CodeIndex + 1, expected_income_record.Code), "Code");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.ReconciledAmountIndex + 1, expected_income_record.Reconciled_amount), "ReconciledAmount");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.DatePaidIndex + 1, expected_income_record.Date_paid), "DatePaid");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.TotalPaidIndex + 1, expected_income_record.Total_paid), "TotalPaid");
            mock_cells.Verify(x => x.Populate_cell(row, ExpectedIncomeRecord.DescriptionIndex + 1, expected_income_record.Description), "Description");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_insert_divider_text_in_second_cell_when_expected_income_record_is_divider()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord {Divider = true};
            var row_number = 10;
            var mock_cells = new Mock<ICellSet>();

            // Act 
            expected_income_record.Populate_spreadsheet_row(mock_cells.Object, row_number);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row_number, ExpectedIncomeRecord.DividerIndex + 1, ReconConsts.DividerText));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_expected_income_record_cells()
        {
            // Arrange
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5.20;
            String expected_code = "ABC";
            Double expected_reconciled_amount = 7567.89;
            DateTime expected_date_paid = DateTime.Today.AddDays(1);
            Double expected_total_paid = 42.45;
            String expected_description = "description4";
            var mock_cells = new Mock<ICellRow>();
            mock_cells.Setup(x => x.Count).Returns(20);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DateIndex)).Returns(expected_date.ToOADate());
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expected_unreconciled_amount);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expected_reconciled_amount);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expected_date_paid.ToOADate());
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expected_total_paid);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expected_description);
            var expected_income_record = new ExpectedIncomeRecord();

            // Act 
            expected_income_record.Read_from_spreadsheet_row(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_date, expected_income_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, expected_income_record.Unreconciled_amount);
            Assert.AreEqual(expected_code, expected_income_record.Code);
            Assert.AreEqual(expected_reconciled_amount, expected_income_record.Reconciled_amount);
            Assert.AreEqual(expected_date_paid, expected_income_record.Date_paid);
            Assert.AreEqual(expected_total_paid, expected_income_record.Total_paid);
            Assert.AreEqual(expected_description, expected_income_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_cope_with_expected_in_cells_when_not_all_cells_are_populated()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord();
            var mock_cells = new Mock<ICellRow>();
            var expected_code = "expectedCode";
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);

            // Act 
            expected_income_record.Read_from_spreadsheet_row(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_code, expected_income_record.Code);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_source_line_when_reading_from_expected_income_record_cells()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5.21;
            String expected_code = "ABC";
            Double expected_reconciled_amount = 567.89;
            DateTime expected_date_paid = DateTime.Today.AddDays(1);
            Double expected_total_paid = 42.45;
            String expected_description = "description4";
            var mock_cells = new Mock<ICellRow>();
            mock_cells.Setup(x => x.Count).Returns(20);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DateIndex)).Returns(expected_date.ToOADate());
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expected_unreconciled_amount);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expected_reconciled_amount);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expected_date_paid.ToOADate());
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expected_total_paid);
            mock_cells.Setup(x => x.Read_cell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expected_description);
            String expected_source_line = 
                $"{expected_date.ToShortDateString()}"
                +$",£{expected_unreconciled_amount}"
                + $",{expected_code}"
                + $",£{expected_reconciled_amount}"
                + $",{expected_date_paid.ToShortDateString()}"
                + $",£{expected_total_paid}"
                + $",{expected_description}";
            var expected_income_record = new ExpectedIncomeRecord();

            // Act 
            expected_income_record.Read_from_spreadsheet_row(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_source_line, expected_income_record.OutputSourceLine);
        }

        [Test]
        public void Csv_is_constructed_correctly()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5.21;
            String expected_code = "ABC";
            Double expected_reconciled_amount = 567.89;
            DateTime expected_date_paid = DateTime.Today.AddDays(1);
            Double expected_total_paid = 42.45;
            String expected_description = "description4";
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = expected_date,
                Unreconciled_amount = expected_unreconciled_amount,
                Code = expected_code,
                Reconciled_amount = expected_reconciled_amount,
                Date_paid = expected_date_paid,
                Total_paid = expected_total_paid,
                Description = expected_description
            };
            String expected_source_line =
                $"{expected_date.ToShortDateString()}"
                + $",£{expected_unreconciled_amount}"
                + $",{expected_code}"
                + $",£{expected_reconciled_amount}"
                + $",{expected_date_paid.ToShortDateString()}"
                + $",£{expected_total_paid}"
                + $",\"{expected_description}\"";

            // Act 
            string constructed_csv_line = expected_income_record.To_csv();

            // Assert
            Assert.AreEqual(expected_source_line, constructed_csv_line);
        }

        [Test]
        public void ToCsv_AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5555.21;
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = expected_date,
                Unreconciled_amount = expected_unreconciled_amount,
            };
            String expected_csv_line =
                $"{expected_date.ToShortDateString()}"
                + $",\"£5,555.21\",,,,,";

            // Act 
            string constructed_csv_line = expected_income_record.To_csv();

            // Assert
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void ToCsv_UnpopulatedValuesShouldBeEmptyStrings()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            var expected_income_record = new ExpectedIncomeRecord();
            String expected_csv_line =$",,,,,,";

            // Act 
            string constructed_csv_line = expected_income_record.To_csv();

            // Assert
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void ToCsv_AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var amount = 123.55;
            var amount_with_pound_sign = $"£{amount}";
            var expected_income_record = new ExpectedIncomeRecord
            {
                Unreconciled_amount = amount,
                Reconciled_amount = amount,
                Total_paid = amount,
            };
            String expected_csv_line = $",{amount_with_pound_sign},,{amount_with_pound_sign},,{amount_with_pound_sign},";

            // Act 
            string constructed_csv_line = expected_income_record.To_csv();

            // Assert
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void UpdateSourceLineForOutput_WillPopulateSourceLine()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5.21;
            String expected_code = "ABC";
            Double expected_reconciled_amount = 567.89;
            DateTime expected_date_paid = DateTime.Today.AddDays(1);
            Double expected_total_paid = 42.45;
            String expected_description = "description4";
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = expected_date,
                Unreconciled_amount = expected_unreconciled_amount,
                Code = expected_code,
                Reconciled_amount = expected_reconciled_amount,
                Date_paid = expected_date_paid,
                Total_paid = expected_total_paid,
                Description = expected_description
            };
            String expected_source_line =
                $"{expected_date.ToShortDateString()}"
                + $",£{expected_unreconciled_amount}"
                + $",{expected_code}"
                + $",£{expected_reconciled_amount}"
                + $",{expected_date_paid.ToShortDateString()}"
                + $",£{expected_total_paid}"
                + $",\"{expected_description}\"";

            // Act 
            expected_income_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(expected_source_line, expected_income_record.OutputSourceLine);
        }

        [Test]
        public void When_copying_record_will_copy_all_important_data()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                Unreconciled_amount = 80.20,
                Code = "Acme Expenses",
                Reconciled_amount = 6666.66,
                Date_paid = DateTime.Today.AddDays(1),
                Total_paid = 42.34,
                Description = "description"
            };
            expected_income_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (ExpectedIncomeRecord)expected_income_record.Copy();

            // Assert
            Assert.AreEqual(expected_income_record.Date, copied_record.Date);
            Assert.AreEqual(expected_income_record.Unreconciled_amount, copied_record.Unreconciled_amount);
            Assert.AreEqual(expected_income_record.Code, copied_record.Code);
            Assert.AreEqual(expected_income_record.Reconciled_amount, copied_record.Reconciled_amount);
            Assert.AreEqual(expected_income_record.Date_paid, copied_record.Date_paid);
            Assert.AreEqual(expected_income_record.Total_paid, copied_record.Total_paid);
            Assert.AreEqual(expected_income_record.Description, copied_record.Description);
            Assert.AreEqual(expected_income_record.OutputSourceLine, copied_record.OutputSourceLine);
        }

        [Test]
        public void When_copying_record_will_create_new_object()
        {
            // Arrange
            DateTime original_date = DateTime.Today;
            Double original_unreconciled_amount = 5.21;
            String original_code = "ABC";
            Double original_reconciled_amount = 567.89;
            DateTime original_date_paid = DateTime.Today.AddDays(1);
            Double original_total_paid = 42.45;
            String original_description = "description4";
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = original_date,
                Unreconciled_amount = original_unreconciled_amount,
                Code = original_code,
                Reconciled_amount = original_reconciled_amount,
                Date_paid = original_date_paid,
                Total_paid = original_total_paid,
                Description = original_description,
            };
            expected_income_record.Update_source_line_for_output(',');
            var original_source_line = expected_income_record.OutputSourceLine;

            // Act 
            var copied_record = (ExpectedIncomeRecord)expected_income_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Unreconciled_amount = copied_record.Unreconciled_amount + 1;
            copied_record.Code = copied_record.Code + "something else";
            copied_record.Reconciled_amount = copied_record.Reconciled_amount + 1;
            copied_record.Date_paid = copied_record.Date_paid.AddDays(1);
            copied_record.Total_paid = copied_record.Total_paid + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, expected_income_record.Date);
            Assert.AreEqual(original_unreconciled_amount, expected_income_record.Unreconciled_amount);
            Assert.AreEqual(original_code, expected_income_record.Code);
            Assert.AreEqual(original_reconciled_amount, expected_income_record.Reconciled_amount);
            Assert.AreEqual(original_date_paid, expected_income_record.Date_paid);
            Assert.AreEqual(original_total_paid, expected_income_record.Total_paid);
            Assert.AreEqual(original_description, expected_income_record.Description);
            Assert.AreEqual(original_source_line, expected_income_record.OutputSourceLine);
        }

        [Test]
        public void Will_convert_to_a_bank_record()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                Unreconciled_amount = 80.20,
                Code = "Acme Expenses",
                Reconciled_amount = 6666.66,
                Date_paid = DateTime.Today.AddDays(1),
                Total_paid = 42.34,
                Description = "description"
            };

            // Act
            BankRecord result = expected_income_record.Convert_to_bank_record();

            // Assert
            Assert.AreEqual(expected_income_record.Date, result.Date);
            Assert.AreEqual(expected_income_record.Unreconciled_amount, result.Unreconciled_amount);
            Assert.AreEqual(expected_income_record.Code, result.Type);
            Assert.AreEqual(expected_income_record.Description, result.Description);
        }
    }
}
