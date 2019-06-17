using System;
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
        private Mock<IInputOutput> _mockInputOutput;

        private void SetUpForLoaderBespokeStuff(Mock<IInputOutput> mockInputOutput, Mock<ISpreadsheet> mockSpreadsheet)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var bank_record = new BankRecord { Date = last_direct_debit_date };

            mockInputOutput.Setup(x => x.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard2Name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mockSpreadsheet.Setup(x => x.GetMostRecentRowContainingText<BankRecord>(
                    MainSheetNames.BankOut, ReconConsts.CredCard2DdDescription, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);

            mockInputOutput.Setup(x => x.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.CredCard1Name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mockSpreadsheet.Setup(x => x.GetMostRecentRowContainingText<BankRecord>(
                    MainSheetNames.BankOut, ReconConsts.CredCard1DdDescription, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestHelper.SetCorrectDateFormatting();
        }

        [SetUp]
        public void SetUp()
        {
            _mockInputOutput = new Mock<IInputOutput>();
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
            int nextUnplannedMonth, int userInput, string firstMonth, string secondMonth, int monthSpan)
        {
            // Arrange
            DateTime unplanned_month = new DateTime(2018, nextUnplannedMonth, 1);
            _getInputMessages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(unplanned_month);
            string confirmation_text = string.Format(ReconConsts.ConfirmMonthInterval, firstMonth, secondMonth, monthSpan);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{userInput}")
                .Returns("Y");
            // Use self-shunt to avoid infinite recursion:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(confirmation_text, _getInputMessages.Last());
        }

        [Test]
        public void WhenBudgetingIsConfirmed_WillReturnRelevantValue()
        {
            // Arrange
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            int month_input = 2;
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{month_input}")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(month_input, result.LastMonthForBudgetPlanning);
        }

        [Test]
        public void IfBudgetingIsNotConfirmed_WillAskForReEntry()
        {
            // Arrange
            _getInputMessages.Clear();
            int month_input = 2;
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{month_input}")
                .Returns("N")
                .Returns($"{month_input + 1}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month.Month);
            string month_input_request = string.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            Assert.AreEqual(2, _getInputMessages.Count(x => x == month_input_request));
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
        public void RecursivelyAskForBudgetingMonths_WillOnlyReturnANumberBetweenZeroAndTwelve(string userInput)
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 11, 1);
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(userInput)
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(result.LastMonthForBudgetPlanning >= 0 && result.LastMonthForBudgetPlanning <= 12);
        }

        [TestCase("13")]
        [TestCase("0")]
        [TestCase("23")]
        [TestCase("0f")]
        [TestCase("df")]
        [TestCase("-")]
        [TestCase("")]
        public void RecursivelyAskForBudgetingMonths_WillAskForConfirmation_IfUserGivesBadOrZeroInput(string userInput)
        {
            // Arrange
            _getInputMessages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{userInput}")
                .Returns("Y")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);
            
            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.AreEqual(1, _getInputMessages.Count(x => x == ReconConsts.ConfirmBadMonth));
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillOutputAcknowledgement_AndReturnZero()
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            _mockInputOutput.Verify(x => x.OutputLine(ReconConsts.ConfirmNoMonthlyBudgeting));
            Assert.AreEqual(0, result.LastMonthForBudgetPlanning);
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillNotTryToCalculateMonthSpan()
        {
            // Arrange
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);
            bool exception_thrown = false;

            // Act
            try
            {
                reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);
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
            _getInputMessages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            DateTime next_unplanned_month = new DateTime(2018, 10, 1);
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(next_unplanned_month);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("N")
                .Returns($"{11}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month.Month);
            string month_input_request = string.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            Assert.AreEqual(2, _getInputMessages.Count(x => x == month_input_request));
        }

        [Test]
        public void IfCantFindNextUnplannedMonth_WillAskUserToEnterIt()
        {
            // Arrange
            _getInputMessages.Clear();
            int user_input_for_next_unplanned_month = 3;
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Throws(new Exception());
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{user_input_for_next_unplanned_month}")
                .Returns($"{user_input_for_next_unplanned_month}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(_getInputMessages.Contains(ReconConsts.CantFindMortgageRow));
            Assert.AreEqual(user_input_for_next_unplanned_month, result.NextUnplannedMonth);
        }
        
        [TestCase("13")]
        [TestCase("0")]
        [TestCase("23")]
        [TestCase("0f")]
        [TestCase("df")]
        [TestCase("-")]
        [TestCase("")]
        public void IfCantFindNextUnplannedMonth_AndUserEntersBadInput_WillDefaultToThisMonth(string badInput)
        {
            // Arrange
            var default_month = DateTime.Today.Month;
            _getInputMessages.Clear();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet.Setup(x => x.GetNextUnplannedMonth()).Throws(new Exception());
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(badInput)
                .Returns("1")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mock_spreadsheet.Object);

            // Assert
            Assert.IsTrue(_outputSingleLineRecordedMessages.Contains(ReconConsts.DefaultUnplannedMonth));
            Assert.AreEqual(default_month, result.NextUnplannedMonth);
        }
    }
}
