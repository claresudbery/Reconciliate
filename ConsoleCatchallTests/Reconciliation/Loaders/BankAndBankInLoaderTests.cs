using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class BankAndBankInLoaderTests
    {
        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillMergeUnreconciledEmployerExpensesWithPendingFile()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            var dividerRowNumber = 10;
            var lastRowNumber = 11;
            var mockCellRow = new Mock<ICellRow>();
            var description = "Hello";
            var code = Codes.Expenses;
            mockCellRow.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(description);
            mockCellRow.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(code);
            mockSpreadsheetRepo.Setup(x => x.FindRowNumberOfLastRowContainingCell(MainSheetNames.ExpectedIn, Dividers.DividerText, 2)).Returns(dividerRowNumber);
            mockSpreadsheetRepo.Setup(x => x.LastRowNumber(MainSheetNames.ExpectedIn)).Returns(lastRowNumber);
            mockSpreadsheetRepo.Setup(x => x.ReadSpecifiedRow(MainSheetNames.ExpectedIn, lastRowNumber)).Returns(mockCellRow.Object);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var pendingRecords = new List<BankRecord>();
            mockPendingFile.Setup(x => x.Records).Returns(pendingRecords);
            var budgetingMonths = new BudgetingMonths();
            var loadingInfo = new BankAndBankInLoader(new FakeSpreadsheetRepoFactory()).LoadingInfo();
            var bankAndBankInLoader = new BankAndBankInLoader(new FakeSpreadsheetRepoFactory());

            // Act
            bankAndBankInLoader.MergeBespokeDataWithPendingFile(
                mockInputOutput.Object,
                spreadsheet,
                mockPendingFile.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            mockInputOutput.Verify(x => x.OutputLine(ReconConsts.LoadingExpenses));
            Assert.AreEqual(1, pendingRecords.Count);
            Assert.AreEqual(description, pendingRecords[0].Description);
        }
    }
}
