using System;
using ConsoleCatchall.Console.Reconciliation.Records;
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
            DateTime expected_date = new DateTime(year: 2018, month: 3, day: 22);
            Double expected_amount = 12.38;
            String expected_description = "some data";
            var cred_card2_record = new CredCard2Record();
            var cells = _spreadsheet.ReadLastRow("CredCard2");

            // Act 
            cred_card2_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
            Assert.AreEqual(expected_amount, cred_card2_record.Amount);
            Assert.AreEqual(expected_description, cred_card2_record.Description);
        }
    }
}
