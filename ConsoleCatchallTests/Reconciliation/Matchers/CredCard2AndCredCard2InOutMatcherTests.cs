using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class CredCard2AndCredCard2InOutMatcherTests : IInputOutput
    {
        private Mock<IInputOutput> _mock_input_output;

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
        }

        [Test]
        public void M_WillFilterOwnedFileForiTunesTransactionsOnly()
        {
            // Arrange
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            var amazon_description =
                $"{ReconConsts.iTunes_description} purchase ";
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Description = ReconConsts.iTunes_description + "1"},
                    new CredCard2InOutRecord {Description = "something"},
                    new CredCard2InOutRecord {Description = ReconConsts.iTunes_description + "2"},
                    new CredCard2InOutRecord {Description = ReconConsts.iTunes_description + "3"},
                    new CredCard2InOutRecord {Description = "something else"}
                });
            var cred_card2_in_out_file = new GenericFile<CredCard2InOutRecord>(new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object));
            var mock_cred_card2_file_io = new Mock<IFileIO<CredCard2Record>>();
            mock_cred_card2_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2Record> {
                    new CredCard2Record {Description = $"\"HOLLYHILL {ReconConsts.iTunes_description}\""}
                });
            var cred_card2_file = new GenericFile<CredCard2Record>(new CSVFile<CredCard2Record>(mock_cred_card2_file_io.Object));
            var data_loading_info = new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> { Sheet_name = MainSheetNames.Cred_card2 };
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(data_loading_info, cred_card2_file, cred_card2_in_out_file);
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.SetiTunesStrings();
            matcher.Filter_matching_transactions_from_cred_card2_in_out(reconciliator);

            // Assert
            Assert.AreEqual(3, reconciliator.Owned_file.Records.Count);
            Assert.AreEqual(ReconConsts.iTunes_description + "1", reconciliator.Owned_file.Records[0].Description);
            Assert.AreEqual(ReconConsts.iTunes_description + "2", reconciliator.Owned_file.Records[1].Description);
            Assert.AreEqual(ReconConsts.iTunes_description + "3", reconciliator.Owned_file.Records[2].Description);
        }

        [Test]
        public void M_WillFilterThirdPartyFileForiTunesTransactionsOnly()
        {
            // Arrange
            var mock_cred_card2_file_io = new Mock<IFileIO<CredCard2Record>>();
            mock_cred_card2_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2Record> {
                    new CredCard2Record {Description = $"\"'{ReconConsts.iTunes_description}\""},
                    new CredCard2Record {Description = "something else"}
                });
            var cred_card2_file = new GenericFile<CredCard2Record>(new CSVFile<CredCard2Record>(mock_cred_card2_file_io.Object));
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<CredCard2InOutRecord> {
                    new CredCard2InOutRecord {Description = ReconConsts.iTunes_description + " purchase 1"}
                });
            var cred_card2_in_out_file = new GenericFile<CredCard2InOutRecord>(new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object));
            var data_loading_info = new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> { Sheet_name = MainSheetNames.Cred_card2 };
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(data_loading_info, cred_card2_file, cred_card2_in_out_file);
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.SetiTunesStrings();
            matcher.Filter_matching_transactions_from_cred_card2(reconciliator);

            // Assert
            Assert.AreEqual(1, reconciliator.Third_party_file.Records.Count);
            Assert.IsTrue(reconciliator.Third_party_file.Records[0].Description.Contains(ReconConsts.iTunes_description));
        }

        [Test]
        public void M_WhenAmazonMatchingWillFilterOwnedFileForiTunesTransactionsOnly()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Filter_owned_file(matcher.Is_not_owned_matching_transaction));
        }

        [Test]
        public void M_WhenAmazonMatchingWillFilterThirdPartyFileForAmazonTransactionsOnly()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Filter_third_party_file(matcher.Is_not_third_party_matching_transaction));
        }

        [Test]
        public void M_WhenAmazonMatchingWillRefreshFilesAtEnd()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Refresh_files());
        }

        [Test]
        public void M_WhenAmazonMatchingWillSetMatchFindingDelegate()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Set_match_finder(matcher.Find_matches));
        }

        [Test]
        public void M_WhenAmazonMatchingWillRewindReconciliator()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Rewind());
        }

        [Test]
        public void M_WhenAmazonMatchingWillResetMatchFindingDelegateAtEnd()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Reset_match_finder());
        }

        [Test]
        public void M_WhenAmazonMatchingWillSetRecordMatchingDelegate()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Set_record_matcher(matcher.Match_specified_records));
        }

        [Test]
        public void M_WhenAmazonMatchingWillDoSemiAutomaticMatching()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliation_interface.Verify(x => x.Do_semi_automatic_matching());
        }

        [Test]
        public void M_WhenAmazonMatchingWillResetRecordMatchingDelegate()
        {
            // Arrange
            var mock_reconciliator = new Mock<IReconciliator<CredCard2Record, CredCard2InOutRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<CredCard2Record, CredCard2InOutRecord>>();
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Reset_record_matcher());
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillMatchRecordWithSpecifiedIndex()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new CredCard2InOutRecord {Matched = false} } },
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new CredCard2InOutRecord {Matched = false} } },
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new CredCard2InOutRecord {Matched = false} } }
            };
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);
            var index = 1;

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(false, record_for_matching.Matches[0].Actual_records[0].Matched, "first record not matched");
            Assert.AreEqual(true, record_for_matching.Matches[1].Actual_records[0].Matched, "second record matched");
            Assert.AreEqual(false, record_for_matching.Matches[2].Actual_records[0].Matched, "third record not matched");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillReplaceMultipleMatchesWithSingleMatch()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30}
                    }
                }
            };
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);
            var index = 0;
            Assert.AreEqual(2, record_for_matching.Matches[index].Actual_records.Count, "num matches before call");

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(1, record_for_matching.Matches[index].Actual_records.Count, "num matches after call");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithExplanatoryDescription()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var desc01 = "Thing 01";
            var desc02 = "Thing 02";
            var desc03 = "Thing 03";
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = $"Amazon {desc01}", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = $"Amazon {desc02}", Unreconciled_amount = 10.33},
                        new CredCard2InOutRecord {Description = $"Amazon {desc03}", Unreconciled_amount = 2.01}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_description =
                $"{ReconConsts.SeveralAmazonTransactions} "
                + $"({desc01} {matches[0].Main_amount().To_csv_string(true)}, "
                + $"{desc02} {matches[1].Main_amount().To_csv_string(true)}, " 
                + $"{desc03} {matches[2].Main_amount().To_csv_string(true)})"
                + $"{ReconConsts.TransactionsDontAddUp} ({potential_matches[index].Actual_records.Sum(x => x.Main_amount()).To_csv_string(true)})";
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(expected_description, record_for_matching.Matches[index].Actual_records[0].Description);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithDateToMatchSourceRecord()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(source_record.Date, record_for_matching.Matches[index].Actual_records[0].Date);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_WillCreateNewRecordWithAmountToMatchSource()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_amount = matches[0].Main_amount() + matches[1].Main_amount() + matches[2].Main_amount();
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(record_for_matching.SourceRecord.Main_amount(), record_for_matching.Matches[index].Actual_records[0].Main_amount());
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_AndSummedAmountsDontMatch_WillMarkDiscrepancy()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_amount = matches[0].Main_amount() + matches[1].Main_amount() + matches[2].Main_amount();
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert 
            Assert.IsTrue(record_for_matching.Matches[index].Actual_records[0].Description.Contains(ReconConsts.TransactionsDontAddUp));
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_BothSourceAndMatchWillHaveMatchedSetToTrue()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(true, record_for_matching.Matches[index].Actual_records[0].Matched, "match is set to matched");
            Assert.AreEqual(true, record_for_matching.SourceRecord.Matched, "source is set to matched");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_SourceAndMatchWillHaveMatchPropertiesPointingAtEachOther()
        {
            // Arrange
            var mock_owned_file = new Mock<ICSVFile<CredCard2InOutRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<CredCard2InOutRecord>());
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(record_for_matching.SourceRecord, record_for_matching.Matches[index].Actual_records[0].Match, "match is pointing at source");
            Assert.AreEqual(record_for_matching.Matches[index].Actual_records[0], record_for_matching.SourceRecord.Match, "source is pointing at match");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillRemoveOriginalMatchesFromOwnedFile()
        {
            // Arrange
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bank_records = new List<CredCard2InOutRecord>
            {
                new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 40.44}
            };
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_records);
            var cred_card2_in_out_file = new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object);
            cred_card2_in_out_file.Load();
            var potential_matches = new List<IPotentialMatch> { new PotentialMatch {Actual_records = new List<ICSVRecord>()} };
            potential_matches[0].Actual_records.Add(bank_records[0]);
            potential_matches[0].Actual_records.Add(bank_records[1]);
            potential_matches[0].Actual_records.Add(bank_records[2]);
            var index = 0;
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);
            foreach (var bank_record in bank_records)
            {
                Assert.IsTrue(cred_card2_in_out_file.Records.Contains(bank_record));
            }

            // Act
            matcher.Match_specified_records(record_for_matching, index, cred_card2_in_out_file);

            // Assert
            foreach (var bank_record in bank_records)
            {
                Assert.IsFalse(cred_card2_in_out_file.Records.Contains(bank_record));
            }
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillAddNewlyCreatedMatchToOwnedFile()
        {
            // Arrange
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            var source_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bank_records = new List<CredCard2InOutRecord>
            {
                new CredCard2InOutRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 10.33},
                new CredCard2InOutRecord {Description = "Match 02", Unreconciled_amount = 4.01}
            };
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_records);
            var cred_card2_in_out_file = new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object);
            cred_card2_in_out_file.Load();
            var potential_matches = new List<IPotentialMatch> { new PotentialMatch { Actual_records = new List<ICSVRecord>() } };
            potential_matches[0].Actual_records.Add(bank_records[0]);
            potential_matches[0].Actual_records.Add(bank_records[1]);
            potential_matches[0].Actual_records.Add(bank_records[2]);
            var index = 0;
            var record_for_matching = new RecordForMatching<CredCard2Record>(source_record, potential_matches);
            foreach (var bank_record in bank_records)
            {
                Assert.IsTrue(cred_card2_in_out_file.Records.Contains(bank_record));
            }

            // Act
            matcher.Match_specified_records(record_for_matching, index, cred_card2_in_out_file);

            // Assert
            Assert.AreEqual(1, cred_card2_in_out_file.Records.Count);
            Assert.IsTrue(cred_card2_in_out_file.Records[0].Description.Contains(ReconConsts.SeveralAmazonTransactions));
        }

        [Test]
        public void Will_not_lose_previously_matched_records_when_files_are_refreshed()
        {
            // Arrange
            var some_other_third_party_description = "Some other third party Cred Card 2 description";
            var some_other_owned_description = "Some other owned Cred Card 2 description";
            var third_party_data = new List<CredCard2Record>
            {
                new CredCard2Record { Description = ReconConsts.Amazon_description },
                new CredCard2Record { Description = some_other_third_party_description }
            };
            var mock_cred_card2_file_io = new Mock<IFileIO<CredCard2Record>>();
            mock_cred_card2_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(third_party_data);
            var cred_card2_file = new GenericFile<CredCard2Record>(new CSVFile<CredCard2Record>(mock_cred_card2_file_io.Object));
            var owned_data = new List<CredCard2InOutRecord>
            {
                new CredCard2InOutRecord {Description = $"{ReconConsts.Amazon_description} CredCard2InOutRecord01"},
                new CredCard2InOutRecord {Description = $"{ReconConsts.Amazon_description} CredCard2InOutRecord02"},
                new CredCard2InOutRecord {Description = $"{ReconConsts.Amazon_description} CredCard2InOutRecord03"},
                new CredCard2InOutRecord {Description = some_other_owned_description}
            };
            var mock_cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            mock_cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(owned_data);
            var cred_card2_in_out_file = new GenericFile<CredCard2InOutRecord>(new CSVFile<CredCard2InOutRecord>(mock_cred_card2_in_out_file_io.Object));
            var data_loading_info = new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> { Sheet_name = MainSheetNames.Cred_card2 };
            var reconciliator = new Reconciliator<CredCard2Record, CredCard2InOutRecord>(data_loading_info, cred_card2_file, cred_card2_in_out_file);
            var expected_potential_matches = new List<PotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>{ owned_data[0], owned_data[1] },
                    Console_lines = new List<ConsoleLine>{ new ConsoleLine { Description_string = "Console Description" } }
                }
            };
            var matcher = new CredCard2AndCredCard2InOutMatcher(this);
            matcher.Filter_matching_transactions_from_cred_card2(reconciliator);
            matcher.Filter_matching_transactions_from_cred_card2_in_out(reconciliator);
            reconciliator.Set_match_finder((record, file) => expected_potential_matches);
            reconciliator.Set_record_matcher(matcher.Match_specified_records);
            reconciliator.Find_reconciliation_matches_for_next_third_party_record();
            reconciliator.Match_current_record(0);
            Assert.AreEqual(1, reconciliator.Third_party_file.Records.Count);
            Assert.AreEqual(2, reconciliator.Owned_file.Records.Count);
            Assert.IsFalse(reconciliator.Owned_file.Records[0].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[1].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[1].Description.Contains(ReconConsts.SeveralAmazonTransactions));

            // Act
            reconciliator.Refresh_files();

            // Assert
            Assert.AreEqual(2, reconciliator.Third_party_file.Records.Count);
            Assert.AreEqual(some_other_third_party_description, reconciliator.Third_party_file.Records[1].Description);
            Assert.AreEqual(3, reconciliator.Owned_file.Records.Count);
            Assert.IsFalse(reconciliator.Owned_file.Records[0].Matched);
            Assert.AreEqual(some_other_owned_description, reconciliator.Owned_file.Records[1].Description);
            Assert.AreEqual(third_party_data[0], reconciliator.Owned_file.Records[2].Match);
            Assert.IsTrue(reconciliator.Owned_file.Records[2].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[2].Description.Contains(ReconConsts.SeveralAmazonTransactions));
        }

        [Test]
        public void M_WillPopulateConsoleLinesForEveryPotentialMatch()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillPopulateRankingsForEveryPotentialMatch()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindSingleMatchingBankInTransactionForOneThirdPartyAmazonTransaction()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindSingleMatchingCollectionOfOwnedTransactionsForOneThirdPartyAmazonTransaction()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindMultipleMatchingCollectionOfOwnedTransactionsForOneThirdPartyAmazonTransaction()
        {
            Assert.AreEqual(true, true);
        }
    }
}
