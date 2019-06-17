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
            ISpreadsheetRepo null_spreadsheet = null; 
            var mock_spreadsheet_factory = new MockSpreadsheetRepoFactory(null_spreadsheet);

            // Act
            bool exception_thrown = false;
            try
            {
                mock_spreadsheet_factory.DisposeOfSpreadsheetRepo();
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