using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation;
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
    public class BankAndBankInDataTests
    {
        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillMergeUnreconciledEmployerExpensesWithPendingFile()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var divider_row_number = 10;
            var last_row_number = 11;
            var mock_cell_row = new Mock<ICellRow>();
            var description = "Hello";
            var code = Codes.Expenses;
            mock_cell_row.Setup(x => x.ReadCell(ExpectedIncomeRecord.DescriptionIndex)).Returns(description);
            mock_cell_row.Setup(x => x.ReadCell(ExpectedIncomeRecord.CodeIndex)).Returns(code);
            mock_spreadsheet_repo.Setup(x => x.FindRowNumberOfLastRowContainingCell(MainSheetNames.ExpectedIn, Dividers.DividerText, 2)).Returns(divider_row_number);
            mock_spreadsheet_repo.Setup(x => x.LastRowNumber(MainSheetNames.ExpectedIn)).Returns(last_row_number);
            mock_spreadsheet_repo.Setup(x => x.ReadSpecifiedRow(MainSheetNames.ExpectedIn, last_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var budgeting_months = new BudgetingMonths();
            var loading_info = BankAndBankInData.LoadingInfo;
            var reconciliation_intro = new ReconciliationIntro(mock_input_output.Object);

            // Act
            reconciliation_intro.BankAndBankIn_MergeBespokeDataWithPendingFile(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                budgeting_months,
                loading_info);

            // Assert
            mock_input_output.Verify(x => x.OutputLine(ReconConsts.LoadingExpenses));
            Assert.AreEqual(1, pending_records.Count);
            Assert.AreEqual(description, pending_records[0].Description);
        }
    }
}
