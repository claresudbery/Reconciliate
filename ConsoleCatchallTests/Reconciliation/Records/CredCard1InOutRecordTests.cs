using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class CredCard1InOutRecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string expectedDateAsString = "19/12/2016";
            string csvLine = String.Format("{0}^£7.99^^ZZZSpecialDescription017^", expectedDateAsString);
            var expectedDate = Convert.ToDateTime(expectedDateAsString, StringHelper.Culture());

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
        }

        [Test]
        public void CanReadUnreconciledAmountFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            double expectedAmount = 7888.99;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountSurroundedByQuotesFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            double expectedAmount = 7888.99;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/12/2016^\"{0}\"^^ZZZSpecialDescription017^", inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountContainingCommaFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = "19/12/2016^£5,678.99^^ZZZSpecialDescription017^";

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(5678.99, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadDataFromCSVWithExtraSeparatorAtEnd()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var expectedDescription = "ZZZSpecialDescription017";
            string csvLine = String.Format("19/12/2016^£7.99^^{0}^^", expectedDescription);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var expectedDescription = "ZZZSpecialDescription017";
            string csvLine = String.Format("19/12/2016^£7.99^^{0}^", expectedDescription);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard1InOutRecord.Description);
        }

        [Test]
        public void CanReadReconciledAmountFromCSV()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var expectedAmount = 238.92;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^{0}", inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            string csvLine = String.Format("^£7.99^^ZZZSpecialDescription017^");

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
        }

        [Test]
        public void CanCopeWithEmptyUnreconciledAmount()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^^^ZZZSpecialDescription017^");

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyReconciledAmount()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^");

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            var badDate = "not a date";
            string csvLine = String.Format("{0}^£7.99^^ZZZSpecialDescription017^", badDate);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1InOutRecord.Date);
        }

        [Test]
        public void CanCopeWithBadUnreconciledAmount()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", badAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithBadReconciledAmount()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^{0}", badAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var negativeAmount = -23.23;
            string inputAmount = "-£" + negativeAmount * -1;
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", inputAmount);
            credCard1InOutRecord.Load(csvLine);

            // Act 
            credCard1InOutRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var positiveAmount = 23.23;
            string inputAmount = "£" + positiveAmount;
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", inputAmount);
            credCard1InOutRecord.Load(csvLine);

            // Act 
            credCard1InOutRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithoutMatchedRecord()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            credCard1InOutRecord.Load(csvLine);
            credCard1InOutRecord.Matched = false;

            // Act 
            string constructedCsvLine = credCard1InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/12/2016,£12.34,,\"Bantams\",£33.44,", constructedCsvLine);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithMatchedRecord()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            credCard1InOutRecord.Load(csvLine);
            credCard1InOutRecord.Matched = false;
            string matchedRecordCsvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE\",-12.33");
            var matchedRecord = new CredCard1Record();
            matchedRecord.Load(matchedRecordCsvLine);
            credCard1InOutRecord.Match = matchedRecord;

            // Act 
            string constructedCsvLine = credCard1InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/12/2016,£12.34,,\"Bantams\",£33.44,,,17/02/2017,£12.33,\"ANY STORE\"", constructedCsvLine);
        }

        [Test]
        public void EmptyFieldsAreOutputAsNothingForCsv()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^^^Bantams^^");
            credCard1InOutRecord.Load(csvLine);
            credCard1InOutRecord.Matched = false;

            // Act 
            string constructedCsvLine = credCard1InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/12/2016,,,\"Bantams\",,", constructedCsvLine);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something , something, something else";
            string csvLine = String.Format("19/12/2016^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something, something, something else";
            string csvLine = String.Format("19/12/2016^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something ,something else";
            string csvLine = String.Format("19/12/2016^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/12/2016^{0}^^Bantams^", amountContainingComma);
            credCard1InOutRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard1InOutRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("19/12/2016,\"{0}\",,\"Bantams\",,", amountContainingComma);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadUnreconciledAmountsContainingCommas()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", amountContainingComma);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard1InOutRecord.UnreconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadReconciledAmountsContainingCommas()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/12/2016^^^ZZZSpecialDescription017^{0}", amountContainingComma);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard1InOutRecord.ReconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadAmountsPrecededByPoundSigns()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var amountWithPoundSign = "£1,234.55";
            string csvLine = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", amountWithPoundSign);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard1InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var amountWithPoundSign = "£123.55";
            string csvLine = String.Format("19/12/2016^{0}^^Bantams^", amountWithPoundSign);
            credCard1InOutRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard1InOutRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("19/12/2016,{0},,\"Bantams\",,", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            var negativeAmount = "-£123.55";
            string csvLine = String.Format("19/12/2016^^^ZZZSpecialDescription017^{0}", negativeAmount);

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(-123.55, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = String.Empty;

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1InOutRecord.ReconciledAmount);
        }

        [Test]
        public void WillAddDefaultDescriptionIfDescriptionIsUnpopulated()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord();
            string csvLine = "19/12/2016^£123.55^^^";

            // Act 
            credCard1InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual("Source record had no description", credCard1InOutRecord.Description);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 12.34,
                Description = "Description",
                ReconciledAmount = 56.78
            };
            credCard1InOutRecord.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (CredCard1InOutRecord)credCard1InOutRecord.Copy();

            // Assert
            Assert.AreEqual(credCard1InOutRecord.Date, copiedRecord.Date);
            Assert.AreEqual(credCard1InOutRecord.UnreconciledAmount, copiedRecord.UnreconciledAmount);
            Assert.AreEqual(credCard1InOutRecord.Description, copiedRecord.Description);
            Assert.AreEqual(credCard1InOutRecord.ReconciledAmount, copiedRecord.ReconciledAmount);
            Assert.AreEqual(credCard1InOutRecord.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalUnreconciledAmount = 12.34;
            var originalDescription = "Description";
            var originalReconciledAmount = 56.78;
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                Date = originalDate,
                UnreconciledAmount = originalUnreconciledAmount,
                Description = originalDescription,
                ReconciledAmount = originalReconciledAmount
            };
            credCard1InOutRecord.UpdateSourceLineForOutput(',');
            var originalSourceLine = credCard1InOutRecord.SourceLine;

            // Act 
            var copiedRecord = (CredCard1InOutRecord)credCard1InOutRecord.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.UnreconciledAmount = copiedRecord.UnreconciledAmount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.ReconciledAmount = copiedRecord.ReconciledAmount + 1;
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, credCard1InOutRecord.Date);
            Assert.AreEqual(originalUnreconciledAmount, credCard1InOutRecord.UnreconciledAmount);
            Assert.AreEqual(originalDescription, credCard1InOutRecord.Description);
            Assert.AreEqual(originalReconciledAmount, credCard1InOutRecord.ReconciledAmount);
            Assert.AreEqual(originalSourceLine, credCard1InOutRecord.SourceLine);
        }
    }
}
