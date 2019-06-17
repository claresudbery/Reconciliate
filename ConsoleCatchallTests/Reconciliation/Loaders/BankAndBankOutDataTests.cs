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
    public class BankAndBankOutDataTests
    {
        private void Assert_direct_debit_details_are_correct(
            BankRecord bankRecord, 
            DateTime expectedDate, 
            double expectedAmount, 
            string expectedDescription)
        {
            Assert.AreEqual(expectedDescription, bankRecord.Description);
            Assert.AreEqual(expectedDate, bankRecord.Date);
            Assert.AreEqual(expectedAmount, bankRecord.Unreconciled_amount);
            Assert.AreEqual("POS", bankRecord.Type);
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
            var next_direct_debit_date03 = lastDirectDebitDate.AddMonths(3);
            var next_direct_debit_date01 = lastDirectDebitDate.AddMonths(1);
            var next_direct_debit_date02 = lastDirectDebitDate.AddMonths(2);
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
        public void M_MergeBespokeDataWithPendingFile_WillAddMostRecentCredCardDirectDebits()
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
            Set_up_for_credit_card_data(
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo, 2);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var budgeting_months = new BudgetingMonths();
            var loading_info = BankAndBankOutData.LoadingInfo;
            var reconciliation_intro = new ReconciliationIntro(mock_input_output.Object);

            // Act
            reconciliation_intro.BankAndBankOut_MergeBespokeDataWithPendingFile(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                budgeting_months,
                loading_info);

            // Assert
            Assert.AreEqual(4, pending_records.Count);
            Assert_direct_debit_details_are_correct(pending_records[0], next_direct_debit_date01, expected_amount1, ReconConsts.Cred_card1_dd_description);
            Assert_direct_debit_details_are_correct(pending_records[1], next_direct_debit_date02, expected_amount2, ReconConsts.Cred_card1_dd_description);
            Assert_direct_debit_details_are_correct(pending_records[2], next_direct_debit_date01, expected_amount1, ReconConsts.Cred_card2_dd_description);
            Assert_direct_debit_details_are_correct(pending_records[3], next_direct_debit_date02, expected_amount2, ReconConsts.Cred_card2_dd_description);
        }
    }
}
