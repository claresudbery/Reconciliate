using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation;
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
        private void Assert_direct_debit_details_are_correct(
            CredCard1InOutRecord credCard1InOutRecord, 
            DateTime expectedDate, 
            double expectedAmount, 
            string expectedDescription)
        {
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.Unreconciled_amount);
        }

        private void Set_up_for_credit_card_data(
            string credCardName,
            string directDebitDescription,
            DateTime lastDirectDebitDate,
            double expectedAmount1,
            double expectedAmount2,
            Mock<IInputOutput> mockInputOutput,
            Mock<ISpreadsheetRepo> mockSpreadsheetRepo,
            int directDebitRowNumber)
        {
            var next_direct_debit_date01 = lastDirectDebitDate.AddMonths(1);
            var next_direct_debit_date02 = lastDirectDebitDate.AddMonths(2);
            var next_direct_debit_date03 = lastDirectDebitDate.AddMonths(3);
            double expected_amount3 = 0;
            mockInputOutput
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        next_direct_debit_date01.ToShortDateString()), ""))
                .Returns(expectedAmount1.ToString);
            mockInputOutput
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        next_direct_debit_date02.ToShortDateString()), ""))
                .Returns(expectedAmount2.ToString);
            mockInputOutput
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        credCardName,
                        next_direct_debit_date03.ToShortDateString()), ""))
                .Returns(expected_amount3.ToString);
            var mock_cell_row = new Mock<ICellRow>();
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DateIndex)).Returns(lastDirectDebitDate.ToOADate());
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DescriptionIndex)).Returns(directDebitDescription);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.TypeIndex)).Returns("Type");
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mockSpreadsheetRepo.Setup(x => x.Find_row_number_of_last_row_with_cell_containing_text(
                    MainSheetNames.Bank_out, directDebitDescription, new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn}))
                .Returns(directDebitRowNumber);
            mockSpreadsheetRepo.Setup(x => x.Read_specified_row(
                    MainSheetNames.Bank_out, directDebitRowNumber))
                .Returns(mock_cell_row.Object);
        }

        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillAddMostRecentCredCard1DirectDebits()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            Set_up_for_credit_card_data(
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo, 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<CredCard1InOutRecord>>();
            var pending_records = new List<CredCard1InOutRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var budgeting_months = new BudgetingMonths();
            var loading_info = CredCard1AndCredCard1InOutData.LoadingInfo; 
            var reconciliation_intro = new ReconciliationIntro(mock_input_output.Object);

            // Act
            reconciliation_intro.CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                budgeting_months,
                loading_info);

            // Assert
            Assert.AreEqual(2, pending_records.Count);
            Assert_direct_debit_details_are_correct(pending_records[0], next_direct_debit_date01, expected_amount1, ReconConsts.Cred_card1_regular_pymt_description);
            Assert_direct_debit_details_are_correct(pending_records[1], next_direct_debit_date02, expected_amount2, ReconConsts.Cred_card1_regular_pymt_description);
        }

        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillUpdateCredCard1BalancesOnTotalsSheet()
        {
            // Arrange
            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            double new_balance = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        ReconConsts.Cred_card1_name,
                        next_direct_debit_date01.ToShortDateString()), ""))
                .Returns(new_balance.ToString);
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        ReconConsts.Cred_card1_name,
                        next_direct_debit_date02.ToShortDateString()), ""))
                .Returns("0");
            var bank_record = new BankRecord { Date = last_direct_debit_date };
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card1_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);
            var mock_pending_file = new Mock<ICSVFile<CredCard1InOutRecord>>();
            var pending_records = new List<CredCard1InOutRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var budgeting_months = new BudgetingMonths();
            var loading_info = CredCard1AndCredCard1InOutData.LoadingInfo; 
            var reconciliation_intro = new ReconciliationIntro(mock_input_output.Object);

            // Act
            reconciliation_intro.CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                mock_pending_file.Object,
                budgeting_months,
                loading_info);

            // Assert
            mock_spreadsheet.Verify(x => x.Update_balance_on_totals_sheet(
                Codes.Cred_card1_bal,
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription, 
                    ReconConsts.Cred_card1_name,
                    $"{last_direct_debit_date.ToString("MMM")} {last_direct_debit_date.Year}"),
                5, 6, 4), Times.Exactly(1));
        }
    }
}
