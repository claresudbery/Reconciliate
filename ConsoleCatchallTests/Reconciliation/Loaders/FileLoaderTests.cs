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
    public class FileLoaderTests
    {
        private void Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(
            BankRecord pending_record,
            DateTime expected_date,
            double expected_amount)
        {
            Assert_pending_record_is_given_the_specified_direct_debit_details(
                pending_record,
                expected_date,
                expected_amount,
                ReconConsts.Cred_card1_dd_description);
        }

        private void Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(
            BankRecord pending_record,
            DateTime expected_date,
            double expected_amount)
        {
            Assert_pending_record_is_given_the_specified_direct_debit_details(
                pending_record,
                expected_date,
                expected_amount,
                ReconConsts.Cred_card2_dd_description);
        }

        public void Assert_pending_record_is_given_the_specified_direct_debit_details(
            ICSVRecord pending_record,
            DateTime expected_date,
            double expected_amount,
            string expected_description)
        {
            Assert.AreEqual(expected_description, pending_record.Description);
            Assert.AreEqual(expected_date, pending_record.Date);
            Assert.AreEqual(expected_amount, pending_record.Main_amount());
        }

        private FileLoader Set_up_for_CredCard1_and_CredCard2_data(
            DateTime last_direct_debit_date,
            double expected_amount1,
            double expected_amount2,
            Mock<IInputOutput> mock_input_output,
            Mock<ISpreadsheetRepo> mock_spreadsheet_repo)
        {
            // CredCard1:
            Set_up_for_credit_card_data(
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo,
                1);

            // CredCard2:
            Set_up_for_credit_card_data(
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description,
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo,
                2);

            return new FileLoader();
        }

        public void Set_up_for_credit_card_data(
            string cred_card_name,
            string direct_debit_description,
            DateTime last_direct_debit_date,
            double expected_amount1,
            double expected_amount2,
            Mock<IInputOutput> mock_input_output,
            Mock<ISpreadsheetRepo> mock_spreadsheet_repo,
            int direct_debit_row_number)
        {
            var next_direct_debit_date03 = last_direct_debit_date.AddMonths(3);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            double expected_amount3 = 0;
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        cred_card_name,
                        next_direct_debit_date01.ToShortDateString()), ""))
                .Returns(expected_amount1.ToString);
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        cred_card_name,
                        next_direct_debit_date02.ToShortDateString()), ""))
                .Returns(expected_amount2.ToString);
            mock_input_output
                .Setup(x => x.Get_input(
                    string.Format(
                        ReconConsts.AskForCredCardDirectDebit,
                        cred_card_name,
                        next_direct_debit_date03.ToShortDateString()), ""))
                .Returns(expected_amount3.ToString);
            var mock_cell_row = new Mock<ICellRow>();
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DateIndex)).Returns(last_direct_debit_date.ToOADate());
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.DescriptionIndex)).Returns(direct_debit_description);
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.TypeIndex)).Returns("Type");
            mock_cell_row.Setup(x => x.Read_cell(BankRecord.UnreconciledAmountIndex)).Returns((double)0);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_with_cell_containing_text(
                    MainSheetNames.Bank_out, direct_debit_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(direct_debit_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(
                    MainSheetNames.Bank_out, direct_debit_row_number))
                .Returns(mock_cell_row.Object);
        }

        [Test]
        public void Bank_and_bank_in__Merge_bespoke_data_with_pending_file__Will_merge_unreconciled_employer_expenses_with_pending_file()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var divider_row_number = 10;
            var last_row_number = 11;
            var mock_cell_row = new Mock<ICellRow>();
            var description = "Hello";
            var code = Codes.Expenses;
            mock_cell_row.Setup(x => x.Read_cell(ExpectedIncomeRecord.DescriptionIndex)).Returns(description);
            mock_cell_row.Setup(x => x.Read_cell(ExpectedIncomeRecord.CodeIndex)).Returns(code);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(MainSheetNames.Expected_in, Dividers.Divider_text, 2)).Returns(divider_row_number);
            mock_spreadsheet_repo.Setup(x => x.Last_row_number(MainSheetNames.Expected_in)).Returns(last_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(MainSheetNames.Expected_in, last_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var budgeting_months = new BudgetingMonths();
            var loading_info = BankAndBankInData.LoadingInfo;
            var file_loader = new FileLoader();

            // Act
            file_loader.Bank_and_bank_in__Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                budgeting_months,
                loading_info);

            // Assert
            mock_input_output.Verify(x => x.Output_line(ReconConsts.Loading_expenses));
            Assert.AreEqual(1, pending_records.Count);
            Assert.AreEqual(description, pending_records[0].Description);
        }

        [Test]
        public void Bank_and_bank_out__Merge_bespoke_data_with_pending_file__Will_add_most_recent_cred_card_direct_debits()
        {
            // Arrange
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);

            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var file_loader = Set_up_for_CredCard1_and_CredCard2_data(
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);

            // Act
            file_loader.Bank_and_bank_out__Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                new BudgetingMonths(),
                BankAndBankOutData.LoadingInfo);

            // Assert
            Assert.AreEqual(4, pending_records.Count);

            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var next_direct_debit_date02 = last_direct_debit_date.AddMonths(2);
            // CredCard1:
            Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[0], next_direct_debit_date01, expected_amount1);
            Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[1], next_direct_debit_date02, expected_amount2);
            // CredCard2:
            Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[2], next_direct_debit_date01, expected_amount1);
            Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[3], next_direct_debit_date02, expected_amount2);
        }
    }
}