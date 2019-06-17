using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ExpectedIncomeFileTests
    {
        [Test]
        public void WillFilterForEmployerExpenseRecordsWhenLoading()
        {
            // Arrange
            var mock_expected_income_file = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expected_income_file = new ExpectedIncomeFile(mock_expected_income_file.Object);

            // Act
            expected_income_file.Load();

            // Assert
            mock_expected_income_file.Verify(x => x.RemoveRecords(It.IsAny<System.Predicate<ExpectedIncomeRecord>>()));
        }

        [Test]
        public void CanFilterForEmployerExpenseRecordsOnly()
        {
            // Arrange
            var mock_expected_income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(mock_expected_income_file_io.Object);
            var expected_description = "description2";
            var expected_income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = expected_description,
                    Code = Codes.Expenses
                },
                new ExpectedIncomeRecord
                {
                    Description = "description1",
                    Code = "other"
                }
            };
            mock_expected_income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_income_records);
            expected_income_csv_file.Load();
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);

            // Act
            expected_income_file.FilterForEmployerExpensesOnly();

            // Assert
            Assert.AreEqual(1, expected_income_csv_file.Records.Count);
            Assert.AreEqual(expected_description, expected_income_csv_file.Records[0].Description);
        }

        [Test]
        public void WillCopyAllRecordsToPendingFile()
        {
            // Arrange
            var mock_expected_income_file = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expected_income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = "description1"
                },
                new ExpectedIncomeRecord
                {
                    Description = "description2"
                }
            };
            mock_expected_income_file.Setup(x => x.Records).Returns(expected_income_records);
            var expected_income_file = new ExpectedIncomeFile(mock_expected_income_file.Object);
            var mock_pending_file_io = new Mock<IFileIO<BankRecord>>();
            mock_pending_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var pending_file = new CSVFile<BankRecord>(mock_pending_file_io.Object);
            pending_file.Load();
            Assert.AreEqual(0, pending_file.Records.Count);

            // Act
            expected_income_file.CopyToPendingFile(pending_file);

            // Assert
            Assert.AreEqual(2, pending_file.Records.Count);
            Assert.AreEqual(expected_income_file.File.Records[0].Description, pending_file.Records[0].Description);
            Assert.AreEqual(expected_income_file.File.Records[1].Description, pending_file.Records[1].Description);
        }
    }
}
