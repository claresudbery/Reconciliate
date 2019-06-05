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
