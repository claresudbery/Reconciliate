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
            TestHelper.SetCorrectDateFormatting();
        }

        private BudgetDataSetup<TRecordType> WhenAddingBudgetedDataToSpreadsheet<TRecordType>(
            int firstMonth,
            int lastMonth,
            string sheetName,
            Mock<ISpreadsheetRepo> mockSpreadsheetRepo,
            string firstDivider,
            string secondDivider,
            int secondBudgetOutColumn,
            int defaultDay = 5) where TRecordType : ICSVRecord, new()
        {
            // Arrange
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = firstMonth,
                LastMonthForBudgetPlanning = lastMonth,
                StartYear = 2018
            };
            var first_monthly_row = 3;
            var last_monthly_row = first_monthly_row + 2;
            int first_budget_out_column = 2;
            string desc1 = "first monthly record";
            string desc2 = "second monthly record";
            var monthly_records = new List<TRecordType>
            {
                new TRecordType {Date = new DateTime(budgeting_months.StartYear, firstMonth, defaultDay), Description = desc1},
                new TRecordType {Date = new DateTime(budgeting_months.StartYear, firstMonth, 20), Description = desc2}
            };
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, firstDivider, 2)).Returns(first_monthly_row);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, secondDivider, 2)).Returns(last_monthly_row);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<TRecordType>(
                sheetName,
                first_monthly_row + 1,
                last_monthly_row - 1,
                first_budget_out_column,
                secondBudgetOutColumn)).Returns(monthly_records);

            return new BudgetDataSetup<TRecordType>
            {
                Desc1 = desc1,
                Desc2 = desc2,
                BudgetingMonths = budgeting_months,
                MonthlyRecords = monthly_records
            };
        }

        private void Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<TRecordType>(
            int firstMonth,
            int lastMonth,
            int monthlyRecordCount,
            List<TRecordType> pendingFileRecords,
            BudgetingMonths budgetingMonths,
            string desc1,
            string desc2) where TRecordType : ICSVRecord, new()
        {
            // Arrange
            var num_repetitions = lastMonth >= firstMonth
                ? (lastMonth - firstMonth) + 1
                : ((lastMonth + 12) - firstMonth) + 1;
            var total_records = num_repetitions * monthlyRecordCount;

            // Assert
            Assert.AreEqual(total_records, pendingFileRecords.Count, "total num records");
            Assert.AreEqual(num_repetitions, pendingFileRecords.Count(x => x.Description == desc1), "num repetitions of desc1");
            Assert.AreEqual(num_repetitions, pendingFileRecords.Count(x => x.Description == desc2), "num repetitions of desc2");
            var desc1_records = pendingFileRecords.Where(x => x.Description == desc1)
                .OrderBy(x => x.Date).ToList();
            int index = 0;
            int final_month = lastMonth >= firstMonth ? lastMonth : lastMonth + 12;
            for (int month = firstMonth; month <= final_month; month++)
            {
                var expected_month = month <= 12 ? month : month - 12;
                var expected_year = month <= 12 ? budgetingMonths.StartYear : budgetingMonths.StartYear + 1;
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
            var fake_cell_row = new FakeCellRow().WithFakeData(new List<object>
            {
                "22/03/2018",
                "22.34",
                "last row",
                "7788"
            });
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.ReadLastRow(sheet_name))
                .Returns(fake_cell_row);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            ICellRow actual_row = spreadsheet.ReadLastRow(sheet_name);

            // Assert
            Assert.AreEqual(fake_cell_row.ReadCell(0), actual_row.ReadCell(0));
            Assert.AreEqual(fake_cell_row.ReadCell(1), actual_row.ReadCell(1));
            Assert.AreEqual(fake_cell_row.ReadCell(2), actual_row.ReadCell(2));
            Assert.AreEqual(fake_cell_row.ReadCell(3), actual_row.ReadCell(3));
        }

        [Test]
        public void M_CanReadLastRowOfSpecifiedWorksheetAsCsv()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            String expected_csv = "22/03/2018,£22.34,\"last row\",7788";
            mock_spreadsheet_repo.Setup(x => x.ReadLastRowAsCsv(sheet_name, It.IsAny<ICSVRecord>()))
                .Returns(expected_csv);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.ReadLastRowAsCsv(sheet_name, new BankRecord());

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
            mock_spreadsheet_repo.Setup(x => x.AppendCsvFile<BankRecord>(sheet_name, csv_file))
                .Verifiable();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.AppendCsvFile<BankRecord>(sheet_name, csv_file);

            // Clean up
            mock_spreadsheet_repo.Verify(x => x.AppendCsvFile<BankRecord>(sheet_name, csv_file), Times.Once); 
        }

        [Test]
        public void M_WillFindRowNumberOfLastDividerRow()
        {
            // Arrange
            var sheet_name = "MockSheet";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            int expected_row_number = 9;
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    sheet_name,
                    Dividers.DividerText,
                    2))
                .Returns(expected_row_number);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.FindRowNumberOfLastDividerRow(sheet_name);

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
            var fake_cell_row1 = new FakeCellRow().WithFakeData(new List<object> { date, amount, null, text2 });
            var fake_cell_row3 = new FakeCellRow().WithFakeData(new List<object> { date + 1, amount, null, text3 });
            var fake_cell_row2 = new FakeCellRow().WithFakeData(new List<object> { date - 1, amount, null, text1 });
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.DividerText, 2))
                .Returns(1);
            mock_spreadsheet_repo.Setup(x => x.LastRowNumber(sheet_name)).Returns(4);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(sheet_name, 2)).Returns(fake_cell_row1);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(sheet_name, 3)).Returns(fake_cell_row2);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(sheet_name, 4)).Returns(fake_cell_row3);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            var expected_num_records = 3;

            var csv_file = new CSVFileFactory<CredCard1InOutRecord>().CreateCSVFile(false);

            // Act
            spreadsheet.AddUnreconciledRowsToCsvFile<CredCard1InOutRecord>(sheet_name, csv_file);

            // Assert
            Assert.AreEqual(expected_num_records, csv_file.Records.Count);
            Assert.IsTrue(csv_file.Records[0].SourceLine.Contains(text1), $"line 0 should contain correct text");
            Assert.IsTrue(csv_file.Records[1].SourceLine.Contains(text2), $"line 1 should contain correct text");
            Assert.IsTrue(csv_file.Records[2].SourceLine.Contains(text3), $"line 1 should contain correct text");
        }

        [Test]
        public void M_CanReadAllMonthlyBankOutBudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            const int expectedFirstRowNumber = 11;
            const int expectedLastRowNumber = 25;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.GetAllMonthlyBankOutBudgetItems(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetRowsAsRecords<BankRecord>(
                budget_item_list_data.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.FirstColumnNumber,
                budget_item_list_data.LastColumnNumber));
        }

        [Test]
        public void M_CanReadAllMonthlyBankInBudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            const int expectedFirstRowNumber = 3;
            const int expectedLastRowNumber = 44;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.GetAllMonthlyBankInBudgetItems(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetRowsAsRecords<BankRecord>(
                budget_item_list_data.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.FirstColumnNumber,
                budget_item_list_data.LastColumnNumber));
        }

        [Test]
        public void M_CanReadAllMonthlyCredCard1BudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };
            const int expectedFirstRowNumber = 6;
            const int expectedLastRowNumber = 20;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.GetAllMonthlyCredCard1BudgetItems(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetRowsAsRecords<CredCard1InOutRecord>(
                budget_item_list_data.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.FirstColumnNumber,
                budget_item_list_data.LastColumnNumber));
        }

        [Test]
        public void M_CanReadAllMonthlyCredCard2BudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard2,
                EndDivider = Dividers.SODDTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };
            const int expectedFirstRowNumber = 50;
            const int expectedLastRowNumber = 60;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.GetAllMonthlyCredCard2BudgetItems(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetRowsAsRecords<CredCard2InOutRecord>(
                budget_item_list_data.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.FirstColumnNumber,
                budget_item_list_data.LastColumnNumber));
        }

        [Test]
        public void M_CanReadAllAnnualBudgetItems()
        {
            // Arrange
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            const int expectedFirstRowNumber = 1;
            const int expectedLastRowNumber = 11;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budget_item_list_data.SheetName, budget_item_list_data.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.GetAllAnnualBudgetItems<BankRecord>(budget_item_list_data);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetRowsAsRecords<BankRecord>(
                budget_item_list_data.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budget_item_list_data.FirstColumnNumber,
                budget_item_list_data.LastColumnNumber));
        }

        [Test]
        public void M_CanReadBalanceFromPocketMoneySpreadsheetBasedOnDate()
        {
            // Arrange
            var expected_date_time = new DateTime(2018, 11, 25).ToShortDateString();
            var expected_sheet_name = PocketMoneySheetNames.SecondChild;
            const int expectedDateColumn = 1;
            const int expectedAmountColumn = 4;
            const int expectedRowNumber = 10;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    expected_sheet_name, expected_date_time, expectedDateColumn))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            
            // Act
            spreadsheet.GetSecondChildPocketMoneyAmount(expected_date_time);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetAmount(
                expected_sheet_name,
                expectedRowNumber,
                expectedAmountColumn));
        }

        [Test]
        public void M_CanInsertNewRowOnExpectedOut()
        {
            // Arrange
            var expected_sheet_name = MainSheetNames.ExpectedOut;
            const int expectedRowNumber = 21;
            double new_amount = 79;
            string new_notes = "Acme Blasters membership";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfFirstRowContainingCell(
                    expected_sheet_name,
                    ReconConsts.ExpectedOutInsertionPoint,
                    4))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.InsertNewRowOnExpectedOut(new_amount, new_notes);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.InsertNewRow(
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
            spreadsheet.GetPlanningExpensesAlreadyDone();

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetAmount(
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
            spreadsheet.GetPlanningMoneyPaidByGuests();

            // Assert
            mock_spreadsheet_repo.Verify(x => x.GetAmount(
                expected_sheet_name,
                expectedRowNumber,
                expectedColumnNumber));
        }

        [Test]
        public void M_CanAddNewTransactionToEndOfCredCard3Sheet()
        {
            // Arrange
            var expected_sheet_name = MainSheetNames.CredCard3;
            DateTime new_date = new DateTime(2018, 11, 25);
            double new_amount = 32.45;
            string new_description = "minimum monthly payment";
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.AddNewTransactionToCredCard3(new_date, new_amount, new_description);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.AppendNewRow(
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
            mock_spreadsheet_repo.Setup(x => x.FindFirstEmptyRowInColumn(
                    expected_sheet_name,
                    first_column))
                .Returns(expected_row_number);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            spreadsheet.AddNewTransactionToSavings(new_date, new_amount);

            // Assert
            mock_spreadsheet_repo.Verify(x => x.UpdateDate(
                expected_sheet_name,
                expected_row_number,
                first_column,
                new_date));
            mock_spreadsheet_repo.Verify(x => x.UpdateAmount(
                expected_sheet_name,
                expected_row_number,
                first_column + 1,
                new_amount));
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillUseMortgageTransactionsToDetermineNextUnplannedMonth()
        {
            // Arrange
            int code_column = 1;
            int expected_budget_out_row_number = 10;
            int expected_bank_out_row_number = 11;
            DateTime expected_last_planned_date = DateTime.Today;
            string mortgage_description = "PASTA PLASTER";
            var mock_cell_row = new Mock<ICellRow>();
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.TypeIndex)).Returns("");
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    MainSheetNames.BudgetOut,
                    Codes.Code042,
                    code_column))
                .Returns(expected_budget_out_row_number);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(
                    MainSheetNames.BudgetOut,
                    expected_budget_out_row_number, 2, 6))
                .Returns(mock_cell_row.Object);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.DescriptionIndex)).Returns(mortgage_description);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    MainSheetNames.BankOut,
                    mortgage_description,
                    BankRecord.DescriptionIndex + 1))
                .Returns(expected_bank_out_row_number);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(
                    MainSheetNames.BankOut,
                    expected_bank_out_row_number))
                .Returns(mock_cell_row.Object);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.DateIndex)).Returns(expected_last_planned_date.ToOADate());
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var result = spreadsheet.GetNextUnplannedMonth();

            // Assert
            Assert.AreEqual(expected_last_planned_date.AddMonths(1).Month, result.Month);
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillThrowExceptionIfMortgageRowCannotBeFound()
        {
            // Arrange
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Throws(new Exception());
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            bool exception = false;

            // Act
            try
            {
                spreadsheet.GetNextUnplannedMonth();
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
            var spreadsheet_name = MainSheetNames.BankOut;
            var text_to_search_for = ReconConsts.CredCard2DdDescription;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var cred_card1_dd_row_number = 10;
            var mock_cell_row = new Mock<ICellRow>();
            var description = "found row";
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.DescriptionIndex)).Returns(description);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.TypeIndex)).Returns(string.Empty);
            mock_cell_row.Setup(x => x.ReadCell(BankRecord.DateIndex)).Returns(DateTime.Today.ToOADate());
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowWithCellContainingText(
                    spreadsheet_name, text_to_search_for, new List<int> {ReconConsts.DescriptionColumn}))
                .Returns(cred_card1_dd_row_number);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(spreadsheet_name, cred_card1_dd_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            BankRecord bank_row = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                spreadsheet_name, 
                text_to_search_for,
                new List<int> {ReconConsts.DescriptionColumn});

            // Assert
            Assert.AreEqual(description, bank_row.Description);
        }

        [TestCase(1, 2)]
        [TestCase(12, 1)]
        public void M_WhenAddingBudgetedBankOutDataToSpreadsheet_WillAddAnnualBankOutTransactionsToPendingFile_IfMonthMatchesBudgetingMonths(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Monthly setup:
            var first_monthly_row = 3;
            var last_monthly_row = first_monthly_row + 2;
            var monthly_bank_records = new List<BankRecord>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.SODDs, 2)).Returns(first_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.CredCard1, 2)).Returns(last_monthly_row);
            mock_spreadsheet_repo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheet_name, first_monthly_row + 1, last_monthly_row - 1, 2, 6)).Returns(monthly_bank_records);
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            string desc1 = "annual record with matching month";
            string desc2 = "other annual record with matching month";
            string desc3 = "annual record with non-matching month";
            var annual_bank_records = new List<BankRecord>
            {
                new BankRecord { Date = new DateTime(2018, firstMonth, 1), Description = desc1 },
                new BankRecord { Date = new DateTime(2018, lastMonth, 1), Description = desc2 },
                new BankRecord { Date = new DateTime(2018, lastMonth + 2, 1), Description = desc3 }
            };
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualSODDs, 2)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualTotal, 2)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = firstMonth,
                LastMonthForBudgetPlanning = lastMonth,
                StartYear = 2018
            };
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budgeting_months, mock_pending_file.Object, monthly_budget_item_list_data, annual_budget_item_list_data);

            // Assert
            Assert.AreEqual(2, pending_file_records.Count, "total num records");
            Assert.AreEqual(1, pending_file_records.Count(x => x.Description == desc1), "num repetitions of matching record");
            Assert.AreEqual(1, pending_file_records.Count(x => x.Description == desc2), "num repetitions of other matching record");
            Assert.AreEqual(0, pending_file_records.Count(x => x.Description == desc3), "num repetitions of non-matching record");
            var expected_record2_year = lastMonth >= firstMonth
                ? budgeting_months.StartYear
                : budgeting_months.StartYear + 1;
            Assert.AreEqual(expected_record2_year, pending_file_records[1].Date.Year);
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(12, 1)]
        [TestCase(12, 4)]
        [TestCase(10, 1)]
        public void M2_WhenAddingBudgetedBankOutDataToSpreadsheet_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            var annual_bank_records = new List<BankRecord>();
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualSODDs, 2)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualTotal, 2)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.SODDs,
                Dividers.CredCard1,
                6);
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budget_data_setup.BudgetingMonths, mock_pending_file.Object, monthly_budget_item_list_data, annual_budget_item_list_data);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<BankRecord>(
                firstMonth,
                lastMonth,
                budget_data_setup.MonthlyRecords.Count,
                pending_file_records,
                budget_data_setup.BudgetingMonths,
                budget_data_setup.Desc1,
                budget_data_setup.Desc2);
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(12, 1)]
        [TestCase(12, 4)]
        [TestCase(10, 1)]
        public void M_WhenAddingBudgetedBankInDataToSpreadsheet_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetIn;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Date,
                Dividers.Total,
                6);
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankInDataToPendingFile(budget_data_setup.BudgetingMonths, mock_pending_file.Object, budget_item_list_data);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<BankRecord>(
                firstMonth,
                lastMonth,
                budget_data_setup.MonthlyRecords.Count,
                pending_file_records,
                budget_data_setup.BudgetingMonths,
                budget_data_setup.Desc1,
                budget_data_setup.Desc2);
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(12, 1)]
        [TestCase(12, 4)]
        [TestCase(10, 1)]
        public void M_WhenAddingBudgetedCredCard1InOutDataToSpreadsheet_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<CredCard1InOutRecord>(
                firstMonth,
                lastMonth,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.CredCard1,
                Dividers.CredCard2,
                5);
            var pending_file_records = new List<CredCard1InOutRecord>();
            var mock_pending_file = new Mock<ICSVFile<CredCard1InOutRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budget_data_setup.BudgetingMonths, mock_pending_file.Object, budget_item_list_data);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<CredCard1InOutRecord>(
                firstMonth,
                lastMonth,
                budget_data_setup.MonthlyRecords.Count,
                pending_file_records,
                budget_data_setup.BudgetingMonths,
                budget_data_setup.Desc1,
                budget_data_setup.Desc2);
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(12, 1)]
        [TestCase(12, 4)]
        [TestCase(10, 1)]
        public void M_WhenAddingBudgetedCredCard2InOutDataToSpreadsheet_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<CredCard2InOutRecord>(
                firstMonth,
                lastMonth,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.CredCard2,
                Dividers.SODDTotal,
                5);
            var pending_file_records = new List<CredCard2InOutRecord>();
            var mock_pending_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard2,
                EndDivider = Dividers.SODDTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budget_data_setup.BudgetingMonths, mock_pending_file.Object, budget_item_list_data);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<CredCard2InOutRecord>(
                firstMonth,
                lastMonth,
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
            var sheet_name = MainSheetNames.BudgetOut;
            var first_month = 12;
            var last_month = 3;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            // Monthly set up:
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.SODDs,
                Dividers.CredCard1,
                6);
            // Annual setup:
            var first_annual_row = 10;
            var last_annual_row = first_annual_row + 2;
            var annual_bank_records = new List<BankRecord>{
                new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 28)},
                new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, last_month, 2)}
            };
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualSODDs, 2)).Returns(first_annual_row);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheet_name, Dividers.AnnualTotal, 2)).Returns(last_annual_row);
            mock_spreadsheet_repo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheet_name, first_annual_row + 1, last_annual_row - 1, 2, 6)).Returns(annual_bank_records);
            // Everything else:
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<BankRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 10)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 4)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 25)}
                });
            var pending_file = new CSVFile<BankRecord>(mock_cred_card2_in_out_file_io.Object);
            pending_file.Load();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var monthly_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annual_budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budget_data_setup.BudgetingMonths, pending_file, monthly_budget_item_list_data, annual_budget_item_list_data);

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
        public void M_WhenAddingBudgetedBankInDataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetIn;
            var first_month = 12;
            var last_month = 3;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Date,
                Dividers.Total,
                6);
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<BankRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 10)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 4)},
                    new BankRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 25)}
                });
            var pending_file = new CSVFile<BankRecord>(mock_cred_card2_in_out_file_io.Object);
            pending_file.Load();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankInDataToPendingFile(budget_data_setup.BudgetingMonths, pending_file, budget_item_list_data);

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
        public void M_WhenAddingBudgetedCredCard1DataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var first_month = 12;
            var last_month = 3;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<CredCard1InOutRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.CredCard1,
                Dividers.CredCard2,
                5);
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard1InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard1InOutRecord> {
                    new CredCard1InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 10)},
                    new CredCard1InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 4)},
                    new CredCard1InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 25)}
                });
            var pending_file = new CSVFile<CredCard1InOutRecord>(mock_cred_card2_in_out_file_io.Object);
            pending_file.Load();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budget_data_setup.BudgetingMonths, pending_file, budget_item_list_data);

            // Assert
            CredCard1InOutRecord previous_record = null;
            foreach (CredCard1InOutRecord record in pending_file.Records)
            {
                if (null != previous_record)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previous_record.Date.ToOADate());
                }
                previous_record = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedCredCard2DataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheet_name = MainSheetNames.BudgetOut;
            var first_month = 12;
            var last_month = 3;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<CredCard2InOutRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.CredCard2,
                Dividers.SODDTotal,
                5);
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 10)},
                    new CredCard2InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 4)},
                    new CredCard2InOutRecord {Date = new DateTime(budget_data_setup.BudgetingMonths.StartYear, first_month, 25)}
                });
            var pending_file = new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object);
            pending_file.Load();
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard2,
                EndDivider = Dividers.SODDTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budget_data_setup.BudgetingMonths, pending_file, budget_item_list_data);

            // Assert
            CredCard2InOutRecord previous_record = null;
            foreach (CredCard2InOutRecord record in pending_file.Records)
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
            var sheet_name = MainSheetNames.BudgetIn;
            var first_month = 12;
            var last_month = 2;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var budget_data_setup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                first_month,
                last_month,
                sheet_name,
                mock_spreadsheet_repo,
                Dividers.Date,
                Dividers.Total,
                6,
                defaultDay: 31);
            var pending_file_records = new List<BankRecord>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_file_records);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            bool exception_thrown = false;
            var budget_item_list_data = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            try
            {
                spreadsheet.AddBudgetedBankInDataToPendingFile(budget_data_setup.BudgetingMonths, mock_pending_file.Object, budget_item_list_data);
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
