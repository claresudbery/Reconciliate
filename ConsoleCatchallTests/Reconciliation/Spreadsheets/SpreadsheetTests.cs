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
            var budgetingMonths = new BudgetingMonths
            {
                NextUnplannedMonth = firstMonth,
                LastMonthForBudgetPlanning = lastMonth,
                StartYear = 2018
            };
            var firstMonthlyRow = 3;
            var lastMonthlyRow = firstMonthlyRow + 2;
            int firstBudgetOutColumn = 2;
            string desc1 = "first monthly record";
            string desc2 = "second monthly record";
            var monthlyRecords = new List<TRecordType>
            {
                new TRecordType {Date = new DateTime(budgetingMonths.StartYear, firstMonth, defaultDay), Description = desc1},
                new TRecordType {Date = new DateTime(budgetingMonths.StartYear, firstMonth, 20), Description = desc2}
            };
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, firstDivider, 2)).Returns(firstMonthlyRow);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, secondDivider, 2)).Returns(lastMonthlyRow);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<TRecordType>(
                sheetName,
                firstMonthlyRow + 1,
                lastMonthlyRow - 1,
                firstBudgetOutColumn,
                secondBudgetOutColumn)).Returns(monthlyRecords);

            return new BudgetDataSetup<TRecordType>
            {
                Desc1 = desc1,
                Desc2 = desc2,
                BudgetingMonths = budgetingMonths,
                MonthlyRecords = monthlyRecords
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
            var numRepetitions = lastMonth >= firstMonth
                ? (lastMonth - firstMonth) + 1
                : ((lastMonth + 12) - firstMonth) + 1;
            var totalRecords = numRepetitions * monthlyRecordCount;

            // Assert
            Assert.AreEqual(totalRecords, pendingFileRecords.Count, "total num records");
            Assert.AreEqual(numRepetitions, pendingFileRecords.Count(x => x.Description == desc1), "num repetitions of desc1");
            Assert.AreEqual(numRepetitions, pendingFileRecords.Count(x => x.Description == desc2), "num repetitions of desc2");
            var desc1Records = pendingFileRecords.Where(x => x.Description == desc1)
                .OrderBy(x => x.Date).ToList();
            int index = 0;
            int finalMonth = lastMonth >= firstMonth ? lastMonth : lastMonth + 12;
            for (int month = firstMonth; month <= finalMonth; month++)
            {
                var expectedMonth = month <= 12 ? month : month - 12;
                var expectedYear = month <= 12 ? budgetingMonths.StartYear : budgetingMonths.StartYear + 1;
                Assert.AreEqual(expectedMonth, desc1Records[index].Date.Month, "correct month");
                Assert.AreEqual(expectedYear, desc1Records[index].Date.Year, "correct year");
                index++;
            }
        }

        [Test]
        public void M_CanReadLastRowOfSpecifiedWorksheetAsObjectList()
        {
            // Arrange
            var sheetName = "MockSheet";
            var fakeCellRow = new FakeCellRow().WithFakeData(new List<object>
            {
                "22/03/2018",
                "22.34",
                "last row",
                "7788"
            });
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.ReadLastRow(sheetName))
                .Returns(fakeCellRow);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            ICellRow actualRow = spreadsheet.ReadLastRow(sheetName);

            // Assert
            Assert.AreEqual(fakeCellRow.ReadCell(0), actualRow.ReadCell(0));
            Assert.AreEqual(fakeCellRow.ReadCell(1), actualRow.ReadCell(1));
            Assert.AreEqual(fakeCellRow.ReadCell(2), actualRow.ReadCell(2));
            Assert.AreEqual(fakeCellRow.ReadCell(3), actualRow.ReadCell(3));
        }

        [Test]
        public void M_CanReadLastRowOfSpecifiedWorksheetAsCsv()
        {
            // Arrange
            var sheetName = "MockSheet";
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            String expectedCsv = "22/03/2018,£22.34,\"last row\",7788";
            mockSpreadsheetRepo.Setup(x => x.ReadLastRowAsCsv(sheetName, It.IsAny<ICSVRecord>()))
                .Returns(expectedCsv);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            var result = spreadsheet.ReadLastRowAsCsv(sheetName, new BankRecord());

            // Assert
            Assert.AreEqual(expectedCsv, result);
        }

        [Test]
        public void M_WillAppendAllRowsInCsvFileToSpecifiedWorksheet()
        {
            // Arrange
            var sheetName = "MockSheet";
            var mockFileIO = new Mock<IFileIO<BankRecord>>();
            mockFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var csvFile = new CSVFile<BankRecord>(mockFileIO.Object);
            csvFile.Load();
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.AppendCsvFile<BankRecord>(sheetName, csvFile))
                .Verifiable();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.AppendCsvFile<BankRecord>(sheetName, csvFile);

            // Clean up
            mockSpreadsheetRepo.Verify(x => x.AppendCsvFile<BankRecord>(sheetName, csvFile), Times.Once); 
        }

        [Test]
        public void M_WillFindRowNumberOfLastDividerRow()
        {
            // Arrange
            var sheetName = "MockSheet";
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            int expectedRowNumber = 9;
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    sheetName,
                    Dividers.DividerText,
                    2))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            var result = spreadsheet.FindRowNumberOfLastDividerRow(sheetName);

            // Assert
            Assert.AreEqual(expectedRowNumber, result);
        }

        [Test]
        public void M_CanAddUnreconciledRowsToCredCard1InOutCsvFileAndPutInDateOrder()
        {
            // Arrange
            var sheetName = "MockSheet";
            double date = 43345;
            var amount = 22.34;
            var text1 = "first row";
            var text2 = "second row";
            var text3 = "third row";
            var fakeCellRow1 = new FakeCellRow().WithFakeData(new List<object> { date, amount, null, text2 });
            var fakeCellRow3 = new FakeCellRow().WithFakeData(new List<object> { date + 1, amount, null, text3 });
            var fakeCellRow2 = new FakeCellRow().WithFakeData(new List<object> { date - 1, amount, null, text1 });
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.DividerText, 2))
                .Returns(1);
            mockSpreadsheetRepo.Setup(x => x.LastRowNumber(sheetName)).Returns(4);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(sheetName, 2)).Returns(fakeCellRow1);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(sheetName, 3)).Returns(fakeCellRow2);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(sheetName, 4)).Returns(fakeCellRow3);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            var expectedNumRecords = 3;

            var csvFile = new CSVFileFactory<CredCard1InOutRecord>().CreateCSVFile(false);

            // Act
            spreadsheet.AddUnreconciledRowsToCsvFile<CredCard1InOutRecord>(sheetName, csvFile);

            // Assert
            Assert.AreEqual(expectedNumRecords, csvFile.Records.Count);
            Assert.IsTrue(csvFile.Records[0].SourceLine.Contains(text1), $"line 0 should contain correct text");
            Assert.IsTrue(csvFile.Records[1].SourceLine.Contains(text2), $"line 1 should contain correct text");
            Assert.IsTrue(csvFile.Records[2].SourceLine.Contains(text3), $"line 1 should contain correct text");
        }

        [Test]
        public void M_CanReadAllBudgetItems()
        {
            // Arrange
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };
            const int expectedFirstRowNumber = 6;
            const int expectedLastRowNumber = 20;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budgetItemListData.SheetName, budgetItemListData.StartDivider, 2))
                .Returns(expectedFirstRowNumber - 1);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    budgetItemListData.SheetName, budgetItemListData.EndDivider, 2))
                .Returns(expectedLastRowNumber + 1);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.GetAllBudgetItems<CredCard1InOutRecord>(budgetItemListData);

            // Assert
            mockSpreadsheetRepo.Verify(x => x.GetRowsAsRecords<CredCard1InOutRecord>(
                budgetItemListData.SheetName,
                expectedFirstRowNumber,
                expectedLastRowNumber,
                budgetItemListData.FirstColumnNumber,
                budgetItemListData.LastColumnNumber));
        }

        [Test]
        public void M_CanReadBalanceFromPocketMoneySpreadsheetBasedOnDate()
        {
            // Arrange
            var expectedDateTime = new DateTime(2018, 11, 25).ToShortDateString();
            var expectedSheetName = PocketMoneySheetNames.SecondChild;
            const int expectedDateColumn = 1;
            const int expectedAmountColumn = 4;
            const int expectedRowNumber = 10;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    expectedSheetName, expectedDateTime, expectedDateColumn))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            
            // Act
            spreadsheet.GetSecondChildPocketMoneyAmount(expectedDateTime);

            // Assert
            mockSpreadsheetRepo.Verify(x => x.GetAmount(
                expectedSheetName,
                expectedRowNumber,
                expectedAmountColumn));
        }

        [Test]
        public void M_CanInsertNewRowOnExpectedOut()
        {
            // Arrange
            var expectedSheetName = MainSheetNames.ExpectedOut;
            const int expectedRowNumber = 21;
            double newAmount = 79;
            string newNotes = "Acme Blasters membership";
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfFirstRowContainingCell(
                    expectedSheetName,
                    ReconConsts.ExpectedOutInsertionPoint,
                    4))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.InsertNewRowOnExpectedOut(newAmount, newNotes);

            // Assert
            mockSpreadsheetRepo.Verify(x => x.InsertNewRow(
                expectedSheetName,
                expectedRowNumber,
                It.Is<Dictionary<int, object>>(y => y.Values.Contains(newAmount) && y.Values.Contains(newNotes))));
        }

        [Test]
        public void M_CanRead_ExpensesAlreadyDone_FromPlanningSpreadsheet()
        {
            // Arrange
            var expectedSheetName = PlanningSheetNames.Expenses;
            const int expectedRowNumber = 2;
            const int expectedColumnNumber = 4;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.GetPlanningExpensesAlreadyDone();

            // Assert
            mockSpreadsheetRepo.Verify(x => x.GetAmount(
                expectedSheetName,
                expectedRowNumber,
                expectedColumnNumber));
        }

        [Test]
        public void M_CanRead_MoneyPaidByGuests_FromPlanningSpreadsheet()
        {
            // Arrange
            var expectedSheetName = PlanningSheetNames.Deposits;
            const int expectedRowNumber = 2;
            const int expectedColumnNumber = 4;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.GetPlanningMoneyPaidByGuests();

            // Assert
            mockSpreadsheetRepo.Verify(x => x.GetAmount(
                expectedSheetName,
                expectedRowNumber,
                expectedColumnNumber));
        }

        [Test]
        public void M_CanAddNewTransactionToEndOfCredCard3Sheet()
        {
            // Arrange
            var expectedSheetName = MainSheetNames.CredCard3;
            DateTime newDate = new DateTime(2018, 11, 25);
            double newAmount = 32.45;
            string newDescription = "minimum monthly payment";
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.AddNewTransactionToCredCard3(newDate, newAmount, newDescription);

            // Assert
            mockSpreadsheetRepo.Verify(x => x.AppendNewRow(
                expectedSheetName,
                It.Is<Dictionary<int, object>>(
                    y => y.Values.Contains(newAmount) 
                    && y.Values.Contains(newDate.ToOADate())
                    && y.Values.Contains(newDescription))));
        }

        [Test]
        public void M_CanAddNewTransactionToEndOfSavginsSheet()
        {
            // Arrange
            var expectedSheetName = MainSheetNames.Savings;
            DateTime newDate = new DateTime(2018, 11, 25);
            double newAmount = 32.45;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            int expectedRowNumber = 49;
            int firstColumn = 1;
            mockSpreadsheetRepo.Setup(x => x.FindFirstEmptyRowInColumn(
                    expectedSheetName,
                    firstColumn))
                .Returns(expectedRowNumber);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            spreadsheet.AddNewTransactionToSavings(newDate, newAmount);

            // Assert
            mockSpreadsheetRepo.Verify(x => x.UpdateDate(
                expectedSheetName,
                expectedRowNumber,
                firstColumn,
                newDate));
            mockSpreadsheetRepo.Verify(x => x.UpdateAmount(
                expectedSheetName,
                expectedRowNumber,
                firstColumn + 1,
                newAmount));
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillUseMortgageTransactionsToDetermineNextUnplannedMonth()
        {
            // Arrange
            int codeColumn = 1;
            int expectedBudgetOutRowNumber = 10;
            int expectedBankOutRowNumber = 11;
            DateTime expectedLastPlannedDate = DateTime.Today;
            string mortgageDescription = "PASTA PLASTER";
            var mockCellRow = new Mock<ICellRow>();
            mockCellRow.Setup(x => x.ReadCell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.TypeIndex)).Returns("");
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    MainSheetNames.BudgetOut,
                    Codes.Code042,
                    codeColumn))
                .Returns(expectedBudgetOutRowNumber);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(
                    MainSheetNames.BudgetOut,
                    expectedBudgetOutRowNumber, 2, 6))
                .Returns(mockCellRow.Object);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DescriptionIndex)).Returns(mortgageDescription);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    MainSheetNames.BankOut,
                    mortgageDescription,
                    BankRecord.DescriptionIndex + 1))
                .Returns(expectedBankOutRowNumber);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(
                    MainSheetNames.BankOut,
                    expectedBankOutRowNumber))
                .Returns(mockCellRow.Object);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DateIndex)).Returns(expectedLastPlannedDate.ToOADate());
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            var result = spreadsheet.GetNextUnplannedMonth();

            // Assert
            Assert.AreEqual(expectedLastPlannedDate.AddMonths(1).Month, result.Month);
        }

        [Test]
        public void M_GetNextUnplannedMonth_WillThrowExceptionIfMortgageRowCannotBeFound()
        {
            // Arrange
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Throws(new Exception());
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
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
            var spreadsheetName = MainSheetNames.BankOut;
            var textToSearchFor = ReconConsts.CredCard2DdDescription;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var credCard1DdRowNumber = 10;
            var mockCellRow = new Mock<ICellRow>();
            var description = "found row";
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DescriptionIndex)).Returns(description);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.TypeIndex)).Returns(string.Empty);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DateIndex)).Returns(DateTime.Today.ToOADate());
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowWithCellContainingText(
                    spreadsheetName, textToSearchFor, new List<int> {ReconConsts.DescriptionColumn}))
                .Returns(credCard1DdRowNumber);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(spreadsheetName, credCard1DdRowNumber)).Returns(mockCellRow.Object);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);

            // Act
            BankRecord bankRow = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                spreadsheetName, 
                textToSearchFor,
                new List<int> {ReconConsts.DescriptionColumn});

            // Assert
            Assert.AreEqual(description, bankRow.Description);
        }

        [TestCase(1, 2)]
        [TestCase(12, 1)]
        public void M_WhenAddingBudgetedBankOutDataToSpreadsheet_WillAddAnnualBankOutTransactionsToPendingFile_IfMonthMatchesBudgetingMonths(
            int firstMonth, int lastMonth)
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetOut;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            // Monthly setup:
            var firstMonthlyRow = 3;
            var lastMonthlyRow = firstMonthlyRow + 2;
            var monthlyBankRecords = new List<BankRecord>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.SODDs, 2)).Returns(firstMonthlyRow);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.CredCard1, 2)).Returns(lastMonthlyRow);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheetName, firstMonthlyRow + 1, lastMonthlyRow - 1, 2, 6)).Returns(monthlyBankRecords);
            // Annual setup:
            var firstAnnualRow = 10;
            var lastAnnualRow = firstAnnualRow + 2;
            string desc1 = "annual record with matching month";
            string desc2 = "other annual record with matching month";
            string desc3 = "annual record with non-matching month";
            var annualBankRecords = new List<BankRecord>
            {
                new BankRecord { Date = new DateTime(2018, firstMonth, 1), Description = desc1 },
                new BankRecord { Date = new DateTime(2018, lastMonth, 1), Description = desc2 },
                new BankRecord { Date = new DateTime(2018, lastMonth + 2, 1), Description = desc3 }
            };
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualSODDs, 2)).Returns(firstAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualTotal, 2)).Returns(lastAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheetName, firstAnnualRow + 1, lastAnnualRow - 1, 2, 6)).Returns(annualBankRecords);
            // Everything else:
            var budgetingMonths = new BudgetingMonths
            {
                NextUnplannedMonth = firstMonth,
                LastMonthForBudgetPlanning = lastMonth,
                StartYear = 2018
            };
            var pendingFileRecords = new List<BankRecord>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var monthlyBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annualBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budgetingMonths, mockPendingFile.Object, monthlyBudgetItemListData, annualBudgetItemListData);

            // Assert
            Assert.AreEqual(2, pendingFileRecords.Count, "total num records");
            Assert.AreEqual(1, pendingFileRecords.Count(x => x.Description == desc1), "num repetitions of matching record");
            Assert.AreEqual(1, pendingFileRecords.Count(x => x.Description == desc2), "num repetitions of other matching record");
            Assert.AreEqual(0, pendingFileRecords.Count(x => x.Description == desc3), "num repetitions of non-matching record");
            var expectedRecord2Year = lastMonth >= firstMonth
                ? budgetingMonths.StartYear
                : budgetingMonths.StartYear + 1;
            Assert.AreEqual(expectedRecord2Year, pendingFileRecords[1].Date.Year);
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
            var sheetName = MainSheetNames.BudgetOut;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            // Annual setup:
            var firstAnnualRow = 10;
            var lastAnnualRow = firstAnnualRow + 2;
            var annualBankRecords = new List<BankRecord>();
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualSODDs, 2)).Returns(firstAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualTotal, 2)).Returns(lastAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheetName, firstAnnualRow + 1, lastAnnualRow - 1, 2, 6)).Returns(annualBankRecords);
            // Everything else:
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.SODDs,
                Dividers.CredCard1,
                6);
            var pendingFileRecords = new List<BankRecord>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var monthlyBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annualBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budgetDataSetup.BudgetingMonths, mockPendingFile.Object, monthlyBudgetItemListData, annualBudgetItemListData);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<BankRecord>(
                firstMonth,
                lastMonth,
                budgetDataSetup.MonthlyRecords.Count,
                pendingFileRecords,
                budgetDataSetup.BudgetingMonths,
                budgetDataSetup.Desc1,
                budgetDataSetup.Desc2);
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
            var sheetName = MainSheetNames.BudgetIn;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.Date,
                Dividers.Total,
                6);
            var pendingFileRecords = new List<BankRecord>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankInDataToPendingFile(budgetDataSetup.BudgetingMonths, mockPendingFile.Object, budgetItemListData);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<BankRecord>(
                firstMonth,
                lastMonth,
                budgetDataSetup.MonthlyRecords.Count,
                pendingFileRecords,
                budgetDataSetup.BudgetingMonths,
                budgetDataSetup.Desc1,
                budgetDataSetup.Desc2);
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
            var sheetName = MainSheetNames.BudgetOut;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<CredCard1InOutRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.CredCard1,
                Dividers.CredCard2,
                5);
            var pendingFileRecords = new List<CredCard1InOutRecord>();
            var mockPendingFile = new Mock<ICSVFile<CredCard1InOutRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budgetDataSetup.BudgetingMonths, mockPendingFile.Object, budgetItemListData);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<CredCard1InOutRecord>(
                firstMonth,
                lastMonth,
                budgetDataSetup.MonthlyRecords.Count,
                pendingFileRecords,
                budgetDataSetup.BudgetingMonths,
                budgetDataSetup.Desc1,
                budgetDataSetup.Desc2);
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
            var sheetName = MainSheetNames.BudgetOut;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<CredCard2InOutRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.CredCard2,
                Dividers.SODDTotal,
                5);
            var pendingFileRecords = new List<CredCard2InOutRecord>();
            var mockPendingFile = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard2,
                EndDivider = Dividers.SODDTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budgetDataSetup.BudgetingMonths, mockPendingFile.Object, budgetItemListData);

            // Assert
            Assert_WillAddAllMonthlyItemsToPendingFile_WithMonthsAndYearsChanged<CredCard2InOutRecord>(
                firstMonth,
                lastMonth,
                budgetDataSetup.MonthlyRecords.Count,
                pendingFileRecords,
                budgetDataSetup.BudgetingMonths,
                budgetDataSetup.Desc1,
                budgetDataSetup.Desc2);
        }

        [Test]
        public void M_WhenAddingBudgetedBankOutDataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetOut;
            var firstMonth = 12;
            var lastMonth = 3;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            // Monthly set up:
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.SODDs,
                Dividers.CredCard1,
                6);
            // Annual setup:
            var firstAnnualRow = 10;
            var lastAnnualRow = firstAnnualRow + 2;
            var annualBankRecords = new List<BankRecord>{
                new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 28)},
                new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, lastMonth, 2)}
            };
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualSODDs, 2)).Returns(firstAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(sheetName, Dividers.AnnualTotal, 2)).Returns(lastAnnualRow);
            mockSpreadsheetRepo.Setup(x => x.GetRowsAsRecords<BankRecord>(sheetName, firstAnnualRow + 1, lastAnnualRow - 1, 2, 6)).Returns(annualBankRecords);
            // Everything else:
            var mockCredCard2InOutFileIO = new Mock<IFileIO<BankRecord>>();
            mockCredCard2InOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 10)},
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 4)},
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 25)}
                });
            var pendingFile = new CSVFile<BankRecord>(mockCredCard2InOutFileIO.Object);
            pendingFile.Load();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var monthlyBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.SODDs,
                EndDivider = Dividers.CredCard1,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };
            var annualBudgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.AnnualSODDs,
                EndDivider = Dividers.AnnualTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankOutDataToPendingFile(budgetDataSetup.BudgetingMonths, pendingFile, monthlyBudgetItemListData, annualBudgetItemListData);

            // Assert
            BankRecord previousRecord = null;
            foreach (BankRecord record in pendingFile.Records)
            {
                if (null != previousRecord)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previousRecord.Date.ToOADate());
                }
                previousRecord = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedBankInDataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetIn;
            var firstMonth = 12;
            var lastMonth = 3;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.Date,
                Dividers.Total,
                6);
            var mockCredCard2InOutFileIO = new Mock<IFileIO<BankRecord>>();
            mockCredCard2InOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 10)},
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 4)},
                    new BankRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 25)}
                });
            var pendingFile = new CSVFile<BankRecord>(mockCredCard2InOutFileIO.Object);
            pendingFile.Load();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetIn,
                StartDivider = Dividers.Date,
                EndDivider = Dividers.Total,
                FirstColumnNumber = 2,
                LastColumnNumber = 6
            };

            // Act
            spreadsheet.AddBudgetedBankInDataToPendingFile(budgetDataSetup.BudgetingMonths, pendingFile, budgetItemListData);

            // Assert
            BankRecord previousRecord = null;
            foreach (BankRecord record in pendingFile.Records)
            {
                if (null != previousRecord)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previousRecord.Date.ToOADate());
                }
                previousRecord = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedCredCard1DataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetOut;
            var firstMonth = 12;
            var lastMonth = 3;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<CredCard1InOutRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.CredCard1,
                Dividers.CredCard2,
                5);
            var mockCredCard2InOutFileIO = new Mock<IFileIO<CredCard1InOutRecord>>();
            mockCredCard2InOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard1InOutRecord> {
                    new CredCard1InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 10)},
                    new CredCard1InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 4)},
                    new CredCard1InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 25)}
                });
            var pendingFile = new CSVFile<CredCard1InOutRecord>(mockCredCard2InOutFileIO.Object);
            pendingFile.Load();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard1,
                EndDivider = Dividers.CredCard2,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budgetDataSetup.BudgetingMonths, pendingFile, budgetItemListData);

            // Assert
            CredCard1InOutRecord previousRecord = null;
            foreach (CredCard1InOutRecord record in pendingFile.Records)
            {
                if (null != previousRecord)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previousRecord.Date.ToOADate());
                }
                previousRecord = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedCredCard2DataToSpreadsheet_WillOrderResultsByDate()
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetOut;
            var firstMonth = 12;
            var lastMonth = 3;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<CredCard2InOutRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.CredCard2,
                Dividers.SODDTotal,
                5);
            var mockCredCard2InOutFileIO = new Mock<IFileIO<CredCard2InOutRecord>>();
            mockCredCard2InOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 10)},
                    new CredCard2InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 4)},
                    new CredCard2InOutRecord {Date = new DateTime(budgetDataSetup.BudgetingMonths.StartYear, firstMonth, 25)}
                });
            var pendingFile = new CSVFile<CredCard2InOutRecord>(mockCredCard2InOutFileIO.Object);
            pendingFile.Load();
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var budgetItemListData = new BudgetItemListData
            {
                SheetName = MainSheetNames.BudgetOut,
                StartDivider = Dividers.CredCard2,
                EndDivider = Dividers.SODDTotal,
                FirstColumnNumber = 2,
                LastColumnNumber = 5
            };

            // Act
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budgetDataSetup.BudgetingMonths, pendingFile, budgetItemListData);

            // Assert
            CredCard2InOutRecord previousRecord = null;
            foreach (CredCard2InOutRecord record in pendingFile.Records)
            {
                if (null != previousRecord)
                {
                    Assert.IsTrue(record.Date.ToOADate() > previousRecord.Date.ToOADate());
                }
                previousRecord = record;
            }
        }

        [Test]
        public void M_WhenAddingBudgetedDataToSpreadsheet_WillAdjustDayIfDaysInMonthIsTooLow()
        {
            // Arrange
            var sheetName = MainSheetNames.BudgetIn;
            var firstMonth = 12;
            var lastMonth = 2;
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var budgetDataSetup = WhenAddingBudgetedDataToSpreadsheet<BankRecord>(
                firstMonth,
                lastMonth,
                sheetName,
                mockSpreadsheetRepo,
                Dividers.Date,
                Dividers.Total,
                6,
                defaultDay: 31);
            var pendingFileRecords = new List<BankRecord>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingFileRecords);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            bool exceptionThrown = false;
            var budgetItemListData = new BudgetItemListData
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
                spreadsheet.AddBudgetedBankInDataToPendingFile(budgetDataSetup.BudgetingMonths, mockPendingFile.Object, budgetItemListData);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }
    }
}
