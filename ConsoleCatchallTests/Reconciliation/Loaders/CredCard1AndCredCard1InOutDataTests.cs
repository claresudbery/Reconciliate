using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class CredCard1AndCredCard1InOutDataTests
    {
        private void AssertDirectDebitDetailsAreCorrect(
            CredCard1InOutRecord credCard1InOutRecord, 
            DateTime expectedDate, 
            double expectedAmount, 
            string expectedDescription)
        {
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.UnreconciledAmount);
        }

        private void SetUpForCreditCardData(
            string credCardName,
            string directDebitDescription,
            DateTime lastDirectDebitDate,
            double expectedAmount1,
            double expectedAmount2,
            Mock<IInputOutput> mockInputOutput,
            Mock<ISpreadsheetRepo> mockSpreadsheetRepo,
            int directDebitRowNumber)
        {
            var nextDirectDebitDate01 = lastDirectDebitDate.AddMonths(1);
            var nextDirectDebitDate02 = lastDirectDebitDate.AddMonths(2);
            var nextDirectDebitDate03 = lastDirectDebitDate.AddMonths(3);
            double expectedAmount3 = 0;
            mockInputOutput
                .Setup(x => x.GetInput(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        nextDirectDebitDate01.ToShortDateString()), ""))
                .Returns(expectedAmount1.ToString);
            mockInputOutput
                .Setup(x => x.GetInput(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        nextDirectDebitDate02.ToShortDateString()), ""))
                .Returns(expectedAmount2.ToString);
            mockInputOutput
                .Setup(x => x.GetInput(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        nextDirectDebitDate03.ToShortDateString()), ""))
                .Returns(expectedAmount3.ToString);
            var mockCellRow = new Mock<ICellRow>();
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DateIndex)).Returns(lastDirectDebitDate.ToOADate());
            mockCellRow.Setup(x => x.ReadCell(BankRecord.DescriptionIndex)).Returns(directDebitDescription);
            mockCellRow.Setup(x => x.ReadCell(BankRecord.TypeIndex)).Returns("Type");
            mockCellRow.Setup(x => x.ReadCell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowWithCellContainingText(
                    MainSheetNames.BankOut, directDebitDescription, new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn}))
                .Returns(directDebitRowNumber);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(
                    MainSheetNames.BankOut, directDebitRowNumber))
                .Returns(mockCellRow.Object);
        }

        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillAddMostRecentCredCard1DirectDebits()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            var mockInputOutput = new Mock<IInputOutput>();
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            double expectedAmount1 = 1234.55;
            double expectedAmount2 = 5673.99;
            DateTime lastDirectDebitDate = new DateTime(2018, 12, 17);
            var nextDirectDebitDate01 = lastDirectDebitDate.AddMonths(1);
            var nextDirectDebitDate02 = lastDirectDebitDate.AddMonths(2);
            SetUpForCreditCardData(
                ReconConsts.CredCard1Name,
                ReconConsts.CredCard1DdDescription,
                lastDirectDebitDate,
                expectedAmount1,
                expectedAmount2,
                mockInputOutput,
                mockSpreadsheetRepo, 1);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var mockPendingFile = new Mock<ICSVFile<CredCard1InOutRecord>>();
            var pendingRecords = new List<CredCard1InOutRecord>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingRecords);
            var budgetingMonths = new BudgetingMonths();
            var loadingInfo = new CredCard1AndCredCard1InOutData().LoadingInfo();
            var credCard1AndCredCard1InOutLoader = new CredCard1AndCredCard1InOutData();

            // Act
            credCard1AndCredCard1InOutLoader.MergeBespokeDataWithPendingFile(
                mockInputOutput.Object,
                spreadsheet,
                mockPendingFile.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            Assert.AreEqual(2, pendingRecords.Count);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[0], nextDirectDebitDate01, expectedAmount1, ReconConsts.CredCard1RegularPymtDescription);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[1], nextDirectDebitDate02, expectedAmount2, ReconConsts.CredCard1RegularPymtDescription);
        }

        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillUpdateCredCard1BalancesOnTotalsSheet()
        {
            // Arrange
            TestHelper.SetCorrectDateFormatting();
            var mockInputOutput = new Mock<IInputOutput>();
            double newBalance = 5673.99;
            DateTime lastDirectDebitDate = new DateTime(2018, 12, 17);
            var nextDirectDebitDate01 = lastDirectDebitDate.AddMonths(1);
            var nextDirectDebitDate02 = lastDirectDebitDate.AddMonths(2);
            mockInputOutput
                .Setup(x => x.GetInput(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        ReconConsts.CredCard1Name,
                        nextDirectDebitDate01.ToShortDateString()), ""))
                .Returns(newBalance.ToString);
            mockInputOutput
                .Setup(x => x.GetInput(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        ReconConsts.CredCard1Name,
                        nextDirectDebitDate02.ToShortDateString()), ""))
                .Returns("0");
            var bankRecord = new BankRecord { Date = lastDirectDebitDate };
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetMostRecentRowContainingText<BankRecord>(
                    MainSheetNames.BankOut, ReconConsts.CredCard1DdDescription, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bankRecord);
            var mockPendingFile = new Mock<ICSVFile<CredCard1InOutRecord>>();
            var pendingRecords = new List<CredCard1InOutRecord>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingRecords);
            var budgetingMonths = new BudgetingMonths();
            var loadingInfo = new CredCard1AndCredCard1InOutData().LoadingInfo();
            var credCard1AndCredCard1InOutLoader = new CredCard1AndCredCard1InOutData();

            // Act
            credCard1AndCredCard1InOutLoader.MergeBespokeDataWithPendingFile(
                mockInputOutput.Object,
                mockSpreadsheet.Object,
                mockPendingFile.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            mockSpreadsheet.Verify(x => x.UpdateBalanceOnTotalsSheet(
                Codes.CredCard1Bal,
                newBalance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription, 
                    ReconConsts.CredCard1Name,
                    $"{lastDirectDebitDate.ToString("MMM")} {lastDirectDebitDate.Year}"),
                5, 6, 4), Times.Exactly(1));
        }
    }
}
