using System;
using ExcelLibrary;
using NUnit.Framework;

namespace ExcelIntegrationTests
{
    [TestFixture]
    public class ExcelSpreadsheetFactoryTests
    {
        [Test]
        public void Will_not_attempt_to_dispose_null_spreadsheet()
        {
            // Arrange
            var excel_spreadsheet_factory = new ExcelSpreadsheetRepoFactory("");

            // Act
            bool exception_thrown = false;
            try
            {
                // Because we haven't called the Create method, spreadsheet should be null.
                excel_spreadsheet_factory.Dispose_of_spreadsheet_repo();
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