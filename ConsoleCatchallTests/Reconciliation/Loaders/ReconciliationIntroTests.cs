using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
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
            DateTime lastDirectDebitDate = new DateTime(2018, 12, 17);
            var nextDirectDebitDate01 = lastDirectDebitDate.AddMonths(1);
            var bankRecord = new BankRecord { Date = lastDirectDebitDate };

            mockInputOutput.Setup(x => x.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard2Name,
                nextDirectDebitDate01.ToShortDateString()), "")).Returns("0");
            mockSpreadsheet.Setup(x => x.GetMostRecentRowContainingText<BankRecord>(
                    MainSheetNames.BankOut, ReconConsts.CredCard2DdDescription, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bankRecord);

            mockInputOutput.Setup(x => x.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.CredCard1Name,
                nextDirectDebitDate01.ToShortDateString()), "")).Returns("0");
            mockSpreadsheet.Setup(x => x.GetMostRecentRowContainingText<BankRecord>(
                    MainSheetNames.BankOut, ReconConsts.CredCard1DdDescription, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bankRecord);
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

        [Test]
        public void M_WillNotDeleteUnreconciledRowsWhenMergingPendingWithUnreconciled()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var reconciliate = new ReconciliationIntro(mockInputOutput.Object);
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankOutFileIO = new Mock<IFileIO<BankRecord>>();
            var budgetingMonths = new BudgetingMonths();
            mockPendingFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), It.IsAny<char>()))
                .Returns(new List<BankRecord>());
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord>());
            mockBankOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var loadingInfo = new DummyLoader().LoadingInfo();

            // Act
            reconciliate.LoadFilesAndMergeData<ActualBankRecord, BankRecord>(
                mockSpreadsheet.Object,
                mockPendingFileIO.Object,
                mockPendingFile.Object,
                mockActualBankFileIO.Object,
                mockBankOutFileIO.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            mockSpreadsheet
                .Verify(x => x.DeleteUnreconciledRows(It.IsAny<string>()),
                    Times.Never);
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
            DateTime unplannedMonth = new DateTime(2018, nextUnplannedMonth, 1);
            _getInputMessages.Clear();
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(unplannedMonth);
            string confirmationText = string.Format(ReconConsts.ConfirmMonthInterval, firstMonth, secondMonth, monthSpan);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{userInput}")
                .Returns("Y");
            // Use self-shunt to avoid infinite recursion:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            Assert.AreEqual(confirmationText, _getInputMessages.Last());
        }

        [Test]
        public void WhenBudgetingIsConfirmed_WillReturnRelevantValue()
        {
            // Arrange
            DateTime nextUnplannedMonth = new DateTime(2018, 11, 1);
            int monthInput = 2;
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{monthInput}")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            Assert.AreEqual(monthInput, result.LastMonthForBudgetPlanning);
        }

        [Test]
        public void IfBudgetingIsNotConfirmed_WillAskForReEntry()
        {
            // Arrange
            _getInputMessages.Clear();
            int monthInput = 2;
            DateTime nextUnplannedMonth = new DateTime(2018, 11, 1);
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{monthInput}")
                .Returns("N")
                .Returns($"{monthInput + 1}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            string nextUnplannedMonthAsString = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nextUnplannedMonth.Month);
            string monthInputRequest = string.Format(ReconConsts.EnterMonths, nextUnplannedMonthAsString);
            Assert.AreEqual(2, _getInputMessages.Count(x => x == monthInputRequest));
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
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            DateTime nextUnplannedMonth = new DateTime(2018, 11, 1);
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(userInput)
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

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
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            DateTime nextUnplannedMonth = new DateTime(2018, 10, 1);
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{userInput}")
                .Returns("Y")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);
            
            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            Assert.AreEqual(1, _getInputMessages.Count(x => x == ReconConsts.ConfirmBadMonth));
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillOutputAcknowledgement_AndReturnZero()
        {
            // Arrange
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            DateTime nextUnplannedMonth = new DateTime(2018, 10, 1);
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            _mockInputOutput.Verify(x => x.OutputLine(ReconConsts.ConfirmNoMonthlyBudgeting));
            Assert.AreEqual(0, result.LastMonthForBudgetPlanning);
        }

        [Test]
        public void IfNoBudgetingIsConfirmed_WillNotTryToCalculateMonthSpan()
        {
            // Arrange
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            DateTime nextUnplannedMonth = new DateTime(2018, 10, 1);
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0")
                .Returns("Y")
                .Returns("Y");
            var reconciliate = new ReconciliationIntro(_mockInputOutput.Object);
            bool exceptionThrown = false;

            // Act
            try
            {
                reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void IfNoBudgetingWasNotIntended_WillAskUserForInputAgain()
        {
            // Arrange
            _getInputMessages.Clear();
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            DateTime nextUnplannedMonth = new DateTime(2018, 10, 1);
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Returns(nextUnplannedMonth);
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{0}")
                .Returns("N")
                .Returns($"{11}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            string nextUnplannedMonthAsString = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nextUnplannedMonth.Month);
            string monthInputRequest = string.Format(ReconConsts.EnterMonths, nextUnplannedMonthAsString);
            Assert.AreEqual(2, _getInputMessages.Count(x => x == monthInputRequest));
        }

        [Test]
        public void IfCantFindNextUnplannedMonth_WillAskUserToEnterIt()
        {
            // Arrange
            _getInputMessages.Clear();
            int userInputForNextUnplannedMonth = 3;
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Throws(new Exception());
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{userInputForNextUnplannedMonth}")
                .Returns($"{userInputForNextUnplannedMonth}")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            Assert.IsTrue(_getInputMessages.Contains(ReconConsts.CantFindMortgageRow));
            Assert.AreEqual(userInputForNextUnplannedMonth, result.NextUnplannedMonth);
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
            var defaultMonth = DateTime.Today.Month;
            _getInputMessages.Clear();
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet.Setup(x => x.GetNextUnplannedMonth()).Throws(new Exception());
            _mockInputOutput.SetupSequence(x => x.GetInput(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(badInput)
                .Returns("1")
                .Returns("Y");
            // Use self-shunt to track calls to GetInput:
            var reconciliate = new ReconciliationIntro(this);

            // Act
            var result = reconciliate.RecursivelyAskForBudgetingMonths(mockSpreadsheet.Object);

            // Assert
            Assert.IsTrue(_outputSingleLineRecordedMessages.Contains(ReconConsts.DefaultUnplannedMonth));
            Assert.AreEqual(defaultMonth, result.NextUnplannedMonth);
        }

        [Test]
        public void LoadFilesAndMergeData_WillUseAllTheCorrectDataToLoadSpreadsheetAndPendingAndBudgetedDataForBankIn()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var reconciliate = new ReconciliationIntro(mockInputOutput.Object);
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankOutFileIO = new Mock<IFileIO<BankRecord>>();
            var budgetingMonths = new BudgetingMonths();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord>());
            mockBankOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var loadingInfo = new BankAndBankOutData().LoadingInfo();
            SetUpForLoaderBespokeStuff(mockInputOutput, mockSpreadsheet);

            // Act
            var reconciliationInterface = reconciliate.LoadFilesAndMergeData<ActualBankRecord, BankRecord>(
                mockSpreadsheet.Object,
                mockPendingFileIO.Object,
                mockPendingFile.Object,
                mockActualBankFileIO.Object,
                mockBankOutFileIO.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            mockPendingFileIO.Verify(x => x.SetFilePaths(loadingInfo.FilePaths.MainPath, loadingInfo.PendingFileName));
            mockActualBankFileIO.Verify(x => x.SetFilePaths(loadingInfo.FilePaths.MainPath, loadingInfo.FilePaths.ThirdPartyFileName));
            mockBankOutFileIO.Verify(x => x.SetFilePaths(loadingInfo.FilePaths.MainPath, loadingInfo.FilePaths.OwnedFileName));
            mockPendingFile.Verify(x => x.Load(true, loadingInfo.DefaultSeparator, true));
            mockPendingFile.Verify(x => x.ConvertSourceLineSeparators(loadingInfo.DefaultSeparator, loadingInfo.LoadingSeparator));
            mockSpreadsheet.Verify(x => x.AddBudgetedMonthlyDataToPendingFile(
                budgetingMonths,
                It.IsAny<ICSVFile<BankRecord>>(),
                It.Is<BudgetItemListData>(y => y == loadingInfo.MonthlyBudgetData)));
            mockSpreadsheet.Verify(x => x.AddBudgetedAnnualDataToPendingFile(
                budgetingMonths,
                It.IsAny<ICSVFile<BankRecord>>(),
                It.Is<BudgetItemListData>(y => y == loadingInfo.AnnualBudgetData)));
            mockPendingFile.Verify(x => x.UpdateSourceLinesForOutput(loadingInfo.LoadingSeparator));
            mockSpreadsheet.Verify(x => x.AddUnreconciledRowsToCsvFile(loadingInfo.SheetName, It.IsAny<ICSVFile<BankRecord>>()));
            mockPendingFile.Verify(x => x.WriteToFileAsSourceLines(loadingInfo.FilePaths.OwnedFileName));
            mockInputOutput.Verify(x => x.OutputLine("Loading data from pending file (which you should have already split out, if necessary)..."));
            mockInputOutput.Verify(x => x.OutputLine("Merging budget data with pending data..."));
            mockInputOutput.Verify(x => x.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data..."));
            mockInputOutput.Verify(x => x.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file..."));
            mockInputOutput.Verify(x => x.OutputLine("Loading data back in from 'owned' and 'third party' files..."));
            mockInputOutput.Verify(x => x.OutputLine("Creating reconciliation interface..."));
            Assert.AreEqual(loadingInfo.ThirdPartyDescriptor, reconciliationInterface.ThirdPartyDescriptor, "Third Party Descriptor");
            Assert.AreEqual(loadingInfo.OwnedFileDescriptor, reconciliationInterface.OwnedFileDescriptor, "Owned File Descriptor");
        }

        [Test]
        public void M_WillCallMergeBespokeDataWithPendingFile_ForPassedInLoader()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var reconciliate = new ReconciliationIntro(mockInputOutput.Object);
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankOutFileIO = new Mock<IFileIO<BankRecord>>();
            var budgetingMonths = new BudgetingMonths();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord>());
            mockBankOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var mockLoader = new Mock<ILoader<ActualBankRecord, BankRecord>>();
            var loadingInfo = new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                Loader = mockLoader.Object,
                FilePaths = new FilePaths()
            };

            // Act
            reconciliate.LoadFilesAndMergeData<ActualBankRecord, BankRecord>(
                mockSpreadsheet.Object,
                mockPendingFileIO.Object,
                mockPendingFile.Object,
                mockActualBankFileIO.Object,
                mockBankOutFileIO.Object,
                budgetingMonths,
                loadingInfo);

            // Assert
            mockLoader.Verify(x => x.MergeBespokeDataWithPendingFile(
                mockInputOutput.Object,
                mockSpreadsheet.Object,
                mockPendingFile.Object,
                budgetingMonths,
                loadingInfo), Times.Exactly(1));
        }

        [Test]
        public void LoadFilesAndMergeData_WillNotLoadData_WhenTesting()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var reconciliate = new ReconciliationIntro(mockInputOutput.Object);
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            var mockPendingFile = new Mock<ICSVFile<BankRecord>>();
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankOutFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord>());
            mockBankOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>());
            var budgetingMonths = new BudgetingMonths();
            var loadingInfo = new DummyLoader().LoadingInfo();
            loadingInfo.FilePaths.MainPath = "This is not a path";
            bool exceptionThrown = false;

            // Act
            try
            {
                reconciliate.LoadFilesAndMergeData<ActualBankRecord, BankRecord>(
                    mockSpreadsheet.Object,
                    mockPendingFileIO.Object,
                    mockPendingFile.Object,
                    mockActualBankFileIO.Object,
                    mockBankOutFileIO.Object,
                    budgetingMonths,
                    loadingInfo);
            }
            catch (DirectoryNotFoundException)
            {
                exceptionThrown = true;

                // Clean up
                loadingInfo.FilePaths.MainPath = ReconConsts.DefaultFilePath;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void WhenLoadingPendingData_WillConvertSourceLineSeparatorsAfterLoading()
        {
            // Arrange
            var mockInputOutput = new Mock<IInputOutput>();
            var reconciliate = new ReconciliationIntro(mockInputOutput.Object);
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            var pendingFile = new CSVFile<BankRecord>(mockPendingFileIO.Object);
            mockPendingFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord>{new BankRecord()});
            var loadingInfo = new BankAndBankOutData().LoadingInfo();
            bool exceptionThrown = false;

            // Act
            try
            {
                reconciliate.LoadPendingData<ActualBankRecord, BankRecord>(
                    mockPendingFileIO.Object,
                    pendingFile,
                    loadingInfo);
            }
            catch (NullReferenceException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }
    }
}
