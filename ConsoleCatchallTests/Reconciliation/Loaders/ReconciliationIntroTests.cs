﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public partial class ReconciliationIntroTests : IInputOutput
    {
        private Mock<IInputOutput> _mock_input_output;

        private void Set_up_for_loader_bespoke_stuff(Mock<IInputOutput> mock_input_output, Mock<ISpreadsheet> mock_spreadsheet)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var bank_record = new BankRecord { Date = last_direct_debit_date };

            mock_input_output.Setup(x => x.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.Cred_card2_name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card2_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);

            mock_input_output.Setup(x => x.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.Cred_card1_name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card1_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);
        }

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            TestHelper.Set_correct_date_formatting();
        }

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
        }

        [TestCase(1, 4, "Jan", "Apr", 4)]
        [TestCase(1, 10, "Jan", "Oct", 10)]
        [TestCase(12, 3, "Dec", "Mar", 4)]
        [TestCase(12, 1, "Dec", "Jan", 2)]
        [TestCase(11, 2, "Nov", "Feb", 4)]
        [TestCase(10, 1, "Oct", "Jan", 4)]
        [TestCase(10, 3, "Oct", "Mar", 6)]
        [TestCase(9, 9, "Sep", "Sep", 1)]
        [TestCase(10, 9, "Oct", "Sep", 12)]
        public void RecursivelyAskForBudgetingMonths_WillCheckResultsWithUserAndAskForConfirmation(
            int next_unplanned_month, int user_input, string first_month, string second_month, int month_span)
        {
            // Arrange
            DateTime unplanned_month = new DateTime(2018, next_unplanned_month, 1);
            _get_input_messages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(unplanned_month);
            string confirmation_text = string.Format(ReconConsts.ConfirmMonthInterval, first_month, second_month, month_span);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{user_input}")
                .Returns("Y");
            // Use self-shunt to avoid infinite recursion:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(confirmation_text, _get_input_messages.Last());
        }

        [Test]
        public void WhenBudgetingIsConfirmed_WillReturnRelevantValue()
        {
            // Arrange
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            int month_input = 2;
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{month_input}")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mock_input_output.Object);

            // Act
            var result = reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(month_input, result.Last_month_for_budget_planning);
        }

        [Test]
        public void IfBudgetingIsNotConfirmed_WillAskForReEntry()
        {
            // Arrange
            _get_input_messages.Clear();
            int month_input = 2;
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{month_input}")
                .Returns("N")
                .Returns($"{month_input + 1}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month.Month);
            string month_input_request = string.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            Assert.AreEqual(2, _get_input_messages.Count(x => x == month_input_request));
        }

        [TestCase("13")]
        [TestCase("0")]
        [TestCase("53")]
        [TestCase("2")]
        [TestCase("xx")]
        [TestCase("Y")]
        [TestCase("N")]
        [TestCase("-4")]
        [TestCase("1")]
        [TestCase("")]
        [TestCase("12")]
        public void RecursivelyAskForBudgetingMonths_WillOnlyReturnANumberBetweenZeroAndTwelve(string user_input)
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(user_input)
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mock_input_output.Object);

            // Act
            var result = reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(result.Last_month_for_budget_planning >= 0 && result.Last_month_for_budget_planning <= 12);
        }

        [TestCase("13")]
        [TestCase("0")]
        [TestCase("23")]
        [TestCase("0f")]
        [TestCase("df")]
        [TestCase("-")]
        [TestCase("")]
        public void RecursivelyAskForBudgetingMonths_WillAskForConfirmation_IfUserGivesBadOrZeroInput(string user_input)
        {
            // Arrange
            _get_input_messages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{user_input}")
                .Returns("Y")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);
            
            // Act
            reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(1, _get_input_messages.Count(x => x == ReconConsts.ConfirmBadMonth));
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillOutputAcknowledgement_AndReturnZero()
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mock_input_output.Object);

            // Act
            var result = reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            _mock_input_output.Verify(x => x.Output_line(ReconConsts.ConfirmNoMonthlyBudgeting));
            Assert.AreEqual(0, result.Last_month_for_budget_planning);
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillNotTryToCalculateMonthSpan()
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mock_input_output.Object);
            bool exception_thrown = false;

            // Act
            try
            {
                reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void IfNoBudgetingWasNotIntended_WillAskUserForInputAgain()
        {
            // Arrange
            _get_input_messages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Returns(next_unplanned_month);
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("N")
                .Returns($"{11}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month.Month);
            string month_input_request = string.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            Assert.AreEqual(2, _get_input_messages.Count(x => x == month_input_request));
        }

        [Test]
        public void IfCantFindNextUnplannedMonth_WillAskUserToEnterIt()
        {
            // Arrange
            _get_input_messages.Clear();
            int user_input_for_next_unplanned_month = 3;
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Throws(new Exception());
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{user_input_for_next_unplanned_month}")
                .Returns($"{user_input_for_next_unplanned_month}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(_get_input_messages.Contains(ReconConsts.CantFindMortgageRow));
            Assert.AreEqual(user_input_for_next_unplanned_month, result.Next_unplanned_month);
        }
        
        [TestCase("13")]
        [TestCase("0")]
        [TestCase("23")]
        [TestCase("0f")]
        [TestCase("df")]
        [TestCase("-")]
        [TestCase("")]
        public void IfCantFindNextUnplannedMonth_AndUserEntersBadInput_WillDefaultToThisMonth(string bad_input)
        {
            // Arrange
            var default_month = DateTime.Today.Month;
            _get_input_messages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.Get_next_unplanned_month()).Throws(new Exception());
            _mock_input_output.SetupSequence(x => x.Get_input(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(bad_input)
                .Returns("1")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.Recursively_ask_for_budgeting_months(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(_output_single_line_recorded_messages.Contains(ReconConsts.DefaultUnplannedMonth));
            Assert.AreEqual(default_month, result.Next_unplanned_month);
        }
    }
}