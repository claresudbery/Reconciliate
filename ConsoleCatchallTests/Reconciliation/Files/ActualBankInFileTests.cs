using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ActualBankInFileTests
    {
        [Test]
        public void WillFilterForPositiveRecordsWhenLoading()
        {
            // Arrange
            var mockActualBankFile = new Mock<ICSVFile<ActualBankRecord>>();
            var actualBankInFile = new ActualBankInFile(mockActualBankFile.Object);

            // Act
            actualBankInFile.Load();

            // Assert
            mockActualBankFile.Verify(x => x.FilterForPositiveRecordsOnly());
        }
    }
}
