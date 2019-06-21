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
            Populate_file_paths();
            TestHelper.Set_correct_date_formatting();
        }

        private void Populate_file_paths()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;

            _testSpreadsheetFileNameAndPath =
                TestHelper.Fully_qualified_spreadsheet_file_path(current_path)
                + "/" + "Test-Spreadsheet.xlsx";

            _csvFilePath = TestHelper.Fully_qualified_csv_file_path(current_path);
        }

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [OneTimeTearDown]
        public void One_time_tear_down()
        {
            _excelSpreadsheet.Dispose();
        }

        private void Close_and_reopen_spreadsheet()
        {
            _excelSpreadsheet.Dispose();
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_read_last_row_of_specified_worksheet_as_object_list()
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
            var actual_row = _excelSpreadsheet.Read_last_row(TestSheetNames.Cred_card);

            // Assert
            actual_row_as_record.Read_from_spreadsheet_row(actual_row);
            Assert.AreEqual(expected_row[0], actual_row_as_record.Date.ToShortDateString());
            Assert.AreEqual(expected_row[1], actual_row_as_record.Unreconciled_amount.ToString());
            Assert.AreEqual(expected_row[3], actual_row_as_record.Description);
            Assert.AreEqual(expected_row[4], actual_row_as_record.Reconciled_amount.ToString());
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_read_last_row_of_worksheet_as_csv()
        {
            // Arrange
            var cred_card1_record = new CredCard1InOutRecord();
            string expected_csv = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";

            // Act
            var result = _excelSpreadsheet.Read_last_row_as_csv(TestSheetNames.Cred_card, cred_card1_record);

            // Assert
            Assert.AreEqual(expected_csv, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_append_all_rows_in_csv_file_to_specified_worksheet()
        {
            // Arrange
            var sheet_name = TestSheetNames.Bank;
            var file_io = new FileIO<BankRecord>(new ExcelSpreadsheetRepoFactory(""), _csvFilePath, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var initial_last_row_number = _excelSpreadsheet.Last_row_number(sheet_name);

            // Act
            _excelSpreadsheet.Append_csv_file<BankRecord>(sheet_name, csv_file);
            var new_last_row = _excelSpreadsheet.Read_last_row(sheet_name);
            var new_row_number = _excelSpreadsheet.Last_row_number(sheet_name);

            // Clean up
            for (int count = 1; count <= csv_file.Records.Count; count++)
            {
                _excelSpreadsheet.Remove_last_row(sheet_name);
            }

            // Assert
            var last_record_in_ordered_csv_file = csv_file.Records_ordered_for_spreadsheet()[csv_file.Records.Count - 1];
            Assert.AreEqual(last_record_in_ordered_csv_file.Date, DateTime.FromOADate((double)new_last_row.Read_cell(0)));
            Assert.AreEqual(last_record_in_ordered_csv_file.Unreconciled_amount, (Double)new_last_row.Read_cell(1));
            Assert.AreEqual(last_record_in_ordered_csv_file.Type, (String)new_last_row.Read_cell(3));
            Assert.AreEqual(last_record_in_ordered_csv_file.Description, (String)new_last_row.Read_cell(4));
            Assert.AreEqual(null, new_last_row.Read_cell(5));
            Assert.AreEqual(null, new_last_row.Read_cell(6));
            Assert.AreEqual(initial_last_row_number + csv_file.Records.Count, new_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_find_row_number_of_last_row_containing_cell()
        {
            // Act
            var result = _excelSpreadsheet.Find_row_number_of_last_row_containing_cell(TestSheetNames.Cred_card, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_exception_if_last_row_containing_cell_does_not_have_cell_in_expected_column()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Bad_divider;

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_last_row_containing_cell(
                    worksheet_name,
                    Dividers.Divider_text);
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
        public void Will_throw_exception_if_there_is_no_row_containing_cell()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Test_record;

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_last_row_containing_cell(
                    worksheet_name,
                    Dividers.Divider_text);
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
        public void Can_find_row_number_of_last_row_with_cell_containing_text()
        {
            // Act
            var result = _excelSpreadsheet.Find_row_number_of_last_row_with_cell_containing_text(
                TestSheetNames.Cred_card,
                "Thingummybob", 
                new List<int>{2, 4, 6});

            // Assert
            Assert.AreEqual(6, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_exception_if_last_row_with_cell_containing_text_does_not_have_cell_in_expected_column()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Bad_divider;
            const string targetText = "Thingummybob";

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_last_row_with_cell_containing_text(
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
        public void Will_throw_exception_if_there_is_no_row_with_cell_containing_text()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Cred_card;

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_last_row_with_cell_containing_text(
                    worksheet_name,
                    ReconConsts.Cred_card2_dd_description,
                    new List<int> {ReconConsts.DdDescriptionColumn});
            }
            catch (Exception e)
            {
                exception_thrown = true;
                error_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(ReconConsts.Cred_card2_dd_description));
            Assert.IsTrue(error_message.Contains("row"));
            Assert.IsTrue(error_message.Contains(worksheet_name));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_find_row_number_of_first_row_containing_cell()
        {
            // Act
            var result = _excelSpreadsheet.Find_row_number_of_first_row_containing_cell(TestSheetNames.Cred_card, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_exception_if_first_row_containing_cell_does_not_have_cell_in_expected_column()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Bad_divider;

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_first_row_containing_cell(
                    worksheet_name,
                    Dividers.Divider_text);
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
        public void Will_throw_exception_if_there_is_no_first_row_containing_cell()
        {
            // Arrange
            bool exception_thrown = false;
            String error_message = String.Empty;
            String worksheet_name = TestSheetNames.Test_record;

            // Act
            try
            {
                _excelSpreadsheet.Find_row_number_of_first_row_containing_cell(
                    worksheet_name,
                    Dividers.Divider_text);
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
        public void Throws_exception_when_adding_row_to_spreadsheet_already_open()
        {
            // Arrange
            var second_spreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
            bool exception_thrown = false;
            TestCsvRecord appended_row = new TestCsvRecord().Build();

            // Act
            try
            {
                second_spreadsheet.Append_csv_record(TestSheetNames.Test_record, appended_row);
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
        public void Will_append_csv_record_to_specified_worksheet()
        {
            // Arrange
            var sheet_name = TestSheetNames.Cred_card1;
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Unreconciled_amount = 1234.56,
                Matched = true,
                Description = "description",
                Reconciled_amount = 356.29
            };
            var initial_last_row_number = _excelSpreadsheet.Last_row_number(sheet_name);

            // Act
            _excelSpreadsheet.Append_csv_record(sheet_name, cred_card1_in_out_record);
            var new_row = _excelSpreadsheet.Read_last_row(sheet_name);
            var new_row_number = _excelSpreadsheet.Last_row_number(sheet_name);

            // Clean up
            _excelSpreadsheet.Remove_last_row(sheet_name);

            // Assert
            Assert.AreEqual(cred_card1_in_out_record.Date, DateTime.FromOADate((double)new_row.Read_cell(CredCard1InOutRecord.DateIndex)));
            Assert.AreEqual(cred_card1_in_out_record.Unreconciled_amount, (Double)new_row.Read_cell(CredCard1InOutRecord.UnreconciledAmountIndex));
            Assert.AreEqual("x", (String)new_row.Read_cell(CredCard1InOutRecord.MatchedIndex));
            Assert.AreEqual(cred_card1_in_out_record.Description, (String)new_row.Read_cell(CredCard1InOutRecord.DescriptionIndex));
            Assert.AreEqual(cred_card1_in_out_record.Reconciled_amount, (Double)new_row.Read_cell(CredCard1InOutRecord.ReconciledAmountIndex));
            Assert.AreEqual(initial_last_row_number + 1, new_row_number);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_append_rows_to_worksheet_ordered_by_matched_and_then_by_date_and_including_divider()
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
            var initial_last_row_number = _excelSpreadsheet.Last_row_number(sheet_name);
            csv_file.Records[0].Matched = true;
            csv_file.Records[1].Matched = true;
            csv_file.Records[3].Matched = true;

            // Act
            _excelSpreadsheet.Append_csv_file<BankRecord>(sheet_name, csv_file);
            List<ICellRow> new_rows = new List<ICellRow>();
            for (int new_row_count = 1; new_row_count <= csv_file.Records.Count + 1; new_row_count++)
            {
                new_rows.Add(_excelSpreadsheet.Read_specified_row(sheet_name, initial_last_row_number + new_row_count));
            }

            // Clean up
            for (int count = 1; count <= csv_file.Records.Count + 1; count++)
            {
                _excelSpreadsheet.Remove_last_row(sheet_name);
            }

            // Assert
            Assert.AreEqual("ZZZThing2", (String)new_rows[0].Read_cell(4));
            Assert.AreEqual("ZZZSpecialDescription001", (String)new_rows[1].Read_cell(4));
            Assert.AreEqual("ZZZThing1", (String)new_rows[2].Read_cell(4));
            Assert.AreEqual("Divider", (String)new_rows[3].Read_cell(1));
            Assert.AreEqual("ZZZThing3", (String)new_rows[4].Read_cell(4));
            Assert.AreEqual("ZZZSpecialDescription005", (String)new_rows[5].Read_cell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_delete_specified_rows()
        {
            // Arrange
            var sheet_name = TestSheetNames.Cred_card;
            var test_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            test_record.Load(csv_line);
            var last_row = new CredCard1InOutRecord();
            string expected_last_row = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";
            var initial_last_row_number = _excelSpreadsheet.Last_row_number(sheet_name);
            _excelSpreadsheet.Append_csv_record(sheet_name, test_record);
            _excelSpreadsheet.Append_csv_record(sheet_name, test_record);

            // Act
            _excelSpreadsheet.Delete_specified_rows(sheet_name, 12, 13);

            // Assert
            var new_last_row = _excelSpreadsheet.Read_last_row_as_csv(sheet_name, last_row);
            var new_last_row_number = _excelSpreadsheet.Last_row_number(sheet_name);
            Assert.AreEqual(new_last_row_number, initial_last_row_number);
            Assert.AreEqual(expected_last_row, new_last_row);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void If_amount_code_is_not_present_then_sensible_error_will_be_thrown()
        {
            // Arrange
            bool exception_thrown = false;
            string non_existent_code = "Non-Existent Code";
            string actual_error = string.Empty;
            string sheet_name = TestSheetNames.Expected_out;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.Update_amount(sheet_name, non_existent_code, 10);
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
        public void If_amount_code_exists_in_wrong_column_then_sensible_error_will_be_thrown()
        {
            // Arrange
            bool exception_thrown = false;
            string wrong_column_code = "CodeInWrongColumn";
            string actual_error = string.Empty;
            string sheet_name = TestSheetNames.Expected_out;
            string expected_error = string.Format(ReconConsts.CodeInWrongPlace, wrong_column_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.Update_amount(sheet_name, wrong_column_code, 10);
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
        public void Can_read_text_identified_by_row_and_column()
        {
            // Arrange
            string expected_text = "description1";
            int row_number = 4;
            int column_number = 3;

            // Act
            string actual_text = _excelSpreadsheet.Get_text(TestSheetNames.Cred_card1, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_text, actual_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_sensible_error_if_text_identified_by_row_and_column_is_not_present()
        {
            // Arrange
            string sheet_name = TestSheetNames.Cred_card1;
            int row_number = 50;
            int column_number = 3;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.Get_text(sheet_name, row_number, column_number);
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
        public void Can_read_date_identified_by_row_and_column()
        {
            // Arrange
            DateTime expected_date = new DateTime(2018, 3, 18);
            int row_number = 8;
            int column_number = 1;

            // Act
            DateTime actual_date = _excelSpreadsheet.Get_date(TestSheetNames.Test_record, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_date, actual_date);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_sensible_error_if_date_identified_by_row_and_column_is_not_present()
        {
            // Arrange
            string sheet_name = TestSheetNames.Test_record;
            int row_number = 50;
            int column_number = 1;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.Get_date(sheet_name, row_number, column_number);
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
        public void Can_read_amount_identified_by_row_and_column()
        {
            // Arrange
            double expected_amount = 37957.90;
            int row_number = 5;
            int column_number = 4;

            // Act
            double actual_amount = _excelSpreadsheet.Get_amount(TestSheetNames.Cred_card1, row_number, column_number);

            // Assert
            Assert.AreEqual(expected_amount, actual_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_sensible_error_if_amount_identified_by_row_and_column_is_not_present()
        {
            // Arrange
            string sheet_name = TestSheetNames.Cred_card1;
            int row_number = 50;
            int column_number = 4;
            bool exception_thrown = false;
            string error_message = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.Get_amount(sheet_name, row_number, column_number);
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
        public void Can_read_amount_identified_by_amount_code()
        {
            // Arrange
            double expected_amount = 54.97;
            string sheet_name = TestSheetNames.Budget_out;
            string amount_code = "Code003";
            int amount_column = 3;

            // Act
            double actual_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_code, amount_column);

            // Assert
            Assert.AreEqual(expected_amount, actual_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_sensible_error_if_amount_identified_by_amount_code_is_not_present()
        {
            // Arrange
            string sheet_name = TestSheetNames.Budget_out;
            string non_existent_code = "Does not exist";
            bool exception_thrown = false;
            string error_message = String.Empty;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.Get_amount(sheet_name, non_existent_code);
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
        public void Can_update_date_identified_by_row_and_column()
        {
            // Arrange
            string sheet_name = TestSheetNames.Test_record;
            int date_row = 10;
            int date_column = 1;
            DateTime previous_date = _excelSpreadsheet.Get_date(sheet_name, date_row, date_column);
            DateTime new_date = previous_date.AddDays(1);

            // Act
            _excelSpreadsheet.Update_date(sheet_name, date_row, date_column, new_date);

            // Assert
            DateTime updated_date = _excelSpreadsheet.Get_date(sheet_name, date_row, date_column);
            Assert.AreEqual(new_date, updated_date);
            Assert.AreNotEqual(previous_date, updated_date);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_update_text_identified_by_row_and_column()
        {
            // Arrange
            string sheet_name = TestSheetNames.Test_record;
            int text_row = 10;
            int text_column = 5;
            string previous_text = _excelSpreadsheet.Get_text(sheet_name, text_row, text_column);
            string new_text = previous_text + "z";

            // Act
            _excelSpreadsheet.Update_text(sheet_name, text_row, text_column, new_text);

            // Assert
            string updated_text = _excelSpreadsheet.Get_text(sheet_name, text_row, text_column);
            Assert.AreEqual(new_text, updated_text);
            Assert.AreNotEqual(previous_text, updated_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_update_amount_identified_by_row_and_column()
        {
            // Arrange
            string sheet_name = TestSheetNames.Test_record;
            int amount_row = 10;
            int amount_column = 2;
            double previous_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_row, amount_column);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.Update_amount(sheet_name, amount_row, amount_column, new_amount);

            // Assert
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_row, amount_column);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_update_amount_and_text_identified_by_amount_code()
        {
            // Arrange
            string sheet_name = TestSheetNames.Budget_out;
            string amount_code = "Code007";
            int text_column = 6;
            int amount_column = 3;
            int row = 9;
            double previous_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_code, amount_column);
            string previous_text = _excelSpreadsheet.Get_text(sheet_name, row, text_column);
            double new_amount = Math.Round(previous_amount + 1.2, 2);
            string new_text = previous_text + "z";

            // Act
            _excelSpreadsheet.Update_amount_and_text(sheet_name, amount_code, new_amount, new_text, amount_column, text_column);

            // Assert
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_code, amount_column);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
            string updated_text = _excelSpreadsheet.Get_text(sheet_name, row, text_column);
            Assert.AreEqual(new_text, updated_text);
            Assert.AreNotEqual(previous_text, updated_text);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_update_amount_identified_by_amount_code()
        {
            // Arrange
            string sheet_name = TestSheetNames.Expected_out;
            string amount_code = "Code005";
            double previous_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_code, 2);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.Update_amount(sheet_name, amount_code, new_amount);

            // Assert
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, amount_code, 2);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Will_throw_sensible_error_if_trying_to_update_amount_with_non_existent_amount_code()
        {
            // Arrange
            string sheet_name = TestSheetNames.Budget_out;
            string non_existent_code = "Does not exist";
            bool exception_thrown = false;
            string error_message = String.Empty;
            string expected_error = string.Format(ReconConsts.MissingCodeInWorksheet, non_existent_code, sheet_name);

            // Act
            try
            {
                _excelSpreadsheet.Update_amount(sheet_name, non_existent_code, 34.56);
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
        public void Updated_amount_is_still_present_after_spreadsheet_is_closed_and_reopened()
        {
            // Arrange
            const string amountCode = "Code004";
            var sheet_name = TestSheetNames.Expected_out;
            double previous_amount = _excelSpreadsheet.Get_amount(sheet_name, amountCode, 2);
            double new_amount = Math.Round(previous_amount + 1.2, 2);

            // Act
            _excelSpreadsheet.Update_amount(sheet_name, amountCode, new_amount);
            Close_and_reopen_spreadsheet();

            // Assert
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, amountCode, 2);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount, updated_amount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_get_subset_of_columns_as_rows_converted_to_records()
        {
            // Arrange
            const int expectedNumItems = 9;
            string sheet_name = TestSheetNames.Budget_out;
            int first_row_number = 3;
            int last_row_number = 11;
            const int firstColumnNumber = 2;
            const int lastColumnNumber = 6;

            // Act
            List<BankRecord> budget_items = _excelSpreadsheet.Get_rows_as_records<BankRecord>(
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
        public void Can_insert_new_row()
        {
            // Arrange
            var sheet_name = TestSheetNames.Expected_out;
            const int testRowNumber = 12;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int previous_num_expected_out_rows = _excelSpreadsheet.Last_row_number(sheet_name);
            string previous_amount_text = _excelSpreadsheet.Get_text(sheet_name, testRowNumber, amountColumn);
            double new_amount = 79;
            string previous_notes_text = _excelSpreadsheet.Get_text(sheet_name, testRowNumber, notesColumn);
            string new_notes = "Some new notes about some new stuff";
            var cell_values = new Dictionary<int, object>()
            {
                { amountColumn, new_amount },
                { notesColumn, new_notes }
            };

            // Act
            _excelSpreadsheet.Insert_new_row(sheet_name, testRowNumber, cell_values);

            // Assert
            int new_num_expected_out_rows = _excelSpreadsheet.Last_row_number(sheet_name);
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, testRowNumber, amountColumn);
            string updated_notes = _excelSpreadsheet.Get_text(sheet_name, testRowNumber, notesColumn);
            Assert.AreEqual(previous_num_expected_out_rows + 1, new_num_expected_out_rows);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreNotEqual(previous_amount_text, updated_amount.ToString());
            Assert.AreEqual(new_notes, updated_notes);
            Assert.AreNotEqual(previous_notes_text, updated_notes);

            // Teardown
            _excelSpreadsheet.Delete_specified_rows(sheet_name, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_append_new_row()
        {
            // Arrange
            var sheet_name = TestSheetNames.Cred_card;
            const int testRowNumber = 12;
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;
            int previous_num_test_record_rows = _excelSpreadsheet.Last_row_number(sheet_name);
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
            _excelSpreadsheet.Append_new_row(sheet_name, cell_values);

            // Assert
            int new_num_test_record_rows = _excelSpreadsheet.Last_row_number(sheet_name);
            DateTime updated_date = _excelSpreadsheet.Get_date(sheet_name, testRowNumber, dateColumn);
            double updated_amount = _excelSpreadsheet.Get_amount(sheet_name, testRowNumber, amountColumn);
            string updated_description = _excelSpreadsheet.Get_text(sheet_name, testRowNumber, descriptionColumn);
            Assert.AreEqual(previous_num_test_record_rows + 1, new_num_test_record_rows);
            Assert.AreEqual(new_date, updated_date);
            Assert.AreEqual(new_amount, updated_amount);
            Assert.AreEqual(new_description, updated_description);

            // Teardown
            _excelSpreadsheet.Delete_specified_rows(sheet_name, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_find_last_row_number_of_specified_sheet()
        {
            // Act
            var result = _excelSpreadsheet.Last_row_number(TestSheetNames.Cred_card);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_read_specified_row_given_row_number()
        {
            // Arrange
            var row_number = 2;

            // Act
            var result = _excelSpreadsheet.Read_specified_row(TestSheetNames.Actual_bank, row_number);

            // Assert
            Assert.AreEqual("Thing", result.Read_cell(1));
            Assert.AreEqual("description", result.Read_cell(2));
            Assert.AreEqual(4567.89, result.Read_cell(3));
            Assert.AreEqual(7898.88, result.Read_cell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_read_specified_row_based_on_specified_range_of_columns()
        {
            // Arrange
            var row_number = 2;
            var start_column = 3;
            var end_column = 4;

            // Act
            var result = _excelSpreadsheet.Read_specified_row(TestSheetNames.Actual_bank, row_number, start_column, end_column);

            // Assert
            Assert.AreEqual(2, result.Count, "Number of cells in row");
            Assert.AreEqual("description", result.Read_cell(0));
            Assert.AreEqual(4567.89, result.Read_cell(1));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void Can_find_first_empty_row_in_column_for_specified_sheet()
        {
            // Arrange
            var column_number = 1;

            // Act
            int result = _excelSpreadsheet.Find_first_empty_row_in_column(TestSheetNames.Test_record, column_number);

            // Assert
            Assert.AreEqual(13, result);
        }
    }
}