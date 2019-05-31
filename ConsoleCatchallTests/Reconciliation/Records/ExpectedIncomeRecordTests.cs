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
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 80.20,
                Code = "Acme Expenses",
                ReconciledAmount = 6666.66,
                DatePaid = DateTime.Today.AddDays(1),
                TotalPaid = 42.34,
                Description = "description"
            };
            var mockCells = new Mock<ICellSet>();
            var row = 10;

            // Act 
            expectedIncomeRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DateIndex + 1, expectedIncomeRecord.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.UnreconciledAmountIndex + 1, expectedIncomeRecord.UnreconciledAmount), "UnreconciledAmount");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.CodeIndex + 1, expectedIncomeRecord.Code), "Code");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.ReconciledAmountIndex + 1, expectedIncomeRecord.ReconciledAmount), "ReconciledAmount");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DatePaidIndex + 1, expectedIncomeRecord.DatePaid), "DatePaid");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.TotalPaidIndex + 1, expectedIncomeRecord.TotalPaid), "TotalPaid");
            mockCells.Verify(x => x.PopulateCell(row, ExpectedIncomeRecord.DescriptionIndex + 1, expectedIncomeRecord.Description), "Description");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenExpectedIncomeRecordIsDivider()
        {
            // Arrange
            var expectedIncomeRecord = new ExpectedIncomeRecord {Divider = true};
            var rowNumber = 10;
            var mockCells = new Mock<ICellSet>();

            // Act 
            expectedIncomeRecord.PopulateSpreadsheetRow(mockCells.Object, rowNumber);

            // Assert
            mockCells.Verify(x => x.PopulateCell(rowNumber, ExpectedIncomeRecord.DividerIndex + 1, ReconConsts.DividerText));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromExpectedIncomeRecordCells()
        {
            // Arrange
            DateTime expectedDate = DateTime.Today;
            Double expectedUnreconciledAmount = 5.20;
            String expectedCode = "ABC";
            Double expectedReconciledAmount = 7567.89;
            DateTime expectedDatePaid = DateTime.Today.AddDays(1);
            Double expectedTotalPaid = 42.45;
            String expectedDescription = "description4";
            var mockCells = new Mock<ICellRow>();
            mockCells.Setup(x => x.Count).Returns(20);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DateIndex)).Returns(expectedDate.ToOADate());
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expectedUnreconciledAmount);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expectedCode);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expectedReconciledAmount);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expectedDatePaid.ToOADate());
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expectedTotalPaid);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expectedDescription);
            var expectedIncomeRecord = new ExpectedIncomeRecord();

            // Act 
            expectedIncomeRecord.ReadFromSpreadsheetRow(mockCells.Object);

            // Assert
            Assert.AreEqual(expectedDate, expectedIncomeRecord.Date);
            Assert.AreEqual(expectedUnreconciledAmount, expectedIncomeRecord.UnreconciledAmount);
            Assert.AreEqual(expectedCode, expectedIncomeRecord.Code);
            Assert.AreEqual(expectedReconciledAmount, expectedIncomeRecord.ReconciledAmount);
            Assert.AreEqual(expectedDatePaid, expectedIncomeRecord.DatePaid);
            Assert.AreEqual(expectedTotalPaid, expectedIncomeRecord.TotalPaid);
            Assert.AreEqual(expectedDescription, expectedIncomeRecord.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithExpectedInCellsWhenNotAllCellsArePopulated()
        {
            // Arrange
            var expectedIncomeRecord = new ExpectedIncomeRecord();
            var mockCells = new Mock<ICellRow>();
            var expectedCode = "expectedCode";
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expectedCode);

            // Act 
            expectedIncomeRecord.ReadFromSpreadsheetRow(mockCells.Object);

            // Assert
            Assert.AreEqual(expectedCode, expectedIncomeRecord.Code);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromExpectedIncomeRecordCells()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expectedDate = DateTime.Today;
            Double expectedUnreconciledAmount = 5.21;
            String expectedCode = "ABC";
            Double expectedReconciledAmount = 567.89;
            DateTime expectedDatePaid = DateTime.Today.AddDays(1);
            Double expectedTotalPaid = 42.45;
            String expectedDescription = "description4";
            var mockCells = new Mock<ICellRow>();
            mockCells.Setup(x => x.Count).Returns(20);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DateIndex)).Returns(expectedDate.ToOADate());
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.UnreconciledAmountIndex)).Returns(expectedUnreconciledAmount);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(expectedCode);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.ReconciledAmountIndex)).Returns(expectedReconciledAmount);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DatePaidIndex)).Returns(expectedDatePaid.ToOADate());
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.TotalPaidIndex)).Returns(expectedTotalPaid);
            mockCells.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(expectedDescription);
            String expectedSourceLine = 
                $"{expectedDate.ToShortDateString()}"
                +$",£{expectedUnreconciledAmount}"
                + $",{expectedCode}"
                + $",£{expectedReconciledAmount}"
                + $",{expectedDatePaid.ToShortDateString()}"
                + $",£{expectedTotalPaid}"
                + $",{expectedDescription}";
            var expectedIncomeRecord = new ExpectedIncomeRecord();

            // Act 
            expectedIncomeRecord.ReadFromSpreadsheetRow(mockCells.Object);

            // Assert
            Assert.AreEqual(expectedSourceLine, expectedIncomeRecord.SourceLine);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expectedDate = DateTime.Today;
            Double expectedUnreconciledAmount = 5.21;
            String expectedCode = "ABC";
            Double expectedReconciledAmount = 567.89;
            DateTime expectedDatePaid = DateTime.Today.AddDays(1);
            Double expectedTotalPaid = 42.45;
            String expectedDescription = "description4";
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = expectedDate,
                UnreconciledAmount = expectedUnreconciledAmount,
                Code = expectedCode,
                ReconciledAmount = expectedReconciledAmount,
                DatePaid = expectedDatePaid,
                TotalPaid = expectedTotalPaid,
                Description = expectedDescription
            };
            String expectedSourceLine =
                $"{expectedDate.ToShortDateString()}"
                + $",£{expectedUnreconciledAmount}"
                + $",{expectedCode}"
                + $",£{expectedReconciledAmount}"
                + $",{expectedDatePaid.ToShortDateString()}"
                + $",£{expectedTotalPaid}"
                + $",\"{expectedDescription}\"";

            // Act 
            string constructedCsvLine = expectedIncomeRecord.ToCsv();

            // Assert
            Assert.AreEqual(expectedSourceLine, constructedCsvLine);
        }

        [Test]
        public void ToCsv_AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expectedDate = DateTime.Today;
            Double expectedUnreconciledAmount = 5555.21;
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = expectedDate,
                UnreconciledAmount = expectedUnreconciledAmount,
            };
            String expectedCsvLine =
                $"{expectedDate.ToShortDateString()}"
                + $",\"£5,555.21\",,,,,";

            // Act 
            string constructedCsvLine = expectedIncomeRecord.ToCsv();

            // Assert
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void ToCsv_UnpopulatedValuesShouldBeEmptyStrings()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            var expectedIncomeRecord = new ExpectedIncomeRecord();
            String expectedCsvLine =$",,,,,,";

            // Act 
            string constructedCsvLine = expectedIncomeRecord.ToCsv();

            // Assert
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void ToCsv_AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var amount = 123.55;
            var amountWithPoundSign = $"£{amount}";
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                UnreconciledAmount = amount,
                ReconciledAmount = amount,
                TotalPaid = amount,
            };
            String expectedCsvLine = $",{amountWithPoundSign},,{amountWithPoundSign},,{amountWithPoundSign},";

            // Act 
            string constructedCsvLine = expectedIncomeRecord.ToCsv();

            // Assert
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void UpdateSourceLineForOutput_WillPopulateSourceLine()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            DateTime expectedDate = DateTime.Today;
            Double expectedUnreconciledAmount = 5.21;
            String expectedCode = "ABC";
            Double expectedReconciledAmount = 567.89;
            DateTime expectedDatePaid = DateTime.Today.AddDays(1);
            Double expectedTotalPaid = 42.45;
            String expectedDescription = "description4";
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = expectedDate,
                UnreconciledAmount = expectedUnreconciledAmount,
                Code = expectedCode,
                ReconciledAmount = expectedReconciledAmount,
                DatePaid = expectedDatePaid,
                TotalPaid = expectedTotalPaid,
                Description = expectedDescription
            };
            String expectedSourceLine =
                $"{expectedDate.ToShortDateString()}"
                + $",£{expectedUnreconciledAmount}"
                + $",{expectedCode}"
                + $",£{expectedReconciledAmount}"
                + $",{expectedDatePaid.ToShortDateString()}"
                + $",£{expectedTotalPaid}"
                + $",\"{expectedDescription}\"";

            // Act 
            expectedIncomeRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(expectedSourceLine, expectedIncomeRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 80.20,
                Code = "Acme Expenses",
                ReconciledAmount = 6666.66,
                DatePaid = DateTime.Today.AddDays(1),
                TotalPaid = 42.34,
                Description = "description"
            };
            expectedIncomeRecord.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (ExpectedIncomeRecord)expectedIncomeRecord.Copy();

            // Assert
            Assert.AreEqual(expectedIncomeRecord.Date, copiedRecord.Date);
            Assert.AreEqual(expectedIncomeRecord.UnreconciledAmount, copiedRecord.UnreconciledAmount);
            Assert.AreEqual(expectedIncomeRecord.Code, copiedRecord.Code);
            Assert.AreEqual(expectedIncomeRecord.ReconciledAmount, copiedRecord.ReconciledAmount);
            Assert.AreEqual(expectedIncomeRecord.DatePaid, copiedRecord.DatePaid);
            Assert.AreEqual(expectedIncomeRecord.TotalPaid, copiedRecord.TotalPaid);
            Assert.AreEqual(expectedIncomeRecord.Description, copiedRecord.Description);
            Assert.AreEqual(expectedIncomeRecord.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            DateTime originalDate = DateTime.Today;
            Double originalUnreconciledAmount = 5.21;
            String originalCode = "ABC";
            Double originalReconciledAmount = 567.89;
            DateTime originalDatePaid = DateTime.Today.AddDays(1);
            Double originalTotalPaid = 42.45;
            String originalDescription = "description4";
            var expectedIncomeRecord = new ExpectedIncomeRecord
            {
                Date = originalDate,
                UnreconciledAmount = originalUnreconciledAmount,
                Code = originalCode,
                ReconciledAmount = originalReconciledAmount,
                DatePaid = originalDatePaid,
                TotalPaid = originalTotalPaid,
                Description = originalDescription,
            };
            expectedIncomeRecord.UpdateSourceLineForOutput(',');
            var originalSourceLine = expectedIncomeRecord.SourceLine;

            // Act 
            var copiedRecord = (ExpectedIncomeRecord)expectedIncomeRecord.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.UnreconciledAmount = copiedRecord.UnreconciledAmount + 1;
            copiedRecord.Code = copiedRecord.Code + "something else";
            copiedRecord.ReconciledAmount = copiedRecord.ReconciledAmount + 1;
            copiedRecord.DatePaid = copiedRecord.DatePaid.AddDays(1);
            copiedRecord.TotalPaid = copiedRecord.TotalPaid + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, expectedIncomeRecord.Date);
            Assert.AreEqual(originalUnreconciledAmount, expectedIncomeRecord.UnreconciledAmount);
            Assert.AreEqual(originalCode, expectedIncomeRecord.Code);
            Assert.AreEqual(originalReconciledAmount, expectedIncomeRecord.ReconciledAmount);
            Assert.AreEqual(originalDatePaid, expectedIncomeRecord.DatePaid);
            Assert.AreEqual(originalTotalPaid, expectedIncomeRecord.TotalPaid);
            Assert.AreEqual(originalDescription, expectedIncomeRecord.Description);
            Assert.AreEqual(originalSourceLine, expectedIncomeRecord.SourceLine);
        }

        [Test]
        public void WillConvertToABankRecord()
        {
            // Arrange
            var expectedIncomeRecord = new ExpectedIncomeRecord
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
            BankRecord result = expectedIncomeRecord.ConvertToBankRecord();

            // Assert
            Assert.AreEqual(expectedIncomeRecord.Date, result.Date);
            Assert.AreEqual(expectedIncomeRecord.UnreconciledAmount, result.UnreconciledAmount);
            Assert.AreEqual(expectedIncomeRecord.Code, result.Type);
            Assert.AreEqual(expectedIncomeRecord.Description, result.Description);
        }
    }
}
