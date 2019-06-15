using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    // NB: Tests prefixed R_ are those that call file.Reload()
    // This was done when I was trying to work out what makes some tests run slower than others
    [TestFixture]
    public class ReconciliatorMatchingTests
    {
        private static CSVFile<BankRecord> _bankInFile;
        private static CSVFile<BankRecord> _bankOutFile;
        private static CSVFile<CredCard1Record> _credCard1File;
        private static CSVFile<CredCard1InOutRecord> _credCard1InOutFile;
        private static CSVFile<CredCard2Record> _credCard2File;
        private static CSVFile<CredCard2InOutRecord> _credCard2InOutFile;
        private static CSVFile<ActualBankRecord> _actualBankFile;
        private static string _tempBankInFileName = "temp-bank-in-file";
        private static string _tempActualBankFileName = "temp-actualBank-file";
        private static string _absoluteCSVFilePath;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            TestHelper.SetCorrectDateFormatting();

            var currentPath = TestContext.CurrentContext.TestDirectory;
            _absoluteCSVFilePath = TestHelper.FullyQualifiedCSVFilePath(currentPath);

            var fileIOActualBank = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-sample");
            _actualBankFile = new CSVFile<ActualBankRecord>(fileIOActualBank);
            _actualBankFile.Load();

            var fileIOBankIn = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "BankIn-formatted-date-only");
            _bankInFile = new CSVFile<BankRecord>(fileIOBankIn);
            _bankInFile.Load();

            var fileIOBankOut = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "BankOut-formatted-date-only");
            _bankOutFile = new CSVFile<BankRecord>(fileIOBankOut);
            _bankOutFile.Load();


            var fileIOCredCard1 = new FileIO<CredCard1Record>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard1-Statement");
            _credCard1File = new CSVFile<CredCard1Record>(fileIOCredCard1);
            _credCard1File.Load();

            var fileIOCredCard1InOut = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard1InOut-formatted-date-only");
            _credCard1InOutFile = new CSVFile<CredCard1InOutRecord>(fileIOCredCard1InOut);
            _credCard1InOutFile.Load();


            var fileIOCredCard2 = new FileIO<CredCard2Record>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard2");
            _credCard2File = new CSVFile<CredCard2Record>(fileIOCredCard2);
            _credCard2File.Load();

            var fileIOCredCard2InOut = new FileIO<CredCard2InOutRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard2InOut");
            _credCard2InOutFile = new CSVFile<CredCard2InOutRecord>(fileIOCredCard2InOut);
            _credCard2InOutFile.Load();
        }

        private CSVFile<TRecordType> CreateCsvFile<TRecordType>(
            string fileName,
            string[] textLines) where TRecordType : ICSVRecord, new()
        {
            var fullFilePath = _absoluteCSVFilePath + "/" + fileName + ".csv";
            File.WriteAllLines(fullFilePath, textLines);
            var fileIO = new FileIO<TRecordType>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, fileName);
            var csvFile = new CSVFile<TRecordType>(fileIO);
            csvFile.Load();
            return csvFile;
        }

        private void AssertMatchIsNoLongerMatched(IPotentialMatch originalMatch)
        {
            foreach (var actualRecord in originalMatch.ActualRecords)
            {
                Assert.IsFalse(actualRecord.Matched);
                Assert.IsNull(actualRecord.Match);
            }
        }

        private void AssertMatchIsMatched(IPotentialMatch originalMatch)
        {
            foreach (var actualRecord in originalMatch.ActualRecords)
            {
                Assert.IsTrue(actualRecord.Matched);
            }
        }

        private void AssertSourceRecordIsNotMatched(AutoMatchedRecord<ActualBankRecord> autoMatchedItem)
        {
            Assert.IsFalse(autoMatchedItem.SourceRecord.Matched);
            Assert.IsNull(autoMatchedItem.Match);
            Assert.IsNull(autoMatchedItem.SourceRecord.Match);
        }

        private void AssertRecordIsNoLongerMatched(ICSVRecord originalMatch)
        {
            Assert.IsFalse(originalMatch.Matched);
            Assert.IsNull(originalMatch.Match);
        }

        [Test]
        public void Constructor_WillFilterForPositiveRecords_WhenBankAndBankIn()
        {
            // Arrange
            var mockActualBankFile = new Mock<ICSVFile<ActualBankRecord>>();
            var mockBankFile = new Mock<ICSVFile<BankRecord>>();

            // Act
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(
                mockActualBankFile.Object,
                mockBankFile.Object,
                BankAndBankInData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mockActualBankFile.Verify(x => x.FilterForPositiveRecordsOnly());
        }

        [Test]
        public void Constructor_WillFilterForNegativeRecords_WhenBankAndBankOut()
        {
            // Arrange
            var mockActualBankFile = new Mock<ICSVFile<ActualBankRecord>>();
            var mockBankFile = new Mock<ICSVFile<BankRecord>>();

            // Act
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(
                mockActualBankFile.Object,
                mockBankFile.Object,
                BankAndBankOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mockActualBankFile.Verify(x => x.FilterForNegativeRecordsOnly());
        }

        [Test]
        public void Constructor_WillPerformNoAction_WhenCredCard1AndCredCard1InOut()
        {
            // Arrange
            var mockCredCard1File = new Mock<ICSVFile<CredCard1Record>>();
            var mockCredCard1InOutFile = new Mock<ICSVFile<CredCard1InOutRecord>>();

            // Act
            var reconciliator = new Reconciliator<CredCard1Record, CredCard1InOutRecord>(
                mockCredCard1File.Object,
                mockCredCard1InOutFile.Object,
                CredCard1AndCredCard1InOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mockCredCard1File.Verify(x => x.FilterForPositiveRecordsOnly(), Times.Never);
            mockCredCard1File.Verify(x => x.FilterForNegativeRecordsOnly(), Times.Never);
            mockCredCard1File.Verify(x => x.SwapSignsOfAllAmounts(), Times.Never);
        }

        [Test]
        public void Constructor_WillPerformNoAction_WhenCredCard2AndCredCard2InOut()
        {
            // Arrange
            var mockCredCard2File = new Mock<ICSVFile<CredCard2Record>>();
            var mockCredCard2InOutFile = new Mock<ICSVFile<CredCard2InOutRecord>>();

            // Act
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(
                mockCredCard2File.Object,
                mockCredCard2InOutFile.Object,
                CredCard2AndCredCard2InOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mockCredCard2File.Verify(x => x.FilterForPositiveRecordsOnly(), Times.Never);
            mockCredCard2File.Verify(x => x.FilterForNegativeRecordsOnly(), Times.Never);
            mockCredCard2File.Verify(x => x.SwapSignsOfAllAmounts(), Times.Never);
        }

        [Test]
        public void R_CanFindSingleMatchForActualBankAndBankIn()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(1, matchedRecord.Matches.Count);
        }

        [Test]
        public void R_CanFindSingleMatchForActualBankBankOut()
        {
            // Arrange
            _bankOutFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForNegativeRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankOutFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(1, matchedRecord.Matches.Count);
        }

        [Test]
        public void R_CanFindMatchesForCredCard1InOut()
        {
            // Arrange
            _credCard1InOutFile.Reload();
            _credCard1File.Reload();
            _credCard1File.FilterForNegativeRecordsOnly();
            var reconciliator = new Reconciliator<CredCard1Record, CredCard1InOutRecord>(_credCard1File, _credCard1InOutFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<CredCard1Record> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(5, matchedRecord.Matches.Count);
        }

        [Test]
        public void R_CanFindMatchesForCredCard2InOut()
        {
            // Arrange
            _credCard2InOutFile.Reload();
            _credCard2File.Reload();
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(_credCard2File, _credCard2InOutFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<CredCard2Record> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Assert
            Assert.AreEqual(9, matchedRecord.Matches.Count);
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetThenRelevantRecordIsMarkedAsMatched()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            AssertMatchIsMatched(matchedRecord.Matches[0]);
            Assert.AreEqual(true, _bankInFile.Records[0].Matched);
            Assert.AreEqual(true, matchedRecord.SourceRecord.Matched);
            Assert.AreEqual(true, _actualBankFile.Records[0].Matched);
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetForMatchWithDifferentAmount_WillAddExplanatoryTextAndChangeAmount()
        {
            // Arrange
            var amount1 = 22.23;
            var matchingText = "DIVIDE YOUR GREEN";
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount1, Description = matchingText}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount1 + 1, Description = matchingText}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.IsTrue(matchedRecord.SourceRecord.Match.Description.Contains(ReconConsts.OriginalAmountWas));
            Assert.AreEqual(matchedRecord.SourceRecord.MainAmount(), matchedRecord.SourceRecord.Match.MainAmount());
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetThenRelevantRecordsHaveMatchedRecordsAttached()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            foreach (var actualRecord in matchedRecord.Matches[0].ActualRecords)
            {
                Assert.AreEqual(_actualBankFile.Records[0], actualRecord.Match);
            }
            Assert.AreEqual(_bankInFile.Records[0], matchedRecord.SourceRecord.Match);
        }

        [Test]
        public void R_CanFindMultipleMatchesForActualBankAndBankIn()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Assert
            Assert.AreEqual(3, matchedRecord.Matches.Count);
        }

        [Test]
        public void R_WhenNonZeroLatestMatchIndexIsSetThenRelevantRecordIsMarkedAsMatched()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(1);

            // Assert
            // (Remember the matches will have been ordered in order of which ones are closest in date to the original)
            AssertMatchIsMatched(matchedRecord.Matches[1]);
            Assert.AreEqual(true, _bankInFile.Records[1].Matched);
            Assert.AreEqual(true, matchedRecord.SourceRecord.Matched);
            Assert.AreEqual(true, _actualBankFile.Records[2].Matched);
            Assert.AreEqual(false, _bankInFile.Records[0].Matched);
            Assert.AreEqual(false, _bankInFile.Records[2].Matched);
            Assert.AreEqual(false, _bankInFile.Records[3].Matched);
            Assert.AreEqual(false, _bankInFile.Records[4].Matched);
        }

        [Test]
        public void R_WhenNonZeroLatestMatchIndexIsSetThenRelevantRecordHasMatchedRecordAttached()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(2);

            // Assert
            // (Remember the matches will have been ordered in order of which ones are closest in date to the original)
            foreach (var actualRecord in matchedRecord.Matches[2].ActualRecords)
            {
                Assert.AreEqual(matchedRecord.SourceRecord, actualRecord.Match);
                Assert.AreEqual(_actualBankFile.Records[2], actualRecord.Match);
                Assert.AreEqual("'ZZZThing", actualRecord.Match.Description);
            }
            Assert.AreEqual("ZZZThing3", matchedRecord.Matches[2].ActualRecords[0].Description);
            Assert.AreEqual(_bankInFile.Records[4], matchedRecord.SourceRecord.Match);
        }

        [Test]
        public void R_WhenOneMatchHasBeenMarkedThenNumMatchedRecordsIsOne()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(2);

            // Assert
            Assert.AreEqual(1, _actualBankFile.NumMatchedRecords());
            Assert.AreEqual(1, _bankInFile.NumMatchedRecords());
            Assert.AreEqual(_actualBankFile.Records.Count - 1, _actualBankFile.NumUnmatchedRecords());
            Assert.AreEqual(_bankInFile.Records.Count - 1, _bankInFile.NumUnmatchedRecords());
        }

        [Test]
        public void M_WhenMatchIndexDoesNotExistErrorIsThrown()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            bool exceptionThrown = false;
            string errorMessage = "";

            // Act
            try
            {
                reconciliator.MatchCurrentRecord(10);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, errorMessage);
        }

        [Test]
        public void R_WhenThreeMatchesHaveBeenMarkedThenNumMatchedRecordsIsThree()
        {
            // Arrange
            _bankOutFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForNegativeRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankOutFile, ThirdPartyFileLoadAction.NoAction);
            
            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);

            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);

            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            
            // Assert
            Assert.AreEqual(3, _actualBankFile.NumMatchedRecords());
            Assert.AreEqual(3, _bankOutFile.NumMatchedRecords());
            Assert.AreEqual(_actualBankFile.Records.Count - 3, _actualBankFile.NumUnmatchedRecords());
            Assert.AreEqual(_bankOutFile.Records.Count - 3, _bankOutFile.NumUnmatchedRecords());
        }

        [Test]
        public void R_WhenThirdPartyRecordHasAlreadyBeenMatchedThenItIsNoLongerACandidateForMatching()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual("'ZZZSpecialDescription001", matchedRecord.SourceRecord.Description);

            // Act
            reconciliator.MatchCurrentRecord(0);
            reconciliator.Rewind();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            matchedRecord = reconciliator.CurrentRecordForMatching();

            // Assert
            // Because the ZZZSpecialDescription001 record was matched on the previous pass, it will now be skipped over in favour of the next record.
            Assert.AreNotEqual("'ZZZSpecialDescription001", matchedRecord.SourceRecord.Description);
        }

        [Test]
        public void M_WhenOwnedRecordHasAlreadyBeenMatchedThenItIsNoLongerACandidateForMatching()
        {
            // Arrange
            var amountForMatching = 22.23;
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Source01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Source01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Source01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Source01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01a"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01b"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01c"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "Match02"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act & Assert
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(3, reconciliator.CurrentRecordForMatching().Matches.Count);

            reconciliator.MatchCurrentRecord(0);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(2, reconciliator.CurrentRecordForMatching().Matches.Count);

            reconciliator.MatchCurrentRecord(0);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count);

            reconciliator.MatchCurrentRecord(0);
            bool matchesFound = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            // There are no matches left that match the text and amount
            Assert.IsFalse(matchesFound);
        }

        [Test]
        public void R_WhenReconciliatingLeftoverRecordsOnlyUnmatchedThirdPartyRecordsAreFound()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);

            // Act
            reconciliator.Rewind();
            bool foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(false, reconciliator.CurrentRecordForMatching().SourceRecord.Matched);
        }

        [Test]
        public void M_WhenReconciliatingLeftoverRecords_MatchesShouldHaveConsoleLinesAttached()
        {
            // Arrange
            var amountForMatching = 22.23;
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Source"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matchedRecords = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(3, matchedRecords.Count);
            foreach (var match in matchedRecords)
            {
                Assert.IsTrue(match.ConsoleLines[0] != null);
            }
        }

        [Test]
        public void R_WhenReconciliatingLeftoverRecordsIfNoUnmatchedThirdPartyRecordsAreLeftThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actualBankFile.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            bool foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(false, foundOne);
        }

        [Test]
        public void R_WhenReconciliatingLeftoverRecordsForTheSecondTimeAnyThirdPartyRecordsMatchedOnThePreviousPassAreNoLongerReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actualBankFile.Records.Count - 1; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MatchCurrentRecord(0);
            reconciliator.Rewind();
            bool foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(false, foundOne);
        }

        [Test]
        public void M_WhenReconciliatingLeftoverRecordsForTheSecondTimeAnyOwnedRecordsMatchedOnThePreviousPassAreNoLongerReturned()
        {
            // Arrange
            var amountForMatching = 122.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amountForMatching, Description = "'DIVIDE YOUR GREEN"},
                new ActualBankRecord {Amount = amountForMatching, Description = "'DIVIDE YOUR GREEN"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord { UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                new BankRecord { UnreconciledAmount = amountForMatching + 2, Description = "Match02"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            // Replicate what happens when you've finished semi-automated matching and you go back to the beginning for manual matching.
            reconciliator.Rewind();

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.IsTrue(foundOne);
            Assert.AreEqual(false, reconciliator.CurrentRecordForMatching().Matches.Any(x => x.ActualRecords[0].Description == "Match01"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count);
        }

        [Test]
        public void R_ReconciliatorUsesAllUnmatchedOwnedRecordsAsPossibleMatchesWhenMovingToUnmatchedThirdPartyRecord()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches.Count);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var amountForMatching = 122.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = "'DIVIDE YOUR GREEN"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {Date = new DateTime(2017,1,6), UnreconciledAmount = amountForMatching + 100, Description = "4TH NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,5), UnreconciledAmount = amountForMatching + 100, Description = "2ND NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,6), UnreconciledAmount = amountForMatching + 100, Description = "1ST NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,8), UnreconciledAmount = amountForMatching + 100, Description = "3RD NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,4,6), UnreconciledAmount = amountForMatching + 100, Description = "5TH NEAREST BY DATE"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches.Count);
            Assert.AreEqual("1ST NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[0].ActualRecords[0].Description);
            Assert.AreEqual("2ND NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[1].ActualRecords[0].Description);
            Assert.AreEqual("3RD NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[2].ActualRecords[0].Description);
            Assert.AreEqual("4TH NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[3].ActualRecords[0].Description);
            Assert.AreEqual("5TH NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[4].ActualRecords[0].Description);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillBeOrderedByAmountsClosestToOriginal()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = "'DIVIDE YOUR GREEN"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching - 40, Description = "5TH NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "2ND NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "1ST NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching - 2, Description = "3RD NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching + 30, Description = "4TH NEAREST AMOUNT"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches.Count);
            Assert.AreEqual("1ST NEAREST AMOUNT", reconciliator.CurrentRecordForMatching().Matches[0].ActualRecords[0].Description);
            Assert.AreEqual("2ND NEAREST AMOUNT", reconciliator.CurrentRecordForMatching().Matches[1].ActualRecords[0].Description);
            Assert.AreEqual("3RD NEAREST AMOUNT", reconciliator.CurrentRecordForMatching().Matches[2].ActualRecords[0].Description);
            Assert.AreEqual("4TH NEAREST AMOUNT", reconciliator.CurrentRecordForMatching().Matches[3].ActualRecords[0].Description);
            Assert.AreEqual("5TH NEAREST AMOUNT", reconciliator.CurrentRecordForMatching().Matches[4].ActualRecords[0].Description);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillBeOrdered_ByDateAndAmount_DependingWhichOnesAreClosest()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,12,20), Amount = amountForMatching, Description = "'DIVIDE YOUR GREEN"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {Date = new DateTime(2017,11,20), UnreconciledAmount = amountForMatching + 5, Description =  "3RD AMOUNT, 5TH MATCH (5)" },
                new BankRecord {Date = new DateTime(2017,12,13), UnreconciledAmount = amountForMatching + 50, Description = "3RD DATE, 6TH MATCH (7)"   },
                new BankRecord {Date = new DateTime(2018,2,20), UnreconciledAmount = amountForMatching + 40, Description = "4TH AMOUNT, 8TH MATCH (40)"},
                new BankRecord {Date = new DateTime(2017,12,20), UnreconciledAmount = amountForMatching + 60, Description = "1ST DATE, 1ST MATCH (0)"   },
                new BankRecord {Date = new DateTime(2017,11,20), UnreconciledAmount = amountForMatching - 3, Description =  "2ND AMOUNT, 3RD MATCH (3)" },
                new BankRecord {Date = new DateTime(2017,12,24), UnreconciledAmount = amountForMatching + 60, Description =      "2ND DATE, 4TH MATCH (4)"   },
                new BankRecord {Date = new DateTime(2018,1,20), UnreconciledAmount = amountForMatching + 50, Description =  "4TH DATE, 7TH MATCH (31)"  },
                new BankRecord {Date = new DateTime(2017,2,20), UnreconciledAmount = amountForMatching + 1, Description =    "1ST AMOUNT, 2ND MATCH (1)" }};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(8, reconciliator.CurrentRecordForMatching().Matches.Count);
            Assert.AreEqual("1ST DATE, 1ST MATCH (0)", reconciliator.CurrentRecordForMatching().Matches[0].ActualRecords[0].Description);
            Assert.AreEqual("1ST AMOUNT, 2ND MATCH (1)", reconciliator.CurrentRecordForMatching().Matches[1].ActualRecords[0].Description);
            Assert.AreEqual("2ND AMOUNT, 3RD MATCH (3)", reconciliator.CurrentRecordForMatching().Matches[2].ActualRecords[0].Description);
            Assert.AreEqual("2ND DATE, 4TH MATCH (4)", reconciliator.CurrentRecordForMatching().Matches[3].ActualRecords[0].Description);
            Assert.AreEqual("3RD AMOUNT, 5TH MATCH (5)" , reconciliator.CurrentRecordForMatching().Matches[4].ActualRecords[0].Description);
            Assert.AreEqual("3RD DATE, 6TH MATCH (7)"   , reconciliator.CurrentRecordForMatching().Matches[5].ActualRecords[0].Description);
            Assert.AreEqual("4TH DATE, 7TH MATCH (31)"  , reconciliator.CurrentRecordForMatching().Matches[6].ActualRecords[0].Description);
            Assert.AreEqual("4TH AMOUNT, 8TH MATCH (40)", reconciliator.CurrentRecordForMatching().Matches[7].ActualRecords[0].Description);
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches[0].Rankings.Combined);
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches[1].Rankings.Combined);
            Assert.AreEqual(3, reconciliator.CurrentRecordForMatching().Matches[2].Rankings.Combined);
            Assert.AreEqual(4, reconciliator.CurrentRecordForMatching().Matches[3].Rankings.Combined);
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches[4].Rankings.Combined);
            Assert.AreEqual(7, reconciliator.CurrentRecordForMatching().Matches[5].Rankings.Combined);
            Assert.AreEqual(31, reconciliator.CurrentRecordForMatching().Matches[6].Rankings.Combined);
            Assert.AreEqual(40, reconciliator.CurrentRecordForMatching().Matches[7].Rankings.Combined);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillHaveAmountMatchValuesPopulated()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = "NEAREST AMOUNT"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "UNMATCHED AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "MATCHED AMOUNT"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == "MATCHED AMOUNT").AmountMatch);
            Assert.AreEqual(false, matches.First(x => x.ActualRecords[0].Description == "UNMATCHED AMOUNT").AmountMatch);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillHaveFullTextMatchValuesPopulated()
        {
            // Arrange
            var amountForMatching = 22.34;
            var textToMatch = "'DIVIDE YOUR GREEN";
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = textToMatch}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "SOME OTHER TEXT"},
                new BankRecord {UnreconciledAmount = amountForMatching, Description = textToMatch}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == textToMatch).FullTextMatch);
            Assert.AreEqual(false, matches.First(x => x.ActualRecords[0].Description == "SOME OTHER TEXT").FullTextMatch);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillHavePartialTextMatchValuesPopulated()
        {
            // Arrange
            var amountForMatching = 22.34;
            var textToMatch = "'DIVIDE YOUR GREEN";
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = textToMatch + "EXTRA"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "SOME OTHER TEXT"},
                new BankRecord {UnreconciledAmount = amountForMatching, Description = textToMatch + "SOME OTHER TEXT"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == textToMatch + "SOME OTHER TEXT").PartialTextMatch);
            Assert.AreEqual(false, matches.First(x => x.ActualRecords[0].Description == "SOME OTHER TEXT").PartialTextMatch);
        }

        [Test]
        public void R_CurrentMatchesWillHaveCorrectIndexNumbers()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            var currentMatches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(true, currentMatches[0].ConsoleLines[0].Index == 0);
            Assert.AreEqual(true, currentMatches[1].ConsoleLines[0].Index == 1);
            Assert.AreEqual(true, currentMatches[2].ConsoleLines[0].Index == 2);
            Assert.AreEqual(true, currentMatches[3].ConsoleLines[0].Index == 3);
            Assert.AreEqual(true, currentMatches[4].ConsoleLines[0].Index == 4);
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsIdentifiedAsMatchedTheCorrectRecordGetsMarkedAsMatched()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amountForMatching, Description = "WHATEVER"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            // Note that we can't use amounts to identify records, because amounts get changed by the matching process.
            Assert.AreEqual(true, bankFile.Records.First(x => x.Description.Contains("THE FIRST MATCH")).Matched);
            Assert.AreEqual(false, bankFile.Records.Any(x => x.Description.Contains("NOT MATCHED") && x.Matched));
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItGetsItsAmountChanged()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amountForMatching, Description = "WHATEVER"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.AreEqual(true, bankFile.Records.First(x => x.Description.StartsWith("THE FIRST MATCH")).Matched);
            Assert.AreEqual(amountForMatching, bankFile.Records.First(x => x.Description.StartsWith("THE FIRST MATCH")).MainAmount());
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItGetsACommentAddedToItsDescription()
        {
            // Arrange
            var matchDescription = "THE FIRST MATCH";
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amountForMatching, Description = "WHATEVER"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = matchDescription},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.AreEqual(true, bankFile.Records.First(x => x.Description.StartsWith(matchDescription)).Matched);
            Assert.AreEqual($"{matchDescription}{ReconConsts.OriginalAmountWas}£{amountForMatching + 1}", 
                bankFile.Records.First(x => x.Description.StartsWith(matchDescription)).Description);
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItIsNoLongerReturnedInListOfUnmatchedOwnedRecords()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amountForMatching, Description = "WHATEVER"},
                new ActualBankRecord {Amount = amountForMatching, Description = "WHATEVER"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amountForMatching + 100, Description = "NOT MATCHED"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MatchCurrentRecord(0);

            // Act
            var foundOne = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(false, reconciliator.CurrentRecordForMatching().Matches.Any(x => x.ActualRecords[0].Description.StartsWith("THE FIRST MATCH")));
        }

        // !! Don't re-order items in CurrentPotentialMatches!! If you do, they will be displayed in a different order than they are stored.
        // This will mean that if a user selects Item with index 4, it might actually be stored with index 9, 
        // and the wrong record will be matched.
        // Instead, if you want to change the ordering, look at CurrentUnmatchedOwnedRecords() and FindLatestMatches()
        [Test]
        public void R_CurrentMatchesShouldNotChangeSourceOrder()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            var sourceMatches = reconciliator
                .CurrentRecordForMatching()
                .Matches
                .ToList();
            var currentMatches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.IsTrue(currentMatches.Count > 1);
            foreach (var match in sourceMatches)
            {
                var index = sourceMatches.IndexOf(match);
                Assert.AreEqual(currentMatches[index].ActualRecords[0].Description, match.ActualRecords[0].Description);
            }
        }

        [Test]
        public void R_CurrentMatchesShouldContainSameItemsAsInternalMatches()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            var matchesStoredInternally = reconciliator.CurrentRecordForMatching().Matches;
            var currentMatches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.AreEqual(matchesStoredInternally.Count, currentMatches.Count);
            foreach (var internalMatch in matchesStoredInternally)
            {
                Assert.IsTrue(currentMatches.Exists(match => match.ActualRecords[0].Description == internalMatch.ActualRecords[0].Description));
            }
        }

        [Test]
        public void CurrentPotentialMatchesShouldBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/1/2017^£24.99^^TILL^4TH NEAREST BY DATE^^^^^",
                "05/2/2017^£24.99^^TILL^2ND NEAREST BY DATE^^^^^",
                "06/2/2017^£24.99^^TILL^1ST NEAREST BY DATE^^^^^",
                "08/2/2017^£24.99^^TILL^3RD NEAREST BY DATE^^^^^",
                "06/4/2017^£24.99^^TILL^5TH NEAREST BY DATE^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                "06/2/2017,POS,'DIVIDE YOUR GREEN,24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var currentPotentialMatches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.IsTrue(currentPotentialMatches.Count == 5);
            Assert.AreEqual("1ST NEAREST BY DATE", currentPotentialMatches[0].ActualRecords[0].Description);
            Assert.AreEqual("2ND NEAREST BY DATE", currentPotentialMatches[1].ActualRecords[0].Description);
            Assert.AreEqual("3RD NEAREST BY DATE", currentPotentialMatches[2].ActualRecords[0].Description);
            Assert.AreEqual("4TH NEAREST BY DATE", currentPotentialMatches[3].ActualRecords[0].Description);
            Assert.AreEqual("5TH NEAREST BY DATE", currentPotentialMatches[4].ActualRecords[0].Description);
        }

        [Test]
        public void MatchesWillBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/1/2017^£24.99^^TILL^4TH NEAREST BY DATE^^^^^",
                "05/2/2017^£24.99^^TILL^2ND NEAREST BY DATE^^^^^",
                "06/2/2017^£24.99^^TILL^1ST NEAREST BY DATE^^^^^",
                "08/2/2017^£24.99^^TILL^3RD NEAREST BY DATE^^^^^",
                "06/4/2017^£24.99^^TILL^5TH NEAREST BY DATE^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                "06/2/2017,POS,'DIVIDE YOUR GREEN,24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches.Count);
            Assert.AreEqual("1ST NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[0].ActualRecords[0].Description);
            Assert.AreEqual("2ND NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[1].ActualRecords[0].Description);
            Assert.AreEqual("3RD NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[2].ActualRecords[0].Description);
            Assert.AreEqual("4TH NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[3].ActualRecords[0].Description);
            Assert.AreEqual("5TH NEAREST BY DATE", reconciliator.CurrentRecordForMatching().Matches[4].ActualRecords[0].Description);
        }

        [Test]
        public void M_MatchesWillHaveAmountRankingValuesPopulated()
        {
            // Arrange
            var amountForMatching = 22.34;
            var actualBankLines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amountForMatching, Description = "NEAREST AMOUNT"}};
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankLines);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankLines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amountForMatching - 4, Description = "5TH NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "2ND NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching, Description = "1ST NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching - 2, Description = "3RD NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amountForMatching + 3, Description = "4TH NEAREST AMOUNT"}};
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankLines);
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, foundOne);
            Assert.AreEqual(5, matches.Count);
            Assert.AreEqual(0, matches.First(x => x.ActualRecords[0].Description == "1ST NEAREST AMOUNT").Rankings.Amount);
            Assert.AreEqual(1, matches.First(x => x.ActualRecords[0].Description == "2ND NEAREST AMOUNT").Rankings.Amount);
            Assert.AreEqual(2, matches.First(x => x.ActualRecords[0].Description == "3RD NEAREST AMOUNT").Rankings.Amount);
            Assert.AreEqual(3, matches.First(x => x.ActualRecords[0].Description == "4TH NEAREST AMOUNT").Rankings.Amount);
            Assert.AreEqual(4, matches.First(x => x.ActualRecords[0].Description == "5TH NEAREST AMOUNT").Rankings.Amount);
        }

        [Test]
        public void R_UnmatchedThirdPartyItemsShouldBeAddedToOwnedFile()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var originalNumOwnedRecords = reconciliator.OwnedFileRecords().Count;

            // Act
            reconciliator.Finish("testing");

            // Assert
            Assert.AreEqual(originalNumOwnedRecords + 3, reconciliator.OwnedFileRecords().Count);
        }

        [Test]
        public void R_UnmatchedThirdPartyItemsShouldHaveUnmatchedFromThirdPartyPrefixedToTheirDescriptions()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var originalNumOwnedRecords = reconciliator.OwnedFileRecords().Count;

            // Act
            reconciliator.Finish("testing");

            // Assert
            Assert.IsTrue(reconciliator.OwnedFileRecords()[originalNumOwnedRecords].Description.StartsWith("!! Unmatched from 3rd party: "));
            Assert.IsTrue(reconciliator.OwnedFileRecords()[originalNumOwnedRecords + 1].Description.StartsWith("!! Unmatched from 3rd party: "));
            Assert.IsTrue(reconciliator.OwnedFileRecords()[originalNumOwnedRecords + 2].Description.StartsWith("!! Unmatched from 3rd party: "));
        }

        [Test]
        public void R_WhenReconciliatingIfNoUnmatchedThirdPartyRecordsAreLeftThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actualBankFile.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, foundOne);
        }

        [Test]
        public void R_WhenReconciliatingIfEndOfFileIsReachedThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actualBankFile.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }

            // Act
            bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, foundOne);
        }

        [Test]
        public void R_WhenReconciliatingIfNoUnmatchedOwnedRecordsAreLeftThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= _bankInFile.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, foundOne);
        }

        [Test]
        public void R_WhenReconciliationFinishesAllMatchedRecordsAreReconciled()
        {
            // Arrange
            _bankInFile.Reload();
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actualBankFile = new CSVFile<ActualBankRecord>(fileIO);
            actualBankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.Finish("testing");

            // Assert
            foreach (var matchedOwnedRecord in reconciliator.OwnedFileRecords().Where(x => x.Matched))
            {
                Assert.AreEqual(0, matchedOwnedRecord.UnreconciledAmount);
                Assert.IsTrue(matchedOwnedRecord.ReconciledAmount > 0);
            }
        }

        [Test]
        public void IfThereAreMultipleMatchesForAmount_ListTextMatchAtTop()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{descriptionWithApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matchedRecord.Matches.Count > 1);
            Assert.AreEqual(description, matchedRecord.Matches[0].ActualRecords[0].Description);
        }

        [Test]
        public void TextMatchesWillIgnoreLeadingApostrophes()
        {
            // Arrange
            var descriptionWithoutApostrophe = "DIVIDE YOUR GREEN";
            var descriptionWithLeadingApostrophe = "'" + descriptionWithoutApostrophe;
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                    "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                    $"02/02/2017^£24.99^^TILL^{descriptionWithoutApostrophe}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{descriptionWithLeadingApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matchedRecord.Matches.Count > 1);
            Assert.AreEqual(descriptionWithoutApostrophe, matchedRecord.Matches[0].ActualRecords[0].Description);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfRecordContainsOriginalDescription()
        {
            // Arrange
            var descriptionWithoutApostrophe = "DIVIDE YOUR GREEN";
            var descriptionWithLeadingApostrophe = "'" + descriptionWithoutApostrophe;
            var descriptionContainingSourceDescription = $"SOME OTHER TEXT {descriptionWithoutApostrophe} AROUND CONTAINED DESCRIPTION";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{descriptionContainingSourceDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{descriptionWithLeadingApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(descriptionContainingSourceDescription, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfRecordExactlyMatchesOriginalDescription()
        {
            // Arrange
            var description = "'DIVIDE YOUR GREEN";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{description},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(description, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfDescriptionIsIdenticalAfterAllPunctuationIsRemoved()
        {
            // Arrange
            var originalDescription = "'DIVIDE, \"YOUR\": GREEN-GREEN; REALLY.@REALLY";
            var descriptionWithoutPunctuation = "DIVIDE YOUR GREENGREEN REALLYREALLY";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{descriptionWithoutPunctuation}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{originalDescription},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(descriptionWithoutPunctuation, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfSomePunctuationIsRemoved()
        {
            // Arrange
            var originalDescription = "'DIVIDE, \"YOUR\": GREEN-GREEN; REALLY.@REALLY";
            var descriptionWithLessPunctuation = "DIVIDE YOUR: GREEN-GREEN REALLYREALLY";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{descriptionWithLessPunctuation}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{originalDescription},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(descriptionWithLessPunctuation, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void FullTextMatchIsCaseInsensitive()
        {
            // Arrange
            var originalDescription = "DIVIDE YOUR GreeN";
            var caseInsensitiveMatch = "Divide YouR green";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{caseInsensitiveMatch}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{originalDescription},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(caseInsensitiveMatch, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void PartialTextMatchIsIncludedInResultsEvenIfNothingElseMatches()
        {
            // Arrange
            var originalDescription = "'DIVIDE YOUR GREEN";
            var partialMatch = "Divide some other thing";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£24.99^^TILL^{partialMatch}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{originalDescription},100.00,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(partialMatch, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
            Assert.IsFalse(matches[0].FullTextMatch);
            Assert.IsFalse(matches[0].AmountMatch);
        }

        [Test]
        public void PartialAmountMatchIsIncludedInResultsEvenIfNothingElseMatches()
        {
            // Arrange
            var amountForMatching = 25;
            var partialAmountMatch = amountForMatching + 2;
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{partialAmountMatch}^^TILL^OTHER^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,THIS,{amountForMatching},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(partialAmountMatch, matches[0].ActualRecords[0].MainAmount());
            Assert.AreEqual(2, matches[0].Rankings.Amount);
            Assert.IsFalse(matches[0].AmountMatch);
            Assert.IsFalse(matches[0].FullTextMatch);
            Assert.IsFalse(matches[0].PartialTextMatch);
        }

        [Test]
        public void IfThereAreMultipleAmountMatchesAndMultipleTextMatches_ListNearestDateAtTop()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var expectedMonth = 3;
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/{expectedMonth - 1}/2017^£24.99^^TILL^{description}^^^^^",
                $"02/{expectedMonth}/2017^£24.99^^TILL^{description}^^^^^",
                $"02/{expectedMonth + 1}/2017^£24.99^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/{expectedMonth}/2017,POS,{descriptionWithApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matchedRecord.Matches.Count > 1);
            Assert.AreEqual(expectedMonth, matchedRecord.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreMultipleAmountAndTextMatches_AndExactDateMatchWhichDoesntMatchText_ThenTopResultIsNearestDateNotExactDate()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var expectedMonth = 3;
            var actualDay = 6;
            var closestMatchingDay = actualDay + 2;
            var exactDate = $"{actualDay}/{expectedMonth}/2017";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{exactDate}^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{closestMatchingDay}/{expectedMonth - 1}/2017^£24.99^^TILL^{description}^^^^^",
                $"{closestMatchingDay}/{expectedMonth}/2017^£24.99^^TILL^{description}^^^^^",
                $"{closestMatchingDay}/{expectedMonth + 1}/2017^£24.99^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{exactDate},POS,{descriptionWithApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matchedRecord.Matches.Count > 1);
            Assert.AreEqual(closestMatchingDay, matchedRecord.Matches[0].ActualRecords[0].Date.Day, "Day");
            Assert.AreEqual(expectedMonth, matchedRecord.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreMultipleAmountMatches_AndNoTextMatches_ListNearestDateAtTop()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var expectedMonth = 3;
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"06/{expectedMonth - 1}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/{expectedMonth}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/{expectedMonth + 1}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/{expectedMonth}/2017,POS,{descriptionWithApostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matchedRecord.Matches.Count > 1);
            Assert.AreEqual(expectedMonth, matchedRecord.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreNoAmountMatches_FindTextMatchesInstead()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"06/2/2017^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/3/2017^{someOtherAmount}^^TILL^{description}^^^^^",
                $"06/4/2017^{someOtherAmount}^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/3/2017,POS,{descriptionWithApostrophe},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(2, matchedRecord.Matches.Count);
            Assert.AreEqual(description, matchedRecord.Matches[0].ActualRecords[0].Description);
            Assert.AreEqual(description, matchedRecord.Matches[1].ActualRecords[0].Description);
        }

        [Test] public void OrderSemiAutoMatches_ByExactAmount_ThenByFullText_ThenByPartialText_ThenByNearestDate_ThenByPartialAmount()
        {
            // Arrange
            var description = "DIVIDE YOUR -Green;";
            var partialDescriptionMatch = $"SOMETHING SOMETHING GREENE";
            var descriptionWithApostrophe = "'" + description;
            var originalAmount = 25;
            var originalAmountString = $"{originalAmount}.00";
            var partialAmount = originalAmount - 2;
            var partialAmountString = $"{partialAmount}.00";
            var someOtherAmount = "66.66";
            var originalMonth = 3;
            var originalDate = $"06/0{originalMonth}/2017";
            var originalDateMinusOne = $"06/0{originalMonth - 1}/2017";
            var originalDatePlusTwo = $"06/0{originalMonth + 2}/2017";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{originalDate}^{originalAmountString}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDatePlusTwo}^{originalAmountString}^^TILL^{description}^^^^^",
                $"{originalDate}^{originalAmountString}^^TILL^{description}^^^^^",
                $"{originalDateMinusOne}^{originalAmountString}^^TILL^{description}^^^^^",
                $"{originalDateMinusOne}^{originalAmountString}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDatePlusTwo}^{originalAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDate}^{someOtherAmount}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDatePlusTwo}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDate}^{originalAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDateMinusOne}^{originalAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDate}^{partialAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDateMinusOne}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDateMinusOne}^{someOtherAmount}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDatePlusTwo}^{someOtherAmount}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDatePlusTwo}^{originalAmountString}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDateMinusOne}^{someOtherAmount}^^TILL^{description}^^^^^",
                $"{originalDateMinusOne}^{partialAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDatePlusTwo}^{partialAmountString}^^TILL^{partialDescriptionMatch}^^^^^",
                $"{originalDatePlusTwo}^{someOtherAmount}^^TILL^{description}^^^^^",
                $"{originalDate}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDate}^{someOtherAmount}^^TILL^{description}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{originalDate},POS,{descriptionWithApostrophe},{originalAmountString},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var currentPotentialMatches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(18, currentPotentialMatches.Count);
            Assert.AreEqual($"0. {originalDate},£{originalAmountString},{description}", currentPotentialMatches[0].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"1. {originalDateMinusOne},£{originalAmountString},{description}", currentPotentialMatches[1].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"2. {originalDatePlusTwo},£{originalAmountString},{description}", currentPotentialMatches[2].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"3. {originalDate},£{originalAmountString},{partialDescriptionMatch}", currentPotentialMatches[3].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"4. {originalDateMinusOne},£{originalAmountString},{partialDescriptionMatch}", currentPotentialMatches[4].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"5. {originalDatePlusTwo},£{originalAmountString},{partialDescriptionMatch}", currentPotentialMatches[5].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"6. {originalDate},£{originalAmountString},SOME OTHER DESCRIPTION", currentPotentialMatches[6].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"7. {originalDateMinusOne},£{originalAmountString},SOME OTHER DESCRIPTION", currentPotentialMatches[7].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"8. {originalDatePlusTwo},£{originalAmountString},SOME OTHER DESCRIPTION", currentPotentialMatches[8].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"9. {originalDate},£{someOtherAmount},{description}", currentPotentialMatches[9].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"10. {originalDateMinusOne},£{someOtherAmount},{description}", currentPotentialMatches[10].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"11. {originalDatePlusTwo},£{someOtherAmount},{description}", currentPotentialMatches[11].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"12. {originalDate},£{partialAmountString},{partialDescriptionMatch}", currentPotentialMatches[12].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"13. {originalDate},£{someOtherAmount},{partialDescriptionMatch}", currentPotentialMatches[13].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"14. {originalDateMinusOne},£{partialAmountString},{partialDescriptionMatch}", currentPotentialMatches[14].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"15. {originalDateMinusOne},£{someOtherAmount},{partialDescriptionMatch}", currentPotentialMatches[15].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"16. {originalDatePlusTwo},£{partialAmountString},{partialDescriptionMatch}", currentPotentialMatches[16].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"17. {originalDatePlusTwo},£{someOtherAmount},{partialDescriptionMatch}", currentPotentialMatches[17].ConsoleLines[0].AsTextLine());
        }

        [Test]
        public void IfThereAreNoAmountOrTextMatches_ThenThereAreNoMatches()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var descriptionWithApostrophe = "'" + description;
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var originalMonth = 3;
            var originalDate = $"06/0{originalMonth}/2017";
            var originalDateMinusOne = $"06/0{originalMonth - 1}/2017";
            var originalDatePlusTwo = $"06/0{originalMonth + 2}/2017";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{originalDatePlusTwo}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDateMinusOne}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{originalDate}^{someOtherAmount}^^TILL^SOME OTHER DESCRIPTION^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{originalDate},POS,{descriptionWithApostrophe},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(null, matchedRecord);
        }

        [Test]
        public void WillFindPartialMatchIfCandidateDescriptionContainsOneWordFromOriginal()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var targetWord = "PINTIPOPLICATION";
            var candidateDescription = $"SOMETHING {targetWord} SOMETHING";
            var sourceDescription = $"DIVIDE {targetWord} GREEN";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{sourceDescription},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidateDescription, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        [TestCase("and")]
        [TestCase("the")]
        [TestCase("or")]
        [TestCase("with")]
        public void WillNotFindPartialMatchIfCandidateDescriptionContainsReservedWordFromOriginal(string targetWord)
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var candidateDescription = $"SOMETHING {targetWord} SOMETHING";
            var sourceDescription = $"DIVIDE {targetWord} GREEN";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{sourceDescription},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(null, matches, $"Should not match on '{targetWord}'");
        }

        [Test]
        public void WillFindPartialMatchIfCandidateDescriptionContainsWordWhichStartsWithWordFromOriginal()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var targetWord = "GREEN";
            var startsWithTargetWord = "GREENE";
            var candidateDescription = $"SOMETHING SOMETHING {startsWithTargetWord}";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR {targetWord},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidateDescription, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void WillFindPartialMatchIfCandidateDescriptionContainsWordWhichWasSurroundedWithPunctuationInOriginal()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var targetWord = "GREEN";
            var candidateDescription = $"SOMETHING SOMETHING {targetWord}";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{targetWord}:,{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidateDescription, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void PartialMatchIsNotCaseSensitive()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var targetWord = "PiNtIPOPliCaTIOn";
            var targetWordToUpper = targetWord.ToUpper();
            var candidateDescription = $"SOMETHING SOMETHING {targetWordToUpper}";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{targetWord}:,{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidateDescription, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void PairedLettersAreOnlyMatchedIfTheyArePresentAsSeparateWords()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var targetWord = "UK";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^SOMETHING FC{targetWord}^^^^^",
                $"02/02/2017^£{someOtherAmount}^^TILL^SOMETHING {targetWord}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{targetWord}:,{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual($"SOMETHING {targetWord}", matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void SingleLettersAreNotMatchedAsWords()
        {
            // Arrange
            var originalAmount = "24.99";
            var someOtherAmount = "66.66";
            var originalDescription = "ZZZOTHERWHERE S S T ZZZOTHERWHERE";
            var candidateDescription = "SOMETHING S S T SOMETHING";
            var tempBankInFile = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{someOtherAmount}^^TILL^{candidateDescription}^^^^^"});
            var tempActualBankFile = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{originalDescription},{originalAmount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(tempActualBankFile, tempBankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matchedRecord = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(null, matchedRecord);
        }

        [Test]
        public void M_CanAutomaticallyMatchItemsBasedOn_IdenticalAmount_AndPartialTextMatch_AndNearDate()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var amount2 = amount1 + 10;
            var text2 = "IMPECCABLE TILING";
            var date2 = date1.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1 + "something"},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2 + "something"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2.AddDays(2), UnreconciledAmount = amount2, Description = text2 + "extra"},
                    new BankRecord {Date = date1.AddDays(2), UnreconciledAmount = amount1, Description = text1 + "extra"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            foreach (var recordForMatching in autoMatchedItems)
            {
                var sourceDesc = recordForMatching.SourceRecord.Description;
                var matchDesc = recordForMatching.Match.ActualRecords[0].Description;
                Assert.IsTrue(reconciliator.CheckForPartialTextMatch(sourceDesc, matchDesc), 
                    "source and match should have partially matching descriptions");
                Assert.AreEqual(recordForMatching.SourceRecord.MainAmount(),
                    recordForMatching.Match.ActualRecords[0].MainAmount(),
                    "source and match should have matching amounts");
                Assert.IsTrue(recordForMatching.Match.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold, 
                    "source and match should have near dates");
            }
        }

        [TestCase('(')]
        [TestCase(')')]
        [TestCase('-')]
        [TestCase('_')]
        [TestCase(';')]
        [TestCase(':')]
        [TestCase(',')]
        [TestCase('.')]
        [TestCase('\'')]
        [TestCase('#')]
        [TestCase('!')]
        [TestCase('?')]
        public void WhenCheckingForPartialTextMatch_WillFindPartialMatchForWordSurroundedByPunctuation(char punctuation)
        {
            // Arrange
            var mockActualBankFile = new Mock<ICSVFile<ActualBankRecord>>().Object;
            var mockBankFile = new Mock<ICSVFile<BankRecord>>().Object;
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(mockActualBankFile, mockBankFile, ThirdPartyFileLoadAction.NoAction);
            var source = $"This string contains the{punctuation}word that we want";
            var target = $"Also has missing {punctuation}word{punctuation} required";

            // Act
            var result = reconciliator.CheckForPartialTextMatch(source, target);

            // Assert
            Assert.IsTrue(result, $"'{source}' and '{target}' should be a partial match.");
        }

        [Test]
        public void M_WhenAutoMatching_WillHaveOneMatchPerRecord()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var amount2 = amount1 + 10;
            var text2 = "IMPECCABLE TILING";
            var date2 = date1.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = "boppl"},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = "bazzl"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = text2},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(2, autoMatchedItems.Count, "should be correct number of auto-matching results");
            foreach (var recordForMatching in autoMatchedItems)
            {
                Assert.IsNotNull(recordForMatching.Match, "every record should have a match");
            }
        }

        [Test]
        public void M_WhenAutoMatching_MatchesAreConnectedToEachOther()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(autoMatchedItems[0].Match.ActualRecords[0], autoMatchedItems[0].SourceRecord.Match);
            Assert.AreEqual(autoMatchedItems[0].SourceRecord, autoMatchedItems[0].Match.ActualRecords[0].Match);
        }

        [Test]
        public void M_WhenAutoMatching_SourceAndMatch_ShouldBeMarkedAsMatched()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.IsTrue(autoMatchedItems[0].SourceRecord.Matched);
            AssertMatchIsMatched(autoMatchedItems[0].Match);
        }

        [Test]
        public void M_WhenAutoMatching_NoOwnedRecordIsMatchedTwice()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(1, autoMatchedItems.Count);
        }

        [Test]
        public void M_WhenAutoMatching_IfSourceRecordHasNoMatch_ThenItIsNotMatched()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var amount2 = amount1 + 50;
            var text2 = "IMPECCABLE TILING";
            var date2 = date1.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = "bazzl"},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(1, autoMatchedItems.Count);
            Assert.AreEqual(text1, autoMatchedItems[0].SourceRecord.Description);
        }

        [Test]
        public void M_WhenAutoMatching_MatchesAreOrderedBySourceRecordDate()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var date2 = date1.AddDays(30);
            var date3 = date2.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(date1, autoMatchedItems[0].SourceRecord.Date);
            Assert.AreEqual(date2, autoMatchedItems[1].SourceRecord.Date);
            Assert.AreEqual(date3, autoMatchedItems[2].SourceRecord.Date);
        }

        [Test]
        public void M_WhenAutoMatching_MatchWillOnlyHappen_IfThereIsExactlyOneMatch()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(0, autoMatchedItems.Count);
        }

        [Test]
        public void M_RecordsReturnedByAutoMatchingAreIndexed()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var amount2 = amount1 + 50;
            var text2 = "IMPECCABLE TILING";
            var date2 = date1.AddDays(30);
            var amount3 = amount2 + 50;
            var text3 = "SOMETHING ELSE";
            var date3 = date2.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount3, Description = text3},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount3, Description = text3}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(0, autoMatchedItems[0].Index);
            Assert.AreEqual(1, autoMatchedItems[1].Index);
            Assert.AreEqual(2, autoMatchedItems[2].Index);
        }

        [Test]
        public void M_CanRetrieveAutoMatchesWithoutReDoingAutoMatching()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            List<AutoMatchedRecord<ActualBankRecord>> autoMatchedItems = reconciliator.ReturnAutoMatches();

            // Act
            var refetchedAutoMatchedItems = reconciliator.GetAutoMatches();

            // Assert
            Assert.AreEqual(autoMatchedItems, refetchedAutoMatchedItems);
        }

        [Test]
        public void M_AutoMatchesAreNotPopulatedUnlessDoAutoMatchingIsCalled()
        {
            // Arrange
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            var autoMatchedItems = reconciliator.GetAutoMatches();

            // Assert
            Assert.AreEqual(null, autoMatchedItems);
        }

        [Test]
        public void M_AfterAutoMatchingIsFinished_AutoMatchedItemsAreNotAvailableForMatching()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var partialMatch = "PINTIPOPLICATION";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = partialMatch + "SOMETHING ELSE"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = partialMatch + "other"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.ReturnAutoMatches();

            // Assert
            bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.IsTrue(foundOne, "can find new semi-auto match");
            var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(partialMatch + "SOMETHING ELSE", currentRecordForMatching.SourceRecord.Description);
            var currentPotentialMatches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(partialMatch + "other", currentPotentialMatches[0].ActualRecords[0].Description);
            foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.IsFalse(foundOne, "only found one new semi-auto match");
        }

        [Test]
        public void M_MatchCanBeRemovedFromAutoMatchList()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var date1 = new DateTime(2018, 10, 31);
            var date2 = date1.AddDays(30);
            var date3 = date2.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            const int matchIndex = 1;
            const int numRecordsAtStart = 3;
            // The where clause is redundant, but it's here for parity with the reassignment of autoMatchedItems below.
            var autoMatchedItems = reconciliator.ReturnAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, autoMatchedItems.Count);
            Assert.AreEqual(matchIndex, autoMatchedItems[1].Index);
            Assert.AreEqual(text2, autoMatchedItems[1].SourceRecord.Description);
            var originalMatch = autoMatchedItems[1].Match;

            // Act
            reconciliator.RemoveAutoMatch(matchIndex);

            // Assert
            autoMatchedItems = reconciliator.GetAutoMatches();
            var filteredAutoMatchedItems = reconciliator.GetAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - 1, filteredAutoMatchedItems.Count);
            Assert.IsFalse(filteredAutoMatchedItems.Any(x => x.SourceRecord.Description == text2));
            Assert.IsFalse(autoMatchedItems[matchIndex].SourceRecord.Matched);
            AssertMatchIsNoLongerMatched(originalMatch);
            Assert.IsNull(autoMatchedItems[matchIndex].Match);
            Assert.IsNull(autoMatchedItems[matchIndex].SourceRecord.Match);
        }

        [Test]
        public void M_MultipleMatchesCanBeRemovedFromAutoMatchList()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var text4 = "Antidisestablishmentarianism";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            // The where clause is redundant, but it's here for parity with the reassignment of autoMatchedItems below.
            var autoMatchedItems = reconciliator.ReturnAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, autoMatchedItems.Count);
            List<IPotentialMatch> originalMatches = new List<IPotentialMatch>();
            List<int> matchIndices = new List<int> { 1, 2, 3 };
            foreach (var matchIndex in matchIndices)
            {
                originalMatches.Add(autoMatchedItems[matchIndex].Match);
            }

            // Act
            reconciliator.RemoveMultipleAutoMatches(matchIndices);

            // Assert
            autoMatchedItems = reconciliator.GetAutoMatches();
            var filteredAutoMatchedItems = reconciliator.GetAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - matchIndices.Count, filteredAutoMatchedItems.Count);
            Assert.IsFalse(filteredAutoMatchedItems.Any(x => x.SourceRecord.Description == text2));
            Assert.IsFalse(filteredAutoMatchedItems.Any(x => x.SourceRecord.Description == text3));
            Assert.IsFalse(filteredAutoMatchedItems.Any(x => x.SourceRecord.Description == text4));
            for (int count = 0; count < matchIndices.Count; count++)
            {
                AssertSourceRecordIsNotMatched(autoMatchedItems[matchIndices[count]]);
                AssertMatchIsNoLongerMatched(originalMatches[count]);
            }
        }

        [Test]
        public void M_AfterMatchesAreRemovedFromAutoMatchList_TheyAreAvailableForMatchingAgain()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var text4 = "Antidisestablishmentarianism";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            var autoMatchedItems = reconciliator.ReturnAutoMatches();
            List<IPotentialMatch> originalMatches = new List<IPotentialMatch>();
            List<int> matchIndices = new List<int> { 1, 2, 3 };
            foreach (var matchIndex in matchIndices)
            {
                originalMatches.Add(autoMatchedItems[matchIndex].Match);
            }

            // Act
            reconciliator.RemoveMultipleAutoMatches(matchIndices);

            // Assert
            for (int count = 0; count < matchIndices.Count; count++)
            {
                bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                Assert.IsTrue(foundOne, "can find new semi-auto match");
                var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
                Assert.AreEqual(autoMatchedItems[matchIndices[count]].SourceRecord, 
                    currentRecordForMatching.SourceRecord, 
                    "3rd party record is available again");
                Assert.AreEqual(originalMatches[count].ActualRecords[0].Description, 
                    currentRecordForMatching.Matches[0].ActualRecords[0].Description, 
                    "Unmatched Owned record is back in the list of matches");
            }
        }

        [Test]
        public void M_MatchCanBeRemovedFromFinalMatchList()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var date1 = new DateTime(2018, 10, 31);
            var date2 = date1.AddDays(30);
            var date3 = date2.AddDays(30);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            const int matchIndex = 1;
            const int numRecordsAtStart = 3;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matchedItems = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matchedItems.Count);
            Assert.AreEqual(text2, matchedItems[1].Description);
            var originalMatch = matchedItems[1].Match;

            // Act
            reconciliator.RemoveFinalMatch(matchIndex);

            // Assert
            matchedItems = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - 1, matchedItems.Count);
            Assert.IsFalse(matchedItems.Any(x => x.Description == text2));
            Assert.IsFalse(reconciliator.ThirdPartyRecords()[matchIndex].Matched);
            Assert.IsFalse(originalMatch.Matched);
            Assert.IsNull(reconciliator.ThirdPartyRecords()[matchIndex].Match);
            Assert.IsNull(originalMatch.Match);
        }

        [Test]
        public void M_MultipleMatchesCanBeRemovedFromFinalMatchList()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var text4 = "Antidisestablishmentarianism";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matchedItems = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matchedItems.Count);
            List<ICSVRecord> originalMatches = new List<ICSVRecord>();
            List<int> matchIndices = new List<int> { 1, 2, 3 };
            foreach (var matchIndex in matchIndices)
            {
                originalMatches.Add(matchedItems[matchIndex].Match);
            }

            // Act
            reconciliator.RemoveMultipleFinalMatches(matchIndices);

            // Assert
            matchedItems = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - matchIndices.Count, matchedItems.Count);
            Assert.IsFalse(matchedItems.Any(x => x.Description == text2));
            Assert.IsFalse(matchedItems.Any(x => x.Description == text3));
            Assert.IsFalse(matchedItems.Any(x => x.Description == text4));
            for (int count = 0; count < matchIndices.Count; count++)
            {
                AssertRecordIsNoLongerMatched(reconciliator.ThirdPartyRecords()[matchIndices[count]]);
                AssertRecordIsNoLongerMatched(originalMatches[count]);
            }
        }

        [Test]
        public void M_AfterMatchesAreRemovedFromFinalMatchList_TheyAreAvailableForMatchingAgain()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var text4 = "Antidisestablishmentarianism";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matchedItems = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matchedItems.Count);
            List<ICSVRecord> originalMatches = new List<ICSVRecord>();
            List<int> matchIndices = new List<int> { 1, 2, 3 };
            foreach (var matchIndex in matchIndices)
            {
                originalMatches.Add(matchedItems[matchIndex].Match);
            }

            // Act
            reconciliator.RemoveMultipleFinalMatches(matchIndices);

            // Assert
            reconciliator.Rewind();
            for (int count = 0; count < matchIndices.Count; count++)
            {
                bool foundOne = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                Assert.IsTrue(foundOne, "can find new semi-auto match");
                var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
                Assert.AreEqual(reconciliator.ThirdPartyRecords()[matchIndices[count]], 
                    currentRecordForMatching.SourceRecord, 
                    "3rd party record is available again");
                Assert.AreEqual(originalMatches[count].Description, 
                    currentRecordForMatching.Matches[0].ActualRecords[0].Description, 
                    "Unmatched Owned record is back in the list of matches");
            }
        }

        [Test]
        public void M_GetAutoMatchesForConsole_ShouldAddThreeIndexedLines_ForEachAutoMatchedRecord()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text3}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = "nonsense"},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text3}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            var numMatches = 2;

            // Act
            var consoleLines = reconciliator.GetAutoMatchesForConsole();

            // Assert
            Assert.AreEqual(numMatches * 3, consoleLines.Count);
            var separatorLines = consoleLines.Where(x => x.DateString == "----------");
            Assert.AreEqual(numMatches, separatorLines.Count(), "output should contain separator lines");
            var text1Lines = consoleLines.Where(x => x.DescriptionString == text1);
            Assert.IsTrue(text1Lines.All(x => x.Index == 0), "text1 lines should be indexed correctly");
            Assert.AreEqual(2, text1Lines.Count(), "text1: one line for third party record and one line for match");
            var text3Lines = consoleLines.Where(x => x.DescriptionString == text3);
            Assert.IsTrue(text3Lines.All(x => x.Index == 1), "text3 lines should be indexed correctly");
            Assert.AreEqual(2, text3Lines.Count(), "text3: one line for third party record and one line for match");
        }

        [Test]
        public void M_GetFinalMatchesForConsole_ShouldAddThreeIndexedLines_ForEachMatchedThirdPartyRecord_CredCard2()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var text3 = "PINTIPOPLICATION";
            var text4 = "TRANSUBSTANTIATION";
            var date1 = new DateTime(2018, 10, 31);
            var mockCredCard2FileIO = new Mock<IFileIO<CredCard2Record>>();
            var mockCredCard2InOutFileIO = new Mock<IFileIO<CredCard2InOutRecord>>();
            mockCredCard2FileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2Record> {
                    new CredCard2Record {Date = date1, Amount = amount1, Description = text1},
                    new CredCard2Record {Date = date1.AddDays(1), Amount = amount1, Description = text2},
                    new CredCard2Record {Date = date1.AddDays(2), Amount = amount1, Description = text3},
                    new CredCard2Record {Date = date1.AddDays(3), Amount = amount1, Description = text4}
                });
            mockCredCard2InOutFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new CredCard2InOutRecord {Date = date1.AddDays(1), UnreconciledAmount = amount1 + 10, Description = "nonsense"},
                    new CredCard2InOutRecord {Date = date1.AddDays(2), UnreconciledAmount = amount1 + 10, Description = text3},
                    new CredCard2InOutRecord {Date = date1.AddDays(3), UnreconciledAmount = amount1, Description = text4}
                });
            var credCard2File = new CSVFile<CredCard2Record>(mockCredCard2FileIO.Object);
            credCard2File.Load();
            var credCard2InOutFile = new CSVFile<CredCard2InOutRecord>(mockCredCard2InOutFileIO.Object);
            credCard2InOutFile.Load();
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(credCard2File, credCard2InOutFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            var numMatches = 3;

            // Act
            var consoleLines = reconciliator.GetFinalMatchesForConsole();

            // Assert
            Assert.AreEqual(numMatches * 3, consoleLines.Count);
            var separatorLines = consoleLines.Where(x => x.DateString == "----------");
            Assert.AreEqual(numMatches, separatorLines.Count(), "output should contain separator lines");
            var text1Lines = consoleLines.Where(x => x.DescriptionString.Contains(text1));
            Assert.IsTrue(text1Lines.All(x => x.Index == 0), "text1 lines should be indexed correctly");
            Assert.AreEqual(2, text1Lines.Count(), "text1: one line for third party record and one line for match");
            var text3Lines = consoleLines.Where(x => x.DescriptionString.Contains(text3));
            Assert.IsTrue(text3Lines.All(x => x.Index == 2), "text3 lines should be indexed correctly");
            Assert.AreEqual(2, text3Lines.Count(), "text3: one line for third party record and one line for match");
            var text4Lines = consoleLines.Where(x => x.DescriptionString.Contains(text4));
            Assert.IsTrue(text4Lines.All(x => x.Index == 3), "text4 lines should be indexed correctly");
            Assert.AreEqual(2, text4Lines.Count(), "text4: one line for third party record and one line for match");
        }

        [TestCase(TransactionMatchType.Auto)]
        [TestCase(TransactionMatchType.Final)]
        public void M_WhenAttemptingToRemoveMatchWithBadIndex_ErrorIsThrown(TransactionMatchType TransactionMatchType)
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            bool exceptionThrown = false;
            string errorMessage = "";

            // Act
            try
            {
                if (TransactionMatchType == TransactionMatchType.Auto)
                {
                    reconciliator.RemoveAutoMatch(3);
                }
                else
                {
                    reconciliator.RemoveFinalMatch(3);
                }
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, errorMessage);
        }

        [TestCase(TransactionMatchType.Auto)]
        [TestCase(TransactionMatchType.Final)]
        public void M_WhenAttemptingToRemoveMultipleMatchesWithBadIndices_ErrorIsThrown(TransactionMatchType TransactionMatchType)
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "haha bonk";
            var date1 = new DateTime(2018, 10, 31);
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text2}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankInFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankInFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            bool exceptionThrown = false;
            string errorMessage = "";

            // Act
            try
            {
                if (TransactionMatchType == TransactionMatchType.Auto)
                {
                    reconciliator.RemoveMultipleAutoMatches(new List<int> { 0, 1, 2 });
                }
                else
                {
                    reconciliator.RemoveMultipleFinalMatches(new List<int>{ 0, 1, 125 });
                }
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, errorMessage);
        }
    }
}
