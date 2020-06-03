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
        public void M_WhenReconcilingExpenses_WillMatchOnASingleAmount()
        {
            // Arrange
            var expense_amount = 10.00;
            List<CredCard2InOutRecord> expected_in_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord
            {
                Unreconciled_amount = expense_amount,
                Description = "HELLOW"
            } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_in_rows);
            _cred_card2_in_out_file.Load();
            ActualBankRecord expense_transaction = new ActualBankRecord { Amount = expense_amount };
            var matcher = new MultipleAmountMatcher<ActualBankRecord, CredCard2InOutRecord>();

            // Act
            var result = matcher.Standby_find_expense_matches(expense_transaction, _cred_card2_in_out_file).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected_in_rows[0], result[0].Actual_records[0]);
        }

        [Test]
        public void M_WhenReconcilingExpenses_WillNotMatchOnASingleDifferentAmount()
        {
            // Arrange
            var expense_amount = 10.00;
            List<CredCard2InOutRecord> expected_in_rows = new List<CredCard2InOutRecord> { new CredCard2InOutRecord { Unreconciled_amount = expense_amount } };
            _cred_card2_in_out_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_in_rows);
            _cred_card2_in_out_file.Load();
            ActualBankRecord expense_transaction = new ActualBankRecord { Amount = expense_amount - 1 };
            var matcher = new MultipleAmountMatcher<ActualBankRecord, CredCard2InOutRecord>();

            // Act
            var result = matcher.Standby_find_expense_matches(expense_transaction, _cred_card2_in_out_file).ToList();

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
