using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Will_find_balance_row_when_most_recent_transaction_has_no_balance()
        {
            // Arrange
            string balance_row_description = "Balance row";
            var fake_records = new List<ActualBankRecord>
            {
                // pending record with no balance
                new ActualBankRecord {Amount = -10, Balance = 0.00, Date = new DateTime(2020, 1, 5)},
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 940.00, Date = new DateTime(2020, 1, 4), Description = balance_row_description},
                new ActualBankRecord {Amount = -10, Balance = 950.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 960.00, Date = new DateTime(2020, 1, 4)},
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_potential_balance_rows();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(balance_row_description, result.ToList()[0].Description);
        }

        [Test]
        public void Will_find_balance_row_when_records_are_not_in_correct_order()
        {
            // Arrange
            string balance_row_description = "Balance row";
            var fake_records = new List<ActualBankRecord>
            {
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 910.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 920.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 900.00, Date = new DateTime(2020, 1, 4), Description = balance_row_description},
                // middle records
                new ActualBankRecord {Amount = -10, Balance = 930.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 940.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 950.00, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Balance = 960.00, Date = new DateTime(2020, 1, 2)},
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1), Description = "First row"},
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_potential_balance_rows();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(balance_row_description, result.ToList()[0].Description);
        }

        [Test]
        public void Will_find_multiple_balance_rows_when_result_is_ambiguous_because_candidates_exist_with_same_balance()
        {
            // Arrange
            string balance_row_desc_01 = "Potential balance row 01";
            string balance_row_desc_02 = "Potential balance row 02";
            var fake_records = new List<ActualBankRecord>
            {
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 900.00, Date = new DateTime(2020, 1, 4), Description = balance_row_desc_02},
                new ActualBankRecord {Amount = 10, Balance = 910.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 900.00, Date = new DateTime(2020, 1, 4), Description = balance_row_desc_01},
                new ActualBankRecord {Amount = -10, Balance = 910.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 920.00, Date = new DateTime(2020, 1, 4)},
                // middle records
                new ActualBankRecord {Amount = -10, Balance = 930.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 940.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 950.00, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Balance = 960.00, Date = new DateTime(2020, 1, 2)},
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_potential_balance_rows();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(x => x.Description == balance_row_desc_01));
            Assert.IsTrue(result.Any(x => x.Description == balance_row_desc_02));
        }

        [Test]
        public void Will_find_multiple_balance_rows_when_result_is_ambiguous_because_multiple_early_records_exist_that_point_to_different_recent_records()
        {
            // Arrange
            string balance_row_desc_01 = "Potential balance row 01";
            string balance_row_desc_02 = "Potential balance row 02";
            var fake_records = new List<ActualBankRecord>
            {
                // most recent records
                new ActualBankRecord {Amount = -10, Balance = 900.00, Date = new DateTime(2020, 1, 4), Description = balance_row_desc_02},
                new ActualBankRecord {Amount = -10, Balance = 910.00, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Balance = 920.00, Date = new DateTime(2020, 1, 4), Description = balance_row_desc_01},
                // middle records
                new ActualBankRecord {Amount = -10, Balance = 930.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 940.00, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = -10, Balance = 950.00, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Balance = 960.00, Date = new DateTime(2020, 1, 2)},
                // earliest records
                new ActualBankRecord {Amount = -10, Balance = 970.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -20, Balance = 980.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -20, Balance = 1000.00, Date = new DateTime(2020, 1, 1), Description = "This record could point at balance_row_desc_01"},
                new ActualBankRecord {Amount = 30, Balance = 1020.00, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Balance = 990.00, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_potential_balance_rows();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(x => x.Description == balance_row_desc_01));
            Assert.IsTrue(result.Any(x => x.Description == balance_row_desc_02));
        }

        [Test]
        public void Will_find_last_bank_out_row_when_latest_records_come_first()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                // most recent records
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_last_bank_out_row();

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
                // most recent records
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_last_bank_out_row();

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
                // most recent records
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 1)},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 3)},
            };
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.Records).Returns(fake_records);
            var actual_bank_out_file = new ActualBankOutFile(mock_actual_bank_file.Object);

            // Act
            var result = actual_bank_out_file.Get_last_bank_out_row();

            // Assert
            Assert.AreEqual(last_row_description, result.Description);
        }
    }
}
