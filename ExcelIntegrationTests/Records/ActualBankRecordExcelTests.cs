using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for ActualBankRecord
    [TestFixture]
    public partial class ExcelRecordTests //(ActualBankRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateActualBankRecordCells()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Type = "Chq",
                Description = "Acme: Esmerelda's birthday",
                Amount = 1234.56
            };
            var row = 10;
            var mockCells = new Mock<ICellSet>();

            // Act 
            actualBankRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DateSpreadsheetIndex + 1, actualBankRecord.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, actualBankRecord.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, actualBankRecord.Type), "Type");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, actualBankRecord.Description), "Desc");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromActualBankRecordCells()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 5, day: 30);
            String expectedType = "Thing";
            String expectedDescription = "description";
            Double expectedAmount = 4567.89;
            Double expectedBalance = 7898.88;
            var actualBankRecord = new ActualBankRecord();
            var cells = _spreadsheet.ReadLastRow("ActualBank");

            // Act 
            actualBankRecord.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, actualBankRecord.Date);
            Assert.AreEqual(expectedType, actualBankRecord.Type);
            Assert.AreEqual(expectedDescription, actualBankRecord.Description);
            Assert.AreEqual(expectedAmount, actualBankRecord.Amount);
            Assert.AreEqual(expectedBalance, actualBankRecord.Balance);
        }
    }
}
