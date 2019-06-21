using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class BankAndBankOutLoaderTests
    {
        private void Prepare_mock_spreadsheet_for_merge_bespoke_data(
            Mock<IInputOutput> mock_input_output,
            Mock<ISpreadsheetRepo> mock_spreadsheet_repo)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            double expected_amount1 = 1234.55;
            double expected_amount2 = 5673.99;

            TestHelper.Set_correct_date_formatting();
            FileLoaderTestHelper.Set_up_for_CredCard1_and_CredCard2_data(
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo);
        }

        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_third_party_file_name_from_loading_info()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var loading_info = BankAndBankOutData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            FileLoaderTestHelper.Prepare_mock_spreadsheet_for_annual_budgeting<BankRecord>(mock_spreadsheet_repo, loading_info);
            Prepare_mock_spreadsheet_for_merge_bespoke_data(mock_input_output, mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var bank_and_bank_out_loader = new BankAndBankOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            var reconciliation_interface = bank_and_bank_out_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths);

            // Assert 
            var third_party_file_io = ((BankReconciliator)reconciliation_interface.Reconciliator).Third_party_file.File_io;
            Assert.AreEqual(loading_info.File_paths.Third_party_file_name, third_party_file_io.File_name);
        }

        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_owned_file_name_from_loading_info()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var loading_info = BankAndBankOutData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            FileLoaderTestHelper.Prepare_mock_spreadsheet_for_annual_budgeting<BankRecord>(mock_spreadsheet_repo, loading_info);
            Prepare_mock_spreadsheet_for_merge_bespoke_data(mock_input_output, mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var bank_and_bank_out_loader = new BankAndBankOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            var reconciliation_interface = bank_and_bank_out_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths);

            // Assert 
            var owned_file_io = ((BankReconciliator)reconciliation_interface.Reconciliator).Owned_file.File_io;
            Assert.AreEqual(loading_info.File_paths.Owned_file_name, owned_file_io.File_name);
        }

        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_third_party_descriptor_from_loading_info()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var loading_info = BankAndBankOutData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            FileLoaderTestHelper.Prepare_mock_spreadsheet_for_annual_budgeting<BankRecord>(mock_spreadsheet_repo, loading_info);
            Prepare_mock_spreadsheet_for_merge_bespoke_data(mock_input_output, mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var bank_and_bank_out_loader = new BankAndBankOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            var reconciliation_interface = bank_and_bank_out_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths);

            // Assert 
            Assert.AreEqual(loading_info.Third_party_descriptor, reconciliation_interface.Third_party_descriptor);
        }

        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_owned_file_descriptor_from_loading_info()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var loading_info = BankAndBankOutData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            FileLoaderTestHelper.Prepare_mock_spreadsheet_for_annual_budgeting<BankRecord>(mock_spreadsheet_repo, loading_info);
            Prepare_mock_spreadsheet_for_merge_bespoke_data(mock_input_output, mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var bank_and_bank_out_loader = new BankAndBankOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            var reconciliation_interface = bank_and_bank_out_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths);

            // Assert 
            Assert.AreEqual(loading_info.Owned_file_descriptor, reconciliation_interface.Owned_file_descriptor);
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
            FileLoaderTestHelper.Set_up_for_CredCard1_and_CredCard2_data(
                last_direct_debit_date,
                expected_amount1,
                expected_amount2,
                mock_input_output,
                mock_spreadsheet_repo);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var file_loader = new BankAndBankOutLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            file_loader.Merge_bespoke_data_with_pending_file(
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
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[0], next_direct_debit_date01, expected_amount1);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[1], next_direct_debit_date02, expected_amount2);
            // CredCard2:
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[2], next_direct_debit_date01, expected_amount1);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[3], next_direct_debit_date02, expected_amount2);
        }
    }
}