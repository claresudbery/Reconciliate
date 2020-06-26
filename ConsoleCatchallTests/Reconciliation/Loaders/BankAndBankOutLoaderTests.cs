using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;
using ConsoleCatchall.Console.Reconciliation.Extensions;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class BankAndBankOutLoaderTests
    {
        private void Assert_direct_debit_details_are_correct(
            BankRecord bank_record, 
            DateTime expected_date, 
            double expected_amount, 
            string expected_description)
        {
            Assert.AreEqual(expected_description, bank_record.Description);
            Assert.AreEqual(expected_date, bank_record.Date);
            Assert.AreEqual(expected_amount, bank_record.Unreconciled_amount);
            Assert.AreEqual("POS", bank_record.Type);
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
            var loading_info = new BankAndBankOutLoader().Loading_info();
            var bank_and_bank_out_loader = new BankAndBankOutLoader();

            // Act
            bank_and_bank_out_loader.Merge_bespoke_data_with_pending_file(
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

        [Test]
        public void Will_update_bank_balance()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockInputOutput = new Mock<IInputOutput>();
            string expected_description = "Balance row";
            double expected_balance = 970.00;
            double expected_transaction_amount = -10.00;
            var fake_records = new List<ActualBankRecord>
            {
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1)},
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = expected_transaction_amount, Balance = expected_balance, Date = new DateTime(2020, 1, 4), Description = expected_description},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);
            var mock_bank_file = new Mock<ICSVFile<BankRecord>>();
            var bank_file = new GenericFile<BankRecord>(mock_bank_file.Object);

            // Act 
            bank_and_bank_out_loader.Do_actions_which_require_third_party_data_access(
                actual_bank_out_file,
                bank_file,
                mockSpreadsheet.Object, 
                mockInputOutput.Object);

            // Assert
            mockSpreadsheet.Verify(x => x.Update_balance_on_totals_sheet(
                Codes.Bank_bal,
                expected_balance,
                It.Is<string>(y => y.Contains(expected_description) 
                                   && y.Contains(expected_transaction_amount.To_csv_string(true))),
                ReconConsts.BankBalanceAmountColumn,
                ReconConsts.BankBalanceTextColumn,
                ReconConsts.BankBalanceCodeColumn,
                It.IsAny<IInputOutput>()));
        }

        [Test]
        public void Will_assume_bank_records_are_in_reverse_date_order_when_looking_for_bank_balance_transaction()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockInputOutput = new Mock<IInputOutput>();
            mockInputOutput.Setup(x => x.Get_generic_input(ReconConsts.MultipleBalanceRows)).Returns("1");
            string expected_description = "Balance row";
            string alternative_candidate = "another potential candidate";
            var fake_records = new List<ActualBankRecord>
            {
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 4), Description = expected_description},
                new ActualBankRecord {Amount = 10, Balance = 980.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 4), Description = alternative_candidate},
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);
            var mock_bank_file = new Mock<ICSVFile<BankRecord>>();
            var bank_file = new GenericFile<BankRecord>(mock_bank_file.Object);

            // Act 
            bank_and_bank_out_loader.Do_actions_which_require_third_party_data_access(
                actual_bank_out_file,
                bank_file,
                mockSpreadsheet.Object, 
                mockInputOutput.Object);

            // Assert
            mockInputOutput.Verify(x => x.Output_line(It.Is<string>(y => y.Contains(expected_description))), Times.Exactly(2));
            mockInputOutput.Verify(x => x.Output_line(It.Is<string>(y => y.Contains(alternative_candidate))), Times.Exactly(1));
            mockSpreadsheet.Verify(x => x.Update_balance_on_totals_sheet(
                Codes.Bank_bal,
                It.IsAny<double>(),
                It.Is<string>(y => y.Contains(expected_description)),
                ReconConsts.BankBalanceAmountColumn,
                ReconConsts.BankBalanceTextColumn,
                ReconConsts.BankBalanceCodeColumn,
                It.IsAny<IInputOutput>()));
        }

        [Test]
        public void Will_update_living_expenses_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            
            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
                );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code004,
                Codes.Code074));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("weekly spends")), ""));
        }

        [Test]
        public void Will_update_groceries_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code005,
                Codes.Code075));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("grocery shopping")), ""));
        }

        [Test]
        public void Will_update_yoga_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_input_output.Setup(x => x.Get_input(It.Is<string>(y => y.Contains("yoga")), ""))
                .Returns("2");

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code078,
                Codes.Code078));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("yoga")), ""));
        }

        [Test]
        public void Will_update_fuel_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_input_output.Setup(x => x.Get_input(It.Is<string>(y => y.Contains("fuel")), ""))
                .Returns("2");

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code006,
                Codes.Code006));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("fuel")), ""));
        }

        [Test]
        public void Will_update_vet_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_input_output.Setup(x => x.Get_input(It.Is<string>(y => y.Contains("vet")), ""))
                .Returns("2");

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code007,
                Codes.Code007));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("vet")), ""));
        }

        [Test]
        public void Will_update_internet_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_input_output.Setup(x => x.Get_input(It.Is<string>(y => y.Contains("internet")), ""))
                .Returns("2");

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                mock_input_output.Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_expected_out(
                It.IsAny<int>(),
                Codes.Code011,
                Codes.Code011));
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("internet")), ""));
        }

        [Test]
        public void Will_update_owed_CHB_when_generating_ad_hoc_data()
        {
            // Arrange
            var bank_and_bank_out_loader = new BankAndBankOutLoader();
            var mock_spreadsheet = new Mock<ISpreadsheet>();

            // Act
            bank_and_bank_out_loader.Generate_ad_hoc_data(
                new Mock<IInputOutput>().Object,
                mock_spreadsheet.Object,
                new Mock<ICSVFile<BankRecord>>().Object,
                new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 },
                new DataLoadingInformation<ActualBankRecord, BankRecord>()
            );

            // Assert
            mock_spreadsheet.Verify(x => x.Update_owed_CHB(It.IsAny<BudgetingMonths>()));
        }
    }
}
