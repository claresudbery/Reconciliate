using System;
using System.Collections.Generic;
using System.IO;
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
    public class CredCard1AndCredCard1InOutLoaderTests
    {
        private void Prepare_mock_spreadsheet_repo_for_merge_bespoke_data(
            Mock<IInputOutput> mock_input_output, 
            Mock<ISpreadsheetRepo> mock_spreadsheet_repo)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            Set_up_for_direct_debit_dates(last_direct_debit_date, mock_input_output);
            FileLoaderTestHelper.Set_up_mock_spreadsheet_repo_for_credit_card_DD(
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                mock_spreadsheet_repo,
                1);
        }

        private void Prepare_mock_spreadsheet_for_merge_bespoke_data(
            Mock<IInputOutput> mock_input_output,
            Mock<ISpreadsheet> mock_spreadsheet)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            Set_up_for_direct_debit_dates(last_direct_debit_date, mock_input_output);
            FileLoaderTestHelper.Set_up_mock_spreadsheet_for_credit_card_DD(
                ReconConsts.Cred_card1_dd_description,
                mock_spreadsheet);
        }

        private void Set_up_for_direct_debit_dates(
            DateTime last_direct_debit_date,
            Mock<IInputOutput> mock_input_output)
        {
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;

            TestHelper.Set_correct_date_formatting();

            FileLoaderTestHelper.Set_up_for_direct_debit_dates(
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output);
        }

        [Test]
        public void CredCard1_and_credCard1_in_out__Merge_bespoke_data_with_pending_file__Will_add_most_recent_CredCard1_direct_debits()
        {
            // Arrange
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);

            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            FileLoaderTestHelper.Set_up_for_direct_debit_dates(
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output);
            FileLoaderTestHelper.Set_up_mock_spreadsheet_repo_for_credit_card_DD(
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                mock_spreadsheet_repo,
                1);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<CredCard1InOutRecord>>();
            var pending_records = new List<CredCard1InOutRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var file_loader = new CredCard1AndCredCard1InOutLoader(mock_input_output.Object);

            // Act
            file_loader.Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                new BudgetingMonths(),
                CredCard1AndCredCard1InOutData.LoadingInfo);

            // Assert
            Assert.AreEqual(2, pending_records.Count);

            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_direct_debit_details(pending_records[0], next_direct_debit_date01, expected_amount1, ReconConsts.Cred_card1_regular_pymt_description);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_direct_debit_details(pending_records[1], next_direct_debit_date02, expected_amount2, ReconConsts.Cred_card1_regular_pymt_description);
        }

        [Test]
        public void Cred_card1_and_cred_card1_in_out__Merge_bespoke_data_with_pending_file__Will_update_CredCard1_balances_on_totals_sheet()
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
            var file_loader = new CredCard1AndCredCard1InOutLoader(mock_input_output.Object);

            // Act
            file_loader.Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                mock_pending_file.Object,
                new BudgetingMonths(),
                CredCard1AndCredCard1InOutData.LoadingInfo);

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