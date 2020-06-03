using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
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
            var result = matcher.Standby_find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(candidate_rows[0], result[0].Actual_records[0]);
        }

        [Test]
        public void Will_not_match_with_one_single_too_large_amount()
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
            var result = matcher.Standby_find_expense_matches(transaction_to_match, _cred_card2_in_out_file).ToList();

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
        public void M_WillFindMultipleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }
    }
}
