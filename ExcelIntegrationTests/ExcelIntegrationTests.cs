using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelIntegrationTests.Records;
using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using Interfaces;
using Interfaces.Constants;
using NUnit.Framework;

namespace ExcelIntegrationTests
{
    [TestFixture]
    public class ExcelIntegrationTests
    {
        private static string _testSpreadsheetFileNameAndPath;
        private static string _csvFilePath;
        private static ExcelSpreadsheetRepo _excelSpreadsheet;

        public ExcelIntegrationTests()
        {
            PopulateFilePaths();
            TestHelper.SetCorrectDateFormatting();
        }

        private void PopulateFilePaths()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;

            _testSpreadsheetFileNameAndPath =
                TestHelper.FullyQualifiedSpreadsheetFilePath(current_path)
                + "/" + "Test-Spreadsheet.xlsx";

            _csvFilePath = TestHelper.FullyQualifiedCSVFilePath(current_path);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _excelSpreadsheet.Dispose();
        }

        private void CloseAndReopenSpreadsheet()
        {
            _excelSpreadsheet.Dispose();
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadLastRowOfSpecifiedWorksheetAsObjectList()
        {
            // Arrange
            CredCard1InOutRecord actual_row_as_record = new CredCard1InOutRecord();
            List<String> expected_row = new List<string>{
                "27/04/2018",
                "5.1",
                "",
                "pintipoplication",
                "10567.89"};

            // Act
            var actual_row = _excelSpreadsheet.ReadLastRow(TestSheetNames.CredCard);

            // Assert
            actual_row_as_record.ReadFromSpreadsheetRow(actual_row);
            Assert.AreEqual(expected_row[0], actual_row_as_record.Date.ToShortDateString());
            Assert.AreEqual(expected_row[1], actual_row_as_record.UnreconciledAmount.ToString());
            Assert.AreEqual(expected_row[3], actual_row_as_record.Description);
            Assert.AreEqual(expected_row[4], actual_row_as_record.ReconciledAmount.ToString());
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadLastRowOfWorksheetAsCsv()
        {
            // Arrange
            var cred_card1_record = new CredCard1InOutRecord();
            string expected_csv = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";

            // Act
            var result = _excelSpreadsheet.ReadLastRowAsCsv(TestSheetNames.CredCard, cred_card1_record);

            // Assert
            Assert.AreEqual(expected_csv, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendAllRowsInCsvFileToSpecifiedWorksheet()
        {
            // Arrange
            var sheet_name = TestSheetNames.Bank;
            var file_io = new FileIO<BankRecord>(new ExcelSpreadsheetRepoFactory(""), _csvFilePath, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var initial_last_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);

            // Act
            _excelSpreadsheet.AppendCsvFile<BankRecord>(sheet_name, csv_file);
            var new_last_row = _excelSpreadsheet.ReadLastRow(sheet_name);
            var new_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);

            // Clean up
            for (int count = 1; count <= csv_file.Records.Count; count++)
            {
                _excelSpreadsheet.RemoveLastRow(sheet_name);
            }

            // Assert
            var last_record_in_ordered_csv_file = csv_file.RecordsOrderedForSpreadsheet()[csv_file.Records.Count - 1];
            Assert.AreEqual(last_record_in_ordered_csv_file.Date, DateTime.FromOADate((double)new_last_row.ReadCell(0)));
            Assert.AreEqual(last_record_in_ordered_csv_file.UnreconciledAmount, (Double)new_last_row.ReadCell(1));
            Assert.AreEqual(last_record_in_ordered_csv_file.Type, (String)new_last_row.ReadCell(3));
            Assert.AreEqual(last_record_in_ordered_csv_file.Description, (String)new_last_row.ReadCell(4));
            Assert.AreEqual(null, new_last_row.ReadCell(5));
            Assert.AreEqual(null, new_last_row.ReadCell(6));
            Assert.AreEqual(initial_last_row_number + csv_file.Records.Count, new_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfLastRowContainingCell()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(TestSheetNames.CredCard, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfLastRowContainingCellDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.BadDivider;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(
                    worksheet_name,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains("divider"));
            Assert.IsFalse(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoRowContainingCell()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.TestRecord;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(
                    worksheet_name,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains("divider"));
            Assert.IsTrue(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfLastRowWithCellContainingText()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                TestSheetNames.CredCard,
                "Thingummybob", 
                new List<int>{2, 4, 6});

            // Assert
            Assert.AreEqual(6, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfLastRowWithCellContainingTextDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.BadDivider;
            const string targetText = "Thingummybob";

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                    worksheet_name,
                    targetText,
                    new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(targetText));
            Assert.IsFalse(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoRowWithCellContainingText()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.CredCard;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                    worksheet_name,
                    ReconConsts.CredCard2DdDescription,
                    new List<int> {ReconConsts.DdDescriptionColumn});
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(ReconConsts.CredCard2DdDescription));
            Assert.IsTrue(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfFirstRowContainingCell()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(TestSheetNames.CredCard, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfFirstRowContainingCellDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.BadDivider;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(
                    worksheet_name,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains("divider"));
            Assert.IsFalse(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoFirstRowContainingCell()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.TestRecord;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(
                    worksheet_name,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains("divider"));
            Assert.IsTrue(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void ThrowsExceptionWhenAddingRowToSpreadsheetAlreadyOpen()
        {
            // Arrange
            var second_spreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
            bool exception_thrown = false;
            TestCsvRecord appended_row = new TestCsvRecord().Build();

            // Act
            try
            {
                second_spreadsheet.AppendCsvRecord(TestSheetNames.TestRecord, appended_row);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendCsvRecordToSpecifiedWorksheet()
        {
            // Arrange
            var sheet_name = TestSheetNames.CredCard1;
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                UnreconciledAmount = 1234.56,
                Matched = true,
                Description = "description",
                ReconciledAmount = 356.29
            };
            var initial_last_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);

            // Act
            _excelSpreadsheet.AppendCsvRecord(sheet_name, cred_card1_in_out_record);
            var new_row = _excelSpreadsheet.ReadLastRow(sheet_name);
            var new_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);

            // Clean up
            _excelSpreadsheet.RemoveLastRow(sheet_name);

            // Assert
            Assert.AreEqual(cred_card1_in_out_record.Date, DateTime.FromOADate((double)new_row.ReadCell(CredCard1InOutRecord.DateIndex)));
            Assert.AreEqual(cred_card1_in_out_record.UnreconciledAmount, (Double)new_row.ReadCell(CredCard1InOutRecord.UnreconciledAmountIndex));
            Assert.AreEqual("x", (String)new_row.ReadCell(CredCard1InOutRecord.MatchedIndex));
            Assert.AreEqual(cred_card1_in_out_record.Description, (String)new_row.ReadCell(CredCard1InOutRecord.DescriptionIndex));
            Assert.AreEqual(cred_card1_in_out_record.ReconciledAmount, (Double)new_row.ReadCell(CredCard1InOutRecord.ReconciledAmountIndex));
            Assert.AreEqual(initial_last_row_number + 1, new_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendRowsToWorksheetOrderedByMatchedAndThenByDateAndIncludingDivider()
        {
            // Arrange
            var sheet_name = TestSheetNames.Bank;
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<BankRecord>(new ExcelSpreadsheetRepoFactory(""), _csvFilePath, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);
            var initial_last_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);
            csv_file.Records[0].Matched = true;
            csv_file.Records[1].Matched = true;
            csv_file.Records[3].Matched = true;

            // Act
            _excelSpreadsheet.AppendCsvFile<BankRecord>(sheet_name, csv_file);
            List<ICellRow> new_rows = new List<ICellRow>();
            for (int new_row_count = 1; new_row_count <= csv_file.Records.Count + 1; new_row_count++)
            {
                new_rows.Add(_excelSpreadsheet.ReadSpecifiedRow(sheet_name, initial_last_row_number + new_row_count));
            }

            // Clean up
            for (int count = 1; count <= csv_file.Records.Count + 1; count++)
            {
                _excelSpreadsheet.RemoveLastRow(sheet_name);
            }

            // Assert
            Assert.AreEqual("ZZZThing2", (String)new_rows[0].ReadCell(4));
            Assert.AreEqual("ZZZSpecialDescription001", (String)new_rows[1].ReadCell(4));
            Assert.AreEqual("ZZZThing1", (String)new_rows[2].ReadCell(4));
            Assert.AreEqual("Divider", (String)new_rows[3].ReadCell(1));
            Assert.AreEqual("ZZZThing3", (String)new_rows[4].ReadCell(4));
            Assert.AreEqual("ZZZSpecialDescription005", (String)new_rows[5].ReadCell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanDeleteSpecifiedRows()
        {
            // Arrange
            var sheet_name = TestSheetNames.CredCard;
            var test_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            test_record.Load(csv_line);
            var last_row = new CredCard1InOutRecord();
            string expected_last_row = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";
            var initial_last_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);
            _excelSpreadsheet.AppendCsvRecord(sheet_name, test_record);
            _excelSpreadsheet.AppendCsvRecord(sheet_name, test_record);

            // Act
            _excelSpreadsheet.DeleteSpecifiedRows(sheet_name, 12, 13);

            // Assert
            var new_last_row = _excelSpreadsheet.ReadLastRowAsCsv(sheet_name, last_row);
            var new_last_row_number = _excelSpreadsheet.LastRowNumber(sheet_name);
            Assert.AreEqual(new_last_row_number, initial_last_row_number);
            Assert.AreEqual(expected_last_row, new_last_row);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void IfAmountCodeIsNotPresentThenSensibleErrorWillBeThrown()
        {
            // Arrange
            bool exception_thrown = false;
            string non_existent_code = "Non-Existent Code";
            string actual_error = string.Empty;
            string sheet_name = TestSheetNames.ExpectedOut;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheet_name, non_existent_code, 10);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                actual_error = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown, "Exception should be thrown");
            Assert.AreEqual(expected_error, actual_error);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void IfAmountCodeExistsInWrongColumnThenSensibleErrorWillBeThrown()
        {
            // Arrange
            bool exception_thrown = false;
            string wrong_column_code = "CodeInWrongColumn";
            string actual_error = string.Empty;
            string sheet_name = TestSheetNames.ExpectedOut;
            string expected_error = string.Format(ReconConsts.CodeInWrongPlace, wrong_column_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheet_name, wrong_column_code, 10);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                actual_error = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown, "Exception should be thrown");
            Assert.AreEqual(expected_error, actual_error);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadTextIdentifiedByRowAndColumn()
        {
            // Arrange
            string expected_text = "description1";
            int row_number = 4;
            int column_number = 3;

            // Act
            string actual_text = _excelSpreadsheet.GetText(TestSheetNames.CredCard1, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_text, actual_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfTextIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheet_name = TestSheetNames.CredCard1;
            int row_number = 50;
            int column_number = 3;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetText(sheet_name, row_number, column_number);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(error_message.Contains(sheet_name), $"error message should contain {sheet_name}");
            Assert.IsTrue(error_message.Contains($"{row_number}"), "error message should contain rowNumber");
            Assert.IsTrue(error_message.Contains($"{column_number}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadDateIdentifiedByRowAndColumn()
        {
            // Arrange
            DateTime expected_date = new DateTime(2018, 3, 18);
            int row_number = 8;
            int column_number = 1;

            // Act
            DateTime actual_date = _excelSpreadsheet.GetDate(TestSheetNames.TestRecord, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_date, actual_date);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfDateIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheet_name = TestSheetNames.TestRecord;
            int row_number = 50;
            int column_number = 1;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetDate(sheet_name, row_number, column_number);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(error_message.Contains(sheet_name), $"error message should contain {sheet_name}");
            Assert.IsTrue(error_message.Contains($"{row_number}"), "error message should contain rowNumber");
            Assert.IsTrue(error_message.Contains($"{column_number}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadAmountIdentifiedByRowAndColumn()
        {
            // Arrange
            double expected_amount = 37957.90;
            int row_number = 5;
            int column_number = 4;

            // Act
            double actual_amount = _excelSpreadsheet.GetAmount(TestSheetNames.CredCard1, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_amount, actual_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfAmountIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheet_name = TestSheetNames.CredCard1;
            int row_number = 50;
            int column_number = 4;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetAmount(sheet_name, row_number, column_number);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(error_message.Contains(sheet_name), $"error message should contain {sheet_name}");
            Assert.IsTrue(error_message.Contains($"{row_number}"), "error message should contain rowNumber");
            Assert.IsTrue(error_message.Contains($"{column_number}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadAmountIdentifiedByAmountCode()
        {
            // Arrange
            double expected_amount = 54.97;
            string sheet_name = TestSheetNames.BudgetOut;
            string amount_code = "Code003";

            // Act
            double actual_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_code);

            // Assert
            Assert.AreEqual(expected_amount, actual_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfAmountIdentifiedByAmountCodeIsNotPresent()
        {
            // Arrange
            string sheet_name = TestSheetNames.BudgetOut;
            string non_existent_code = "Does not exist";
            bool exception_thrown = false;
            string error_message = String.Empty;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.GetAmount(sheet_name, non_existent_code);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(expected_error, error_message);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateDateIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheet_name = TestSheetNames.TestRecord;
            int date_row = 10;
            int date_column = 1;
            DateTime previous_date = _excelSpreadsheet.GetDate(sheet_name, date_row, date_column);
            DateTime new_date = previous_date.AddDays(1);

            // Act
            _excelSpreadsheet.UpdateDate(sheet_name, date_row, date_column, new_date);

            // Assert
            DateTime updated_date = _excelSpreadsheet.GetDate(sheet_name, date_row, date_column);
            Assert.AreEqual(new_date, updated_date);
            Assert.AreNotEqual(previous_date, updated_date);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateTextIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheet_name = TestSheetNames.TestRecord;
            int text_row = 10;
            int text_column = 5;
            string previous_text = _excelSpreadsheet.GetText(sheet_name, text_row, text_column);
            string new_text = previous_text + "z";

            // Act
            _excelSpreadsheet.UpdateText(sheet_name, text_row, text_column, new_text);

            // Assert
            string updated_text = _excelSpreadsheet.GetText(sheet_name, text_row, text_column);
            Assert.AreEqual(new_text, updated_text);
            Assert.AreNotEqual(previous_text, updated_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheet_name = TestSheetNames.TestRecord;
            int amount_row = 10;
            int amount_column = 2;
            double previous_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_row, amount_column);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheet_name, amount_row, amount_column, new_amount);

            // Assert
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_row, amount_column);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountAndTextIdentifiedByAmountCode()
        {
            // Arrange
            string sheet_name = TestSheetNames.BudgetOut;
            string amount_code = "Code007";
            int text_column = 6;
            int amount_column = 3;
            int row = 9;
            double previous_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_code, amount_column);
            string previous_text = _excelSpreadsheet.GetText(sheet_name, row, text_column);
            double new_amount = Math.Round(previous_amount + 1.2, 2);
            string new_text = previous_text + "z";

            // Act
            _excelSpreadsheet.UpdateAmountAndText(sheet_name, amount_code, new_amount, new_text, amount_column, text_column);

            // Assert
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_code, amount_column);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
            string updated_text = _excelSpreadsheet.GetText(sheet_name, row, text_column);
            Assert.AreEqual(new_text, updated_text);
            Assert.AreNotEqual(previous_text, updated_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountIdentifiedByAmountCode()
        {
            // Arrange
            string sheet_name = TestSheetNames.ExpectedOut;
            string amount_code = "Code005";
            double previous_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_code, 2);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheet_name, amount_code, new_amount);

            // Assert
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, amount_code, 2);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfTryingToUpdateAmountWithNonExistentAmountCode()
        {
            // Arrange
            string sheet_name = TestSheetNames.BudgetOut;
            string non_existent_code = "Does not exist";
            bool exception_thrown = false;
            string error_message = String.Empty;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheet_name, non_existent_code, 34.56);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(expected_error, error_message);
        }

        //[Ignore("NCrunch has stopped coping with the closing / reopening of a spreadsheet. Runs fine in other test runners.")]
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void UpdatedAmountIsStillPresentAfterSpreadsheetIsClosedAndReopened()
        {
            // Arrange
            const string amountCode = "Code004";
            var sheet_name = TestSheetNames.ExpectedOut;
            double previous_amount = _excelSpreadsheet.GetAmount(sheet_name, amountCode, 2);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheet_name, amountCode, new_amount);
            CloseAndReopenSpreadsheet();

            // Assert
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, amountCode, 2);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanGetSubsetOfColumnsAsRowsConvertedToRecords()
        {
            // Arrange
            const int expectedNumItems = 9;
            string sheet_name = TestSheetNames.BudgetOut;
            int first_row_number = 3;
            int last_row_number = 11;
            const int firstColumnNumber = 2;
            const int lastColumnNumber = 6;

            // Act
            List<BankRecord> budget_items = _excelSpreadsheet.GetRowsAsRecords<BankRecord>(
                sheet_name,
                first_row_number,
                last_row_number,
                firstColumnNumber,
                lastColumnNumber);

            // Assert
            Assert.AreEqual(expectedNumItems, budget_items.Count);
            Assert.AreEqual("Description001", budget_items[0].Description);
            Assert.AreEqual("Description009", budget_items[expectedNumItems - 1].Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanInsertNewRow()
        {
            // Arrange
            var sheet_name = TestSheetNames.ExpectedOut;
            const int testRowNumber = 12;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int previous_num_expected_out_rows = _excelSpreadsheet.LastRowNumber(sheet_name);
            string previous_amount_text = _excelSpreadsheet.GetText(sheet_name, testRowNumber, amountColumn);
            double new_amount = 79;
            string previous_notes_text = _excelSpreadsheet.GetText(sheet_name, testRowNumber, notesColumn);
            string new_notes = "Some new notes about some new stuff";
            var cell_values = new Dictionary<int, object>()
            {
                { amountColumn, new_amount },
                { notesColumn, new_notes }
            };

            // Act
            _excelSpreadsheet.InsertNewRow(sheet_name, testRowNumber, cell_values);

            // Assert
            int new_num_expected_out_rows = _excelSpreadsheet.LastRowNumber(sheet_name);
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, testRowNumber, amountColumn);
            string updated_notes = _excelSpreadsheet.GetText(sheet_name, testRowNumber, notesColumn);
            Assert.AreEqual(previous_num_expected_out_rows + 1, new_num_expected_out_rows);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount_text, updated_amount.ToString());
            Assert.AreEqual(new_notes, updated_notes);
            Assert.AreNotEqual(previous_notes_text, updated_notes);

            // Teardown
            _excelSpreadsheet.DeleteSpecifiedRows(sheet_name, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanAppendNewRow()
        {
            // Arrange
            var sheet_name = TestSheetNames.CredCard;
            const int testRowNumber = 12;
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;
            int previous_num_test_record_rows = _excelSpreadsheet.LastRowNumber(sheet_name);
            DateTime new_date = new DateTime(2018, 11, 25);
            double new_amount = 32.45;
            string new_description = "monthly payment";
            var cell_values = new Dictionary<int, object>()
            {
                { dateColumn, new_date.ToOADate() },
                { amountColumn, new_amount },
                { descriptionColumn, new_description }
            };

            // Act
            _excelSpreadsheet.AppendNewRow(sheet_name, cell_values);

            // Assert
            int new_num_test_record_rows = _excelSpreadsheet.LastRowNumber(sheet_name);
            DateTime updated_date = _excelSpreadsheet.GetDate(sheet_name, testRowNumber, dateColumn);
            double updated_amount = _excelSpreadsheet.GetAmount(sheet_name, testRowNumber, amountColumn);
            string updated_description = _excelSpreadsheet.GetText(sheet_name, testRowNumber, descriptionColumn);
            Assert.AreEqual(previous_num_test_record_rows + 1, new_num_test_record_rows);
            Assert.AreEqual(new_date, updated_date);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreEqual(new_description, updated_description);

            // Teardown
            _excelSpreadsheet.DeleteSpecifiedRows(sheet_name, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindLastRowNumberOfSpecifiedSheet()
        {
            // Act
            var result = _excelSpreadsheet.LastRowNumber(TestSheetNames.CredCard);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadSpecifiedRowGivenRowNumber()
        {
            // Arrange
            var row_number = 2;

            // Act
            var result = _excelSpreadsheet.ReadSpecifiedRow(TestSheetNames.ActualBank, row_number);

            // Assert
            Assert.AreEqual("Thing", result.ReadCell(1));
            Assert.AreEqual("description", result.ReadCell(2));
            Assert.AreEqual(4567.89, result.ReadCell(3));
            Assert.AreEqual(7898.88, result.ReadCell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadSpecifiedRowBasedOnSpecifiedRangeOfColumns()
        {
            // Arrange
            var row_number = 2;
            var start_column = 3;
            var end_column = 4;

            // Act
            var result = _excelSpreadsheet.ReadSpecifiedRow(TestSheetNames.ActualBank, row_number, start_column, end_column);

            // Assert
            Assert.AreEqual(2, result.Count, "Number of cells in row");
            Assert.AreEqual("description", result.ReadCell(0));
            Assert.AreEqual(4567.89, result.ReadCell(1));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindFirstEmptyRowInColumnForSpecifiedSheet()
        {
            // Arrange
            var column_number = 1;

            // Act
            int result = _excelSpreadsheet.FindFirstEmptyRowInColumn(TestSheetNames.TestRecord, column_number);

            // Assert
            Assert.AreEqual(13, result);
        }
    }
}