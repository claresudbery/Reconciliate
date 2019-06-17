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
            var worksheet_name = "worksheet";
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory());
            var mock_file_io = new Mock<IFileIO<ActualBankRecord>>();
            var actual_bank_file = new CSVFile<ActualBankRecord>(mock_file_io.Object);
            actual_bank_file.Load();
            mock_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord> { new ActualBankRecord() });
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            mock_spreadsheet
                .Setup(x => x.Delete_unreconciled_rows(worksheet_name))
                .Verifiable();

            // Act
            file_io.Write_back_to_spreadsheet(mock_spreadsheet.Object, actual_bank_file, worksheet_name);

            // Assert
            mock_spreadsheet.Verify();
        }
    }
}
