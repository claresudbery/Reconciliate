﻿using System;
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
    // These tests are just testing the BankAndBankOut functionality in ReconciliationIntro
    [TestFixture]
    public class ReconciliationIntro_BankAndBankOut_Tests
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

        private void Assert_pending_record_is_given_the_specified_direct_debit_details(
            BankRecord pending_record,
            DateTime expected_date,
            double expected_amount,
            string expected_description)
        {
            Assert.AreEqual(expected_description, pending_record.Description);
            Assert.AreEqual(expected_date, pending_record.Date);
            Assert.AreEqual(expected_amount, pending_record.Unreconciled_amount);
            Assert.AreEqual("POS", pending_record.Type);
        }

        private ReconciliationIntro Set_up_for_CredCard1_and_CredCard2_data(
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

            return new ReconciliationIntro(mock_input_output.Object);
        }

        private void Set_up_for_credit_card_data(
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
                    MainSheetNames.Bank_out, direct_debit_description, new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn}))
                .Returns(direct_debit_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(
                    MainSheetNames.Bank_out, direct_debit_row_number))
                .Returns(mock_cell_row.Object);
        }

        [Test]
        public void M_MergeBespokeDataWithPendingFile_WillAddMostRecentCredCardDirectDebits()
        {
            // Arrange
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);

            TestHelper.Set_correct_date_formatting();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var reconciliation_intro = Set_up_for_CredCard1_and_CredCard2_data(
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
            reconciliation_intro.Bank_and_bank_out__Merge_bespoke_data_with_pending_file(
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
