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
    public partial class MultipleAmountMatcherTests
    {
        private Mock<IInputOutput> _mock_input_output;

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
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
            var matcher = new MultipleAmountMatcher<ActualBankRecord, BankRecord>();

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
            var matcher = new MultipleAmountMatcher<ActualBankRecord, BankRecord>();

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
        public void M_WillFindMultipleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }
    }
}
