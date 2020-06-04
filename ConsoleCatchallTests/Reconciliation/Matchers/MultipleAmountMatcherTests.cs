using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Extensions;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class MultipleAmountMatcherTests
    {
        private Mock<IFileIO<CredCard2InOutRecord>> _cred_card2_in_out_file_io;
        private CSVFile<CredCard2InOutRecord> _cred_card2_in_out_file;

        [SetUp]
        public void Set_up()
        {
            _cred_card2_in_out_file_io = new Mock<IFileIO<CredCard2InOutRecord>>();
            _cred_card2_in_out_file = new CSVFile<CredCard2InOutRecord>(_cred_card2_in_out_file_io.Object);
        }

        [Test]
        public void Will_populate_console_lines_for_every_potential_match()
        {
            // Arrange
            var amount_to_match = 10.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord
            {
                Unreconciled_amount = amount_to_match
            } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.NotNull(result[0].Console_lines);
        }

        [Test]
        public void Will_not_return_transactions_that_have_already_been_matched()
        {
            // Arrange
            var amount_to_match = 10.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { 
                new CredCard2InOutRecord { Unreconciled_amount = amount_to_match, Description = "Not matched", Matched = false },
                new CredCard2InOutRecord { Unreconciled_amount = amount_to_match, Description = "Matched", Matched = true }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.Any(x => x.Actual_records.Any(y => y.Matched)));
        }

        [Test]
        public void Will_match_on_a_single_identical_amount()
        {
            // Arrange
            var amount_to_match = 10.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord
            {
                Unreconciled_amount = amount_to_match
            } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
        }

        [Test]
        public void Will_not_match_with_one_amount_that_is_too_large()
        {
            // Arrange
            var amount_to_match = 10.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord
            {
                Unreconciled_amount = amount_to_match + 1
            } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Will_create_single_match_with_one_amount_that_is_too_small()
        {
            // Arrange
            var amount_to_match = 10.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord
            {
                Unreconciled_amount = amount_to_match - 1
            } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
        }

        [Test]
        public void Will_create_single_match_with_multiple_candidates_whose_total_sum_is_equal_to_target()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match02" },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match03" }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Actual_records.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
            Assert.AreEqual(candidate_rows[1], result[0].Actual_records[1]);
            Assert.AreEqual(candidate_rows[2], result[0].Actual_records[2]);
        }

        [Test]
        public void Will_create_single_match_with_multiple_candidates_whose_total_sum_is_too_small()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> { 
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match02" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match03" }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Actual_records.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
            Assert.AreEqual(candidate_rows[1], result[0].Actual_records[1]);
            Assert.AreEqual(candidate_rows[2], result[0].Actual_records[2]);
        }

        [Test]
        public void Will_only_create_single_match_when_duplicate_amounts_can_be_combined_to_create_exact_target_in_different_ways()
        {
            // Arrange
            var amount_to_match = 50.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01" },
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = "Match02" },
                new CredCard2InOutRecord { Unreconciled_amount = 60.00, Description = "Match03" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match04" }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Actual_records.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
            Assert.AreEqual(candidate_rows[1], result[0].Actual_records[1]);
            Assert.AreEqual(candidate_rows[3], result[0].Actual_records[2]);
        }

        [Test]
        public void Will_create_multiple_matches_when_amounts_are_duplicated_but_descriptions_are_different()
        {
            // Arrange
            var amount_to_match = 20.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc03 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0].Actual_records.Count);
            Assert.AreEqual(2, result[1].Actual_records.Count);
            Assert.AreEqual(2, result[2].Actual_records.Count);
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc01)));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc02)));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc03)));
        }

        [Test]
        public void Will_create_multiple_matches_when_multiple_candidates_can_be_combined_to_exact_target_in_different_ways()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            var desc04 = "Match04";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc03 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc04 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc01)));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc02)));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc03)));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Description == desc04)));
        }

        [Test]
        public void When_both_exact_matches_and_inexact_matches_are_available_will_prioritise_exact_matches()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            var desc04 = "Match04";
            var desc05 = "Match05";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc03 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc04 },
                new CredCard2InOutRecord { Unreconciled_amount = 5.00, Description = desc05 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count(x => x.Amount_match));
            Assert.IsTrue(result[0].Amount_match);
            Assert.IsTrue(result[1].Amount_match);
            Assert.IsTrue(result[2].Amount_match);
        }

        [Test]
        public void Exact_matches_will_have_zero_amount_ranking()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            var desc04 = "Match04";
            var desc05 = "Match05";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc03 },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = desc04 },
                new CredCard2InOutRecord { Unreconciled_amount = 5.00, Description = desc05 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count(x => x.Rankings.Amount == 0));
            Assert.AreEqual(0, result[0].Rankings.Amount);
            Assert.AreEqual(0, result[1].Rankings.Amount);
            Assert.AreEqual(0, result[2].Rankings.Amount);
        }

        [Test]
        public void Inexact_matches_will_have_amount_ranking_equal_to_distance_from_target_and_will_be_prioritised_accordingly()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 35.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 25.00, Description = desc03 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(5, result[0].Rankings.Amount);
            Assert.AreEqual(10, result[1].Rankings.Amount);
            Assert.AreEqual(15, result[2].Rankings.Amount);
            Assert.AreEqual(desc02, result[0].Actual_records[0].Description);
            Assert.AreEqual(desc01, result[1].Actual_records[0].Description);
            Assert.AreEqual(desc03, result[2].Actual_records[0].Description);
        }

        [Test]
        public void Will_prioritise_closer_dates_when_multiple_matches_are_available_with_amount_less_than_target()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 25.00, Description = desc01, Date = DateTime.Today },
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = desc02, Date = DateTime.Today.AddDays(1) },
                new CredCard2InOutRecord { Unreconciled_amount = 35.00, Description = desc03, Date = DateTime.Today.AddDays(2) }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(desc01, result[0].Actual_records[0].Description);
            Assert.AreEqual(desc02, result[1].Actual_records[0].Description);
            Assert.AreEqual(desc03, result[2].Actual_records[0].Description);
        }

        [Test]
        public void Will_prioritise_closer_amounts_when_multiple_matches_are_available_with_amount_less_than_target_on_same_date()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 25.00, Description = desc01, Date = DateTime.Today },
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = desc02, Date = DateTime.Today },
                new CredCard2InOutRecord { Unreconciled_amount = 35.00, Description = desc03, Date = DateTime.Today }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(desc03, result[0].Actual_records[0].Description);
            Assert.AreEqual(desc02, result[1].Actual_records[0].Description);
            Assert.AreEqual(desc01, result[2].Actual_records[0].Description);
        }

        [Test]
        public void Will_prioritise_closer_dates_when_multiple_exact_amount_matches_are_available()
        {
            // Arrange
            var amount_to_match = 40.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 40.00, Description = desc01, Date = DateTime.Today.AddDays(1) },
                new CredCard2InOutRecord { Unreconciled_amount = 40.00, Description = desc02, Date = DateTime.Today.AddDays(2) },
                new CredCard2InOutRecord { Unreconciled_amount = 40.00, Description = desc03, Date = DateTime.Today }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(desc03, result[0].Actual_records[0].Description);
            Assert.AreEqual(desc01, result[1].Actual_records[0].Description);
            Assert.AreEqual(desc02, result[2].Actual_records[0].Description);
        }

        [Test]
        public void Will_prioritise_closer_dates_when_duplicate_transactions_exist()
        {
            // Arrange
            var amount_to_match = 50.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            var desc04 = "Match04";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc01, Date = DateTime.Today },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = desc02, Date = DateTime.Today.AddDays(10) },
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = desc03, Date = DateTime.Today },
                new CredCard2InOutRecord { Unreconciled_amount = 30.00, Description = desc04, Date = DateTime.Today.AddDays(10) }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.IsTrue(result[0].Actual_records.Any(x => x.Description == desc01));
            Assert.IsTrue(result[0].Actual_records.Any(x => x.Description == desc03));
            Assert.IsTrue(result[3].Actual_records.Any(x => x.Description == desc02));
            Assert.IsTrue(result[3].Actual_records.Any(x => x.Description == desc04));
        }

        [Test]
        public void Will_find_exact_matches_when_numbers_are_prone_to_rounding_errors()
        {
            // Arrange
            var amount_to_match = 66.57;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 13.00, Description = "Match01", Date = DateTime.Today.AddDays(-2) },
                new CredCard2InOutRecord { Unreconciled_amount = 10.65, Description = "Match02", Date = DateTime.Today.AddDays(-1) },
                new CredCard2InOutRecord { Unreconciled_amount = 29.94, Description = "Match03", Date = DateTime.Today.AddDays(0) },
                new CredCard2InOutRecord { Unreconciled_amount = 13.50, Description = "Match04", Date = DateTime.Today.AddDays(1) },
                new CredCard2InOutRecord { Unreconciled_amount = 14.38, Description = "Match05", Date = DateTime.Today.AddDays(2) },
                new CredCard2InOutRecord { Unreconciled_amount = 12.98, Description = "Match06", Date = DateTime.Today.AddDays(3) },
                new CredCard2InOutRecord { Unreconciled_amount = 46.49, Description = "Match07", Date = DateTime.Today.AddDays(4) },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match08", Date = DateTime.Today.AddDays(9) },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match09", Date = DateTime.Today.AddDays(9) },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match10", Date = DateTime.Today.AddDays(9) },
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match11", Date = DateTime.Today.AddDays(9) },
                new CredCard2InOutRecord { Unreconciled_amount = 50.00, Description = "Match12", Date = DateTime.Today.AddDays(9) }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.IsTrue(amount_to_match.Double_equals((result[0].Actual_records.Sum(x => x.Main_amount()))), 
                $"Expected {result[0].Actual_records.Sum(x => x.Main_amount())} to equal {amount_to_match}");
        }

        [Test]
        public void Will_return_multiple_individual_matches_when_multiple_single_exact_matches_exist()
        {
            // Arrange
            var amount_to_match = 50.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            var desc01 = "Match01";
            var desc02 = "Match02";
            var desc03 = "Match03";
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 50.00, Description = desc01 },
                new CredCard2InOutRecord { Unreconciled_amount = 50.00, Description = desc02 },
                new CredCard2InOutRecord { Unreconciled_amount = 50.00, Description = desc03 }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].Actual_records.Count);
            Assert.AreEqual(1, result[1].Actual_records.Count);
            Assert.AreEqual(1, result[2].Actual_records.Count);
        }

        [Test]
        public void Given_all_candidates_sum_to_less_than_target_then_only_one_result_is_returned()
        {
            // Arrange
            var amount_to_match = 55.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 20.00, Description = "Match01" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match02" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match03" },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match04" }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void Will_detect_that_permutations_are_not_duplicates_when_transactions_have_identical_amounts_and_descriptions_but_different_dates()
        {
            // Arrange
            var amount_to_match = 20.00;
            CredCard2Record transaction_to_match = new CredCard2Record
            {
                Amount = amount_to_match,
                Date = DateTime.Today
            };
            List<CredCard2InOutRecord> candidate_rows = new List<CredCard2InOutRecord> {
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01", Date = DateTime.Today.AddDays(1) },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01", Date = DateTime.Today.AddDays(2) },
                new CredCard2InOutRecord { Unreconciled_amount = 10.00, Description = "Match01", Date = DateTime.Today.AddDays(3) }
            };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(candidate_rows);
            _cred_card2_in_out_file.Load();
            var matcher = new MultipleAmountMatcher<CredCard2Record, CredCard2InOutRecord>();

            // Act
            var result = matcher.Find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Date == DateTime.Today.AddDays(1))));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Date == DateTime.Today.AddDays(2))));
            Assert.AreEqual(2, result.Count(x => x.Actual_records.Any(y => y.Date == DateTime.Today.AddDays(3))));
        }
    }
}
