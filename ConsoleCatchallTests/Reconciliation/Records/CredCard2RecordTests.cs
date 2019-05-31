using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Records;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class CredCard2RecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string expectedDateAsString = "08/06/2017";
            string csvLine = String.Format("{0},ref,13.49,ACME UK HOLDINGS", expectedDateAsString);
            var expectedDate = DateTime.ParseExact(expectedDateAsString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2Record.Date);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var expectedDescription = "ACME UK HOLDINGS";
            string csvLine = String.Format("08/06/2017,ref,13.49,{0},", expectedDescription);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard2Record.Description);
        }

        [Test]
        public void CanReadAmountFromCSVWithQuotesAndLeadingSpace()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var expectedAmount = 1.94;
            string inputAmount = "\" " + expectedAmount + "\"";
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS,General Purchases", inputAmount);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2Record.Amount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var expectedDate = new DateTime(9999, 9, 9);
            string csvLine = String.Format(",ref,13.49,ACME UK HOLDINGS");

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2Record.Date);
        }

        [Test]
        public void CanCopeWithEmptyDescription()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string csvLine = String.Format("08/06/2017,ref,13.49,");

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual("", credCard2Record.Description);
        }

        [Test]
        public void CanCopeWithEmptyAmount()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string csvLine = String.Format("08/06/2017,ref,,ACME UK HOLDINGS");

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2Record.Amount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var expectedDate = new DateTime(9999, 9, 9);
            var badDate = "not a date";
            string csvLine = String.Format("{0},ref,13.49,ACME UK HOLDINGS", badDate);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2Record.Date);
        }

        [Test]
        public void CanCopeWithBadAmount()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var badAmount = "not an amount";
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", badAmount);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2Record.Amount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var negativeAmount = -23.23;
            string inputAmount = "-" + negativeAmount * -1;
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", inputAmount);
            credCard2Record.Load(csvLine);

            // Act 
            credCard2Record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, credCard2Record.Amount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var positiveAmount = 23.23;
            string inputAmount = positiveAmount.ToString();
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", inputAmount);
            credCard2Record.Load(csvLine);

            // Act 
            credCard2Record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, credCard2Record.Amount);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string csvLine = String.Format("08/06/2017,ref,13.49,ACME UK HOLDINGS");
            credCard2Record.Load(csvLine);
            credCard2Record.Matched = false;

            // Act 
            string constructedCsvLine = credCard2Record.ToCsv();

            // Assert
            Assert.AreEqual("08/06/2017,£13.49,\"ACME UK HOLDINGS\"", constructedCsvLine);
        }

        // This doesn't apply to CredCard2Record and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void CommasInInputAreReplacedBySemiColons()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string textContainingCommas = "\"something, something, something else\"";
            string csvLine = String.Format("08/06/2017,ref,13.49,{0}", textContainingCommas);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual("\"something; something; something else\"", credCard2Record.Description);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var amount = "123.55";
            var amountWithPoundSign = "£" + amount;
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", amount);
            credCard2Record.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard2Record.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("08/06/2017,{0},\"ACME UK HOLDINGS\"", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            var negativeAmount = "-123.55";
            string csvLine = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", negativeAmount);

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(-123.55, credCard2Record.Amount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var credCard2Record = new CredCard2Record();
            string csvLine = String.Empty;

            // Act 
            credCard2Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2Record.Amount);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var credCard2Record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description"
            };
            credCard2Record.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (CredCard2Record)credCard2Record.Copy();

            // Assert
            Assert.AreEqual(credCard2Record.Date, copiedRecord.Date);
            Assert.AreEqual(credCard2Record.Amount, copiedRecord.Amount);
            Assert.AreEqual(credCard2Record.Description, copiedRecord.Description);
            Assert.AreEqual(credCard2Record.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalAmount = 12.34;
            var originalDescription = "Description";
            var credCard2Record = new CredCard2Record
            {
                Date = originalDate,
                Amount = originalAmount,
                Description = originalDescription,
            };
            credCard2Record.UpdateSourceLineForOutput(',');
            var originalSourceLine = credCard2Record.SourceLine;

            // Act 
            var copiedRecord = (CredCard2Record)credCard2Record.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.Amount = copiedRecord.Amount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, credCard2Record.Date);
            Assert.AreEqual(originalAmount, credCard2Record.Amount);
            Assert.AreEqual(originalDescription, credCard2Record.Description);
            Assert.AreEqual(originalSourceLine, credCard2Record.SourceLine);
        }
    }
}
