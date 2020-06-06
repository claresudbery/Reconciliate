using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelIntegrationTests.TestUtils;
using Interfaces.Constants;
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
            String expected_transaction_marker = ReconConsts.LastOnlineTransaction;
            DateTime expected_date = new DateTime(year: 2018, month: 5, day: 30);
            String expected_type = "Thing";
            String expected_description = "description";
            Double expected_amount = 4567.89;
            var actual_bank_record = new ActualBankRecord();
            var cells = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Act 
            actual_bank_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_transaction_marker, actual_bank_record.LastTransactionMarker);
            Assert.AreEqual(expected_date, actual_bank_record.Date);
            Assert.AreEqual(expected_type, actual_bank_record.Type);
            Assert.AreEqual(expected_description, actual_bank_record.Description);
            Assert.AreEqual(expected_amount, actual_bank_record.Amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_populate_actual_bank_record_cells()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord
            {
                Date = new DateTime(year: 2018, month: 7, day: 1),
                Type = "Chq",
                Description = "New description which will overwrite what's normally there.",
                Amount = 6666.66,
                LastTransactionMarker = ReconConsts.LastOnlineTransaction + "x"
            };
            var cells = _spreadsheet.Current_cells(TestSheetNames.Bank);
            var last_row_number = _spreadsheet.Last_row_number(TestSheetNames.Bank);
            var previous_record = new ActualBankRecord();
            previous_record.Read_from_spreadsheet_row(_spreadsheet.Read_last_row(TestSheetNames.Bank));

            // Act 
            actual_bank_record.Populate_spreadsheet_row(cells, last_row_number);
            var new_row = _spreadsheet.Read_last_row(TestSheetNames.Bank);

            // Assert
            Assert.AreEqual(actual_bank_record.LastTransactionMarker, (String)new_row.Read_cell(ActualBankRecord.LastTransactionMarkerSpreadsheetIndex));
            Assert.AreEqual(actual_bank_record.Date, DateTime.FromOADate((double)new_row.Read_cell(ActualBankRecord.DateSpreadsheetIndex)));
            Assert.AreEqual(actual_bank_record.Type, (String)new_row.Read_cell(ActualBankRecord.TypeSpreadsheetIndex));
            Assert.AreEqual(actual_bank_record.Description, (String)new_row.Read_cell(ActualBankRecord.DescriptionSpreadsheetIndex));
            Assert.AreEqual(actual_bank_record.Amount, (Double)new_row.Read_cell(ActualBankRecord.AmountSpreadsheetIndex));

            // Clean up
            previous_record.Populate_spreadsheet_row(cells, last_row_number);
        }
    }
}
