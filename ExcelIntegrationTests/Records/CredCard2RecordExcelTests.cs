using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for CredCard2Record
    [TestFixture]
    public partial class ExcelRecordTests //(CredCard2Record)
    {
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
