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
            var mock_spreadsheet_factory = new MockSpreadsheetRepoFactory(null_spreadsheet);

            // Act
            bool exception_thrown = false;
            try
            {
                mock_spreadsheet_factory.Dispose_of_spreadsheet_repo();
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