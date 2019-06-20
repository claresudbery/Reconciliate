using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
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
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[0], next_direct_debit_date01, expected_amount1);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard1_direct_debit_details(pending_records[1], next_direct_debit_date02, expected_amount2);
            // CredCard2:
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[2], next_direct_debit_date01, expected_amount1);
            FileLoaderTestHelper.Assert_pending_record_is_given_the_specified_CredCard2_direct_debit_details(pending_records[3], next_direct_debit_date02, expected_amount2);
        }
    }
}