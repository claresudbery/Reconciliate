using System;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Spreadsheets
{
    [TestFixture]
    public class MockSpreadsheetFactoryTests
    {
        [Test]
        public void WillNotAttemptToDisposeNullSpreadsheet()
        {
            // Arrange
            ISpreadsheetRepo nullSpreadsheet = null; 
            var mockSpreadsheetFactory = new MockSpreadsheetRepoFactory(nullSpreadsheet);

            // Act
            bool exceptionThrown = false;
            try
            {
                mockSpreadsheetFactory.DisposeOfSpreadsheetRepo();
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