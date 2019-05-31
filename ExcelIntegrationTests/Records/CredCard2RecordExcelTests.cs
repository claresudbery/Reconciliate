using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for CredCard2Record
    [TestFixture]
    public partial class ExcelRecordTests //(CredCard2Record)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateCredCard2RecordCells()
        {
            // Arrange
            var credCard2Record = new CredCard2Record
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Amount = 13.48,
                Description = "Acme: Esmerelda's birthday"
            };
            var row = 10;
            var mockCells = new Mock<ICellSet>();

            // Act 
            credCard2Record.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.DateSpreadsheetIndex + 1, credCard2Record.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.AmountSpreadsheetIndex + 1, credCard2Record.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.DescriptionSpreadsheetIndex + 1, credCard2Record.Description), "Desc");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromCredCard2RecordCells()
        {
            // Arrange
            DateTime expectedDate = new DateTime(year: 2018, month: 3, day: 22);
            Double expectedAmount = 12.38;
            String expectedDescription = "some data";
            var credCard2Record = new CredCard2Record();
            var cells = _spreadsheet.ReadLastRow("CredCard2");

            // Act 
            credCard2Record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard2Record.Date);
            Assert.AreEqual(expectedAmount, credCard2Record.Amount);
            Assert.AreEqual(expectedDescription, credCard2Record.Description);
        }
    }
}
