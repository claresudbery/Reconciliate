using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    [TestFixture]
    internal partial class ReconciliationInterfaceTests : IInputOutput
    {
        private const string SourceDescription = "SOURCE";
        private const string MatchDescription = "MATCH";
        private const double AmountForMatching = 22.23;
        private Mock<IInputOutput> _mockInputOutput;
        private Mock<IFileIO<ActualBankRecord>> _mockActualBankFileIO;
        private Mock<IFileIO<BankRecord>> _mockBankFileIO;
        private int _numTimesCalled;

        [SetUp]
        public void SetUp()
        {
            _mockInputOutput = new Mock<IInputOutput>();
            _mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            _mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            ClearSelfShuntVariables();
        }

        [Test]
        public void M_CanShowMatchesWithoutImpactingOnThoseMatches()
        {
            // Arrange
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = AmountForMatching, Description = "Source"} 
                });
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}
                });
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var previousMatches = reconciliator.CurrentPotentialMatches();
            var numPreviousMatches = previousMatches.Count;
            var previousFirstMatchDescription = previousMatches[0].ActualRecords[0].Description;
            var previousLastMatchDescription = previousMatches[numPreviousMatches - 1].ActualRecords[0].Description;

            // Act
            reconciliationInterface.ShowCurrentRecordAndSemiAutoMatches();

            // Assert
            var currentMatches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(numPreviousMatches, currentMatches.Count);
            Assert.AreEqual(previousFirstMatchDescription, currentMatches[0].ActualRecords[0].Description);
            Assert.AreEqual(previousLastMatchDescription, currentMatches[numPreviousMatches - 1].ActualRecords[0].Description);
        }

        [Test]
        public void M_WhenMatchingIsDone_AllRecordsAreProcessed()
        {
            // Arrange
            SetupForAllMatchesChosenWithIndexZero();
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = SourceDescription + "01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = SourceDescription + "02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = SourceDescription + "03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = SourceDescription + "04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = SourceDescription + "05"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = MatchDescription + "01a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = MatchDescription + "01b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = MatchDescription + "02a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = MatchDescription + "02b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = MatchDescription + "03a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = MatchDescription + "03b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = MatchDescription + "04a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = MatchDescription + "04b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = MatchDescription + "05a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = MatchDescription + "05b"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            for (int actualBankCount = 0; actualBankCount < actualBankLines.Count; actualBankCount++)
            {
                Assert.IsTrue(actualBankLines[actualBankCount].Matched, $"actualBank record {actualBankCount} should be matched");
                VerifyIsOutputAsConsoleLine(actualBankLines[actualBankCount].Description, numTimes:1);

                var firstOfEachBankPair = actualBankCount * 2;
                Assert.IsTrue(bankLines[firstOfEachBankPair].Matched, $"bank record {firstOfEachBankPair} should be matched");
                VerifyIsOutputAsConsoleSnippet(bankLines[firstOfEachBankPair].Description, numTimes: 1);

                // Every other line in bankLines won't get matched.
                var secondOfEachBankPair = (actualBankCount * 2) + 1;
                Assert.IsFalse(bankLines[secondOfEachBankPair].Matched, $"bank record {secondOfEachBankPair} should NOT be matched");
                VerifyIsOutputAmongstNonPrioritisedMatches(bankLines[secondOfEachBankPair].Description, numTimes: 1);
            }
        }

        [Test]
        public void M_WhenMatchingIsDoneForNonMatchingRecords_AllRecordsAreProcessed()
        {
            // Arrange
            SetupForAllMatchesChosenWithIndexZero();
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = SourceDescription + "01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = SourceDescription + "02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = SourceDescription + "03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = SourceDescription + "04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = SourceDescription + "05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "06"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "07"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "08"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "09"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = MatchDescription + "01a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = MatchDescription + "01b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = MatchDescription + "02a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = MatchDescription + "02b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = MatchDescription + "03a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = MatchDescription + "03b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = MatchDescription + "04a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = MatchDescription + "04b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = MatchDescription + "05a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = MatchDescription + "05b"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();
            reconciliator.Rewind();
            reconciliationInterface.DoMatchingForNonMatchingRecords();

            // Assert
            // The first 5 ActualBank lines should get matched on the first time through, the next 4 lines get matched on the second pass.
            for (int actualBankCount = 0; actualBankCount < actualBankLines.Count; actualBankCount++)
            {
                Assert.IsTrue(actualBankLines[actualBankCount].Matched, $"actualBank record {actualBankCount} should be matched");
                Assert.IsTrue(bankLines[actualBankCount].Matched, $"bank record {actualBankCount} should be matched");
            }
            // Every other bank line is not matched on the first time round. 
            // On the second time round, four of them are matched one by one, 
            // so each one gets output to console one more time than the one before.
            for (int bankCount = 0; bankCount < 4; bankCount++)
            {
                VerifyIsOutputAmongstAllMatches($"{MatchDescription}0{bankCount + 1}b", numTimes: bankCount + 1);
            }
            // The very last bank line is never matched.
            var lastBankLine = bankLines.Count - 1;
            Assert.IsFalse(bankLines[lastBankLine].Matched, $"bank record {lastBankLine} should NOT be matched");
            VerifyIsOutputAmongstAllMatches(MatchDescription + "05b", numTimes: 4);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_ShouldTryToMatchNextRecord(bool autoMatching)
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source02"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}
                });
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupToDeleteThirdPartyRecord(actualBankRecords[0].Description);
            SetupToChooseMatch(actualBankRecords[1].Description, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            var previousNumThirdPartyRecords = reconciliator.NumThirdPartyRecords();

            // Act
            if (autoMatching)
            {
                reconciliationInterface.DoTheMatching();
            }
            else
            {
                reconciliationInterface.DoMatchingForNonMatchingRecords();
            }

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords - 1, reconciliator.NumThirdPartyRecords());
            Assert.AreEqual(actualBankRecords[1].Description, reconciliator.CurrentSourceDescription());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_IfThereAreNoRecordsLeft_ShouldMoveOn(bool autoMatching)
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"}
                });
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupToDeleteThirdPartyRecord(actualBankRecords[0].Description);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            if (autoMatching)
            {
                reconciliationInterface.DoTheMatching();
            }
            else
            {
                reconciliationInterface.DoMatchingForNonMatchingRecords();
            }

            // Assert
            _mockInputOutput.Verify(x => x.OutputLine("Writing new data..."), Times.Once);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterAutoMatching_ShouldUpdateListOfMatches()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching - 1, Description = "Matchxx"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 1;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var descriptionOfDeletedRecord = bankRecords[2].Description;
            PrepareToVerifyRecordIsOutputAmongstNonPrioritisedMatches(descriptionOfDeletedRecord);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            // The records that aren't deleted should get output twice.
            VerifyIsOutputAsConsoleSnippet(bankRecords[1].Description, numTimes: 2);
            VerifyIsOutputAmongstNonPrioritisedMatches(bankRecords[3].Description, numTimes: 2);
            // The record that's deleted should only get output once.
            _mockInputOutput.Verify();
            Assert.AreEqual(1, _numTimesCalled);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterManualMatching_ShouldUpdateListOfMatches()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Matchxx"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 2;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var descriptionOfDeletedRecord = bankRecords[2].Description;
            var descriptionOfMatchedRecord = bankRecords[0].Description;
            PrepareToVerifyRecordIsOutputAmongstAllMatches(descriptionOfDeletedRecord);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoMatchingForNonMatchingRecords();

            // Assert
            // The records that aren't deleted should get output twice.
            VerifyIsOutputAmongstAllMatches(descriptionOfMatchedRecord, numTimes: 2);
            VerifyIsOutputAmongstAllMatches(bankRecords[1].Description, numTimes: 2);
            VerifyIsOutputAmongstAllMatches(bankRecords[3].Description, numTimes: 2);
            // The record that's deleted should only get output once.
            _mockInputOutput.Verify();
            Assert.AreEqual(1, _numTimesCalled);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenOwnedRecordDeletionResultsInAnError_ThenUserNotified_AndAskedForNewInput(bool autoMatching)
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexTooHigh = bankRecords.Count + 1;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexTooHigh, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            bool exceptionThrown = false;
            try
            {
                if (autoMatching)
                {
                    reconciliationInterface.DoTheMatching();
                }
                else
                {
                    reconciliationInterface.DoMatchingForNonMatchingRecords();
                }
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
            _mockInputOutput.Verify(x =>
                    x.OutputLine(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteOwnedRecordDoesNotExist),
                Times.Once);
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description),
                Times.Exactly(2));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenOwnedRecordIsDeleted_IfMatchesStillExist_ShouldNotMoveOnToNextThirdPartyRecord(bool autoMatching)
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 1;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            if (autoMatching)
            {
                reconciliationInterface.DoTheMatching();
            }
            else
            {
                reconciliationInterface.DoMatchingForNonMatchingRecords();
            }

            // Assert
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description),
                Times.Exactly(2));
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterAutoMatching_IfNoMatchesAreLeft_ShouldShowMessage_AndMoveOnToNextThirdPartyRecord()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source02"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match02"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToChooseMatch(actualBankRecords[1].Description, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description),
                Times.Once);
            _mockInputOutput.Verify(x =>
                    x.OutputLine(ReconConsts.NoMatchesLeft),
                Times.Once);
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[1].Description),
                Times.Once);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterManualMatching_IfNoMatchesAreLeft_ShouldShowMessage_AndMoveOnToNextThirdPartyRecord()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source02"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoMatchingForNonMatchingRecords();

            // Assert
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description),
                Times.Once);
            _mockInputOutput.Verify(x =>
                    x.OutputLine(ReconConsts.NoMatchesLeft),
                Times.Once);
            _mockInputOutput.Verify(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[1].Description),
                Times.Never);
        }

        [Test]
        public void M_DespiteAMixtureOfDeletionsAfterAutoMatching_AllRecordsShouldBeOutputToUser()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = "Source06"}};
            List<string> actualBankRecordDescriptions = actualBankRecords.Select(x => x.Description).ToList();
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01c"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = "Match03"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = "Match04"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = "Match05"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 60, Description = "Match06a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 60, Description = "Match06b"}};
            List<string> bankRecordDescriptions = bankRecords.Select(x => x.Description).ToList();
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToDeleteOwnedRecordTwiceOnly(actualBankRecords[1].Description, indexOfDeletedRecord, 0);
            SetupToDeleteThirdPartyRecord(actualBankRecords[2].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch, 
                    actualBankRecords[3].Description))
                .Returns("0");
            SetupToDeleteThirdPartyRecord(actualBankRecords[4].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[5].Description))
                .Returns("0");
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[6].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            foreach (var actualBankRecord in actualBankRecords)
            {
                VerifyIsOutputAsConsoleLine(actualBankRecordDescriptions[actualBankRecords.IndexOf(actualBankRecord)], -1);
            }
            foreach (var description in bankRecordDescriptions)
            {
                VerifyIsOutputAsConsoleSnippet(description, 1);
            }
        }

        [Test]
        public void M_DespiteAMixtureOfDeletionsAfterManualMatching_AllRecordsShouldBeOutputToUser()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 2, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 3, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 4, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 5, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 6, Description = "Source06"}};
            List<string> actualBankRecordDescriptions = actualBankRecords.Select(x => x.Description).ToList();
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match04"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match05"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match06"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match07"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match08"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match09"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match10"}};
            List<string> bankRecordDescriptions = bankRecords.Select(x => x.Description).ToList();
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToDeleteOwnedRecordTwiceOnly(actualBankRecords[1].Description, indexOfDeletedRecord, 0);
            SetupToDeleteThirdPartyRecord(actualBankRecords[2].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[3].Description))
                .Returns("0");
            SetupToDeleteThirdPartyRecord(actualBankRecords[4].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[5].Description))
                .Returns("0");
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[6].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoMatchingForNonMatchingRecords();

            // Assert
            foreach (var actualBankRecord in actualBankRecords)
            {
                _mockInputOutput.Verify(x => 
                        x.OutputLine(It.Is<string>(y => 
                            y.Contains(actualBankRecordDescriptions[actualBankRecords.IndexOf(actualBankRecord)]))), 
                    Times.AtLeastOnce);
            }
            foreach (var description in bankRecordDescriptions)
            {
                Assert.IsTrue(_outputAllLinesRecordedDescriptions.Any(x => x.Contains(description)), 
                    $"OutputAllLines params should include {description}");
            }
        }

        [Test]
        public void M_AfterAMixtureOfDeletionsDuringAutoMatching_TheNumberOfRecordsFinallyReconciledShouldBeCorrect()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = "Source06"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01b"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = "Match01c"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 20, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 30, Description = "Match03"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 40, Description = "Match04"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 50, Description = "Match05"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 60, Description = "Match06a"},
                new BankRecord {UnreconciledAmount = AmountForMatching + 60, Description = "Match06b"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToDeleteOwnedRecordTwiceOnly(actualBankRecords[1].Description, indexOfDeletedRecord, 0);
            SetupToDeleteThirdPartyRecord(actualBankRecords[2].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[3].Description))
                .Returns("0");
            SetupToDeleteThirdPartyRecord(actualBankRecords[4].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[5].Description))
                .Returns("0");
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[6].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            // 4 records were deleted but that left the first source record unmatched, which would then have been added to owned records.
            Assert.AreEqual(bankRecords.Count - 3, reconciliator.NumOwnedRecords(), "NumOwnedRecords");
            Assert.AreEqual(actualBankRecords.Count - 2, reconciliator.NumThirdPartyRecords(), "NumThirdPartyRecords");
        }

        [Test]
        public void M_AfterAMixtureOfDeletionsDuringManualMatching_TheNumberOfRecordsFinallyReconciledShouldBeCorrect()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 2, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 3, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 4, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 5, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 6, Description = "Source06"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match01"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match02"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match03"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match04"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match05"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match06"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match07"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match08"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match09"},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match10"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var indexOfDeletedRecord = 0;
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[0].Description, indexOfDeletedRecord, 0);
            SetupToDeleteOwnedRecordTwiceOnly(actualBankRecords[1].Description, indexOfDeletedRecord, 0);
            SetupToDeleteThirdPartyRecord(actualBankRecords[2].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[3].Description))
                .Returns("0");
            SetupToDeleteThirdPartyRecord(actualBankRecords[4].Description);
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.EnterNumberOfMatch,
                    actualBankRecords[5].Description))
                .Returns("0");
            SetupToDeleteOwnedRecordOnceOnly(actualBankRecords[6].Description, indexOfDeletedRecord, 0);
            SetupToExitAtTheEnd();
            _outputAllLinesRecordedDescriptions.Clear();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoMatchingForNonMatchingRecords();

            // Assert
            Assert.AreEqual(bankRecords.Count - 4, reconciliator.NumOwnedRecords(), "NumOwnedRecords");
            Assert.AreEqual(actualBankRecords.Count - 2, reconciliator.NumThirdPartyRecords(), "NumThirdPartyRecords");
        }

        [Test]
        public void M_WhenSemiAutoMatchHasDifferentAmount_WillAddExplanatoryTextAndChangeAmount()
        {
            // Arrange
            var commonDescription = "common description";
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = commonDescription}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching + 1, Description = commonDescription}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupForAllMatchesChosenWithIndexZero();
            SetupToExitAtTheEnd();
            var mockMatcher = new Mock<IMatcher>();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            Assert.IsTrue(bankRecords[0].Description.Contains(ReconConsts.OriginalAmountWas));
            Assert.AreEqual(actualBankRecords[0].MainAmount(), bankRecords[0].ReconciledAmount);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForSemiAutoMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching + 1, Description = "Match00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupToExitAtTheEnd();
            var mockMatcher = new Mock<IMatcher>();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            bool exceptionThrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(EnterNumberOfMatch, 
                // which means it will have returned null.
                reconciliationInterface.DoTheMatching();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForAutoMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Source00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            _mockInputOutput.Setup(x => x.GetGenericInput(ReconConsts.GoAgainFinish)).Returns("2");
            var mockMatcher = new Mock<IMatcher>();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            bool exceptionThrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(ChooseWhatToDoWithMatches), 
                // which means it will have returned null.
                reconciliationInterface.DoTheMatching();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForFinalMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                .Returns("0");
            _mockInputOutput.Setup(x => x.GetGenericInput(ReconConsts.GoAgainFinish)).Returns("2");
            var mockMatcher = new Mock<IMatcher>();
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            bool exceptionThrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(ChooseWhatToDoWithMatches), 
                // which means it will have returned null.
                reconciliationInterface.DoTheMatching();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhetherToGoAgainOrFinish_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);
            bool exceptionThrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetGenericInput(GoAgainFinish), 
                // which means it will have returned null.
                reconciliationInterface.DoTheMatching();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhichRecordToDelete_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            _mockInputOutput.SetupSequence(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description))
                .Returns("D")
                .Returns("");
            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.WhetherToDeleteThirdParty, actualBankRecords[0].Description))
                .Returns("N");
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            // We didn't mock _mockInputOutput.GetInput(EnterDeletionIndex, 
            // which means it will have returned null.
            reconciliationInterface.DoTheMatching();

            // Assert
            _mockInputOutput.Verify(
                x => x.OutputLine("Object reference not set to an instance of an object."),
                Times.Never);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhetherToDeleteThirdPartyRecord_NoErrorShouldOccur()
        {
            // Arrange
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = "Match00"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            _mockInputOutput.SetupSequence(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, actualBankRecords[0].Description))
                .Returns("D")
                .Returns("");
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(_mockInputOutput.Object, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            // We didn't mock _mockInputOutput.GetInput(WhetherToDeleteThirdParty, 
            // which means it will have returned null.
            reconciliationInterface.DoTheMatching();

            // Assert
            _mockInputOutput.Verify(
                x => x.OutputLine("Object reference not set to an instance of an object."),
                Times.Never);
        }

        [Test]
        public void M_WhenRecursivelyHandlingAutoMatches_IfMatchesAreRemoved_CorrectResultsAreShown()
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "something else";
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = text1},
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = text2}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupToRemoveAutoMatch();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            var text1Lines = _outputAllLinesRecordedConsoleLines.Where(x => x.DescriptionString == text1);
            var text2Lines = _outputAllLinesRecordedConsoleLines.Where(x => x.DescriptionString == text2);
            Assert.AreEqual(2, text1Lines.Count(), "text1 source and match are shown for first auto matching .");
            Assert.AreEqual(6, text2Lines.Count(), "text2 source and match are shown for both auto matchings and also final match.");
        }

        [Test]
        public void M_WhenRecursivelyHandlingFinalMatches_IfMatchesAreRemoved_CorrectResultsAreShown()
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "something else";
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = text1},
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = text2}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupForAllMatchesChosenWithIndexZero();
            SetupToRemoveFinalMatch();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            var text1Lines = _outputAllLinesRecordedConsoleLines.Where(x => x.DescriptionString.Contains(text1));
            var text2Lines = _outputAllLinesRecordedConsoleLines.Where(x => x.DescriptionString.Contains(text2));
            Assert.AreEqual(2, text1Lines.Count(), "text1 source and match are shown for first final match showing.");
            Assert.AreEqual(4, text2Lines.Count(), "text2 source and match are shown for both final match showings.");
        }

        [TestCase(TransactionMatchType.Auto, "10")]
        [TestCase(TransactionMatchType.Auto, "0,3,6")]
        [TestCase(TransactionMatchType.Final, "1")]
        [TestCase(TransactionMatchType.Final, "25,26")]
        public void M_WhenRemovingMatches_IfBadIndexEntered_ThenErrorShownAndUserAskedAgain(
            TransactionMatchType TransactionMatchType, 
            string matchIndices)
        {
            // Arrange
            var text1 = "Source00";
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching, Description = text1}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            SetupForAllMatchesChosenWithIndexZero();
            if (TransactionMatchType == TransactionMatchType.Auto)
            {
                SetupToRemoveAutoMatch(matchIndices);
            }
            else
            {
                SetupToRemoveFinalMatch(matchIndices);
            }
            var mockMatcher = new Mock<IMatcher>();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            Assert.IsTrue(_outputSingleLineRecordedMessages.Contains(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber),
                "Bad match error message should be shown");
            var text1Lines = _outputAllLinesRecordedConsoleLines.Where(x => x.DescriptionString == text1);
            Assert.AreEqual(4, text1Lines.Count(), "source and match are shown for auto match and final match.");
        }

        [TestCase(TransactionMatchType.SemiAuto)]
        [TestCase(TransactionMatchType.Manual)]
        public void M_WhenChoosingMatch_IfBadIndexEntered_ThenErrorShownAndUserAskedAgain(TransactionMatchType TransactionMatchType)
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "Pickle pop";
            var actualBankRecords = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actualBankRecords);
            var bankRecords = new List<BankRecord>{
                new BankRecord {UnreconciledAmount = AmountForMatching + 10, Description = text1},
                new BankRecord {UnreconciledAmount = AmountForMatching + 100, Description = "something else"}};
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            ClearSelfShuntVariables();
            if (TransactionMatchType == TransactionMatchType.SemiAuto)
            {
                _mockInputOutput.SetupSequence(x =>
                        x.GetInput(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                    .Returns("10") // bad index (semi-auto)
                    .Returns("0") // second attempt (semi-auto) - match the only option
                    .Returns("0"); // first attempt (manual) - match the only option
            }
            else
            {
                _mockInputOutput.SetupSequence(x =>
                        x.GetInput(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                    .Returns("0") // first attempt (semi-auto) - match the only option
                    .Returns("1") // bad index (manual)
                    .Returns("0"); // second attempt (manual) - match the only option
            }
            SetupToMoveOnToManualMatchingThenExit();
            var mockMatcher = new Mock<IMatcher>();
            // Use self-shunt for this one.
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            Assert.IsTrue(_outputSingleLineRecordedMessages.Contains(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber),
                "Bad match error message should be shown");
            var text1Lines = _outputSingleLineRecordedConsoleLines.Where(x => x.DescriptionString == text1);
            Assert.AreEqual(1, text1Lines.Count(), "semiauto-matched source should only be output once.");
            var text2Lines = _outputSingleLineRecordedMessages.Where(x => x.Contains(text2));
            Assert.AreEqual(1, text2Lines.Count(), "manually-matched source should only be output once.");
        }

        [Test]
        public void M_WhenMatchingTheFirstActionIsMatcherPreliminaryStuff()
        {
            // Arrange
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.DoTheMatching();

            // Assert
            mockMatcher.Verify(x => x.DoPreliminaryStuff(reconciliator, reconciliationInterface), Times.Exactly(1));
        }

        [Test]
        public void M_WhenFinishingShouldCallMatcherFinishingFunctionality()
        {
            // Arrange
            _mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            _mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var mockMatcher = new Mock<IMatcher>();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", _mockActualBankFileIO.Object, _mockBankFileIO.Object);
            var reconciliationInterface = new ReconciliationInterface<ActualBankRecord, BankRecord>(this, reconciliator, "ActualBank", "Bank In", mockMatcher.Object);

            // Act
            reconciliationInterface.Finish();

            // Assert
            mockMatcher.Verify(x => x.Finish(), Times.Exactly(1));
        }
    }
}
