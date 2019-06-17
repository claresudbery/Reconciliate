using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    [TestFixture]
    public class ReconciliatorDeletionTests
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void M_CanDeleteCurrentThirdPartyRecord(int indexOfDeletedRecord)
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            Assert.IsTrue(indexOfDeletedRecord < actual_bank_file.Records.Count);
            var description_of_deleted_record = actual_bank_file.Records[indexOfDeletedRecord].Description;
            for (int record_count = 0; record_count <= indexOfDeletedRecord; record_count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            var previous_num_third_party_records = actual_bank_file.Records.Count;
            var previous_num_owned_records = bank_file.Records.Count;

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.AreEqual(previous_num_third_party_records - 1, actual_bank_file.Records.Count);
            Assert.AreEqual(previous_num_owned_records, bank_file.Records.Count);
            Assert.AreEqual(0, actual_bank_file.Records.Count(x => x.Description == description_of_deleted_record));
        }

        [Test]
        public void M_GivenNoMatchingHasBeenDoneYet_DeletingCurrentThirdPartyRecord_ResultsInAnError()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var exception_thrown = false;
            var exception_message = String.Empty;
            try
            {
                reconciliator.DeleteCurrentThirdPartyRecord();
            }
            catch (Exception e)
            {
                exception_thrown = true;
                exception_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteCurrentRecordNoMatching, exception_message);
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_CurrentRecordForMatchingIsCleared()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_CurrentIndexIsRewoundSoThatNextRecordForSemiAutoMatchingWillBeNextRecord
            (int deletedRecordIndex)
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            for (int record_count = 0; record_count <= deletedRecordIndex; record_count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description, 
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var records_still_available_for_matching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var current_record_for_matching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, records_still_available_for_matching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description, 
                current_record_for_matching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bank_file.Records[deletedRecordIndex + 1].Description, 
                current_record_for_matching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_CurrentIndexIsRewoundSoThatNextRecordForManualMatchingWillBeNextRecord
            (int deletedRecordIndex)
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            for (int record_count = 0; record_count <= deletedRecordIndex; record_count++)
            {
                reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            }
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description,
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var records_still_available_for_matching = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var current_record_for_matching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, records_still_available_for_matching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description,
                current_record_for_matching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bank_file.Records[0].Description,
                current_record_for_matching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var records_still_available_for_matching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(false, records_still_available_for_matching);
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForManualMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var records_still_available_for_matching = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual(false, records_still_available_for_matching);
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenCurrentRecordThatHasBeenMatchedIsDeleted_ThenMatchedRecordGetsUnmatched()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            Assert.IsTrue(bank_file.Records[0].Matched);
            Assert.IsNotNull(bank_file.Records[0].Match);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.IsFalse(bank_file.Records[0].Matched);
            Assert.IsNull(bank_file.Records[0].Match);
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeleted_ThenYouCanStillAskForSemiAutoMatches()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.IsFalse(reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeleted_ThenYouCanStillAskForManualMatches()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.IsFalse(reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
        }

        [Test]
        public void M_CanDeleteSpecificThirdPartyRecord()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            int index_of_deleted_record = 1;
            var description_of_deleted_record = actual_bank_file.Records[index_of_deleted_record].Description;
            var previous_num_third_party_records = actual_bank_file.Records.Count;
            var previous_num_owned_records = bank_file.Records.Count;

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(index_of_deleted_record);

            // Assert
            Assert.AreEqual(previous_num_third_party_records - 1, actual_bank_file.Records.Count);
            Assert.AreEqual(previous_num_owned_records, bank_file.Records.Count);
            Assert.AreEqual(0, actual_bank_file.Records.Count(x => x.Description == description_of_deleted_record));
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentRecordForMatchingIsCleared()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(0);

            // Assert
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentRecordForMatchingIsNotCleared()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(1);

            // Assert
            Assert.IsNotNull(reconciliator.CurrentRecordForMatching());
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void M_WhenCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentIndexIsRewoundSoThatNextRecordForSemiAutoMatchingWillBeNextRecord
            (int deletedRecordIndex)
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            for (int record_count = 0; record_count <= deletedRecordIndex; record_count++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description,
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(deletedRecordIndex);

            // Assert
            var records_still_available_for_matching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var current_record_for_matching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, records_still_available_for_matching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actual_bank_file.Records[deletedRecordIndex].Description,
                current_record_for_matching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bank_file.Records[deletedRecordIndex + 1].Description,
                current_record_for_matching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        public void M_WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentIndexIsNotRewound_ForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(1);

            // Assert
            Assert.IsFalse(reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
        }

        [Test]
        public void M_WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentIndexIsNotRewound_ForManualMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(1);

            // Assert
            Assert.IsFalse(reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeletedViaSpecificDelete_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(false, reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_AndItIsLastRecord_CurrentIndexIsNotRewoundForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(true, reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
            Assert.IsNotNull(reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenSpecificThirdPartyRecordIsDeleted_AndItIsLastRecord_AndCurrentRecordIsJustBefore_NoMoreSemiAutoMatchesAreFound()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(false, reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeletedViaSpecificDelete_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForManualMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(false, reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_AndItIsLastRecord_CurrentIndexIsNotRewoundForManualMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(true, reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
            Assert.IsNotNull(reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenSpecificThirdPartyRecordIsDeleted_AndItIsLastRecord_AndCurrentRecordIsJustBefore_NoMoreManualMatchesAreFound()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amount_for_matching + 2, Description = "Record03"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching + 2, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(2);

            // Assert
            Assert.AreEqual(false, reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
        }

        [Test]
        public void M_WhenRecordThatHasBeenMatchedIsDeletedViaSpecificDelete_ThenMatchedRecordGetsUnmatched()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            Assert.IsTrue(bank_file.Records[0].Matched);
            Assert.IsNotNull(bank_file.Records[0].Match);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(0);

            // Assert
            Assert.IsFalse(bank_file.Records[0].Matched);
            Assert.IsNull(bank_file.Records[0].Match);
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeletedViaSpecificDelete_ThenYouCanStillAskForSemiAutoMatches()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(0);

            // Assert
            Assert.IsFalse(reconciliator.FindReconciliationMatchesForNextThirdPartyRecord());
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeletedViaSpecificDelete_ThenYouCanStillAskForManualMatches()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(0);

            // Assert
            Assert.IsFalse(reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching());
        }

        [Test]
        public void M_WhenSpecificDeleteIsUsedOnNonExistentRecordThenErrorIsThrown()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);

            // Act
            var exception_thrown = false;
            var exception_message = String.Empty;
            try
            {
                reconciliator.DeleteSpecificThirdPartyRecord(1);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                exception_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteThirdPartyRecordDoesNotExist, exception_message);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void M_CanDeleteSpecifiedOwnedRecordAfterSemiAutoMatching(int indexOfDeletedRecord)
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            Assert.IsTrue(indexOfDeletedRecord < bank_file.Records.Count);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var description_of_deleted_record = reconciliator.CurrentRecordForMatching().Matches[indexOfDeletedRecord].ActualRecords[0].Description;
            var previous_num_third_party_records = actual_bank_file.Records.Count;
            var previous_num_owned_records = bank_file.Records.Count;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(previous_num_third_party_records, actual_bank_file.Records.Count);
            Assert.AreEqual(previous_num_owned_records - 1, bank_file.Records.Count);
            Assert.AreEqual(0, bank_file.Records.Count(x => x.Description == description_of_deleted_record));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void M_CanDeleteSpecifiedOwnedRecordAfterManualMatching(int indexOfDeletedRecord)
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            Assert.IsTrue(indexOfDeletedRecord < bank_file.Records.Count);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var description_of_deleted_record = reconciliator.CurrentRecordForMatching().Matches[indexOfDeletedRecord].ActualRecords[0].Description;
            var previous_num_third_party_records = actual_bank_file.Records.Count;
            var previous_num_owned_records = bank_file.Records.Count;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(previous_num_third_party_records, actual_bank_file.Records.Count);
            Assert.AreEqual(previous_num_owned_records - 1, bank_file.Records.Count);
            Assert.AreEqual(0, bank_file.Records.Count(x => x.Description == description_of_deleted_record));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenSpecifiedOwnedRecordIsDeleted_MatchesAreRenumbered(bool manualMatching)
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            if (manualMatching)
            {
                reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            }
            else
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            var index_of_deleted_record = 1;

            // Act
            var original_matches = reconciliator.CurrentPotentialMatches();
            var original_index_of_last_record = original_matches[original_matches.Count - 1].ConsoleLines[0].Index;
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);
            var newly_indexed_records = reconciliator.CurrentPotentialMatches();
            var new_index_of_last_record = original_matches[newly_indexed_records.Count - 1].ConsoleLines[0].Index;

            // Assert
            Assert.AreEqual(original_index_of_last_record - 1, new_index_of_last_record);
        }

        [Test]
        public void M_DeletingOwnedRecordHasNoEffectOnOtherOwnedRecordsForSemiAutoMatching()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var index_of_deleted_record = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_DeletingOwnedRecordHasNoEffectOnOtherOwnedRecordsForManualMatching()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var index_of_deleted_record = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match01"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match01"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
            Assert.AreEqual(1, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromSemiAutoMatchList_ItIsNoLongerInThatList()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var index_of_deleted_record = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromManualMatchList_ItIsNoLongerInThatList()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var index_of_deleted_record = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromSemiAutoMatchList_ItIsNoLongerAvailableForMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var index_of_deleted_record = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromManualMatchList_ItIsNoLongerAvailableForMatching()
        {//xxxxxx
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var index_of_deleted_record = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);

            // Assert
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
        }

        [Test]
        public void M_WhenYouTryToDeleteAMatchedOwnedRecord_YouGetAnError()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var index_of_deleted_record = 1;
            reconciliator.MatchCurrentRecord(index_of_deleted_record);

            // Act
            var exception_thrown = false;
            var exception_message = String.Empty;
            try
            {
                reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                exception_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteMatchedOwnedRecord, exception_message);
        }

        [Test]
        public void M_WhenYouTryToDeleteAMatchedOwnedRecord_WithANonExistentIndex_YouGetAnError()
        {
            // Arrange
            var amount_for_matching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record01"},
                    new ActualBankRecord {Amount = amount_for_matching, Description = "Record02"}
                });
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amount_for_matching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amount_for_matching, Description = "Match04"}
                });
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            actual_bank_file.Load();
            var bank_file = new CSVFile<BankRecord>(mock_bank_file_io.Object);
            bank_file.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actual_bank_file, bank_file, ThirdPartyFileLoadAction.NoAction);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var index_of_deleted_record = 10;

            // Act
            var exception_thrown = false;
            var exception_message = String.Empty;
            try
            {
                reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(index_of_deleted_record);
            }
            catch (Exception e)
            {
                exception_thrown = true;
                exception_message = e.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteOwnedRecordDoesNotExist, exception_message);
        }
    }
}
