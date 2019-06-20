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
    public class CredCard2AndCredCard2InOutLoaderTests
    {
        [Test]
        public void CredCard2_and_credCard2_in_out__Merge_bespoke_data_with_pending_file__Will_add_most_recent_CredCard2_direct_debits()
        {
            // Arrange
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);

            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            FileLoaderTestHelper.Set_up_for_credit_card_data(
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo, 1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            var pending_records = new List<CredCard2InOutRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var file_loader = new CredCard2AndCredCard2InOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            file_loader.Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                new BudgetingMonths(),
                CredCard2AndCredCard2InOutData.LoadingInfo);

            // Assert
            Assert.AreEqual(2, pending_records.Count);

            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_direct_debit_details(pending_records[0], next_direct_debit_date01, expected_amount1, ReconConsts.Cred_card2_regular_pymt_description);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_direct_debit_details(pending_records[1], next_direct_debit_date02, expected_amount2, ReconConsts.Cred_card2_regular_pymt_description);
        }

        [Test]
        public void CredCard2_and_credCard2_in_out__Merge_bespoke_data_with_pending_file__Will_update_CredCard2_balances_on_totals_sheet()
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
                        ReconConsts.Cred_card2_name,
                        next_direct_debit_date01.ToShortDateString()), ""))
                .Returns(new_balance.ToString);
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        ReconConsts.Cred_card2_name,
                        next_direct_debit_date02.ToShortDateString()), ""))
                .Returns("0");
            var bank_record = new BankRecord { Date = last_direct_debit_date };
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card2_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);
            var mock_pending_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            var pending_records = new List<CredCard2InOutRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var file_loader = new CredCard2AndCredCard2InOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            file_loader.Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                mock_pending_file.Object,
                new BudgetingMonths(),
                CredCard2AndCredCard2InOutData.LoadingInfo);

            // Assert
            mock_spreadsheet.Verify(x => x.Update_balance_on_totals_sheet(
                Codes.Cred_card2_bal,
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.Cred_card2_name,
                    $"{last_direct_debit_date.ToString("MMM")} {last_direct_debit_date.Year}"),
                5, 6, 4), Times.Exactly(1));
        }
    }
}