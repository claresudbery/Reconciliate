using System;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Spreadsheets
{
    [TestFixture]
    public class FakeSpreadsheetFactoryTests
    {
        [Test]
        public void WillNotAttemptToDisposeNullSpreadsheet()
        {
            // Arrange
            var fake_spreadsheet_factory = new FakeSpreadsheetRepoFactory();

            // Act
            bool exception_thrown = false;
            try
            {
                // Because we haven't called the Create method, spreadsheet should be null.
                fake_spreadsheet_factory.DisposeOfSpreadsheetRepo();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsFalse(exception_thrown, "Exception should not be thrown");
        }
    }
}