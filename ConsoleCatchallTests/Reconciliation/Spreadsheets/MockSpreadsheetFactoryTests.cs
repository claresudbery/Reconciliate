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
        public void Will_not_attempt_to_dispose_null_spreadsheet()
        {
            // Arrange
            ISpreadsheetRepo null_spreadsheet = null; 
            var mock_spreadsheet_factory = new MockSpreadsheetRepoFactory(nullSpreadsheet);

            // Act
            bool exception_thrown = false;
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