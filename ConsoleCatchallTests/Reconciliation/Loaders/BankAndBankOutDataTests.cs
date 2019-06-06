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
    public class BankAndBankOutDataTests
    {
        private void AssertDirectDebitDetailsAreCorrect(
            BankRecord bankRecord, 
            DateTime expectedDate, 
            double expectedAmount, 
            string expectedDescription)
        {
            Assert.AreEqual(expectedDescription, bankRecord.Description);
            Assert.AreEqual(expectedDate, bankRecord.Date);
            Assert.AreEqual(expectedAmount, bankRecord.UnreconciledAmount);
            Assert.AreEqual("POS", bankRecord.Type);
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
            var nextDirectDebitDate03 = lastDirectDebitDate.AddMonths(3);
            var nextDirectDebitDate01 = lastDirectDebitDate.AddMonths(1);
            var nextDirectDebitDate02 = lastDirectDebitDate.AddMonths(2);
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
        public void M_MergeBespokeDataWithPendingFile_WillAddMostRecentCredCardDirectDebits()
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
            SetUpForCreditCardData(
                ReconConsts.CredCard2Name,
                ReconConsts.CredCard2DdDescription,
                lastDirectDebitDate,
                expectedAmount1,
                expectedAmount2,
                mockInputOutput,
                mockSpreadsheetRepo, 2);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var pendingRecords = new List<BankRecord>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingRecords);
            var budgetingMonths = new BudgetingMonths();
            var loadingInfo = new BankAndBankOutData().LoadingInfo();
            var bankAndBankOutLoader = new BankAndBankOutData();

            // Act
            bankAndBankOutLoader.MergeBespokeDataWithPendingFile(
                mockInputOutput.Object,
                spreadsheet,
                mockPendingFile.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            Assert.AreEqual(4, pendingRecords.Count);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[0], nextDirectDebitDate01, expectedAmount1, ReconConsts.CredCard1DdDescription);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[1], nextDirectDebitDate02, expectedAmount2, ReconConsts.CredCard1DdDescription);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[2], nextDirectDebitDate01, expectedAmount1, ReconConsts.CredCard2DdDescription);
            AssertDirectDebitDetailsAreCorrect(pendingRecords[3], nextDirectDebitDate02, expectedAmount2, ReconConsts.CredCard2DdDescription);
        }
    }
}
