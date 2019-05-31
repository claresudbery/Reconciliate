using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class ActualBankRecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string expectedDateAsString = "06/03/2017";
            string csvLine = string.Format("{0},BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expectedDateAsString);
            var expectedDate = Convert.ToDateTime(expectedDateAsString, StringHelper.Culture());

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, actualBankRecord.Date);
        }

        [Test]
        public void CanReadTypeFromCSV()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedType = "BAC";
            string csvLine = String.Format("06/03/2017,{0},\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expectedType);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedType, actualBankRecord.Type);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedDescription = "\"'99999999-BFGH\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expectedDescription);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, actualBankRecord.Description);
        }

        [Test]
        public void CanReadAmountFromCSVWithoutPoundSign()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedAmount = 261.40;
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", expectedAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, actualBankRecord.Amount);
        }

        [Test]
        public void CanReadAmountFromCSVWithPoundSign()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedAmount = 261.40;
            var inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", inputAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, actualBankRecord.Amount);
        }

        [Test]
        public void CanReadBalanceFromCSVWithoutPoundSign()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedAmount = 4273.63;
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", expectedAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, actualBankRecord.Balance);
        }

        [Test]
        public void CanReadBalanceFromCSVWithPoundSign()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedAmount = 427.63;
            var inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", inputAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, actualBankRecord.Balance);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            string csvLine = String.Format(",BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, actualBankRecord.Date);
        }

        [Test]
        public void CanCopeWithEmptyType()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Format("06/03/2017,,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual("", actualBankRecord.Type);
        }

        [Test]
        public void CanCopeWithEmptyDescription()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Format("06/03/2017,BAC,,261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual("", actualBankRecord.Description);
        }

        [Test]
        public void CanCopeWithEmptyAmount()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, actualBankRecord.Amount);
        }

        [Test]
        public void CanCopeWithEmptyBalance()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, actualBankRecord.Balance);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            var badDate = "not a date";
            string csvLine = String.Format("{0},BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", badDate);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, actualBankRecord.Date);
        }

        [Test]
        public void CanCopeWithBadAmount()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", badAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, actualBankRecord.Amount);
        }

        [Test]
        public void CanCopeWithBadBalance()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", badAmount);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, actualBankRecord.Balance);
        }

        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            double expectedBalance = 12.35;
            string textContainingCommas = "\"'0363 23MAR17 C ,TFL.GOV.UK/CP , TFL TRAVEL CH GB\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", textContainingCommas, expectedBalance);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedBalance, actualBankRecord.Balance);
        }

        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            double expectedBalance = 12.35;
            string textContainingCommas = "\"'0363 23MAR17 C, TFL.GOV.UK/CP, TFL TRAVEL CH GB\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", textContainingCommas, expectedBalance);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedBalance, actualBankRecord.Balance);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Empty;

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, actualBankRecord.Amount);
        }

        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            double expectedBalance = 12.35;
            string textContainingCommas = "\"'0363 23MAR17 C ,TFL.GOV.UK/CP ,TFL TRAVEL CH GB\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", textContainingCommas, expectedBalance);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedBalance, actualBankRecord.Balance);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void CommasInInputAreReplacedBySemiColons()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string textContainingCommas = "\"something, something, something else\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},261.40,12.35,\"'Envelope\",\"'228822-99933422\",", textContainingCommas);

            // Act 
            actualBankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual("\"something; something; something else\"", actualBankRecord.Description);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var negativeAmount = -23.23;
            string csvLine = String.Format("06/03/2017,BAC,\"'0363 23MAR17 C , TFL.GOV.UK/CP , TFL TRAVEL CH GB\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", negativeAmount);
            actualBankRecord.Load(csvLine);

            // Act 
            actualBankRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, actualBankRecord.Amount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var positiveAmount = 23.23;
            string csvLine = String.Format("06/03/2017,BAC,\"'0363 23MAR17 C , TFL.GOV.UK/CP , TFL TRAVEL CH GB\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", positiveAmount);
            actualBankRecord.Load(csvLine);

            // Act 
            actualBankRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, actualBankRecord.Amount);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            string csvLine = String.Format("06/03/2017,BAC,\"'Some , description\",127.69,261.40,\"'Envelope\",\"'228822-99933422\",");
            actualBankRecord.Load(csvLine);
            actualBankRecord.Matched = false;

            // Act 
            string constructedCsvLine = actualBankRecord.ToCsv();

            // Assert
            Assert.AreEqual("06/03/2017,£127.69,BAC,\"'Some ; description\"", constructedCsvLine);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord and CredCard2Record because the input is never encased in quotes.
        [Test]
        public void IfInputIsEncasedInQuotesThenOutputOnlyHasOneEncasingSetOfQuotes()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var descriptionEncasedInOneSetOfQuotes = "\"'Some description\"";
            string csvLine = String.Format("06/03/2017,BAC,{0},127.69,261.40,\"'Envelope\",\"'228822-99933422\",", descriptionEncasedInOneSetOfQuotes);
            actualBankRecord.Load(csvLine);
            actualBankRecord.Matched = false;

            // Act 
            string constructedCsvLine = actualBankRecord.ToCsv();

            // Assert
            var expectedCsvLine = String.Format("06/03/2017,£127.69,BAC,{0}", descriptionEncasedInOneSetOfQuotes);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var amount = 1234.55;
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("06/03/2017,BAC,\"description\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", amount);
            actualBankRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = actualBankRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("06/03/2017,\"{0}\",BAC,\"description\"", amountContainingComma);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord();
            var amount = "123.55";
            var amountWithPoundSign = "£" + amount;
            string csvLine = String.Format("06/03/2017,BAC,\"description\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", amount);
            actualBankRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = actualBankRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("06/03/2017,{0},BAC,\"description\"", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void WhenCopyingCredCard2RecordWillCopyAllImportantData()
        {
            // Arrange
            var actualBankRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description",
                Type = "Type"
            };
            actualBankRecord.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (ActualBankRecord)actualBankRecord.Copy();

            // Assert
            Assert.AreEqual(actualBankRecord.Date, copiedRecord.Date);
            Assert.AreEqual(actualBankRecord.Amount, copiedRecord.Amount);
            Assert.AreEqual(actualBankRecord.Description, copiedRecord.Description);
            Assert.AreEqual(actualBankRecord.Type, copiedRecord.Type);
            Assert.AreEqual(actualBankRecord.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingCredCard2RecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalAmount = 12.34;
            var originalDescription = "Description";
            var originalType = "Type";
            var actualBankRecord = new ActualBankRecord
            {
                Date = originalDate,
                Amount = originalAmount,
                Description = originalDescription,
                Type = originalType
            };
            actualBankRecord.UpdateSourceLineForOutput(',');
            var originalSourceLine = actualBankRecord.SourceLine;

            // Act 
            var copiedRecord = (ActualBankRecord)actualBankRecord.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.Amount = copiedRecord.Amount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.Type = copiedRecord.Type + "something else";
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, actualBankRecord.Date);
            Assert.AreEqual(originalAmount, actualBankRecord.Amount);
            Assert.AreEqual(originalDescription, actualBankRecord.Description);
            Assert.AreEqual(originalType, actualBankRecord.Type);
            Assert.AreEqual(originalSourceLine, actualBankRecord.SourceLine);
        }
    }
}
