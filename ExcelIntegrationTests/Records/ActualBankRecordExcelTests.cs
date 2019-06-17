using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for ActualBankRecord
    [TestFixture]
    public partial class ExcelRecordTests //(ActualBankRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_read_from_actual_bank_record_cells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 5, day: 30);
            String expected_type = "Thing";
            String expected_description = "description";
            Double expected_amount = 4567.89;
            Double expected_balance = 7898.88;
            var actual_bank_record = new ActualBankRecord();
            var cells = _spreadsheet.Read_last_row("ActualBank");

            // Act 
            actual_bank_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, actual_bank_record.Date);
            Assert.AreEqual(expected_type, actual_bank_record.Type);
            Assert.AreEqual(expected_description, actual_bank_record.Description);
            Assert.AreEqual(expected_amount, actual_bank_record.Amount);
            Assert.AreEqual(expected_balance, actual_bank_record.Balance);
        }
    }
}
