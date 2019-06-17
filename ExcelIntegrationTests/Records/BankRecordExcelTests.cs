using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelLibrary;
using Interfaces.Constants;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for BankRecord
    [TestFixture]
    public partial class ExcelRecordTests //(BankRecord)
    {
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateBankRecordCells()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                Date = new DateTime(year: 2018, month: 7, day: 1),
                UnreconciledAmount = 80.20,
                Type = "Chq",
                Description = "New description which will overwrite what's normally there.",
                ChequeNumber = 4234566,
                ReconciledAmount = 6666.66
            };
            var cells = _spreadsheet.CurrentCells("Bank");
            var last_row_number = _spreadsheet.LastRowNumber("Bank");
            var previous_record = new BankRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bank_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(bank_record.Date, DateTime.FromOADate((double)new_row.ReadCell(0)));
            Assert.AreEqual(bank_record.UnreconciledAmount, (Double)new_row.ReadCell(1));
            Assert.AreEqual(bank_record.Type, (String)new_row.ReadCell(3));
            Assert.AreEqual(bank_record.Description, (String)new_row.ReadCell(4));
            Assert.AreEqual(bank_record.ChequeNumber, (Double)new_row.ReadCell(5));
            Assert.AreEqual(bank_record.ReconciledAmount, (Double)new_row.ReadCell(6));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertDividerTextInSecondCellWhenBankRecordIsDivider()
        {
            // Arrange
            var bank_record = new BankRecord {Divider = true};
            var cells = _spreadsheet.CurrentCells("Bank");
            var last_row_number = _spreadsheet.LastRowNumber("Bank");
            var previous_record = new BankRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bank_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(ReconConsts.DividerText, (String)new_row.ReadCell(1));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertXInCellWhenBankRecordIsMatched()
        {
            // Arrange
            var bank_record = new BankRecord {Matched = true};
            var cells = _spreadsheet.CurrentCells("Bank");
            var last_row_number = _spreadsheet.LastRowNumber("Bank");
            var previous_record = new BankRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bank_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual("x", (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillInsertNullInRelevantCellWhenBankRecordIsNotMatched()
        {
            // Arrange
            var bank_record = new BankRecord {Matched = false};
            var cells = _spreadsheet.CurrentCells("Bank");
            var last_row_number = _spreadsheet.LastRowNumber("Bank");
            var previous_record = new BankRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bank_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(null, (String)new_row.ReadCell(2));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateZeroAmountsAsEmptyBankRecordCells()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                UnreconciledAmount = 0,
                ChequeNumber = 0,
                ReconciledAmount = 0
            };
            var cells = _spreadsheet.CurrentCells("Bank");
            var last_row_number = _spreadsheet.LastRowNumber("Bank");
            var previous_record = new BankRecord();
            previous_record.ReadFromSpreadsheetRow(_spreadsheet.ReadLastRow("Bank"));

            // Act 
            bank_record.PopulateSpreadsheetRow(cells, last_row_number);
            var new_row = _spreadsheet.ReadLastRow("Bank");

            // Assert
            Assert.AreEqual(null, new_row.ReadCell(1));
            Assert.AreEqual(null, new_row.ReadCell(5));
            Assert.AreEqual(null, new_row.ReadCell(6));

            // Clean up
            previous_record.PopulateSpreadsheetRow(cells, last_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromBankRecordCells()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 7, day: 2);
            Double expected_unreconciled_amount = 5.20;
            String expected_type = "ABC";
            String expected_description = "description4";
            int expected_cheque_number = 42345;
            Double expected_reconciled_amount = 7567.89;
            var bank_record = new BankRecord();
            var cells = _spreadsheet.ReadLastRow("Bank");

            // Act 
            bank_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, bank_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, bank_record.UnreconciledAmount);
            Assert.AreEqual(expected_type, bank_record.Type);
            Assert.AreEqual(expected_description, bank_record.Description);
            Assert.AreEqual(expected_cheque_number, bank_record.ChequeNumber);
            Assert.AreEqual(expected_reconciled_amount, bank_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithBankCellsWhenMatchedCellIsNotPopulated()
        {
            // Arrange
            String expected_description = "PASTA PLASTER";
            var bank_record = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                13.95,
                null,
                "POS",
                expected_description
            };

            // Act 
            bank_record.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillCopeWithBankCellsWhenUnreconciledAmountIsNotPopulated()
        {
            // Arrange
            String expected_description = "PASTA PLASTER";
            var bank_record = new BankRecord();
            List<object> cells = new List<object>
            {
                (double)43405,
                null,
                "x",
                "POS",
                expected_description
            };

            // Act 
            bank_record.ReadFromSpreadsheetRow(new ExcelRow(cells));

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillReadFromBankRecordCellsWhenValuesAreNull()
        {
            // Arrange
            DateTime expected_date = new DateTime(year: 2018, month: 7, day: 1);
            Double expected_unreconciled_amount = 5.15;
            String expected_type = "ABC";
            String expected_description = "description3";
            int expected_cheque_number = 0;
            Double expected_reconciled_amount = 0;
            var bank_record = new BankRecord();
            var cells = _spreadsheet.ReadSpecifiedRow("Bank", 6);

            // Act 
            bank_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_date, bank_record.Date);
            Assert.AreEqual(expected_unreconciled_amount, bank_record.UnreconciledAmount);
            Assert.AreEqual(expected_type, bank_record.Type);
            Assert.AreEqual(expected_description, bank_record.Description);
            Assert.AreEqual(expected_cheque_number, bank_record.ChequeNumber);
            Assert.AreEqual(expected_reconciled_amount, bank_record.ReconciledAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillPopulateSourceLineWhenReadingFromBankRecordCells()
        {
            // Arrange
            String expected_source_line = String.Format("02/07/2018^£5.20^^ABC^description4^42345^\"£7,567.89\"^^^");
            var bank_record = new BankRecord();
            var cells = _spreadsheet.ReadLastRow("Bank");

            // Act 
            bank_record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expected_source_line, bank_record.SourceLine);
        }
    }
}
