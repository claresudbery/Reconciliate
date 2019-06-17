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

            var current_path = TestContext.CurrentContext.TestDirectory;
            _absoluteCSVFilePath = TestHelper.FullyQualifiedCSVFilePath(current_path);

            var file_io_actual_bank = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-sample");
            _actualBankFile = new CSVFile<ActualBankRecord>(file_io_actual_bank);
            _actualBankFile.Load();

            var file_io_bank_in = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "BankIn-formatted-date-only");
            _bankInFile = new CSVFile<BankRecord>(file_io_bank_in);
            _bankInFile.Load();

            var file_io_bank_out = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "BankOut-formatted-date-only");
            _bankOutFile = new CSVFile<BankRecord>(file_io_bank_out);
            _bankOutFile.Load();


            var file_io_cred_card1 = new FileIO<CredCard1Record>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard1-Statement");
            _credCard1File = new CSVFile<CredCard1Record>(file_io_cred_card1);
            _credCard1File.Load();

            var file_io_cred_card1_in_out = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard1InOut-formatted-date-only");
            _credCard1InOutFile = new CSVFile<CredCard1InOutRecord>(file_io_cred_card1_in_out);
            _credCard1InOutFile.Load();


            var file_io_cred_card2 = new FileIO<CredCard2Record>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard2");
            _credCard2File = new CSVFile<CredCard2Record>(file_io_cred_card2);
            _credCard2File.Load();

            var file_io_cred_card2_in_out = new FileIO<CredCard2InOutRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "CredCard2InOut");
            _credCard2InOutFile = new CSVFile<CredCard2InOutRecord>(file_io_cred_card2_in_out);
            _credCard2InOutFile.Load();
        }

        private CSVFile<TRecordType> CreateCsvFile<TRecordType>(
            string fileName,
            string[] textLines) where TRecordType : ICSVRecord, new()
        {
            var full_file_path = _absoluteCSVFilePath + "/" + fileName + ".csv";
            File.WriteAllLines(full_file_path, textLines);
            var file_io = new FileIO<TRecordType>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, fileName);
            var csv_file = new CSVFile<TRecordType>(file_io);
            csv_file.Load();
            return csv_file;
        }

        private void AssertMatchIsNoLongerMatched(IPotentialMatch originalMatch)
        {
            foreach (var actual_record in originalMatch.ActualRecords)
            {
                Assert.IsFalse(actual_record.Matched);
                Assert.IsNull(actual_record.Match);
            }
        }

        private void AssertMatchIsMatched(IPotentialMatch originalMatch)
        {
            foreach (var actual_record in originalMatch.ActualRecords)
            {
                Assert.IsTrue(actual_record.Matched);
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
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            var mock_bank_file = new Mock<ICSVFile<BankRecord>>();

            // Act
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(
                mock_actual_bank_file.Object,
                mock_bank_file.Object,
                BankAndBankInData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mock_actual_bank_file.Verify(x => x.FilterForPositiveRecordsOnly());
        }

        [Test]
        public void Constructor_WillFilterForNegativeRecords_WhenBankAndBankOut()
        {
            // Arrange
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            var mock_bank_file = new Mock<ICSVFile<BankRecord>>();

            // Act
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(
                mock_actual_bank_file.Object,
                mock_bank_file.Object,
                BankAndBankOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mock_actual_bank_file.Verify(x => x.FilterForNegativeRecordsOnly());
        }

        [Test]
        public void Constructor_WillPerformNoAction_WhenCredCard1AndCredCard1InOut()
        {
            // Arrange
            var mock_cred_card1_file = new Mock<ICSVFile<CredCard1Record>>();
            var mock_cred_card1_in_out_file = new Mock<ICSVFile<CredCard1InOutRecord>>();

            // Act
            var reconciliator = new Reconciliator<CredCard1Record, CredCard1InOutRecord>(
                mock_cred_card1_file.Object,
                mock_cred_card1_in_out_file.Object,
                CredCard1AndCredCard1InOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mock_cred_card1_file.Verify(x => x.FilterForPositiveRecordsOnly(), Times.Never);
            mock_cred_card1_file.Verify(x => x.FilterForNegativeRecordsOnly(), Times.Never);
            mock_cred_card1_file.Verify(x => x.SwapSignsOfAllAmounts(), Times.Never);
        }

        [Test]
        public void Constructor_WillPerformNoAction_WhenCredCard2AndCredCard2InOut()
        {
            // Arrange
            var mock_cred_card2_file = new Mock<ICSVFile<CredCard2Record>>();
            var mock_cred_card2_in_out_file = new Mock<ICSVFile<CredCard2InOutRecord>>();

            // Act
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(
                mock_cred_card2_file.Object,
                mock_cred_card2_in_out_file.Object,
                CredCard2AndCredCard2InOutData.LoadingInfo.ThirdPartyFileLoadAction);

            // Assert
            mock_cred_card2_file.Verify(x => x.FilterForPositiveRecordsOnly(), Times.Never);
            mock_cred_card2_file.Verify(x => x.FilterForNegativeRecordsOnly(), Times.Never);
            mock_cred_card2_file.Verify(x => x.SwapSignsOfAllAmounts(), Times.Never);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(1, matched_record.Matches.Count);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(1, matched_record.Matches.Count);
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
            RecordForMatching<CredCard1Record> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(5, matched_record.Matches.Count);
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
            RecordForMatching<CredCard2Record> matched_record = reconciliator.CurrentRecordForMatching();

            // Assert
            Assert.AreEqual(9, matched_record.Matches.Count);
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetThenRelevantRecordIsMarkedAsMatched()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            AssertMatchIsMatched(matched_record.Matches[0]);
            Assert.AreEqual(true, _bankInFile.Records[0].Matched);
            Assert.AreEqual(true, matched_record.SourceRecord.Matched);
            Assert.AreEqual(true, _actualBankFile.Records[0].Matched);
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetForMatchWithDifferentAmount_WillAddExplanatoryTextAndChangeAmount()
        {
            // Arrange
            var amount1 = 22.23;
            var matching_text = "DIVIDE YOUR GREEN";
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount1, Description = matching_text}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount1 + 1, Description = matching_text}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.IsTrue(matched_record.SourceRecord.Match.Description.Contains(ReconConsts.OriginalAmountWas));
            Assert.AreEqual(matched_record.SourceRecord.MainAmount(), matched_record.SourceRecord.Match.MainAmount());
        }

        [Test]
        public void R_WhenLatestMatchIndexIsSetThenRelevantRecordsHaveMatchedRecordsAttached()
        {
            // Arrange
            _bankInFile.Reload();
            _actualBankFile.FilterForPositiveRecordsOnly();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(_actualBankFile, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            foreach (var actual_record in matched_record.Matches[0].ActualRecords)
            {
                Assert.AreEqual(_actualBankFile.Records[0], actual_record.Match);
            }
            Assert.AreEqual(_bankInFile.Records[0], matched_record.SourceRecord.Match);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Assert
            Assert.AreEqual(3, matched_record.Matches.Count);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(1);

            // Assert
            // (Remember the matches will have been ordered in order of which ones are closest in date to the original)
            AssertMatchIsMatched(matched_record.Matches[1]);
            Assert.AreEqual(true, _bankInFile.Records[1].Matched);
            Assert.AreEqual(true, matched_record.SourceRecord.Matched);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

            // Act
            reconciliator.MatchCurrentRecord(2);

            // Assert
            // (Remember the matches will have been ordered in order of which ones are closest in date to the original)
            foreach (var actual_record in matched_record.Matches[2].ActualRecords)
            {
                Assert.AreEqual(matched_record.SourceRecord, actual_record.Match);
                Assert.AreEqual(_actualBankFile.Records[2], actual_record.Match);
                Assert.AreEqual("'ZZZThing", actual_record.Match.Description);
            }
            Assert.AreEqual("ZZZThing3", matched_record.Matches[2].ActualRecords[0].Description);
            Assert.AreEqual(_bankInFile.Records[4], matched_record.SourceRecord.Match);
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
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();

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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            bool exception_thrown = false;
            string error_message = "";

            // Act
            try
            {
                reconciliator.MatchCurrentRecord(10);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, error_message);
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
            var matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual("'ZZZSpecialDescription001", matched_record.SourceRecord.Description);

            // Act
            reconciliator.MatchCurrentRecord(0);
            reconciliator.Rewind();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            matched_record = reconciliator.CurrentRecordForMatching();

            // Assert
            // Because the ZZZSpecialDescription001 record was matched on the previous pass, it will now be skipped over in favour of the next record.
            Assert.AreNotEqual("'ZZZSpecialDescription001", matched_record.SourceRecord.Description);
        }

        [Test]
        public void M_WhenOwnedRecordHasAlreadyBeenMatchedThenItIsNoLongerACandidateForMatching()
        {
            // Arrange
            var amount_for_matching = 22.23;
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Source01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Source01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Source01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Source01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01a"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01b"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01c"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "Match02"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

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
            bool matches_found = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            // There are no matches left that match the text and amount
            Assert.IsFalse(matches_found);
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
            bool found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
            Assert.AreEqual(false, reconciliator.CurrentRecordForMatching().SourceRecord.Matched);
        }

        [Test]
        public void M_WhenReconciliatingLeftoverRecords_MatchesShouldHaveConsoleLinesAttached()
        {
            // Arrange
            var amount_for_matching = 22.23;
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Source"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matched_records = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(3, matched_records.Count);
            foreach (var match in matched_records)
            {
                Assert.IsTrue(match.ConsoleLines[0] != null);
            }
        }

        [Test]
        public void R_WhenReconciliatingLeftoverRecordsIfNoUnmatchedThirdPartyRecordsAreLeftThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actual_bank_file.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            bool found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(false, found_one);
        }

        [Test]
        public void R_WhenReconciliatingLeftoverRecordsForTheSecondTimeAnyThirdPartyRecordsMatchedOnThePreviousPassAreNoLongerReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actual_bank_file.Records.Count - 1; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MatchCurrentRecord(0);
            reconciliator.Rewind();
            bool found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(false, found_one);
        }

        [Test]
        public void M_WhenReconciliatingLeftoverRecordsForTheSecondTimeAnyOwnedRecordsMatchedOnThePreviousPassAreNoLongerReturned()
        {
            // Arrange
            var amount_for_matching = 122.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amount_for_matching, Description = "'DIVIDE YOUR GREEN"},
                new ActualBankRecord {Amount = amount_for_matching, Description = "'DIVIDE YOUR GREEN"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord { UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                new BankRecord { UnreconciledAmount = amount_for_matching + 2, Description = "Match02"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            // Replicate what happens when you've finished semi-automated matching and you go back to the beginning for manual matching.
            reconciliator.Rewind();

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.IsTrue(found_one);
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
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
            Assert.AreEqual(5, reconciliator.CurrentRecordForMatching().Matches.Count);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var amount_for_matching = 122.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = "'DIVIDE YOUR GREEN"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {Date = new DateTime(2017,1,6), UnreconciledAmount = amount_for_matching + 100, Description = "4TH NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,5), UnreconciledAmount = amount_for_matching + 100, Description = "2ND NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,6), UnreconciledAmount = amount_for_matching + 100, Description = "1ST NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,2,8), UnreconciledAmount = amount_for_matching + 100, Description = "3RD NEAREST BY DATE"},
                new BankRecord {Date = new DateTime(2017,4,6), UnreconciledAmount = amount_for_matching + 100, Description = "5TH NEAREST BY DATE"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
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
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = "'DIVIDE YOUR GREEN"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching - 40, Description = "5TH NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "2ND NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "1ST NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching - 2, Description = "3RD NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 30, Description = "4TH NEAREST AMOUNT"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
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
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,12,20), Amount = amount_for_matching, Description = "'DIVIDE YOUR GREEN"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {Date = new DateTime(2017,11,20), UnreconciledAmount = amount_for_matching + 5, Description =  "3RD AMOUNT, 5TH MATCH (5)" },
                new BankRecord {Date = new DateTime(2017,12,13), UnreconciledAmount = amount_for_matching + 50, Description = "3RD DATE, 6TH MATCH (7)"   },
                new BankRecord {Date = new DateTime(2018,2,20), UnreconciledAmount = amount_for_matching + 40, Description = "4TH AMOUNT, 8TH MATCH (40)"},
                new BankRecord {Date = new DateTime(2017,12,20), UnreconciledAmount = amount_for_matching + 60, Description = "1ST DATE, 1ST MATCH (0)"   },
                new BankRecord {Date = new DateTime(2017,11,20), UnreconciledAmount = amount_for_matching - 3, Description =  "2ND AMOUNT, 3RD MATCH (3)" },
                new BankRecord {Date = new DateTime(2017,12,24), UnreconciledAmount = amount_for_matching + 60, Description =      "2ND DATE, 4TH MATCH (4)"   },
                new BankRecord {Date = new DateTime(2018,1,20), UnreconciledAmount = amount_for_matching + 50, Description =  "4TH DATE, 7TH MATCH (31)"  },
                new BankRecord {Date = new DateTime(2017,2,20), UnreconciledAmount = amount_for_matching + 1, Description =    "1ST AMOUNT, 2ND MATCH (1)" }};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
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
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = "NEAREST AMOUNT"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "UNMATCHED AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "MATCHED AMOUNT"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, found_one);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == "MATCHED AMOUNT").AmountMatch);
            Assert.AreEqual(false, matches.First(x => x.ActualRecords[0].Description == "UNMATCHED AMOUNT").AmountMatch);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillHaveFullTextMatchValuesPopulated()
        {
            // Arrange
            var amount_for_matching = 22.34;
            var text_to_match = "'DIVIDE YOUR GREEN";
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = text_to_match}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "SOME OTHER TEXT"},
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = text_to_match}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, found_one);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == text_to_match).FullTextMatch);
            Assert.AreEqual(false, matches.First(x => x.ActualRecords[0].Description == "SOME OTHER TEXT").FullTextMatch);
        }

        [Test]
        public void M_UnmatchedOwnedRecordsWillHavePartialTextMatchValuesPopulated()
        {
            // Arrange
            var amount_for_matching = 22.34;
            var text_to_match = "'DIVIDE YOUR GREEN";
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = text_to_match + "EXTRA"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "SOME OTHER TEXT"},
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = text_to_match + "SOME OTHER TEXT"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, found_one);
            Assert.AreEqual(true, matches.First(x => x.ActualRecords[0].Description == text_to_match + "SOME OTHER TEXT").PartialTextMatch);
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
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
            var current_matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(true, current_matches[0].ConsoleLines[0].Index == 0);
            Assert.AreEqual(true, current_matches[1].ConsoleLines[0].Index == 1);
            Assert.AreEqual(true, current_matches[2].ConsoleLines[0].Index == 2);
            Assert.AreEqual(true, current_matches[3].ConsoleLines[0].Index == 3);
            Assert.AreEqual(true, current_matches[4].ConsoleLines[0].Index == 4);
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsIdentifiedAsMatchedTheCorrectRecordGetsMarkedAsMatched()
        {
            // Arrange
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amount_for_matching, Description = "WHATEVER"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            // Note that we can't use amounts to identify records, because amounts get changed by the matching process.
            Assert.AreEqual(true, bank_file.Records.First(x => x.Description.Contains("THE FIRST MATCH")).Matched);
            Assert.AreEqual(false, bank_file.Records.Any(x => x.Description.Contains("NOT MATCHED") && x.Matched));
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItGetsItsAmountChanged()
        {
            // Arrange
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amount_for_matching, Description = "WHATEVER"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.AreEqual(true, bank_file.Records.First(x => x.Description.StartsWith("THE FIRST MATCH")).Matched);
            Assert.AreEqual(amount_for_matching, bank_file.Records.First(x => x.Description.StartsWith("THE FIRST MATCH")).MainAmount());
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItGetsACommentAddedToItsDescription()
        {
            // Arrange
            var match_description = "THE FIRST MATCH";
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amount_for_matching, Description = "WHATEVER"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = match_description},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.MatchCurrentRecord(0);

            // Assert
            Assert.AreEqual(true, bank_file.Records.First(x => x.Description.StartsWith(match_description)).Matched);
            Assert.AreEqual($"{match_description}{ReconConsts.OriginalAmountWas}£{amount_for_matching + 1}", 
                bank_file.Records.First(x => x.Description.StartsWith(match_description)).Description);
        }

        [Test]
        public void M_WhenUnmatchedOwnedRecordIsMarkedAsMatchedItIsNoLongerReturnedInListOfUnmatchedOwnedRecords()
        {
            // Arrange
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = amount_for_matching, Description = "WHATEVER"},
                new ActualBankRecord {Amount = amount_for_matching, Description = "WHATEVER"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "THE FIRST MATCH"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 100, Description = "NOT MATCHED"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MatchCurrentRecord(0);

            // Act
            var found_one = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Assert
            Assert.AreEqual(true, found_one);
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

            var source_matches = reconciliator
                .CurrentRecordForMatching()
                .Matches
                .ToList();
            var current_matches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.IsTrue(current_matches.Count > 1);
            foreach (var match in source_matches)
            {
                var index = source_matches.IndexOf(match);
                Assert.AreEqual(current_matches[index].ActualRecords[0].Description, match.ActualRecords[0].Description);
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

            var matches_stored_internally = reconciliator.CurrentRecordForMatching().Matches;
            var current_matches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.AreEqual(matches_stored_internally.Count, current_matches.Count);
            foreach (var internal_match in matches_stored_internally)
            {
                Assert.IsTrue(current_matches.Exists(match => match.ActualRecords[0].Description == internal_match.ActualRecords[0].Description));
            }
        }

        [Test]
        public void CurrentPotentialMatchesShouldBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/1/2017^£24.99^^TILL^4TH NEAREST BY DATE^^^^^",
                "05/2/2017^£24.99^^TILL^2ND NEAREST BY DATE^^^^^",
                "06/2/2017^£24.99^^TILL^1ST NEAREST BY DATE^^^^^",
                "08/2/2017^£24.99^^TILL^3RD NEAREST BY DATE^^^^^",
                "06/4/2017^£24.99^^TILL^5TH NEAREST BY DATE^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                "06/2/2017,POS,'DIVIDE YOUR GREEN,24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var current_potential_matches = reconciliator.CurrentPotentialMatches();

            // Assert
            Assert.IsTrue(current_potential_matches.Count == 5);
            Assert.AreEqual("1ST NEAREST BY DATE", current_potential_matches[0].ActualRecords[0].Description);
            Assert.AreEqual("2ND NEAREST BY DATE", current_potential_matches[1].ActualRecords[0].Description);
            Assert.AreEqual("3RD NEAREST BY DATE", current_potential_matches[2].ActualRecords[0].Description);
            Assert.AreEqual("4TH NEAREST BY DATE", current_potential_matches[3].ActualRecords[0].Description);
            Assert.AreEqual("5TH NEAREST BY DATE", current_potential_matches[4].ActualRecords[0].Description);
        }

        [Test]
        public void MatchesWillBeOrderedByDatesClosestToOriginal()
        {
            // Arrange
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/1/2017^£24.99^^TILL^4TH NEAREST BY DATE^^^^^",
                "05/2/2017^£24.99^^TILL^2ND NEAREST BY DATE^^^^^",
                "06/2/2017^£24.99^^TILL^1ST NEAREST BY DATE^^^^^",
                "08/2/2017^£24.99^^TILL^3RD NEAREST BY DATE^^^^^",
                "06/4/2017^£24.99^^TILL^5TH NEAREST BY DATE^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                "06/2/2017,POS,'DIVIDE YOUR GREEN,24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

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
            var amount_for_matching = 22.34;
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Date = new DateTime(2017,2,6), Amount = amount_for_matching, Description = "NEAREST AMOUNT"}};
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_lines = new List<BankRecord> {
                new BankRecord {UnreconciledAmount = amount_for_matching - 4, Description = "5TH NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "2ND NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching, Description = "1ST NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching - 2, Description = "3RD NEAREST AMOUNT"},
                new BankRecord {UnreconciledAmount = amount_for_matching + 3, Description = "4TH NEAREST AMOUNT"}};
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentRecordForMatching().Matches;
            Assert.AreEqual(true, found_one);
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
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var original_num_owned_records = reconciliator.OwnedFileRecords().Count;

            // Act
            reconciliator.Finish("testing");

            // Assert
            Assert.AreEqual(original_num_owned_records + 3, reconciliator.OwnedFileRecords().Count);
        }

        [Test]
        public void R_UnmatchedThirdPartyItemsShouldHaveUnmatchedFromThirdPartyPrefixedToTheirDescriptions()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var original_num_owned_records = reconciliator.OwnedFileRecords().Count;

            // Act
            reconciliator.Finish("testing");

            // Assert
            Assert.IsTrue(reconciliator.OwnedFileRecords()[original_num_owned_records].Description.StartsWith("!! Unmatched from 3rd party: "));
            Assert.IsTrue(reconciliator.OwnedFileRecords()[original_num_owned_records + 1].Description.StartsWith("!! Unmatched from 3rd party: "));
            Assert.IsTrue(reconciliator.OwnedFileRecords()[original_num_owned_records + 2].Description.StartsWith("!! Unmatched from 3rd party: "));
        }

        [Test]
        public void R_WhenReconciliatingIfNoUnmatchedThirdPartyRecordsAreLeftThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actual_bank_file.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }

            // Act
            bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, found_one);
        }

        [Test]
        public void R_WhenReconciliatingIfEndOfFileIsReachedThenFalseIsReturned()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);

            for (int count = 1; count <= actual_bank_file.Records.Count; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }

            // Act
            bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, found_one);
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
            bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            Assert.AreEqual(false, found_one);
        }

        [Test]
        public void R_WhenReconciliationFinishesAllMatchedRecordsAreReconciled()
        {
            // Arrange
            _bankInFile.Reload();
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _absoluteCSVFilePath, "ActualBank-Small-PositiveOnly");
            var actual_bank_file = new CSVFile<ActualBankRecord>(file_io);
            actual_bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, _bankInFile, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.Finish("testing");

            // Assert
            foreach (var matched_owned_record in reconciliator.OwnedFileRecords().Where(x => x.Matched))
            {
                Assert.AreEqual(0, matched_owned_record.UnreconciledAmount);
                Assert.IsTrue(matched_owned_record.ReconciledAmount > 0);
            }
        }

        [Test]
        public void IfThereAreMultipleMatchesForAmount_ListTextMatchAtTop()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var description_with_apostrophe = "'" + description;
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{description_with_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matched_record.Matches.Count > 1);
            Assert.AreEqual(description, matched_record.Matches[0].ActualRecords[0].Description);
        }

        [Test]
        public void TextMatchesWillIgnoreLeadingApostrophes()
        {
            // Arrange
            var description_without_apostrophe = "DIVIDE YOUR GREEN";
            var description_with_leading_apostrophe = "'" + description_without_apostrophe;
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                    "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                    $"02/02/2017^£24.99^^TILL^{description_without_apostrophe}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{description_with_leading_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matched_record.Matches.Count > 1);
            Assert.AreEqual(description_without_apostrophe, matched_record.Matches[0].ActualRecords[0].Description);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfRecordContainsOriginalDescription()
        {
            // Arrange
            var description_without_apostrophe = "DIVIDE YOUR GREEN";
            var description_with_leading_apostrophe = "'" + description_without_apostrophe;
            var description_containing_source_description = $"SOME OTHER TEXT {description_without_apostrophe} AROUND CONTAINED DESCRIPTION";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description_containing_source_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{description_with_leading_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(description_containing_source_description, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfRecordExactlyMatchesOriginalDescription()
        {
            // Arrange
            var description = "'DIVIDE YOUR GREEN";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{description},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

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
            var original_description = "'DIVIDE, \"YOUR\": GREEN-GREEN; REALLY.@REALLY";
            var description_without_punctuation = "DIVIDE YOUR GREENGREEN REALLYREALLY";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description_without_punctuation}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{original_description},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(description_without_punctuation, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void RecordWillBeMarkedAsFullTextMatchIfSomePunctuationIsRemoved()
        {
            // Arrange
            var original_description = "'DIVIDE, \"YOUR\": GREEN-GREEN; REALLY.@REALLY";
            var description_with_less_punctuation = "DIVIDE YOUR: GREEN-GREEN REALLYREALLY";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{description_with_less_punctuation}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{original_description},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(description_with_less_punctuation, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void FullTextMatchIsCaseInsensitive()
        {
            // Arrange
            var original_description = "DIVIDE YOUR GreeN";
            var case_insensitive_match = "Divide YouR green";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                "06/03/2017^£24.99^^TILL^SOME OTHER THING^^^^^",
                $"02/02/2017^£24.99^^TILL^{case_insensitive_match}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{original_description},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count(x => x.FullTextMatch));
            Assert.AreEqual(case_insensitive_match, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].FullTextMatch);
        }

        [Test]
        public void PartialTextMatchIsIncludedInResultsEvenIfNothingElseMatches()
        {
            // Arrange
            var original_description = "'DIVIDE YOUR GREEN";
            var partial_match = "Divide some other thing";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£24.99^^TILL^{partial_match}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{original_description},100.00,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(partial_match, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
            Assert.IsFalse(matches[0].FullTextMatch);
            Assert.IsFalse(matches[0].AmountMatch);
        }

        [Test]
        public void PartialAmountMatchIsIncludedInResultsEvenIfNothingElseMatches()
        {
            // Arrange
            var amount_for_matching = 25;
            var partial_amount_match = amount_for_matching + 2;
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{partial_amount_match}^^TILL^OTHER^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,THIS,{amount_for_matching},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(partial_amount_match, matches[0].ActualRecords[0].MainAmount());
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
            var description_with_apostrophe = "'" + description;
            var expected_month = 3;
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/{expected_month - 1}/2017^£24.99^^TILL^{description}^^^^^",
                $"02/{expected_month}/2017^£24.99^^TILL^{description}^^^^^",
                $"02/{expected_month + 1}/2017^£24.99^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/{expected_month}/2017,POS,{description_with_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matched_record.Matches.Count > 1);
            Assert.AreEqual(expected_month, matched_record.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreMultipleAmountAndTextMatches_AndExactDateMatchWhichDoesntMatchText_ThenTopResultIsNearestDateNotExactDate()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var description_with_apostrophe = "'" + description;
            var expected_month = 3;
            var actual_day = 6;
            var closest_matching_day = actual_day + 2;
            var exact_date = $"{actual_day}/{expected_month}/2017";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{exact_date}^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{closest_matching_day}/{expected_month - 1}/2017^£24.99^^TILL^{description}^^^^^",
                $"{closest_matching_day}/{expected_month}/2017^£24.99^^TILL^{description}^^^^^",
                $"{closest_matching_day}/{expected_month + 1}/2017^£24.99^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{exact_date},POS,{description_with_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matched_record.Matches.Count > 1);
            Assert.AreEqual(closest_matching_day, matched_record.Matches[0].ActualRecords[0].Date.Day, "Day");
            Assert.AreEqual(expected_month, matched_record.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreMultipleAmountMatches_AndNoTextMatches_ListNearestDateAtTop()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var description_with_apostrophe = "'" + description;
            var expected_month = 3;
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"06/{expected_month - 1}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/{expected_month}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/{expected_month + 1}/2017^£24.99^^TILL^SOME OTHER DESCRIPTION^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/{expected_month}/2017,POS,{description_with_apostrophe},24.99,4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.IsTrue(matched_record.Matches.Count > 1);
            Assert.AreEqual(expected_month, matched_record.Matches[0].ActualRecords[0].Date.Month, "Month");
        }

        [Test]
        public void IfThereAreNoAmountMatches_FindTextMatchesInstead()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var description_with_apostrophe = "'" + description;
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"06/2/2017^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"06/3/2017^{some_other_amount}^^TILL^{description}^^^^^",
                $"06/4/2017^{some_other_amount}^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/3/2017,POS,{description_with_apostrophe},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(2, matched_record.Matches.Count);
            Assert.AreEqual(description, matched_record.Matches[0].ActualRecords[0].Description);
            Assert.AreEqual(description, matched_record.Matches[1].ActualRecords[0].Description);
        }

        [Test] public void OrderSemiAutoMatches_ByExactAmount_ThenByFullText_ThenByPartialText_ThenByNearestDate_ThenByPartialAmount()
        {
            // Arrange
            var description = "DIVIDE YOUR -Green;";
            var partial_description_match = $"SOMETHING SOMETHING GREENE";
            var description_with_apostrophe = "'" + description;
            var original_amount = 25;
            var original_amount_string = $"{original_amount}.00";
            var partial_amount = original_amount - 2;
            var partial_amount_string = $"{partial_amount}.00";
            var some_other_amount = "66.66";
            var original_month = 3;
            var original_date = $"06/0{original_month}/2017";
            var original_date_minus_one = $"06/0{original_month - 1}/2017";
            var original_date_plus_two = $"06/0{original_month + 2}/2017";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{original_date}^{original_amount_string}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date_plus_two}^{original_amount_string}^^TILL^{description}^^^^^",
                $"{original_date}^{original_amount_string}^^TILL^{description}^^^^^",
                $"{original_date_minus_one}^{original_amount_string}^^TILL^{description}^^^^^",
                $"{original_date_minus_one}^{original_amount_string}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date_plus_two}^{original_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date}^{some_other_amount}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_plus_two}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date}^{original_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_minus_one}^{original_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date}^{partial_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_minus_one}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date_minus_one}^{some_other_amount}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_plus_two}^{some_other_amount}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_plus_two}^{original_amount_string}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date_minus_one}^{some_other_amount}^^TILL^{description}^^^^^",
                $"{original_date_minus_one}^{partial_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_plus_two}^{partial_amount_string}^^TILL^{partial_description_match}^^^^^",
                $"{original_date_plus_two}^{some_other_amount}^^TILL^{description}^^^^^",
                $"{original_date}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date}^{some_other_amount}^^TILL^{description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{original_date},POS,{description_with_apostrophe},{original_amount_string},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var current_potential_matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(18, current_potential_matches.Count);
            Assert.AreEqual($"0. {original_date},£{original_amount_string},{description}", current_potential_matches[0].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"1. {original_date_minus_one},£{original_amount_string},{description}", current_potential_matches[1].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"2. {original_date_plus_two},£{original_amount_string},{description}", current_potential_matches[2].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"3. {original_date},£{original_amount_string},{partial_description_match}", current_potential_matches[3].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"4. {original_date_minus_one},£{original_amount_string},{partial_description_match}", current_potential_matches[4].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"5. {original_date_plus_two},£{original_amount_string},{partial_description_match}", current_potential_matches[5].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"6. {original_date},£{original_amount_string},SOME OTHER DESCRIPTION", current_potential_matches[6].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"7. {original_date_minus_one},£{original_amount_string},SOME OTHER DESCRIPTION", current_potential_matches[7].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"8. {original_date_plus_two},£{original_amount_string},SOME OTHER DESCRIPTION", current_potential_matches[8].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"9. {original_date},£{some_other_amount},{description}", current_potential_matches[9].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"10. {original_date_minus_one},£{some_other_amount},{description}", current_potential_matches[10].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"11. {original_date_plus_two},£{some_other_amount},{description}", current_potential_matches[11].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"12. {original_date},£{partial_amount_string},{partial_description_match}", current_potential_matches[12].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"13. {original_date},£{some_other_amount},{partial_description_match}", current_potential_matches[13].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"14. {original_date_minus_one},£{partial_amount_string},{partial_description_match}", current_potential_matches[14].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"15. {original_date_minus_one},£{some_other_amount},{partial_description_match}", current_potential_matches[15].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"16. {original_date_plus_two},£{partial_amount_string},{partial_description_match}", current_potential_matches[16].ConsoleLines[0].AsTextLine());
            Assert.AreEqual($"17. {original_date_plus_two},£{some_other_amount},{partial_description_match}", current_potential_matches[17].ConsoleLines[0].AsTextLine());
        }

        [Test]
        public void IfThereAreNoAmountOrTextMatches_ThenThereAreNoMatches()
        {
            // Arrange
            var description = "DIVIDE YOUR GREEN";
            var description_with_apostrophe = "'" + description;
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var original_month = 3;
            var original_date = $"06/0{original_month}/2017";
            var original_date_minus_one = $"06/0{original_month - 1}/2017";
            var original_date_plus_two = $"06/0{original_month + 2}/2017";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"{original_date_plus_two}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date_minus_one}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^",
                $"{original_date}^{some_other_amount}^^TILL^SOME OTHER DESCRIPTION^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"{original_date},POS,{description_with_apostrophe},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(null, matched_record);
        }

        [Test]
        public void WillFindPartialMatchIfCandidateDescriptionContainsOneWordFromOriginal()
        {
            // Arrange
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var target_word = "PINTIPOPLICATION";
            var candidate_description = $"SOMETHING {target_word} SOMETHING";
            var source_description = $"DIVIDE {target_word} GREEN";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{source_description},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidate_description, matches[0].ActualRecords[0].Description);
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
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var candidate_description = $"SOMETHING {targetWord} SOMETHING";
            var source_description = $"DIVIDE {targetWord} GREEN";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{source_description},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

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
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var target_word = "GREEN";
            var starts_with_target_word = "GREENE";
            var candidate_description = $"SOMETHING SOMETHING {starts_with_target_word}";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR {target_word},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidate_description, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void WillFindPartialMatchIfCandidateDescriptionContainsWordWhichWasSurroundedWithPunctuationInOriginal()
        {
            // Arrange
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var target_word = "GREEN";
            var candidate_description = $"SOMETHING SOMETHING {target_word}";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{target_word}:,{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidate_description, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void PartialMatchIsNotCaseSensitive()
        {
            // Arrange
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var target_word = "PiNtIPOPliCaTIOn";
            var target_word_to_upper = target_word.ToUpper();
            var candidate_description = $"SOMETHING SOMETHING {target_word_to_upper}";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{target_word}:,{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual(candidate_description, matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void PairedLettersAreOnlyMatchedIfTheyArePresentAsSeparateWords()
        {
            // Arrange
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var target_word = "UK";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^SOMETHING FC{target_word}^^^^^",
                $"02/02/2017^£{some_other_amount}^^TILL^SOMETHING {target_word}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,DIVIDE YOUR '{target_word}:,{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            var matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(1, matches.Count(x => x.PartialTextMatch));
            Assert.AreEqual($"SOMETHING {target_word}", matches[0].ActualRecords[0].Description);
            Assert.IsTrue(matches[0].PartialTextMatch);
        }

        [Test]
        public void SingleLettersAreNotMatchedAsWords()
        {
            // Arrange
            var original_amount = "24.99";
            var some_other_amount = "66.66";
            var original_description = "ZZZOTHERWHERE S S T ZZZOTHERWHERE";
            var candidate_description = "SOMETHING S S T SOMETHING";
            var temp_bank_in_file = CreateCsvFile<BankRecord>(_tempBankInFileName, new string[] {
                $"02/02/2017^£{some_other_amount}^^TILL^{candidate_description}^^^^^"});
            var temp_actual_bank_file = CreateCsvFile<ActualBankRecord>(_tempActualBankFileName, new string[] {
                $"06/03/2017,POS,{original_description},{original_amount},4724.01,'Envelope,'228822-99933422"});
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(temp_actual_bank_file, temp_bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Assert
            RecordForMatching<ActualBankRecord> matched_record = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(null, matched_record);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1 + "something"},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2 + "something"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2.AddDays(2), UnreconciledAmount = amount2, Description = text2 + "extra"},
                    new BankRecord {Date = date1.AddDays(2), UnreconciledAmount = amount1, Description = text1 + "extra"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            foreach (var record_for_matching in auto_matched_items)
            {
                var source_desc = record_for_matching.SourceRecord.Description;
                var match_desc = record_for_matching.Match.ActualRecords[0].Description;
                Assert.IsTrue(reconciliator.CheckForPartialTextMatch(source_desc, match_desc), 
                    "source and match should have partially matching descriptions");
                Assert.AreEqual(record_for_matching.SourceRecord.MainAmount(),
                    record_for_matching.Match.ActualRecords[0].MainAmount(),
                    "source and match should have matching amounts");
                Assert.IsTrue(record_for_matching.Match.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold, 
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
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>().Object;
            var mock_bank_file = new Mock<ICSVFile<BankRecord>>().Object;
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(mock_actual_bank_file, mock_bank_file, ThirdPartyFileLoadAction.NoAction);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = "boppl"},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = "bazzl"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = text2},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(2, auto_matched_items.Count, "should be correct number of auto-matching results");
            foreach (var record_for_matching in auto_matched_items)
            {
                Assert.IsNotNull(record_for_matching.Match, "every record should have a match");
            }
        }

        [Test]
        public void M_WhenAutoMatching_MatchesAreConnectedToEachOther()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(auto_matched_items[0].Match.ActualRecords[0], auto_matched_items[0].SourceRecord.Match);
            Assert.AreEqual(auto_matched_items[0].SourceRecord, auto_matched_items[0].Match.ActualRecords[0].Match);
        }

        [Test]
        public void M_WhenAutoMatching_SourceAndMatch_ShouldBeMarkedAsMatched()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.IsTrue(auto_matched_items[0].SourceRecord.Matched);
            AssertMatchIsMatched(auto_matched_items[0].Match);
        }

        [Test]
        public void M_WhenAutoMatching_NoOwnedRecordIsMatchedTwice()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(1, auto_matched_items.Count);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = "bazzl"},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(1, auto_matched_items.Count);
            Assert.AreEqual(text1, auto_matched_items[0].SourceRecord.Description);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(date1, auto_matched_items[0].SourceRecord.Date);
            Assert.AreEqual(date2, auto_matched_items[1].SourceRecord.Date);
            Assert.AreEqual(date3, auto_matched_items[2].SourceRecord.Date);
        }

        [Test]
        public void M_WhenAutoMatching_MatchWillOnlyHappen_IfThereIsExactlyOneMatch()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(0, auto_matched_items.Count);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date2, Amount = amount2, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount3, Description = text3},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount2, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount3, Description = text3}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Assert
            Assert.AreEqual(0, auto_matched_items[0].Index);
            Assert.AreEqual(1, auto_matched_items[1].Index);
            Assert.AreEqual(2, auto_matched_items[2].Index);
        }

        [Test]
        public void M_CanRetrieveAutoMatchesWithoutReDoingAutoMatching()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            List<AutoMatchedRecord<ActualBankRecord>> auto_matched_items = reconciliator.ReturnAutoMatches();

            // Act
            var refetched_auto_matched_items = reconciliator.GetAutoMatches();

            // Assert
            Assert.AreEqual(auto_matched_items, refetched_auto_matched_items);
        }

        [Test]
        public void M_AutoMatchesAreNotPopulatedUnlessDoAutoMatchingIsCalled()
        {
            // Arrange
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var auto_matched_items = reconciliator.GetAutoMatches();

            // Assert
            Assert.AreEqual(null, auto_matched_items);
        }

        [Test]
        public void M_AfterAutoMatchingIsFinished_AutoMatchedItemsAreNotAvailableForMatching()
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "SOMETHING ELSE";
            var partial_match = "PINTIPOPLICATION";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = partial_match + "SOMETHING ELSE"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = partial_match + "other"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            reconciliator.ReturnAutoMatches();

            // Assert
            bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.IsTrue(found_one, "can find new semi-auto match");
            var current_record_for_matching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(partial_match + "SOMETHING ELSE", current_record_for_matching.SourceRecord.Description);
            var current_potential_matches = reconciliator.CurrentPotentialMatches();
            Assert.AreEqual(partial_match + "other", current_potential_matches[0].ActualRecords[0].Description);
            found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.IsFalse(found_one, "only found one new semi-auto match");
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            const int matchIndex = 1;
            const int numRecordsAtStart = 3;
            // The where clause is redundant, but it's here for parity with the reassignment of autoMatchedItems below.
            var auto_matched_items = reconciliator.ReturnAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, auto_matched_items.Count);
            Assert.AreEqual(matchIndex, auto_matched_items[1].Index);
            Assert.AreEqual(text2, auto_matched_items[1].SourceRecord.Description);
            var original_match = auto_matched_items[1].Match;

            // Act
            reconciliator.RemoveAutoMatch(matchIndex);

            // Assert
            auto_matched_items = reconciliator.GetAutoMatches();
            var filtered_auto_matched_items = reconciliator.GetAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - 1, filtered_auto_matched_items.Count);
            Assert.IsFalse(filtered_auto_matched_items.Any(x => x.SourceRecord.Description == text2));
            Assert.IsFalse(auto_matched_items[matchIndex].SourceRecord.Matched);
            AssertMatchIsNoLongerMatched(original_match);
            Assert.IsNull(auto_matched_items[matchIndex].Match);
            Assert.IsNull(auto_matched_items[matchIndex].SourceRecord.Match);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            // The where clause is redundant, but it's here for parity with the reassignment of autoMatchedItems below.
            var auto_matched_items = reconciliator.ReturnAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, auto_matched_items.Count);
            List<IPotentialMatch> original_matches = new List<IPotentialMatch>();
            List<int> match_indices = new List<int> { 1, 2, 3 };
            foreach (var match_index in match_indices)
            {
                original_matches.Add(auto_matched_items[match_index].Match);
            }

            // Act
            reconciliator.RemoveMultipleAutoMatches(match_indices);

            // Assert
            auto_matched_items = reconciliator.GetAutoMatches();
            var filtered_auto_matched_items = reconciliator.GetAutoMatches().Where(x => x.SourceRecord.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - match_indices.Count, filtered_auto_matched_items.Count);
            Assert.IsFalse(filtered_auto_matched_items.Any(x => x.SourceRecord.Description == text2));
            Assert.IsFalse(filtered_auto_matched_items.Any(x => x.SourceRecord.Description == text3));
            Assert.IsFalse(filtered_auto_matched_items.Any(x => x.SourceRecord.Description == text4));
            for (int count = 0; count < match_indices.Count; count++)
            {
                AssertSourceRecordIsNotMatched(auto_matched_items[match_indices[count]]);
                AssertMatchIsNoLongerMatched(original_matches[count]);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            var auto_matched_items = reconciliator.ReturnAutoMatches();
            List<IPotentialMatch> original_matches = new List<IPotentialMatch>();
            List<int> match_indices = new List<int> { 1, 2, 3 };
            foreach (var match_index in match_indices)
            {
                original_matches.Add(auto_matched_items[match_index].Match);
            }

            // Act
            reconciliator.RemoveMultipleAutoMatches(match_indices);

            // Assert
            for (int count = 0; count < match_indices.Count; count++)
            {
                bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                Assert.IsTrue(found_one, "can find new semi-auto match");
                var current_record_for_matching = reconciliator.CurrentRecordForMatching();
                Assert.AreEqual(auto_matched_items[match_indices[count]].SourceRecord, 
                    current_record_for_matching.SourceRecord, 
                    "3rd party record is available again");
                Assert.AreEqual(original_matches[count].ActualRecords[0].Description, 
                    current_record_for_matching.Matches[0].ActualRecords[0].Description, 
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date2, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date3, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date2, UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date3, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            const int matchIndex = 1;
            const int numRecordsAtStart = 3;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matched_items = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matched_items.Count);
            Assert.AreEqual(text2, matched_items[1].Description);
            var original_match = matched_items[1].Match;

            // Act
            reconciliator.RemoveFinalMatch(matchIndex);

            // Assert
            matched_items = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - 1, matched_items.Count);
            Assert.IsFalse(matched_items.Any(x => x.Description == text2));
            Assert.IsFalse(reconciliator.ThirdPartyRecords()[matchIndex].Matched);
            Assert.IsFalse(original_match.Matched);
            Assert.IsNull(reconciliator.ThirdPartyRecords()[matchIndex].Match);
            Assert.IsNull(original_match.Match);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matched_items = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matched_items.Count);
            List<ICSVRecord> original_matches = new List<ICSVRecord>();
            List<int> match_indices = new List<int> { 1, 2, 3 };
            foreach (var match_index in match_indices)
            {
                original_matches.Add(matched_items[match_index].Match);
            }

            // Act
            reconciliator.RemoveMultipleFinalMatches(match_indices);

            // Assert
            matched_items = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart - match_indices.Count, matched_items.Count);
            Assert.IsFalse(matched_items.Any(x => x.Description == text2));
            Assert.IsFalse(matched_items.Any(x => x.Description == text3));
            Assert.IsFalse(matched_items.Any(x => x.Description == text4));
            for (int count = 0; count < match_indices.Count; count++)
            {
                AssertRecordIsNoLongerMatched(reconciliator.ThirdPartyRecords()[match_indices[count]]);
                AssertRecordIsNoLongerMatched(original_matches[count]);
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1.AddDays(10), Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1.AddDays(20), Amount = amount1, Description = text3},
                    new ActualBankRecord {Date = date1.AddDays(30), Amount = amount1, Description = text4},
                    new ActualBankRecord {Date = date1.AddDays(40), Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1.AddDays(10), UnreconciledAmount = amount1, Description = text2},
                    new BankRecord {Date = date1.AddDays(20), UnreconciledAmount = amount1, Description = text3},
                    new BankRecord {Date = date1.AddDays(30), UnreconciledAmount = amount1, Description = text4},
                    new BankRecord {Date = date1.AddDays(40), UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            const int numRecordsAtStart = 5;
            for (int count = 1; count <= numRecordsAtStart; count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                reconciliator.MatchCurrentRecord(0);
            }
            List<ActualBankRecord> matched_items = reconciliator.ThirdPartyRecords().Where(x => x.Matched).ToList();
            Assert.AreEqual(numRecordsAtStart, matched_items.Count);
            List<ICSVRecord> original_matches = new List<ICSVRecord>();
            List<int> match_indices = new List<int> { 1, 2, 3 };
            foreach (var match_index in match_indices)
            {
                original_matches.Add(matched_items[match_index].Match);
            }

            // Act
            reconciliator.RemoveMultipleFinalMatches(match_indices);

            // Assert
            reconciliator.Rewind();
            for (int count = 0; count < match_indices.Count; count++)
            {
                bool found_one = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
                Assert.IsTrue(found_one, "can find new semi-auto match");
                var current_record_for_matching = reconciliator.CurrentRecordForMatching();
                Assert.AreEqual(reconciliator.ThirdPartyRecords()[match_indices[count]], 
                    current_record_for_matching.SourceRecord, 
                    "3rd party record is available again");
                Assert.AreEqual(original_matches[count].Description, 
                    current_record_for_matching.Matches[0].ActualRecords[0].Description, 
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
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text3}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = "nonsense"},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text3}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            var num_matches = 2;

            // Act
            var console_lines = reconciliator.GetAutoMatchesForConsole();

            // Assert
            Assert.AreEqual(num_matches * 3, console_lines.Count);
            var separator_lines = console_lines.Where(x => x.DateString == "----------");
            Assert.AreEqual(num_matches, separator_lines.Count(), "output should contain separator lines");
            var text1_lines = console_lines.Where(x => x.DescriptionString == text1);
            Assert.IsTrue(text1_lines.All(x => x.Index == 0), "text1 lines should be indexed correctly");
            Assert.AreEqual(2, text1_lines.Count(), "text1: one line for third party record and one line for match");
            var text3_lines = console_lines.Where(x => x.DescriptionString == text3);
            Assert.IsTrue(text3_lines.All(x => x.Index == 1), "text3 lines should be indexed correctly");
            Assert.AreEqual(2, text3_lines.Count(), "text3: one line for third party record and one line for match");
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
            var mock_cred_card2_file_io = new Mock<IFileIO<CredCard2Record>>();
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2Record> {
                    new CredCard2Record {Date = date1, Amount = amount1, Description = text1},
                    new CredCard2Record {Date = date1.AddDays(1), Amount = amount1, Description = text2},
                    new CredCard2Record {Date = date1.AddDays(2), Amount = amount1, Description = text3},
                    new CredCard2Record {Date = date1.AddDays(3), Amount = amount1, Description = text4}
                });
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new CredCard2InOutRecord {Date = date1.AddDays(1), UnreconciledAmount = amount1 + 10, Description = "nonsense"},
                    new CredCard2InOutRecord {Date = date1.AddDays(2), UnreconciledAmount = amount1 + 10, Description = text3},
                    new CredCard2InOutRecord {Date = date1.AddDays(3), UnreconciledAmount = amount1, Description = text4}
                });
            var cred_card2_file = new CSVFile<CredCard2Record>(mock_cred_card2_file_io.Object);
            cred_card2_file.Load();
            var cred_card2_in_out_file = new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object);
            cred_card2_in_out_file.Load();
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(cred_card2_file, cred_card2_in_out_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            var num_matches = 3;

            // Act
            var console_lines = reconciliator.GetFinalMatchesForConsole();

            // Assert
            Assert.AreEqual(num_matches * 3, console_lines.Count);
            var separator_lines = console_lines.Where(x => x.DateString == "----------");
            Assert.AreEqual(num_matches, separator_lines.Count(), "output should contain separator lines");
            var text1_lines = console_lines.Where(x => x.DescriptionString.Contains(text1));
            Assert.IsTrue(text1_lines.All(x => x.Index == 0), "text1 lines should be indexed correctly");
            Assert.AreEqual(2, text1_lines.Count(), "text1: one line for third party record and one line for match");
            var text3_lines = console_lines.Where(x => x.DescriptionString.Contains(text3));
            Assert.IsTrue(text3_lines.All(x => x.Index == 2), "text3 lines should be indexed correctly");
            Assert.AreEqual(2, text3_lines.Count(), "text3: one line for third party record and one line for match");
            var text4_lines = console_lines.Where(x => x.DescriptionString.Contains(text4));
            Assert.IsTrue(text4_lines.All(x => x.Index == 3), "text4 lines should be indexed correctly");
            Assert.AreEqual(2, text4_lines.Count(), "text4: one line for third party record and one line for match");
        }

        [TestCase(TransactionMatchType.Auto)]
        [TestCase(TransactionMatchType.Final)]
        public void M_WhenAttemptingToRemoveMatchWithBadIndex_ErrorIsThrown(TransactionMatchType transactionMatchType)
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            bool exception_thrown = false;
            string error_message = "";

            // Act
            try
            {
                if (transactionMatchType == TransactionMatchType.Auto)
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
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, error_message);
        }

        [TestCase(TransactionMatchType.Auto)]
        [TestCase(TransactionMatchType.Final)]
        public void M_WhenAttemptingToRemoveMultipleMatchesWithBadIndices_ErrorIsThrown(TransactionMatchType transactionMatchType)
        {
            // Arrange
            var amount1 = 22.23;
            var text1 = "'DIVIDE YOUR GREEN";
            var text2 = "haha bonk";
            var date1 = new DateTime(2018, 10, 31);
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text1},
                    new ActualBankRecord {Date = date1, Amount = amount1, Description = text2}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text1},
                    new BankRecord {Date = date1, UnreconciledAmount = amount1, Description = text2}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_in_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_in_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.ReturnAutoMatches();
            bool exception_thrown = false;
            string error_message = "";

            // Act
            try
            {
                if (transactionMatchType == TransactionMatchType.Auto)
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
                exception_thrown = true;
                error_message = exception.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber, error_message);
        }
    }
}
