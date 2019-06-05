using System;
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
            var mockExpectedIncomeFile = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expectedIncomeFile = new ExpectedIncomeFile(mockExpectedIncomeFile.Object);

            // Act
            expectedIncomeFile.Load();

            // Assert
            mockExpectedIncomeFile.Verify(x => x.RemoveRecords(It.IsAny<System.Predicate<ExpectedIncomeRecord>>()));
        }

        [Test]
        public void CanFilterForEmployerExpenseRecordsOnly()
        {
            // Arrange
            var mockExpectedIncomeFileIO = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var expectedIncomeCSVFile = new CSVFile<ExpectedIncomeRecord>(mockExpectedIncomeFileIO.Object);
            var expectedDescription = "description2";
            var expectedIncomeRecords = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = expectedDescription,
                    Code = Codes.Expenses
                },
                new ExpectedIncomeRecord
                {
                    Description = "description1",
                    Code = "other"
                }
            };
            mockExpectedIncomeFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expectedIncomeRecords);
            expectedIncomeCSVFile.Load();
            var expectedIncomeFile = new ExpectedIncomeFile(expectedIncomeCSVFile);

            // Act
            expectedIncomeFile.FilterForEmployerExpensesOnly();

            // Assert
            Assert.AreEqual(1, expectedIncomeCSVFile.Records.Count);
            Assert.AreEqual(expectedDescription, expectedIncomeCSVFile.Records[0].Description);
        }

        [Test]
        public void WillCopyAllRecordsToPendingFile()
        {
            // Arrange
            var mockExpectedIncomeFile = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expectedIncomeRecords = new List<ExpectedIncomeRecord>
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
            mockExpectedIncomeFile.Setup(x => x.Records).Returns(expectedIncomeRecords);
            var expectedIncomeFile = new ExpectedIncomeFile(mockExpectedIncomeFile.Object);
            var mockPendingFileIO = new Mock<IFileIO<BankRecord>>();
            mockPendingFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var pendingFile = new CSVFile<BankRecord>(mockPendingFileIO.Object);
            pendingFile.Load();
            Assert.AreEqual(0, pendingFile.Records.Count);

            // Act
            expectedIncomeFile.CopyToPendingFile(pendingFile);

            // Assert
            Assert.AreEqual(2, pendingFile.Records.Count);
            Assert.AreEqual(expectedIncomeFile.File.Records[0].Description, pendingFile.Records[0].Description);
            Assert.AreEqual(expectedIncomeFile.File.Records[1].Description, pendingFile.Records[1].Description);
        }
    }
}
