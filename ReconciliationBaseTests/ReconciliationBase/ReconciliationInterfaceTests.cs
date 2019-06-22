using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
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
        private Mock<IInputOutput> _mock_input_output;
        private Mock<IFileIO<ActualBankRecord>> _mock_actual_bank_file_io;
        private Mock<IFileIO<BankRecord>> _mock_bank_file_io;
        private int _num_times_called;

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
            _mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            _mock_bank_file_io = new Mock<IFileIO<BankRecord>>();
            Clear_self_shunt_variables();
        }

        [Test]
        public void M_CanShowMatchesWithoutImpactingOnThoseMatches()
        {
            // Arrange
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Amount = AmountForMatching, Description = "Source"} 
                });
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}
                });
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            reconciliator.Find_reconciliation_matches_for_next_third_party_record();
            var previous_matches = reconciliator.Current_potential_matches();
            var num_previous_matches = previous_matches.Count;
            var previous_first_match_description = previous_matches[0].Actual_records[0].Description;
            var previous_last_match_description = previous_matches[num_previous_matches - 1].Actual_records[0].Description;

            // Act
            reconciliation_interface.Show_current_record_and_semi_auto_matches();

            // Assert
            var current_matches = reconciliator.Current_potential_matches();
            Assert.AreEqual(num_previous_matches, current_matches.Count);
            Assert.AreEqual(previous_first_match_description, current_matches[0].Actual_records[0].Description);
            Assert.AreEqual(previous_last_match_description, current_matches[num_previous_matches - 1].Actual_records[0].Description);
        }

        [Test]
        public void M_WhenMatchingIsDone_AllRecordsAreProcessed()
        {
            // Arrange
            Setup_for_all_matches_chosen_with_index_zero();
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = SourceDescription + "01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = SourceDescription + "02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = SourceDescription + "03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = SourceDescription + "04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = SourceDescription + "05"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var bank_lines = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = MatchDescription + "01a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = MatchDescription + "01b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = MatchDescription + "02a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = MatchDescription + "02b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = MatchDescription + "03a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = MatchDescription + "03b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = MatchDescription + "04a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = MatchDescription + "04b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = MatchDescription + "05a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = MatchDescription + "05b"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            for (int actual_bank_count = 0; actual_bank_count < actual_bank_lines.Count; actual_bank_count++)
            {
                Assert.IsTrue(actual_bank_lines[actual_bank_count].Matched, $"actualBank record {actual_bank_count} should be matched");
                Verify_is_output_as_console_line(actual_bank_lines[actual_bank_count].Description, num_times:1);

                var first_of_each_bank_pair = actual_bank_count * 2;
                Assert.IsTrue(bank_lines[first_of_each_bank_pair].Matched, $"bank record {first_of_each_bank_pair} should be matched");
                Verify_is_output_as_console_snippet(bank_lines[first_of_each_bank_pair].Description, num_times: 1);

                // Every other line in bankLines won't get matched.
                var second_of_each_bank_pair = (actual_bank_count * 2) + 1;
                Assert.IsFalse(bank_lines[second_of_each_bank_pair].Matched, $"bank record {second_of_each_bank_pair} should NOT be matched");
                Verify_is_output_amongst_non_prioritised_matches(bank_lines[second_of_each_bank_pair].Description, num_times: 1);
            }
        }

        [Test]
        public void M_WhenMatchingIsDoneForNonMatchingRecords_AllRecordsAreProcessed()
        {
            // Arrange
            Setup_for_all_matches_chosen_with_index_zero();
            var actual_bank_lines = new List<ActualBankRecord> {
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = SourceDescription + "01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = SourceDescription + "02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = SourceDescription + "03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = SourceDescription + "04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = SourceDescription + "05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "06"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "07"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "08"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = SourceDescription + "09"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actual_bank_lines);
            var bank_lines = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = MatchDescription + "01a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = MatchDescription + "01b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = MatchDescription + "02a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = MatchDescription + "02b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = MatchDescription + "03a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = MatchDescription + "03b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = MatchDescription + "04a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = MatchDescription + "04b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = MatchDescription + "05a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = MatchDescription + "05b"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bank_lines);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();
            reconciliator.Rewind();
            reconciliation_interface.Do_matching_for_non_matching_records();

            // Assert
            // The first 5 ActualBank lines should get matched on the first time through, the next 4 lines get matched on the second pass.
            for (int actual_bank_count = 0; actual_bank_count < actual_bank_lines.Count; actual_bank_count++)
            {
                Assert.IsTrue(actual_bank_lines[actual_bank_count].Matched, $"actualBank record {actual_bank_count} should be matched");
                Assert.IsTrue(bank_lines[actual_bank_count].Matched, $"bank record {actual_bank_count} should be matched");
            }
            // Every other bank line is not matched on the first time round. 
            // On the second time round, four of them are matched one by one, 
            // so each one gets output to console one more time than the one before.
            for (int bank_count = 0; bank_count < 4; bank_count++)
            {
                Verify_is_output_amongst_all_matches($"{MatchDescription}0{bank_count + 1}b", num_times: bank_count + 1);
            }
            // The very last bank line is never matched.
            var last_bank_line = bank_lines.Count - 1;
            Assert.IsFalse(bank_lines[last_bank_line].Matched, $"bank record {last_bank_line} should NOT be matched");
            Verify_is_output_amongst_all_matches(MatchDescription + "05b", num_times: 4);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_ShouldTryToMatchNextRecord(bool auto_matching)
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source02"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}
                });
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_to_delete_third_party_record(actual_bank_records[0].Description);
            Setup_to_choose_match(actual_bank_records[1].Description, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            var previous_num_third_party_records = reconciliator.Num_third_party_records();

            // Act
            if (auto_matching)
            {
                reconciliation_interface.Do_the_matching();
            }
            else
            {
                reconciliation_interface.Do_matching_for_non_matching_records();
            }

            // Assert
            Assert.AreEqual(previous_num_third_party_records - 1, reconciliator.Num_third_party_records());
            Assert.AreEqual(actual_bank_records[1].Description, reconciliator.Current_source_description());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenCurrentThirdPartyRecordIsDeleted_IfThereAreNoRecordsLeft_ShouldMoveOn(bool auto_matching)
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"}
                });
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_to_delete_third_party_record(actual_bank_records[0].Description);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            if (auto_matching)
            {
                reconciliation_interface.Do_the_matching();
            }
            else
            {
                reconciliation_interface.Do_matching_for_non_matching_records();
            }

            // Assert
            _mock_input_output.Verify(x => x.Output_line("Writing new data..."), Times.Once);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterAutoMatching_ShouldUpdateListOfMatches()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching - 1, Description = "Matchxx"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 1;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var description_of_deleted_record = bank_records[2].Description;
            Prepare_to_verify_record_is_output_amongst_non_prioritised_matches(description_of_deleted_record);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            // The records that aren't deleted should get output twice.
            Verify_is_output_as_console_snippet(bank_records[1].Description, num_times: 2);
            Verify_is_output_amongst_non_prioritised_matches(bank_records[3].Description, num_times: 2);
            // The record that's deleted should only get output once.
            _mock_input_output.Verify();
            Assert.AreEqual(1, _num_times_called);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterManualMatching_ShouldUpdateListOfMatches()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Matchxx"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 2;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var description_of_deleted_record = bank_records[2].Description;
            var description_of_matched_record = bank_records[0].Description;
            Prepare_to_verify_record_is_output_amongst_all_matches(description_of_deleted_record);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_matching_for_non_matching_records();

            // Assert
            // The records that aren't deleted should get output twice.
            Verify_is_output_amongst_all_matches(description_of_matched_record, num_times: 2);
            Verify_is_output_amongst_all_matches(bank_records[1].Description, num_times: 2);
            Verify_is_output_amongst_all_matches(bank_records[3].Description, num_times: 2);
            // The record that's deleted should only get output once.
            _mock_input_output.Verify();
            Assert.AreEqual(1, _num_times_called);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenOwnedRecordDeletionResultsInAnError_ThenUserNotified_AndAskedForNewInput(bool auto_matching)
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_too_high = bank_records.Count + 1;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_too_high, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            bool exception_thrown = false;
            try
            {
                if (auto_matching)
                {
                    reconciliation_interface.Do_the_matching();
                }
                else
                {
                    reconciliation_interface.Do_matching_for_non_matching_records();
                }
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
            _mock_input_output.Verify(x =>
                    x.Output_line(Reconciliator<ActualBankRecord, BankRecord>.CannotDeleteOwnedRecordDoesNotExist),
                Times.Once);
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description),
                Times.Exactly(2));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void M_WhenOwnedRecordIsDeleted_IfMatchesStillExist_ShouldNotMoveOnToNextThirdPartyRecord(bool auto_matching)
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 1;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            if (auto_matching)
            {
                reconciliation_interface.Do_the_matching();
            }
            else
            {
                reconciliation_interface.Do_matching_for_non_matching_records();
            }

            // Assert
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description),
                Times.Exactly(2));
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterAutoMatching_IfNoMatchesAreLeft_ShouldShowMessage_AndMoveOnToNextThirdPartyRecord()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source02"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match02"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_choose_match(actual_bank_records[1].Description, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description),
                Times.Once);
            _mock_input_output.Verify(x =>
                    x.Output_line(ReconConsts.NoMatchesLeft),
                Times.Once);
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[1].Description),
                Times.Once);
        }

        [Test]
        public void M_WhenOwnedRecordIsDeletedAfterManualMatching_IfNoMatchesAreLeft_ShouldShowMessage_AndMoveOnToNextThirdPartyRecord()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source02"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_matching_for_non_matching_records();

            // Assert
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description),
                Times.Once);
            _mock_input_output.Verify(x =>
                    x.Output_line(ReconConsts.NoMatchesLeft),
                Times.Once);
            _mock_input_output.Verify(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[1].Description),
                Times.Never);
        }

        [Test]
        public void M_DespiteAMixtureOfDeletionsAfterAutoMatching_AllRecordsShouldBeOutputToUser()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = "Source06"}};
            List<string> actual_bank_record_descriptions = actual_bank_records.Select(x => x.Description).ToList();
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01c"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = "Match03"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = "Match04"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = "Match05"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 60, Description = "Match06a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 60, Description = "Match06b"}};
            List<string> bank_record_descriptions = bank_records.Select(x => x.Description).ToList();
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_delete_owned_record_twice_only(actual_bank_records[1].Description, index_of_deleted_record, 0);
            Setup_to_delete_third_party_record(actual_bank_records[2].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch, 
                    actual_bank_records[3].Description))
                .Returns("0");
            Setup_to_delete_third_party_record(actual_bank_records[4].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[5].Description))
                .Returns("0");
            Setup_to_delete_owned_record_once_only(actual_bank_records[6].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            foreach (var actual_bank_record in actual_bank_records)
            {
                Verify_is_output_as_console_line(actual_bank_record_descriptions[actual_bank_records.IndexOf(actual_bank_record)], -1);
            }
            foreach (var description in bank_record_descriptions)
            {
                Verify_is_output_as_console_snippet(description, 1);
            }
        }

        [Test]
        public void M_DespiteAMixtureOfDeletionsAfterManualMatching_AllRecordsShouldBeOutputToUser()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 2, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 3, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 4, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 5, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 6, Description = "Source06"}};
            List<string> actual_bank_record_descriptions = actual_bank_records.Select(x => x.Description).ToList();
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match04"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match05"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match06"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match07"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match08"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match09"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match10"}};
            List<string> bank_record_descriptions = bank_records.Select(x => x.Description).ToList();
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_delete_owned_record_twice_only(actual_bank_records[1].Description, index_of_deleted_record, 0);
            Setup_to_delete_third_party_record(actual_bank_records[2].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[3].Description))
                .Returns("0");
            Setup_to_delete_third_party_record(actual_bank_records[4].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[5].Description))
                .Returns("0");
            Setup_to_delete_owned_record_once_only(actual_bank_records[6].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_matching_for_non_matching_records();

            // Assert
            foreach (var actual_bank_record in actual_bank_records)
            {
                _mock_input_output.Verify(x => 
                        x.Output_line(It.Is<string>(y => 
                            y.Contains(actual_bank_record_descriptions[actual_bank_records.IndexOf(actual_bank_record)]))), 
                    Times.AtLeastOnce);
            }
            foreach (var description in bank_record_descriptions)
            {
                Assert.IsTrue(_output_all_lines_recorded_descriptions.Any(x => x.Contains(description)), 
                    $"OutputAllLines params should include {description}");
            }
        }

        [Test]
        public void M_AfterAMixtureOfDeletionsDuringAutoMatching_TheNumberOfRecordsFinallyReconciledShouldBeCorrect()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 10, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 20, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 30, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 40, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 50, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 60, Description = "Source06"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01b"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = "Match01c"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 20, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 30, Description = "Match03"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 40, Description = "Match04"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 50, Description = "Match05"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 60, Description = "Match06a"},
                new BankRecord {Unreconciled_amount = AmountForMatching + 60, Description = "Match06b"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_delete_owned_record_twice_only(actual_bank_records[1].Description, index_of_deleted_record, 0);
            Setup_to_delete_third_party_record(actual_bank_records[2].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[3].Description))
                .Returns("0");
            Setup_to_delete_third_party_record(actual_bank_records[4].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[5].Description))
                .Returns("0");
            Setup_to_delete_owned_record_once_only(actual_bank_records[6].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            // 4 records were deleted but that left the first source record unmatched, which would then have been added to owned records.
            Assert.AreEqual(bank_records.Count - 3, reconciliator.Num_owned_records(), "NumOwnedRecords");
            Assert.AreEqual(actual_bank_records.Count - 2, reconciliator.Num_third_party_records(), "NumThirdPartyRecords");
        }

        [Test]
        public void M_AfterAMixtureOfDeletionsDuringManualMatching_TheNumberOfRecordsFinallyReconciledShouldBeCorrect()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"},
                new ActualBankRecord {Amount = AmountForMatching + 1, Description = "Source01"},
                new ActualBankRecord {Amount = AmountForMatching + 2, Description = "Source02"},
                new ActualBankRecord {Amount = AmountForMatching + 3, Description = "Source03"},
                new ActualBankRecord {Amount = AmountForMatching + 4, Description = "Source04"},
                new ActualBankRecord {Amount = AmountForMatching + 5, Description = "Source05"},
                new ActualBankRecord {Amount = AmountForMatching + 6, Description = "Source06"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord> {
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match01"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match02"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match03"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match04"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match05"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match06"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match07"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match08"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match09"},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match10"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var index_of_deleted_record = 0;
            Setup_to_delete_owned_record_once_only(actual_bank_records[0].Description, index_of_deleted_record, 0);
            Setup_to_delete_owned_record_twice_only(actual_bank_records[1].Description, index_of_deleted_record, 0);
            Setup_to_delete_third_party_record(actual_bank_records[2].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[3].Description))
                .Returns("0");
            Setup_to_delete_third_party_record(actual_bank_records[4].Description);
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.EnterNumberOfMatch,
                    actual_bank_records[5].Description))
                .Returns("0");
            Setup_to_delete_owned_record_once_only(actual_bank_records[6].Description, index_of_deleted_record, 0);
            Setup_to_exit_at_the_end();
            _output_all_lines_recorded_descriptions.Clear();
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_matching_for_non_matching_records();

            // Assert
            Assert.AreEqual(bank_records.Count - 4, reconciliator.Num_owned_records(), "NumOwnedRecords");
            Assert.AreEqual(actual_bank_records.Count - 2, reconciliator.Num_third_party_records(), "NumThirdPartyRecords");
        }

        [Test]
        public void M_WhenSemiAutoMatchHasDifferentAmount_WillAddExplanatoryTextAndChangeAmount()
        {
            // Arrange
            var common_description = "common description";
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = common_description}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching + 1, Description = common_description}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_for_all_matches_chosen_with_index_zero();
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            Assert.IsTrue(bank_records[0].Description.Contains(ReconConsts.OriginalAmountWas));
            Assert.AreEqual(actual_bank_records[0].Main_amount(), bank_records[0].Reconciled_amount);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForSemiAutoMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching + 1, Description = "Match00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_to_exit_at_the_end();
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            bool exception_thrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(EnterNumberOfMatch, 
                // which means it will have returned null.
                reconciliation_interface.Do_the_matching();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForAutoMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Source00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            _mock_input_output.Setup(x => x.Get_generic_input(ReconConsts.GoAgainFinish)).Returns("2");
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            bool exception_thrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(ChooseWhatToDoWithMatches), 
                // which means it will have returned null.
                reconciliation_interface.Do_the_matching();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedForFinalMatchChoice_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                .Returns("0");
            _mock_input_output.Setup(x => x.Get_generic_input(ReconConsts.GoAgainFinish)).Returns("2");
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            bool exception_thrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetInput(ChooseWhatToDoWithMatches), 
                // which means it will have returned null.
                reconciliation_interface.Do_the_matching();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhetherToGoAgainOrFinish_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");
            bool exception_thrown = false;

            // Act
            try
            {
                // We didn't mock _mockInputOutput.GetGenericInput(GoAgainFinish), 
                // which means it will have returned null.
                reconciliation_interface.Do_the_matching();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhichRecordToDelete_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            _mock_input_output.SetupSequence(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description))
                .Returns("D")
                .Returns("");
            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.WhetherToDeleteThirdParty, actual_bank_records[0].Description))
                .Returns("N");
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            // We didn't mock _mockInputOutput.GetInput(EnterDeletionIndex, 
            // which means it will have returned null.
            reconciliation_interface.Do_the_matching();

            // Assert
            _mock_input_output.Verify(
                x => x.Output_line("Object reference not set to an instance of an object."),
                Times.Never);
        }

        [Test]
        public void M_IfUserPressesEnter_WhenAskedWhetherToDeleteThirdPartyRecord_NoErrorShouldOccur()
        {
            // Arrange
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = "Source00"}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = "Match00"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            _mock_input_output.SetupSequence(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, actual_bank_records[0].Description))
                .Returns("D")
                .Returns("");
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            var reconciliation_interface = new ReconciliationInterface(_mock_input_output.Object, reconciliator, "ActualBank", "Bank In");

            // Act
            // We didn't mock _mockInputOutput.GetInput(WhetherToDeleteThirdParty, 
            // which means it will have returned null.
            reconciliation_interface.Do_the_matching();

            // Assert
            _mock_input_output.Verify(
                x => x.Output_line("Object reference not set to an instance of an object."),
                Times.Never);
        }

        [Test]
        public void M_WhenRecursivelyHandlingAutoMatches_IfMatchesAreRemoved_CorrectResultsAreShown()
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "something else";
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = text1},
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = text2}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_to_remove_auto_match();
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            var text1_lines = _output_all_lines_recorded_console_lines.Where(x => x.Description_string == text1);
            var text2_lines = _output_all_lines_recorded_console_lines.Where(x => x.Description_string == text2);
            Assert.AreEqual(2, text1_lines.Count(), "text1 source and match are shown for first auto matching .");
            Assert.AreEqual(6, text2_lines.Count(), "text2 source and match are shown for both auto matchings and also final match.");
        }

        [Test]
        public void M_WhenRecursivelyHandlingFinalMatches_IfMatchesAreRemoved_CorrectResultsAreShown()
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "something else";
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = text1},
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = text2}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_for_all_matches_chosen_with_index_zero();
            Setup_to_remove_final_match();
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            var text1_lines = _output_all_lines_recorded_console_lines.Where(x => x.Description_string.Contains(text1));
            var text2_lines = _output_all_lines_recorded_console_lines.Where(x => x.Description_string.Contains(text2));
            Assert.AreEqual(2, text1_lines.Count(), "text1 source and match are shown for first final match showing.");
            Assert.AreEqual(4, text2_lines.Count(), "text2 source and match are shown for both final match showings.");
        }

        [TestCase(TransactionMatchType.Auto, "10")]
        [TestCase(TransactionMatchType.Auto, "0,3,6")]
        [TestCase(TransactionMatchType.Final, "1")]
        [TestCase(TransactionMatchType.Final, "25,26")]
        public void M_WhenRemovingMatches_IfBadIndexEntered_ThenErrorShownAndUserAskedAgain(
            TransactionMatchType transaction_match_type, 
            string match_indices)
        {
            // Arrange
            var text1 = "Source00";
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching, Description = text1}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Setup_for_all_matches_chosen_with_index_zero();
            if (transaction_match_type == TransactionMatchType.Auto)
            {
                Setup_to_remove_auto_match(match_indices);
            }
            else
            {
                Setup_to_remove_final_match(match_indices);
            }
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            Assert.IsTrue(_output_single_line_recorded_messages.Contains(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber),
                "Bad match error message should be shown");
            var text1_lines = _output_all_lines_recorded_console_lines.Where(x => x.Description_string == text1);
            Assert.AreEqual(4, text1_lines.Count(), "source and match are shown for auto match and final match.");
        }

        [TestCase(TransactionMatchType.SemiAuto)]
        [TestCase(TransactionMatchType.Manual)]
        public void M_WhenChoosingMatch_IfBadIndexEntered_ThenErrorShownAndUserAskedAgain(TransactionMatchType transaction_match_type)
        {
            // Arrange
            var text1 = "Source00";
            var text2 = "Pickle pop";
            var actual_bank_records = new List<ActualBankRecord>{
                new ActualBankRecord {Amount = AmountForMatching, Description = text1},
                new ActualBankRecord {Amount = AmountForMatching, Description = text2}};
            _mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(actual_bank_records);
            var bank_records = new List<BankRecord>{
                new BankRecord {Unreconciled_amount = AmountForMatching + 10, Description = text1},
                new BankRecord {Unreconciled_amount = AmountForMatching + 100, Description = "something else"}};
            _mock_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(bank_records);
            var reconciliator = new BankReconciliatorNew<ActualBankRecord, BankRecord>(_mock_actual_bank_file_io.Object, _mock_bank_file_io.Object, BankAndBankInData.LoadingInfo);
            Clear_self_shunt_variables();
            if (transaction_match_type == TransactionMatchType.SemiAuto)
            {
                _mock_input_output.SetupSequence(x =>
                        x.Get_input(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                    .Returns("10") // bad index (semi-auto)
                    .Returns("0") // second attempt (semi-auto) - match the only option
                    .Returns("0"); // first attempt (manual) - match the only option
            }
            else
            {
                _mock_input_output.SetupSequence(x =>
                        x.Get_input(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                    .Returns("0") // first attempt (semi-auto) - match the only option
                    .Returns("1") // bad index (manual)
                    .Returns("0"); // second attempt (manual) - match the only option
            }
            Setup_to_move_on_to_manual_matching_then_exit();
            // Use self-shunt for this one.
            var reconciliation_interface = new ReconciliationInterface(this, reconciliator, "ActualBank", "Bank In");

            // Act
            reconciliation_interface.Do_the_matching();

            // Assert
            Assert.IsTrue(_output_single_line_recorded_messages.Contains(Reconciliator<ActualBankRecord, BankRecord>.BadMatchNumber),
                "Bad match error message should be shown");
            var text1_lines = _output_single_line_recorded_console_lines.Where(x => x.Description_string == text1);
            Assert.AreEqual(1, text1_lines.Count(), "semiauto-matched source should only be output once.");
            var text2_lines = _output_single_line_recorded_messages.Where(x => x.Contains(text2));
            Assert.AreEqual(1, text2_lines.Count(), "manually-matched source should only be output once.");
        }
    }
}
