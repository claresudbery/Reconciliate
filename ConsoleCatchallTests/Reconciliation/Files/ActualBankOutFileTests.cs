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
    }
}
