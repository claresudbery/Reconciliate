using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class CredCard1FileTests
    {
        [Test]
        public void Will_swap_signs_on_amounts_when_loading()
        {
            // Arrange
            var mock_cred_card1_file = new Mock<ICSVFile<CredCard1Record>>();
            var cred_card1_file = new CredCard1File(mock_cred_card1_file.Object);

            // Act
            cred_card1_file.Load();

            // Assert
            mock_cred_card1_file.Verify(x => x.Swap_signs_of_all_amounts());
        }
    }
}
