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
            var fakeSpreadsheetFactory = new FakeSpreadsheetRepoFactory();

            // Act
            bool exceptionThrown = false;
            try
            {
                // Because we haven't called the Create method, spreadsheet should be null.
                fakeSpreadsheetFactory.DisposeOfSpreadsheetRepo();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsFalse(exceptionThrown, "Exception should not be thrown");
        }
    }
}