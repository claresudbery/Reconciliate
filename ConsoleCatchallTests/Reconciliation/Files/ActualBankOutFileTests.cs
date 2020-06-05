using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ActualBankOutFileTests
    {
        [Test]
        public void Will_filter_for_negative_records_when_loading()
        {
            // Arrange
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            actual_bank_out_file.Load();

            // Assert
            mock_actual_bank_file.Verify(x => x.Filter_for_negative_records_only());
        }

        [Test]
        public void Will_find_balance_row()
        {
            // Arrange
            string balance_row_description = "Balance row";
            var fake_records = new List<ActualBankRecord>
            {
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 1)},
                // middle records
                new ActualBankRecord {Amount = -10, Balance = 960.00, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Balance = 950.00, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Balance = 940.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 930.00, Date = new DateTime(2020, 1, 3)},
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 900.00, Date = new DateTime(2020, 1, 4), Description = balance_row_description},
                new ActualBankRecord {Amount = -10, Balance = 920.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 910.00, Date = new DateTime(2020, 1, 4)},
                // pending record with no balance
                new ActualBankRecord {Amount = -10, Balance = 0.00, Date = new DateTime(2020, 1, 5)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_balance_row();

            // Assert
            Assert.AreEqual(balance_row_description, result.Description);
        }
    }
}
