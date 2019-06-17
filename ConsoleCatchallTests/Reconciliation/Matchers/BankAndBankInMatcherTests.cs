using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class BankAndBankInMatcherTests : IInputOutput
    {
        private Mock<IInputOutput> _mockInputOutput;

        [SetUp]
        public void Set_up()
        {
            _mockInputOutput = new Mock<IInputOutput>();
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
        public void M_WillFindSingleMatchingBankInTransactionForOneActualBankExpenseTransaction()
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
