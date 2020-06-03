using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class BankAndBankInMatcherTests : IInputOutput
    {
        private Mock<IInputOutput> _mock_input_output;

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
        }

        [Test]
        public void M_WhenDoingExpenseMatchingWillFilterOwnedFileForWagesRowsAndExpenseTransactionsOnly()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            var wages_description =
                $"Wages ({ReconConsts.Employer_expense_description}) (!! in an inestimable manner, not forthwith - forever 1st outstanding)";
            mock_bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "1"},
                    new BankRecord {Type = "Chq", Description = "something"},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "2"},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "3"},
                    new BankRecord {Type = "Chq", Description = "something"},
                    new BankRecord {Type = "PCL", Description = wages_description}
                });
            var bank_in_file = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mock_bank_in_file_io.Object));
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Description = $"\"'{ReconConsts.Employer_expense_description}\""}
                });
            var actual_bank_file = new ActualBankInFile(new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object));
            var data_loading_info = new DataLoadingInformation<ActualBankRecord, BankRecord> { Sheet_name = MainSheetNames.Bank_in };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(data_loading_info, actual_bank_file, bank_in_file);
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);

            // Assert
            Assert.AreEqual(4, reconciliator.Owned_file.Records.Count);
            Assert.AreEqual(Codes.Expenses + "1", reconciliator.Owned_file.Records[0].Description);
            Assert.AreEqual(Codes.Expenses + "2", reconciliator.Owned_file.Records[1].Description);
            Assert.AreEqual(Codes.Expenses + "3", reconciliator.Owned_file.Records[2].Description);
            Assert.AreEqual(wages_description, reconciliator.Owned_file.Records[3].Description);
        }

        [Test]
        public void M_WillFilterActualBankFileForExpenseTransactionsOnly()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Description = $"\"'{ReconConsts.Employer_expense_description}\""},
                    new ActualBankRecord {Description = "something else"}
                });
            var actual_bank_file = new ActualBankInFile(new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object));
            var mock_bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "1"}
                });
            var bank_in_file = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mock_bank_in_file_io.Object));
            var data_loading_info = new DataLoadingInformation<ActualBankRecord, BankRecord> { Sheet_name = MainSheetNames.Bank_in };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(data_loading_info, actual_bank_file, bank_in_file);
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Filter_for_all_expense_transactions_from_actual_bank_in(reconciliator);

            // Assert
            Assert.AreEqual(1, reconciliator.Third_party_file.Records.Count);
            Assert.AreEqual(ReconConsts.Employer_expense_description, reconciliator.Third_party_file.Records[0].Description.Remove_punctuation());
        }

        [Test]
        public void M_WhenExpenseMatchingWillFilterOwnedFileForWagesRowsAndExpenseTransactionsOnly()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Filter_owned_file(matcher.Is_not_wages_row_or_expense_transaction));
        }

        [Test]
        public void M_AfterExpenseMatching_WillRemoveUnmatchedExpensesFromOwnedFile()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Filter_owned_file(matcher.Is_unmatched_expense_row));
        }

        [Test]
        public void M_WhenExpenseMatchingWillFilterActualBankFileForExpenseTransactionsOnly()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Filter_third_party_file(matcher.Is_not_expense_transaction));
        }

        [Test]
        public void M_WhenExpenseMatchingWillRefreshFilesAtEnd()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Refresh_files());
        }

        [Test]
        public void M_WhenExpenseMatchingWillSetMatchFindingDelegate()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Set_match_finder(matcher.Find_expense_matches));
        }

        [Test]
        public void M_WhenExpenseMatchingWillResetMatchFindingDelegateAtEnd()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Reset_match_finder());
        }

        [Test]
        public void M_WhenExpenseMatchingWillSetRecordMatchingDelegate()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Set_record_matcher(matcher.Match_specified_records));
        }

        [Test]
        public void M_WhenExpenseMatchingWillDoSemiAutomaticMatching()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliation_interface.Verify(x => x.Do_semi_automatic_matching());
        }

        [Test]
        public void M_WhenExpenseMatchingWillResetRecordMatchingDelegate()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_reconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var mock_reconciliation_interface = new Mock<IReconciliationInterface<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            matcher.Do_preliminary_stuff(mock_reconciliator.Object, mock_reconciliation_interface.Object);

            // Assert
            mock_reconciliator.Verify(x => x.Reset_record_matcher());
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillMatchRecordWithSpecifiedIndex()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new BankRecord {Matched = false} } },
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new BankRecord {Matched = false} } },
                new PotentialMatch { Actual_records = new List<ICSVRecord> { new BankRecord {Matched = false} } }
            };
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
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
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30}
                    }
                }
            };
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
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
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Expense Thing 01", Unreconciled_amount = 20.22},
                        new BankRecord {Description = "Expense Thing 02", Unreconciled_amount = 10.33},
                        new BankRecord {Description = "Expense Thing 03", Unreconciled_amount = 2.01}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_description =
                $"{ReconConsts.SeveralExpenses} "
                + $"({matches[0].Main_amount().To_csv_string(true)}, {matches[1].Main_amount().To_csv_string(true)}, {matches[2].Main_amount().To_csv_string(true)})"
                + $"{ReconConsts.ExpensesDontAddUp} ({potential_matches[index].Actual_records.Sum(x => x.Main_amount()).To_csv_string(true)})";
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(expected_description, record_for_matching.Matches[index].Actual_records[0].Description);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithDateToMatchSourceRecord()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(source_record.Date, record_for_matching.Matches[index].Actual_records[0].Date);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_WillCreateNewRecordWithAmountToMatchSource()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_amount = matches[0].Main_amount() + matches[1].Main_amount() + matches[2].Main_amount();
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(record_for_matching.SourceRecord.Main_amount(), record_for_matching.Matches[index].Actual_records[0].Main_amount());
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_AndSummedAmountsDontMatch_WillMarkDiscrepancy()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potential_matches[index].Actual_records;
            var expected_amount = matches[0].Main_amount() + matches[1].Main_amount() + matches[2].Main_amount();
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert 
            Assert.IsTrue(record_for_matching.Matches[index].Actual_records[0].Description.Contains(ReconConsts.ExpensesDontAddUp));
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithTypeOfFirstMatch()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var expected_type = (potential_matches[index].Actual_records[0] as BankRecord).Type;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            Assert.AreEqual(expected_type, (record_for_matching.Matches[index].Actual_records[0] as BankRecord).Type);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_BothSourceAndMatchWillHaveMatchedSetToTrue()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

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
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
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
                        new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                        new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);

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
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bank_records = new List<BankRecord>
            {
                new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22, Type = "Type"},
                new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
            };
            var mock_bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_records);
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_in_file_io.Object);
            bank_in_file.Load();
            var potential_matches = new List<IPotentialMatch> { new PotentialMatch {Actual_records = new List<ICSVRecord>()} };
            potential_matches[0].Actual_records.Add(bank_records[0]);
            potential_matches[0].Actual_records.Add(bank_records[1]);
            potential_matches[0].Actual_records.Add(bank_records[2]);
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            foreach (var bank_record in bank_records)
            {
                Assert.IsTrue(bank_in_file.Records.Contains(bank_record));
            }

            // Act
            matcher.Match_specified_records(record_for_matching, index, bank_in_file);

            // Assert
            foreach (var bank_record in bank_records)
            {
                Assert.IsFalse(bank_in_file.Records.Contains(bank_record));
            }
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillAddNewlyCreatedMatchToOwnedFile()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bank_records = new List<BankRecord>
            {
                new BankRecord {Description = "Match 01", Unreconciled_amount = 20.22, Type = "Type"},
                new BankRecord {Description = "Match 02", Unreconciled_amount = 30.33},
                new BankRecord {Description = "Match 02", Unreconciled_amount = 40.44}
            };
            var mock_bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_records);
            var bank_in_file = new CSVFile<BankRecord>(mock_bank_in_file_io.Object);
            bank_in_file.Load();
            var potential_matches = new List<IPotentialMatch> { new PotentialMatch { Actual_records = new List<ICSVRecord>() } };
            potential_matches[0].Actual_records.Add(bank_records[0]);
            potential_matches[0].Actual_records.Add(bank_records[1]);
            potential_matches[0].Actual_records.Add(bank_records[2]);
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            foreach (var bank_record in bank_records)
            {
                Assert.IsTrue(bank_in_file.Records.Contains(bank_record));
            }

            // Act
            matcher.Match_specified_records(record_for_matching, index, bank_in_file);

            // Assert
            Assert.AreEqual(1, bank_in_file.Records.Count);
            Assert.IsTrue(bank_in_file.Records[0].Description.Contains(ReconConsts.SeveralExpenses));
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillUpdateExpectedIncomeRecordForEachOriginalMatch()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var desc0 = "Source 01";
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today.AddDays(10), Amount = 40.44, Description = desc0
            };
            var desc1 = "Match 01";
            var desc2 = "Match 02";
            var amount1 = 20.22;
            var amount2 = 30.33;
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddDays(1);
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new BankRecord {Description = desc1, Unreconciled_amount = amount1, Date = date1},
                        new BankRecord {Description = desc2, Unreconciled_amount = amount2, Date = date2}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            Assert.AreEqual(0, matcher.MatchedExpectedIncomeRecords.Count);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            mock_bank_and_bank_in_loader.Verify(x => x.Update_expected_income_record_when_matched(
                It.Is<ICSVRecord>(y => y.Description == desc0), 
                It.Is<ICSVRecord>(y => y.Description == desc1)));
            mock_bank_and_bank_in_loader.Verify(x => x.Update_expected_income_record_when_matched(
                It.Is<ICSVRecord>(y => y.Description == desc0),
                It.Is<ICSVRecord>(y => y.Description == desc2)));
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_AndSummedAmountsDontMatch_WillCreateNewExpectedIncomeRecordForMissingAmount()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var desc0 = "Source 01";
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today.AddDays(10),
                Amount = 40.44,
                Description = desc0
            };
            var desc1 = "Match 01";
            var desc2 = "Match 02";
            var amount1 = source_record.Main_amount() / 3;
            var amount2 = source_record.Main_amount() / 3 + 1;
            var remaining_amount = source_record.Main_amount() - (amount1 + amount2);
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddDays(1);
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new BankRecord {Description = desc1, Unreconciled_amount = amount1, Date = date1},
                        new BankRecord {Description = desc2, Unreconciled_amount = amount2, Date = date2}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            Assert.AreEqual(0, matcher.MatchedExpectedIncomeRecords.Count);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            mock_bank_and_bank_in_loader.Verify(x => x.Create_new_expenses_record_to_match_balance(
                It.Is<ICSVRecord>(y => y.Description == source_record.Description
                                       && y.Date == source_record.Date
                                       && y.Main_amount().Double_equals(source_record.Amount)),
                It.Is<double>(y => y.Double_equals(remaining_amount))));
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesAreChosen_AndSummedAmountsMatch_WillNOTCreateNewExpectedIncomeRecord()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var desc0 = "Source 01";
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today.AddDays(10),
                Amount = 30,
                Description = desc0
            };
            var desc1 = "Match 01";
            var desc2 = "Match 02";
            var amount1 = source_record.Main_amount() / 3;
            var amount2 = source_record.Main_amount() - amount1;
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddDays(1);
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new BankRecord {Description = desc1, Unreconciled_amount = amount1, Date = date1},
                        new BankRecord {Description = desc2, Unreconciled_amount = amount2, Date = date2}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            Assert.AreEqual(0, matcher.MatchedExpectedIncomeRecords.Count);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            mock_bank_and_bank_in_loader.Verify(x => x.Create_new_expenses_record_to_match_balance(
                It.IsAny<ICSVRecord>(),
                It.IsAny<double>()),
                Times.Never);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillUpdateExpectedIncomeRecordForSingleMatches()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var mock_owned_file = new Mock<ICSVFile<BankRecord>>();
            mock_owned_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            var desc0 = "Source 01";
            var source_record = new ActualBankRecord
            {
                Date = DateTime.Today.AddDays(10),
                Amount = 40.44,
                Description = desc0
            };
            var desc1 = "Match 01";
            var amount1 = 20.22;
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddDays(1);
            var potential_matches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>
                    {
                        new BankRecord {Description = desc1, Unreconciled_amount = amount1, Date = date1}
                    }
                }
            };
            var index = 0;
            var record_for_matching = new RecordForMatching<ActualBankRecord>(source_record, potential_matches);
            Assert.AreEqual(0, matcher.MatchedExpectedIncomeRecords.Count);

            // Act
            matcher.Match_specified_records(record_for_matching, index, mock_owned_file.Object);

            // Assert
            mock_bank_and_bank_in_loader.Verify(x => x.Update_expected_income_record_when_matched(
                It.Is<ICSVRecord>(y => y.Description == desc0),
                It.Is<ICSVRecord>(y => y.Description == desc1)));
        }

        [Test]
        public void Will_not_lose_previously_matched_records_when_files_are_refreshed()
        {
            // Arrange
            var some_other_actual_bank_description = "Some other ActualBank description";
            var some_other_bank_description = "Some other bank description";
            var actual_bank_data = new List<ActualBankRecord>
            {
                new ActualBankRecord { Description = ReconConsts.Employer_expense_description },
                new ActualBankRecord { Description = some_other_actual_bank_description }
            };
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_data);
            var actual_bank_file = new GenericFile<ActualBankRecord>(new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object));
            var bank_data = new List<BankRecord>
            {
                new BankRecord {Description = "BankRecord01", Type = Codes.Expenses},
                new BankRecord {Description = "BankRecord02", Type = Codes.Expenses},
                new BankRecord {Description = "BankRecord03", Type = Codes.Expenses},
                new BankRecord {Description = some_other_bank_description, Type = "Not an expense"}
            };
            var mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_data);
            var bank_file = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mock_bank_file_io.Object));
            var data_loading_info = new DataLoadingInformation<ActualBankRecord, BankRecord> { Sheet_name = MainSheetNames.Bank_in };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(data_loading_info, actual_bank_file, bank_file);
            var expected_potential_matches = new List<PotentialMatch>
            {
                new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>{ bank_data[0], bank_data[1] },
                    Console_lines = new List<ConsoleLine>{ new ConsoleLine { Description_string = "Console Description" } }
                }
            };
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);
            matcher.Filter_for_all_expense_transactions_from_actual_bank_in(reconciliator);
            matcher.Filter_for_all_wages_rows_and_expense_transactions_from_expected_in(reconciliator);
            reconciliator.Set_match_finder((record, file) => expected_potential_matches);
            reconciliator.Set_record_matcher(matcher.Match_specified_records);
            reconciliator.Find_reconciliation_matches_for_next_third_party_record();
            reconciliator.Match_current_record(0);
            Assert.AreEqual(1, reconciliator.Third_party_file.Records.Count);
            Assert.AreEqual(2, reconciliator.Owned_file.Records.Count);
            Assert.IsFalse(reconciliator.Owned_file.Records[0].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[1].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[1].Description.Contains(ReconConsts.SeveralExpenses));

            // Act
            reconciliator.Refresh_files();

            // Assert
            Assert.AreEqual(2, reconciliator.Third_party_file.Records.Count);
            Assert.AreEqual(some_other_actual_bank_description, reconciliator.Third_party_file.Records[1].Description);
            Assert.AreEqual(3, reconciliator.Owned_file.Records.Count);
            Assert.IsFalse(reconciliator.Owned_file.Records[0].Matched);
            Assert.AreEqual(some_other_bank_description, reconciliator.Owned_file.Records[1].Description);
            Assert.AreEqual(actual_bank_data[0], reconciliator.Owned_file.Records[2].Match);
            Assert.IsTrue(reconciliator.Owned_file.Records[2].Matched);
            Assert.IsTrue(reconciliator.Owned_file.Records[2].Description.Contains(ReconConsts.SeveralExpenses));
        }

        [Test]
        public void M_WhenReconcilingExpenses_WillMatchOnASingleAmount()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var expense_amount = 10.00;
            List<BankRecord> expected_in_rows = new List<BankRecord> { new BankRecord
            {
                Unreconciled_amount = expense_amount,
                Description = "HELLOW"
            } };
            var bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_in_rows);
            var bank_in_file = new CSVFile<BankRecord>(bank_in_file_io.Object);
            bank_in_file.Load();
            ActualBankRecord expense_transaction = new ActualBankRecord { Amount = expense_amount };
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            var result = matcher.Standby_find_expense_matches(expense_transaction, bank_in_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected_in_rows[0], result[0].Actual_records[0]);
        }

        [Test]
        public void M_WhenReconcilingExpenses_WillNotMatchOnASingleDifferentAmount()
        {
            // Arrange
            var mock_bank_and_bank_in_loader = new Mock<IBankAndBankInLoader>();
            var expense_amount = 10.00;
            List<BankRecord> expected_in_rows = new List<BankRecord> { new BankRecord { Unreconciled_amount = expense_amount } };
            var bank_in_file_io = new Mock<IFileIO<BankRecord>>();
            bank_in_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_in_rows);
            var bank_in_file = new CSVFile<BankRecord>(bank_in_file_io.Object);
            bank_in_file.Load();
            ActualBankRecord expense_transaction = new ActualBankRecord { Amount = expense_amount - 1 };
            var matcher = new BankAndBankInMatcher(this, mock_bank_and_bank_in_loader.Object);

            // Act
            var result = matcher.Standby_find_expense_matches(expense_transaction, bank_in_file).ToList();

            // Assert
            Assert.AreEqual(0, result.Count);
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
        public void M_WillFindSingleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindMultipleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }
    }
}
