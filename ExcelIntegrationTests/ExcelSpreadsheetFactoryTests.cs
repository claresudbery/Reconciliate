using System;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests
{
    [TestFixture]
    public class ExcelSpreadsheetFactoryTests
    {
        [Test]
        public void WillNotAttemptToDisposeNullSpreadsheet()
        {
            // Arrange
            var excelSpreadsheetFactory = new ExcelSpreadsheetRepoFactory("");

            // Act
            bool exceptionThrown = false;
            try
            {
                // Because we haven't called the Create method, spreadsheet should be null.
                excelSpreadsheetFactory.DisposeOfSpreadsheetRepo();
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