using System;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Spreadsheets
{
    [TestFixture]
    public class FakeSpreadsheetFactoryTests
    {
        [Test]
        public void Will_not_attempt_to_dispose_null_spreadsheet()
        {
            // Arrange
            var fake_spreadsheet_factory = new FakeSpreadsheetRepoFactory();

            // Act
            bool exception_thrown = false;
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