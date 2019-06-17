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
        public void WillPopulateExpectedIncomeRecordCells()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 80.20,
                Code = "Acme Expenses",
                ReconciledAmount = 6666.66,
                DatePaid = DateTime.Today.AddDays(1),
                TotalPaid = 42.34,
                Description = "description"
            };
            var mock_cells = new Mock<ICellSet>();
            var row = 10;

            // Act 
            expected_income_record.PopulateSpreadsheetRow(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DateIndex + 1, expected_income_record.Date), "Date");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.UnreconciledAmountIndex + 1, expected_income_record.UnreconciledAmount), "UnreconciledAmount");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.CodeIndex + 1, expected_income_record.Code), "Code");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.ReconciledAmountIndex + 1, expected_income_record.ReconciledAmount), "ReconciledAmount");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DatePaidIndex + 1, expected_income_record.DatePaid), "DatePaid");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.TotalPaidIndex + 1, expected_income_record.TotalPaid), "TotalPaid");
            mock_cells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DescriptionIndex + 1, expected_income_record.Description), "Description");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenExpectedIncomeRecordIsDivider()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord {Divider = true};
            var row_number = 10;
            var mock_cells = new Mock<ICellSet>();

            // Act 
            expected_income_record.PopulateSpreadsheetRow(mock_cells.Object, row_number);

            // Assert
            mock_cells.Verify(x => x.PopulateCell(row_number, ExpectedIncomeRecord.DividerIndex + 1, ReconConsts.DividerText));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromExpectedIncomeRecordCells()
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
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DateIndex)).Returns(expected_date.ToOADate());
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expected_unreconciled_amount);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expected_reconciled_amount);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expected_date_paid.ToOADate());
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expected_total_paid);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expected_description);
            var expected_income_record = new ExpectedIncomeRecord();

            // Act 
            expected_income_record.ReadFromSpreadsheetRow(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_date, expected_income_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, expected_income_record.UnreconciledAmount);
            Assert.AreEqual(expected_code, expected_income_record.Code);
            Assert.AreEqual(expected_reconciled_amount, expected_income_record.ReconciledAmount);
            Assert.AreEqual(expected_date_paid, expected_income_record.DatePaid);
            Assert.AreEqual(expected_total_paid, expected_income_record.TotalPaid);
            Assert.AreEqual(expected_description, expected_income_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithExpectedInCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord();
            var mock_cells = new Mock<ICellRow>();
            var expected_code = "expectedCode";
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);

            // Act 
            expected_income_record.ReadFromSpreadsheetRow(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_code, expected_income_record.Code);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromExpectedIncomeRecordCells()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5.21;
            String expected_code = "ABC";
            Double expected_reconciled_amount = 567.89;
            DateTime expected_date_paid = DateTime.Today.AddDays(1);
            Double expected_total_paid = 42.45;
            String expected_description = "description4";
            var mock_cells = new Mock<ICellRow>();
            mock_cells.Setup(x => x.Count).Returns(20);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DateIndex)).Returns(expected_date.ToOADate());
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expected_unreconciled_amount);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expected_code);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expected_reconciled_amount);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expected_date_paid.ToOADate());
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expected_total_paid);
            mock_cells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expected_description);
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
            expected_income_record.ReadFromSpreadsheetRow(mock_cells.Object);

            // Assert
            Assert.AreEqual(expected_source_line, expected_income_record.SourceLine);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
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
                UnreconciledAmount = expected_unreconciled_amount,
                Code = expected_code,
                ReconciledAmount = expected_reconciled_amount,
                DatePaid = expected_date_paid,
                TotalPaid = expected_total_paid,
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
            string constructed_csv_line = expected_income_record.ToCsv();

            // Assert
            Assert.AreEqual(expected_source_line, constructed_csv_line);
        }

        [Test]
        public void ToCsv_AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expected_date = DateTime.Today;
            Double expected_unreconciled_amount = 5555.21;
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = expected_date,
                UnreconciledAmount = expected_unreconciled_amount,
            };
            String expected_csv_line =
                $"{expected_date.ToShortDateString()}"
                + $",\"£5,555.21\",,,,,";

            // Act 
            string constructed_csv_line = expected_income_record.ToCsv();

            // Assert
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void ToCsv_UnpopulatedValuesShouldBeEmptyStrings()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            var expected_income_record = new ExpectedIncomeRecord();
            String expected_csv_line =$",,,,,,";

            // Act 
            string constructed_csv_line = expected_income_record.ToCsv();

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
                UnreconciledAmount = amount,
                ReconciledAmount = amount,
                TotalPaid = amount,
            };
            String expected_csv_line = $",{amount_with_pound_sign},,{amount_with_pound_sign},,{amount_with_pound_sign},";

            // Act 
            string constructed_csv_line = expected_income_record.ToCsv();

            // Assert
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void UpdateSourceLineForOutput_WillPopulateSourceLine()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
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
                UnreconciledAmount = expected_unreconciled_amount,
                Code = expected_code,
                ReconciledAmount = expected_reconciled_amount,
                DatePaid = expected_date_paid,
                TotalPaid = expected_total_paid,
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
            expected_income_record.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(expected_source_line, expected_income_record.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 80.20,
                Code = "Acme Expenses",
                ReconciledAmount = 6666.66,
                DatePaid = DateTime.Today.AddDays(1),
                TotalPaid = 42.34,
                Description = "description"
            };
            expected_income_record.UpdateSourceLineForOutput(',');

            // Act 
            var copied_record = (ExpectedIncomeRecord)expected_income_record.Copy();

            // Assert
            Assert.AreEqual(expected_income_record.Date, copied_record.Date);
            Assert.AreEqual(expected_income_record.UnreconciledAmount, copied_record.UnreconciledAmount);
            Assert.AreEqual(expected_income_record.Code, copied_record.Code);
            Assert.AreEqual(expected_income_record.ReconciledAmount, copied_record.ReconciledAmount);
            Assert.AreEqual(expected_income_record.DatePaid, copied_record.DatePaid);
            Assert.AreEqual(expected_income_record.TotalPaid, copied_record.TotalPaid);
            Assert.AreEqual(expected_income_record.Description, copied_record.Description);
            Assert.AreEqual(expected_income_record.SourceLine, copied_record.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
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
                UnreconciledAmount = original_unreconciled_amount,
                Code = original_code,
                ReconciledAmount = original_reconciled_amount,
                DatePaid = original_date_paid,
                TotalPaid = original_total_paid,
                Description = original_description,
            };
            expected_income_record.UpdateSourceLineForOutput(',');
            var original_source_line = expected_income_record.SourceLine;

            // Act 
            var copied_record = (ExpectedIncomeRecord)expected_income_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.UnreconciledAmount = copied_record.UnreconciledAmount + 1;
            copied_record.Code = copied_record.Code + "something else";
            copied_record.ReconciledAmount = copied_record.ReconciledAmount + 1;
            copied_record.DatePaid = copied_record.DatePaid.AddDays(1);
            copied_record.TotalPaid = copied_record.TotalPaid + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(original_date, expected_income_record.Date);
            Assert.AreEqual(original_unreconciled_amount, expected_income_record.UnreconciledAmount);
            Assert.AreEqual(original_code, expected_income_record.Code);
            Assert.AreEqual(original_reconciled_amount, expected_income_record.ReconciledAmount);
            Assert.AreEqual(original_date_paid, expected_income_record.DatePaid);
            Assert.AreEqual(original_total_paid, expected_income_record.TotalPaid);
            Assert.AreEqual(original_description, expected_income_record.Description);
            Assert.AreEqual(original_source_line, expected_income_record.SourceLine);
        }

        [Test]
        public void WillConvertToABankRecord()
        {
            // Arrange
            var expected_income_record = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 80.20,
                Code = "Acme Expenses",
                ReconciledAmount = 6666.66,
                DatePaid = DateTime.Today.AddDays(1),
                TotalPaid = 42.34,
                Description = "description"
            };

            // Act
            BankRecord result = expected_income_record.ConvertToBankRecord();

            // Assert
            Assert.AreEqual(expected_income_record.Date, result.Date);
            Assert.AreEqual(expected_income_record.UnreconciledAmount, result.UnreconciledAmount);
            Assert.AreEqual(expected_income_record.Code, result.Type);
            Assert.AreEqual(expected_income_record.Description, result.Description);
        }
    }
}
