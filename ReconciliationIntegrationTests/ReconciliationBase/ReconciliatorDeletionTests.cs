using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ReconciliationIntegrationTests.ReconciliationBase
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            Assert.IsTrue(indexOfDeletedRecord < actualBankFile.Records.Count);
            var descriptionOfDeletedRecord = actualBankFile.Records[indexOfDeletedRecord].Description;
            for (int recordCount = 0; recordCount <= indexOfDeletedRecord; recordCount++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            var previousNumThirdPartyRecords = actualBankFile.Records.Count;
            var previousNumOwnedRecords = bankFile.Records.Count;

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords - 1, actualBankFile.Records.Count);
            Assert.AreEqual(previousNumOwnedRecords, bankFile.Records.Count);
            Assert.AreEqual(0, actualBankFile.Records.Count(x => x.Description == descriptionOfDeletedRecord));
        }

        [Test]
        public void M_GivenNoMatchingHasBeenDoneYet_DeletingCurrentThirdPartyRecord_ResultsInAnError()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);

            // Act
            var exceptionThrown = false;
            var exceptionMessage = String.Empty;
            try
            {
                reconciliator.DeleteCurrentThirdPartyRecord();
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                exceptionMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteCurrentRecordNoMatching, exceptionMessage);
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_CurrentRecordForMatchingIsCleared()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            for (int recordCount = 0; recordCount <= deletedRecordIndex; recordCount++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description, 
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var recordsStillAvailableForMatching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, recordsStillAvailableForMatching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description, 
                currentRecordForMatching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bankFile.Records[deletedRecordIndex + 1].Description, 
                currentRecordForMatching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_CurrentIndexIsRewoundSoThatNextRecordForManualMatchingWillBeNextRecord
            (int deletedRecordIndex)
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            for (int recordCount = 0; recordCount <= deletedRecordIndex; recordCount++)
            {
                reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            }
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description,
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var recordsStillAvailableForMatching = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, recordsStillAvailableForMatching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description,
                currentRecordForMatching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bankFile.Records[0].Description,
                currentRecordForMatching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var recordsStillAvailableForMatching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(false, recordsStillAvailableForMatching);
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_AndItIsLastRecord_CurrentIndexIsRewoundAndThereAreNoMoreRecordsForManualMatching()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual("Record03", reconciliator.CurrentRecordForMatching().SourceRecord.Description);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            var recordsStillAvailableForMatching = reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual(false, recordsStillAvailableForMatching);
            Assert.AreEqual(null, reconciliator.CurrentRecordForMatching());
        }

        [Test]
        public void M_WhenCurrentRecordThatHasBeenMatchedIsDeleted_ThenMatchedRecordGetsUnmatched()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            Assert.IsTrue(bankFile.Records[0].Matched);
            Assert.IsNotNull(bankFile.Records[0].Match);

            // Act
            reconciliator.DeleteCurrentThirdPartyRecord();

            // Assert
            Assert.IsFalse(bankFile.Records[0].Matched);
            Assert.IsNull(bankFile.Records[0].Match);
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeleted_ThenYouCanStillAskForSemiAutoMatches()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            int indexOfDeletedRecord = 1;
            var descriptionOfDeletedRecord = actualBankFile.Records[indexOfDeletedRecord].Description;
            var previousNumThirdPartyRecords = actualBankFile.Records.Count;
            var previousNumOwnedRecords = bankFile.Records.Count;

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords - 1, actualBankFile.Records.Count);
            Assert.AreEqual(previousNumOwnedRecords, bankFile.Records.Count);
            Assert.AreEqual(0, actualBankFile.Records.Count(x => x.Description == descriptionOfDeletedRecord));
        }

        [Test]
        public void M_WhenCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentRecordForMatchingIsCleared()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            for (int recordCount = 0; recordCount <= deletedRecordIndex; recordCount++)
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description,
                reconciliator.CurrentRecordForMatching().SourceRecord.Description,
                "CurrentRecordForMatching - Description before record deleted");

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(deletedRecordIndex);

            // Assert
            var recordsStillAvailableForMatching = reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var currentRecordForMatching = reconciliator.CurrentRecordForMatching();
            Assert.AreEqual(true, recordsStillAvailableForMatching, "recordsStillAvailableForMatching");
            // Note that because a record has been deleted, Records[deletedRecordIndex] is now the record AFTER the deleted record.
            Assert.AreEqual(actualBankFile.Records[deletedRecordIndex].Description,
                currentRecordForMatching.SourceRecord.Description,
                "CurrentRecordForMatching - Description after record deleted");
            Assert.AreEqual(bankFile.Records[deletedRecordIndex + 1].Description,
                currentRecordForMatching.Matches[0].ActualRecords[0].Description,
                "First match after record deleted");
        }

        [Test]
        public void M_WhenNonCurrentThirdPartyRecordIsDeletedViaSpecificDelete_CurrentIndexIsNotRewound_ForSemiAutoMatching()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching + 1, Description = "Record02"},
                    new ActualBankRecord {Amount = amountForMatching + 2, Description = "Record03"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching - 1, Description = "Matchxx"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching + 2, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            Assert.IsTrue(bankFile.Records[0].Matched);
            Assert.IsNotNull(bankFile.Records[0].Match);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            // Act
            reconciliator.DeleteSpecificThirdPartyRecord(0);

            // Assert
            Assert.IsFalse(bankFile.Records[0].Matched);
            Assert.IsNull(bankFile.Records[0].Match);
        }

        [Test]
        public void M_WhenTheOnlyRecordIsDeletedViaSpecificDelete_ThenYouCanStillAskForSemiAutoMatches()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);

            // Act
            var exceptionThrown = false;
            var exceptionMessage = String.Empty;
            try
            {
                reconciliator.DeleteSpecificThirdPartyRecord(1);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                exceptionMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteThirdPartyRecordDoesNotExist, exceptionMessage);
        }

        [Test]
        public void M_CanDeleteMultipleOwnedRecordsAfterSemiAutoMatching()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            var bankRecords = new List<BankRecord>
            {
                new BankRecord {Description = "Match01"},
                new BankRecord {Description = "Match02"},
                new BankRecord {Description = "Match03"},
                new BankRecord {Description = "Match04"}
            };
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bankRecords);
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.SetMatchFinder((record, file) => new List<PotentialMatch>{new PotentialMatch
            {
                ActualRecords = new List<ICSVRecord> { bankRecords[0], bankRecords[2] }
            }});
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var previousNumThirdPartyRecords = actualBankFile.Records.Count;
            var previousNumOwnedRecords = bankFile.Records.Count;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(0);

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords, actualBankFile.Records.Count);
            Assert.AreEqual(previousNumOwnedRecords - 2, bankFile.Records.Count);
            Assert.AreEqual(0, bankFile.Records.Count(x => x.Description == bankRecords[0].Description));
            Assert.AreEqual(0, bankFile.Records.Count(x => x.Description == bankRecords[2].Description));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void M_CanDeleteSpecifiedOwnedRecordAfterSemiAutoMatching(int indexOfDeletedRecord)
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            Assert.IsTrue(indexOfDeletedRecord < bankFile.Records.Count);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var descriptionOfDeletedRecord = reconciliator.CurrentRecordForMatching().Matches[indexOfDeletedRecord].ActualRecords[0].Description;
            var previousNumThirdPartyRecords = actualBankFile.Records.Count;
            var previousNumOwnedRecords = bankFile.Records.Count;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords, actualBankFile.Records.Count);
            Assert.AreEqual(previousNumOwnedRecords - 1, bankFile.Records.Count);
            Assert.AreEqual(0, bankFile.Records.Count(x => x.Description == descriptionOfDeletedRecord));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void M_CanDeleteSpecifiedOwnedRecordAfterManualMatching(int indexOfDeletedRecord)
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            Assert.IsTrue(indexOfDeletedRecord < bankFile.Records.Count);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var descriptionOfDeletedRecord = reconciliator.CurrentRecordForMatching().Matches[indexOfDeletedRecord].ActualRecords[0].Description;
            var previousNumThirdPartyRecords = actualBankFile.Records.Count;
            var previousNumOwnedRecords = bankFile.Records.Count;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(previousNumThirdPartyRecords, actualBankFile.Records.Count);
            Assert.AreEqual(previousNumOwnedRecords - 1, bankFile.Records.Count);
            Assert.AreEqual(0, bankFile.Records.Count(x => x.Description == descriptionOfDeletedRecord));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenSpecifiedOwnedRecordIsDeleted_MatchesAreRenumbered(bool manualMatching)
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            if (manualMatching)
            {
                reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            }
            else
            {
                reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
            var indexOfDeletedRecord = 1;

            // Act
            var originalMatches = reconciliator.CurrentPotentialMatches();
            var originalIndexOfLastRecord = originalMatches[originalMatches.Count - 1].ConsoleLines[0].Index;
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);
            var newlyIndexedRecords = reconciliator.CurrentPotentialMatches();
            var newIndexOfLastRecord = originalMatches[newlyIndexedRecords.Count - 1].ConsoleLines[0].Index;

            // Assert
            Assert.AreEqual(originalIndexOfLastRecord - 1, newIndexOfLastRecord);
        }

        [Test]
        public void M_DeletingOwnedRecordHasNoEffectOnOtherOwnedRecordsForSemiAutoMatching()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var indexOfDeletedRecord = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var indexOfDeletedRecord = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

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
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var indexOfDeletedRecord = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromManualMatchList_ItIsNoLongerInThatList()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var indexOfDeletedRecord = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromSemiAutoMatchList_ItIsNoLongerAvailableForMatching()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            var indexOfDeletedRecord = 2;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match04"));
        }

        [Test]
        public void M_AfterOwnedRecordIsDeletedFromManualMatchList_ItIsNoLongerAvailableForMatching()
        {//xxxxxx
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var indexOfDeletedRecord = 1;

            // Act
            reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);

            // Assert
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            Assert.AreEqual(0, reconciliator.CurrentRecordForMatching().Matches.Count(x => x.ActualRecords[0].Description == "Match02"));
        }

        [Test]
        public void M_WhenYouTryToDeleteAMatchedOwnedRecord_YouGetAnError()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var indexOfDeletedRecord = 1;
            reconciliator.MatchCurrentRecord(indexOfDeletedRecord);

            // Act
            var exceptionThrown = false;
            var exceptionMessage = String.Empty;
            try
            {
                reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                exceptionMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteMatchedOwnedRecord, exceptionMessage);
        }

        [Test]
        public void M_WhenYouTryToDeleteAMatchedOwnedRecord_WithANonExistentIndex_YouGetAnError()
        {
            // Arrange
            var amountForMatching = 23.45;
            Mock<IFileIO<ActualBankRecord>> mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            Mock<IFileIO<BankRecord>> mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record01"},
                    new ActualBankRecord {Amount = amountForMatching, Description = "Record02"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {UnreconciledAmount = amountForMatching + 1, Description = "Match01"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match02"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match03"},
                    new BankRecord {UnreconciledAmount = amountForMatching, Description = "Match04"}
                });
            var actualBankFile = new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object);
            actualBankFile.Load();
            var bankFile = new CSVFile<BankRecord>(mockBankFileIO.Object);
            bankFile.Load();
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(actualBankFile, bankFile);
            reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            var indexOfDeletedRecord = 10;

            // Act
            var exceptionThrown = false;
            var exceptionMessage = String.Empty;
            try
            {
                reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(indexOfDeletedRecord);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                exceptionMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteOwnedRecordDoesNotExist, exceptionMessage);
        }
    }
}
