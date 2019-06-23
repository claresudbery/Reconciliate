using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Spreadsheets
{
    [TestFixture]
    public class SpreadSheetTests
    {
        private struct BudgetDataSetup<TRecordType>
        {
            public string Desc1;
            public string Desc2;
            public List<TRecordType> MonthlyRecords;
            public BudgetingMonths BudgetingMonths;
        }

        public SpreadSheetTests()
        {
            TestHelper.Set_correct_date_formatting();
        }

        private BudgetDataSetup<TRecordType> When_adding_budgeted_data_to_spreadsheet<TRecordType>(
            int first_month,
            int last_month,
            string sheet_name,
            Mock<ISpreadsheetRepo> mock_spreadsheet_repo,
            string first_divider,
            string second_divider,
            int second_budget_out_column,
            int default_day = 5) where TRecordType : ICSVRecord, new()
        {
            // Arrange
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = first_month,
                Last_month_for_budget_planning = last_month,
                Start_year = 2018
            };
            var first_monthly_row = 3;
            var last_monthly_row = first_monthly_row + 2;
            int first_budget_out_column = 2;
            string desc1 = "first monthly record";
            string desc2 = "second monthly record";
            var monthly_records = new List<TRecordType>
            {
                new TRecordType {Date = new DateTime(budgeting_months.Start_year, first_month, default_day), Description = desc1},
                new TRecordType {Date = new DateTime(budgeting_months.Start_year, first_month, 20), Description = desc2}
            };
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, first_divider, SpreadsheetConsts.DefaultDividerColumn)).Returns(first_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, second_divider, SpreadsheetConsts.DefaultDividerColumn)).Returns(last_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<TRecordType>(
                sheet_name,
                first_monthly_row + 1,
                last_monthly_row - 1,
                first_budget_out_column,
                second_budget_out_column)).Returns(monthly_records);

            return new BudgetDataSetup<TRecordType>
            {
                Desc1 = desc1,
                Desc2 = desc2,
                BudgetingMonths = budgeting_months,
                MonthlyRecords = monthly_records
            };
        }

        private void Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<TRecordType>(
            int first_month,
            int last_month,
            int monthly_record_count,
            List<TRecordType> pending_file_records,
            BudgetingMonths budgeting_months,
            string desc1,
            string desc2) where TRecordType : ICSVRecord, new()
        {
            // Arrange
            var num_repetitions = last_month >= first_month
                ? (last_month - first_month) + 1
                : ((last_month + 12) - first_month) + 1;
            var total_records = num_repetitions * monthly_record_count;

            // Assert
            Assert.AreEqual(total_records, pending_file_records.Count, "total num records");
            Assert.AreEqual(num_repetitions, pending_file_records.Count(x => x.Description == desc1), "num repetitions of desc1");
            Assert.AreEqual(num_repetitions, pending_file_records.Count(x => x.Description == desc2), "num repetitions of desc2");
            var desc1_records = pending_file_records.Where(x => x.Description == desc1)
                .OrderBy(x => x.Date).ToList();
            int index = 0;
            int final_month = last_month >= first_month ? last_month : last_month + 12;
            for (int month = first_month; month <= final_month; month++)
            {
                var expected_month = month <= 12 ? month : month - 12;
                var expected_year = month <= 12 ? budgeting_months.Start_year : budgeting_months.Start_year + 1;
                Assert.AreEqual(expected_month, desc1_records[index].Date.Month, "correct month");
                Assert.AreEqual(expected_year, desc1_records[index].Date.Year, "correct year");
                index++;
            }
        }

        [Test]
        public void M_CanReadLastRowOfSpecifiedWorksheetAsObjectList()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var fake_cell_row = new FakeCellRow().With_fake_data(new List<object>
            {
                "22/03/2018",
                "22.34",
                "last row",
                "7788"
            });
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Read_last_row(sheet_name))
                .Returns(fake_cell_row);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            ICellRow actual_row = spreadsheet.Read_last_row(sheet_name);

            // Assert
            Assert.AreEqual(fake_cell_row.Read_cell(0), actual_row.Read_cell(0));
            Assert.AreEqual(fake_cell_row.Read_cell(1), actual_row.Read_cell(1));
            Assert.AreEqual(fake_cell_row.Read_cell(2), actual_row.Read_cell(2));
            Assert.AreEqual(fake_cell_row.Read_cell(3), actual_row.Read_cell(3));
        }

        [Test]
        public void M_CanReadLastRowOfSpecifiedWorksheetAsCsv()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            String expected_csv = "22/03/2018,£22.34,\"last row\",7788";
            mock_spreadsheet_repo.Setup(x => x.Read_last_row_as_csv(sheet_name, It.IsAny<ICSVRecord>()))
                .Returns(expected_csv);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.Read_last_row_as_csv(sheet_name, new BankRecord());

            // Assert
            Assert.AreEqual(expected_csv, result);
        }

        [Test]
        public void M_WillAppendAllRowsInCsvFileToSpecifiedWorksheet()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var mock_file_io = new Mock<IFileIO<BankRecord>>();
            mock_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var csv_file = new CSVFile<BankRecord>(mock_file_io.Object);
            csv_file.Load();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Append_csv_file<BankRecord>(sheet_name, csv_file))
                .Verifiable();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Append_csv_file<BankRecord>(sheet_name, csv_file);

            // Clean up
            mock_spreadsheet_repo.Verify(x => x.Append_csv_file<BankRecord>(sheet_name, csv_file), Times.Once); 
        }

        [Test]
        public void M_WillFindRowNumberOfLastDividerRow()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            int expected_row_number = 9;
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    sheet_name,
                    Dividers.Divider_text,
                    SpreadsheetConsts.DefaultDividerColumn))
                .Returns(expected_row_number);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.Find_row_number_of_last_divider_row(sheet_name);

            // Assert
            Assert.AreEqual(expected_row_number, result);
        }

        [Test]
        public void M_CanAddUnreconciledRowsToCredCard1InOutCsvFileAndPutInDateOrder()
        {
            // Arrange
            var sheet_name = "MockSheet";
            double date = 43345;
            var amount = 22.34;
            var text1 = "first row";
            var text2 = "second row";
            var text3 = "third row";
            var fake_cell_row1 = new FakeCellRow().With_fake_data(new List<object> { date, amount, null, text2 });
            var fake_cell_row3 = new FakeCellRow().With_fake_data(new List<object> { date + 1, amount, null, text3 });
            var fake_cell_row2 = new FakeCellRow().With_fake_data(new List<object> { date - 1, amount, null, text1 });
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Divider_text, SpreadsheetConsts.DefaultDividerColumn))
                .Returns(1);
            mock_spreadsheet_repo.Setup(x => x.Last_row_number(sheet_name)).Returns(4);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(sheet_name, 2)).Returns(fake_cell_row1);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(sheet_name, 3)).Returns(fake_cell_row2);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(sheet_name, 4)).Returns(fake_cell_row3);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            var expected_num_records = 3;

            var csv_file = new CSVFileFactory<CredCard1InOutRecord>().Create_csv_file(false);

            // Act
            spreadsheet.Add_unreconciled_rows_to_csv_file<CredCard1InOutRecord>(sheet_name, csv_file);

            // Assert
            Assert.AreEqual(expected_num_records, csv_file.Records.Count);
            Assert.IsTrue(csv_file.Records[0].Source_line.Contains(text1), $"line 0 should contain correct text");
            Assert.IsTrue(csv_file.Records[1].Source_line.Contains(text2), $"line 1 should contain correct text");
            Assert.IsTrue(csv_file.Records[2].Source_line.Contains(text3), $"line 1 should contain correct text");
        }

        [Test]
        public void M_CanReadAllMonthlyBudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Sodds,
                End_divider = Dividers.Cred_card1,
                First_column_number = 2,
                Last_column_number = 6
            };
            const int expectedFirstRowNumber = 11;
            const int expectedLastRowNumber = 25;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    budget_item_list_data.Sheet_name, budget_item_list_data.Start_divider, SpreadsheetConsts.DefaultDividerColumn))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    budget_item_list_data.Sheet_name, budget_item_list_data.End_divider, SpreadsheetConsts.DefaultDividerColumn))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Get_all_budget_items<BankRecord>(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Get_rows_as_records<BankRecord>(
                budget_item_list_data.Sheet_name,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.First_column_number,
                budget_item_list_data.Last_column_number));
        }

        [Test]
        public void M_CanReadBalanceFromPocketMoneySpreadsheetBasedOnDate()
        {
            // Arrange
            var expected_date_time = new DateTime(2018, 11, 25).ToShortDateString();
            var expected_sheet_name = PocketMoneySheetNames.Second_child;
            const int expectedDateColumn = 1;
            const int expectedAmountColumn = 4;
            const int expectedRowNumber = 10;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    expected_sheet_name, expected_date_time, expectedDateColumn))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            
            // Act
            spreadsheet.Get_second_child_pocket_money_amount(expected_date_time);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Get_amount(
                expected_sheet_name,
                expectedRowNumber,
                expectedAmountColumn));
        }

        [Test]
        public void M_CanInsertNewRowOnExpectedOut()
        {
            // Arrange
            var expected_sheet_name = MainSheetNames.Expected_out;
            const int expectedRowNumber = 21;
            double new_amount = 79;
            string new_notes = "Acme Blasters membership";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_first_row_containing_cell(
                    expected_sheet_name,
                    ReconConsts.ExpectedOutInsertionPoint,
                    4))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Insert_new_row_on_expected_out(new_amount, new_notes);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Insert_new_row(
                expected_sheet_name,
                expectedRowNumber,
                It.Is<Dictionary<int, object>>(y => y.Values.Contains(new_amount) && y.Values.Contains(new_notes))));
        }

        [Test]
        public void M_CanRead_ExpensesAlreadyDone_FromPlanningSpreadsheet()
        {
            // Arrange
            var expected_sheet_name = PlanningSheetNames.Expenses;
            const int expectedRowNumber = 2;
            const int expectedColumnNumber = 4;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Get_planning_expenses_already_done();

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Get_amount(
                expected_sheet_name,
                expectedRowNumber,
                expectedColumnNumber));
        }

        [Test]
        public void M_CanRead_MoneyPaidByGuests_FromPlanningSpreadsheet()
        {
            // Arrange
            var expected_sheet_name = PlanningSheetNames.Deposits;
            const int expectedRowNumber = 2;
            const int expectedColumnNumber = 4;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Get_planning_money_paid_by_guests();

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Get_amount(
                expected_sheet_name,
                expectedRowNumber,
                expectedColumnNumber));
        }

        [Test]
        public void M_CanAddNewTransactionToEndOfCredCard3Sheet()
        {
            // Arrange
            var expected_sheet_name = MainSheetNames.Cred_card3;
            DateTime new_date = new DateTime(2018, 11, 25);
            double new_amount = 32.45;
            string new_description = "minimum monthly payment";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Add_new_transaction_to_cred_card3(new_date, new_amount, new_description);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Append_new_row(
                expected_sheet_name,
                It.Is<Dictionary<int, object>>(
                    y => y.Values.Contains(new_amount) 
                    && y.Values.Contains(new_date.ToOADate())
                    && y.Values.Contains(new_description))));
        }

        [Test]
        public void M_CanAddNewTransactionToEndOfSavginsSheet()
        {
            // Arrange
            var expected_sheet_name = MainSheetNames.Savings;
            DateTime new_date = new DateTime(2018, 11, 25);
            double new_amount = 32.45;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            int expected_row_number = 49;
            int first_column = 1;
            mock_spreadsheet_repo.Setup(x => x.Find_first_empty_row_in_column(
                    expected_sheet_name,
                    first_column))
                .Returns(expected_row_number);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.Add_new_transaction_to_savings(new_date, new_amount);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.Update_date(
                expected_sheet_name,
                expected_row_number,
                first_column,
                new_date));
            mock_spreadsheet_repo.Verify(x => x.Update_amount(
                expected_sheet_name,
                expected_row_number,
                first_column + 1,
                new_amount));
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillUseMortgageTransactionsToDetermineNextUnplannedMonth()
        {
            // Arrange
            int code_column = SpreadsheetConsts.DefaultCodeColumn;
            int expected_budget_out_row_number = 10;
            int expected_bank_out_row_number = 11;
            DateTime expected_last_planned_date = DateTime.Today;
            string mortgage_description = "PASTA PLASTER";
            var mock_cell_row = new Mock<ICellRow>();
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.TypeIndex)).Returns("");
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    MainSheetNames.Budget_out,
                    Codes.Code042,
                    code_column))
                .Returns(expected_budget_out_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(
                    MainSheetNames.Budget_out,
                    expected_budget_out_row_number, 2, 6))
                .Returns(mock_cell_row.Object);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DescriptionIndex)).Returns(mortgage_description);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    MainSheetNames.Bank_out,
                    mortgage_description,
                    BankRecord.DescriptionIndex + 1))
                .Returns(expected_bank_out_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(
                    MainSheetNames.Bank_out,
                    expected_bank_out_row_number))
                .Returns(mock_cell_row.Object);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DateIndex)).Returns(expected_last_planned_date.ToOADate());
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.Get_next_unplanned_month();

            // Assert
            Assert.AreEqual(expected_last_planned_date.AddMonths(1).Month, result.Month);
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillThrowExceptionIfMortgageRowCannotBeFound()
        {
            // Arrange
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Throws(new Exception());
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            bool exception = false;

            // Act
            try
            {
                spreadsheet.Get_next_unplanned_month();
            }
            catch (Exception)
            {
                exception = true;
            }

            // Assert
            Assert.IsTrue(exception);
        }

        [Test]
        public void M_WillReturnMostRecentRowContainingSpecifiedText()
        {
            // Arrange
            var spreadsheet_name = MainSheetNames.Bank_out;
            var text_to_search_for = ReconConsts.Cred_card2_dd_description;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var cred_card1_dd_row_number = 10;
            var mock_cell_row = new Mock<ICellRow>();
            var description = "found row";
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DescriptionIndex)).Returns(description);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.TypeIndex)).Returns(string.Empty);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DateIndex)).Returns(DateTime.Today.ToOADate());
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_with_cell_containing_text(
                    spreadsheet_name, text_to_search_for, new List<int> {ReconConsts.DescriptionColumn}))
                .Returns(cred_card1_dd_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(spreadsheet_name, cred_card1_dd_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            BankRecord bank_row = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                spreadsheet_name, 
                text_to_search_for,
                new List<int> {ReconConsts.DescriptionColumn});

            // Assert
            Assert.AreEqual(description, bank_row.Description);
        }

        [TestCase(1, 2)]
        [TestCase(12, 1)]
        public void M_WhenAddingBudgetedBankOutDataToSpreadsheet_WillAddAnnualBankOutTransactionsToPendingFile_IfMonthMatchesBudgetingMonths(
            int first_month, int last_month)
        {
            // Arrange
            var sheet_name = MainSheetNames.Budget_out;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Monthly setup:
            var first_monthly_row = 3;
            var last_monthly_row = first_monthly_row + 2;
            var monthly_bank_records = new List<BankRecord>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Sodds, SpreadsheetConsts.DefaultDividerColumn)).Returns(first_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Cred_card1, SpreadsheetConsts.DefaultDividerColumn)).Returns(last_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<BankRecord>(sheet_name, first_monthly_row + 1, last_monthly_row - 1, 2, 6)).Returns(monthly_bank_records);
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            string desc1 = "annual record with matching month";
            string desc2 = "other annual record with matching month";
            string desc3 = "annual record with non-matching month";
            var annual_bank_records = new List<BankRecord>
            {
                new BankRecord { Date = new DateTime(2018, first_month, 1), Description = desc1 },
                new BankRecord { Date = new DateTime(2018, last_month, 1), Description = desc2 },
                new BankRecord { Date = new DateTime(2018, last_month + 2, 1), Description = desc3 }
            };
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_sodds, SpreadsheetConsts.DefaultDividerColumn)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_total, SpreadsheetConsts.DefaultDividerColumn)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = first_month,
                Last_month_for_budget_planning = last_month,
                Start_year = 2018
            };
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Sodds,
                End_divider = Dividers.Cred_card1,
                First_column_number = 2,
                Last_column_number = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Annual_sodds,
                End_divider = Dividers.Annual_total,
                First_column_number = 2,
                Last_column_number = 6
            };

            // Act
            spreadsheet.Add_budgeted_bank_out_data_to_pending_file<BankRecord>(
                budgeting_months, 
                mock_pending_file.Object, 
                monthly_budget_item_list_data, 
                annual_budget_item_list_data);

            // Assert
            Assert.AreEqual(2, pending_file_records.Count, "total num records");
            Assert.AreEqual(1, pending_file_records.Count(x => x.Description == desc1), "num repetitions of matching record");
            Assert.AreEqual(1, pending_file_records.Count(x => x.Description == desc2), "num repetitions of other matching record");
            Assert.AreEqual(0, pending_file_records.Count(x => x.Description == desc3), "num repetitions of non-matching record");
            var expected_record2_year = last_month >= first_month
                ? budgeting_months.Start_year
                : budgeting_months.Start_year + 1;
            Assert.AreEqual(expected_record2_year, pending_file_records[1].Date.Year);
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(12, 1)]
        [TestCase(12, 4)]
        [TestCase(10, 1)]
        public void M2_WhenAddingBudgetedBankOutDataToSpreadsheet_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged(
            int first_month, int last_month)
        {
            // Arrange
            var sheet_name = MainSheetNames.Budget_out;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            var annual_bank_records = new List<BankRecord>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_sodds, SpreadsheetConsts.DefaultDividerColumn)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_total, SpreadsheetConsts.DefaultDividerColumn)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var budget_data_setup = When_adding_budgeted_data_to_spreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Sodds,
                Dividers.Cred_card1,
                6);
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Sodds,
                End_divider = Dividers.Cred_card1,
                First_column_number = 2,
                Last_column_number = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Annual_sodds,
                End_divider = Dividers.Annual_total,
                First_column_number = 2,
                Last_column_number = 6
            };

            // Act
            spreadsheet.Add_budgeted_bank_out_data_to_pending_file<BankRecord>(
                budget_data_setup.BudgetingMonths, 
                mock_pending_file.Object, 
                monthly_budget_item_list_data, 
                annual_budget_item_list_data);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<BankRecord>(
                first_month,
                last_month,
                budget_data_setup.MonthlyRecords.Count,
                pending_file_records,
                budget_data_setup.BudgetingMonths,
                budget_data_setup.Desc1,
                budget_data_setup.Desc2);
        }

        [Test]
        public void M_WhenAddingBudgetedBankOutDataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheet_name = MainSheetNames.Budget_out;
            var first_month = 12;
            var last_month = 3;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Monthly set up:
            var budget_data_setup = When_adding_budgeted_data_to_spreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Sodds,
                Dividers.Cred_card1,
                6);
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            var annual_bank_records = new List<BankRecord>{
                new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.Start_year, first_month, 28)},
                new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.Start_year, last_month, 2)}
            };
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_sodds, SpreadsheetConsts.DefaultDividerColumn)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(sheet_name, Dividers.Annual_total, SpreadsheetConsts.DefaultDividerColumn)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<BankRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.Start_year, first_month, 10)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.Start_year, first_month, 4)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.Start_year, first_month, 25)}
                });
            var pending_file = new CSVFile<BankRecord>(mock_cred_card2_in_out_file_io.Object);
            pending_file.Load();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Sodds,
                End_divider = Dividers.Cred_card1,
                First_column_number = 2,
                Last_column_number = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_out,
                Start_divider = Dividers.Annual_sodds,
                End_divider = Dividers.Annual_total,
                First_column_number = 2,
                Last_column_number = 6
            };

            // Act
            spreadsheet.Add_budgeted_bank_out_data_to_pending_file<BankRecord>(
                budget_data_setup.BudgetingMonths, 
                pending_file, 
                monthly_budget_item_list_data, 
                annual_budget_item_list_data);

            // Assert
            BankRecord previous_record = null;
            foreach (BankRecord record in pending_file.Records)
            {
                if (null != previous_record)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previous_record.Date.ToOADate());
                }
                previous_record = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedDataToSpreadsheet_WillAdjustDayIfDaysInMonthIsTooLow()
        {
            // Arrange
            var sheet_name = MainSheetNames.Budget_in;
            var first_month = 12;
            var last_month = 2;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = When_adding_budgeted_data_to_spreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Date,
                Dividers.Total,
                6,
                default_day: 31);
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            bool exception_thrown = false;
            var budget_item_list_data = new BudgetItemListData
            {
                Sheet_name = MainSheetNames.Budget_in,
                Start_divider = Dividers.Date,
                End_divider = Dividers.Total,
                First_column_number = 2,
                Last_column_number = 6
            };

            // Act
            try
            {
                spreadsheet.Add_budgeted_bank_in_data_to_pending_file<BankRecord>(
                    budget_data_setup.BudgetingMonths, 
                    mock_pending_file.Object, 
                    budget_item_list_data);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }
    }
}
