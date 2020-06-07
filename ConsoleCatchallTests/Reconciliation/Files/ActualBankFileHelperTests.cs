using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ActualBankFileHelperTests
    {

        [Test]
        public void Will_find_last_bank_out_row_when_latest_records_come_first()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
            };

            // Act
            var result = ActualBankFileHelper.Get_last_bank_out_row(fake_records);

            // Assert
            Assert.AreEqual(last_row_description, result.Description);
        }

        [Test]
        public void Will_find_last_bank_out_row_when_latest_records_come_last()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
            };

            // Act
            var result = ActualBankFileHelper.Get_last_bank_out_row(fake_records);

            // Assert
            Assert.AreEqual(last_row_description, result.Description);
        }

        [Test]
        public void Will_find_last_bank_out_row_when_records_are_not_in_order()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 3)},
            };

            // Act
            var result = ActualBankFileHelper.Get_last_bank_out_row(fake_records);

            // Assert
            Assert.AreEqual(last_row_description, result.Description);
        }
    }
}