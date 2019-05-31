using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class IOFileTests
    {
        [Test]
        public void M_WillOnlyDeleteUnreconciledRowsAtTheLastMinute()
        {
            // Arrange
            var worksheetName = "worksheet";
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory());
            var mockFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var actualBankFile = new CSVFile<ActualBankRecord>(mockFileIO.Object);
            actualBankFile.Load();
            mockFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord> { new ActualBankRecord() });
            var mockSpreadsheet = new Mock<ISpreadsheet>();
            mockSpreadsheet
                .Setup(x => x.DeleteUnreconciledRows(worksheetName))
                .Verifiable();

            // Act
            fileIO.WriteBackToSpreadsheet(mockSpreadsheet.Object, actualBankFile, worksheetName);

            // Assert
            mockSpreadsheet.Verify();
        }
    }
}
