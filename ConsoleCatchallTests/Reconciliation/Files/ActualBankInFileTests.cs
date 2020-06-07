using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ActualBankInFileTests
    {
        [Test]
        public void Will_filter_for_positive_records_when_loading()
        {
            // Arrange
            var mock_actual_bank_file = new Mock<ICSVFile<ActualBankRecord>>();
            mock_actual_bank_file.Setup(x => x.SourceRecords).Returns(new List<ActualBankRecord>());
            var actual_bank_in_file = new ActualBankInFile(mock_actual_bank_file.Object);

            // Act
            actual_bank_in_file.Load();

            // Assert
            mock_actual_bank_file.Verify(x => x.Filter_for_positive_records_only());
        }

        [Test]
        public void Will_mark_last_bank_in_row_when_loading_given_file_contains_bank_out_transactions_too()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 4)},
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = -10, Date = new DateTime(2020, 1, 3)},
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 2)},
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 1)},
            };
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), It.IsAny<char?>()))
                .Returns(fake_records);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            var actual_bank_in_file = new ActualBankInFile(actual_bank_file);

            // Act
            actual_bank_in_file.Load();

            // Assert
            var last_record = actual_bank_in_file.File.Records.First(x => x.Description == last_row_description);
            Assert.AreEqual(ReconConsts.LastOnlineTransaction, last_record.LastTransactionMarker);
        }

        [Test]
        public void Will_mark_last_bank_out_row_when_loading_before_records_are_ordered_in_date_order()
        {
            // Arrange
            string last_row_description = "Last row";
            var fake_records = new List<ActualBankRecord>
            {
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 4), Description = last_row_description},
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 4), Description = "This will be the last row when ordered in date order."},
                new ActualBankRecord {Amount = 10, Date = new DateTime(2020, 1, 2)},
            };
            var mock_actual_bank_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_actual_bank_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), It.IsAny<char?>()))
                .Returns(fake_records);
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_actual_bank_file_io.Object);
            var actual_bank_in_file = new ActualBankInFile(actual_bank_file);

            // Act
            actual_bank_in_file.Load();

            // Assert
            var last_record = actual_bank_in_file.File.Records.First(x => x.Description == last_row_description);
            Assert.AreEqual(ReconConsts.LastOnlineTransaction, last_record.LastTransactionMarker);
        }
    }
}
