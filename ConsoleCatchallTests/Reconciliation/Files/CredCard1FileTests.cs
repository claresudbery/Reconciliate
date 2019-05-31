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
        public void WillSwapSignsOnAmountsWhenLoading()
        {
            // Arrange
            var mockCredCard1File = new Mock<ICSVFile<CredCard1Record>>();
            var credCard1File = new CredCard1File(mockCredCard1File.Object);

            // Act
            credCard1File.Load();

            // Assert
            mockCredCard1File.Verify(x => x.SwapSignsOfAllAmounts());
        }
    }
}
