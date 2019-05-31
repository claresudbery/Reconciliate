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
        public void WillFilterForNegativeRecordsWhenLoading()
        {
            // Arrange
            var mockActualBankFile = new Mock<ICSVFile<ActualBankRecord>>();
            var actualBankOutFile = new ActualBankOutFile(mockActualBankFile.Object);

            // Act
            actualBankOutFile.Load();

            // Assert
            mockActualBankFile.Verify(x => x.FilterForNegativeRecordsOnly());
        }
    }
}
