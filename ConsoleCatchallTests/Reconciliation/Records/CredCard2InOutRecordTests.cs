using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class CredCard2InOutRecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string expectedDateAsString = "01/04/2017";
            string csvLine = String.Format("{0}^£13.48^^Acme: Esmerelda's birthday^", expectedDateAsString);
            var expectedDate = Convert.ToDateTime(expectedDateAsString, StringHelper.Culture());

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2InOutRecord.Date);
        }

        [Test]
        public void CanReadUnreconciledAmountFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedAmount = 13.95;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountSurroundedByQuotesFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            double expectedAmount = 7888.99;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/12/2016^\"{0}\"^^ZZZSpecialDescription017^", inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountContainingCommaFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = "19/12/2016^£5,678.99^^ZZZSpecialDescription017^";

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(5678.99, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadDataFromCSVWithExtraSeparatorAtEnd()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedDescription = "ZZZSpecialDescription017";
            string csvLine = String.Format("19/12/2016^£7.99^^{0}^^", expectedDescription);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard2InOutRecord.Description);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedDescription = "Acme: Esmerelda's birthday";
            string csvLine = String.Format("19/04/2017^£13.48^^{0}^", expectedDescription);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard2InOutRecord.Description);
        }

        [Test]
        public void CanReadReconciledAmountFromCSV()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedAmount = 238.92;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            string csvLine = String.Format("^£13.48^^Acme: Esmerelda's birthday^");

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2InOutRecord.Date);
        }

        [Test]
        public void CanCopeWithEmptyUnreconciledAmount()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyReconciledAmount()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var expectedDate = new DateTime(9999, 9, 9);
            var badDate = "not a date";
            string csvLine = String.Format("{0}^£13.48^^Acme: Esmerelda's birthday^", badDate);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard2InOutRecord.Date);
        }

        [Test]
        public void CanCopeWithBadUnreconciledAmount()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", badAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithBadReconciledAmount()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", badAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var negativeAmount = -23.23;
            string inputAmount = "-£" + negativeAmount * -1;
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", inputAmount);
            credCard2InOutRecord.Load(csvLine);

            // Act 
            credCard2InOutRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var positiveAmount = 23.23;
            string inputAmount = "£" + positiveAmount;
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", inputAmount);
            credCard2InOutRecord.Load(csvLine);

            // Act 
            credCard2InOutRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, credCard2InOutRecord.UnreconciledAmount);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithoutMatchedRecord()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^£33.44");
            credCard2InOutRecord.Load(csvLine);
            credCard2InOutRecord.Matched = true;

            // Act 
            string constructedCsvLine = credCard2InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,£13.48,x,\"Acme: Esmerelda's birthday\",£33.44,", constructedCsvLine);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithMatchedRecord()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^£33.44");
            credCard2InOutRecord.Load(csvLine);
            credCard2InOutRecord.Matched = true;
            string matchedRecordCsvLine = String.Format("08/06/2017,ref,13.49,ACME UK HOLDINGS");
            var matchedRecord = new CredCard2Record();
            matchedRecord.Load(matchedRecordCsvLine);
            credCard2InOutRecord.Match = matchedRecord;

            // Act 
            string constructedCsvLine = credCard2InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,£13.48,x,\"Acme: Esmerelda's birthday\",£33.44,,,08/06/2017,£13.49,\"ACME UK HOLDINGS\"", constructedCsvLine);
        }

        [Test]
        public void EmptyFieldsAreOutputAsNothingForCsv()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");
            credCard2InOutRecord.Load(csvLine);
            credCard2InOutRecord.Matched = true;

            // Act 
            string constructedCsvLine = credCard2InOutRecord.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,,x,\"Acme: Esmerelda's birthday\",,", constructedCsvLine);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something , something, something else";
            string csvLine = String.Format("19/04/2017^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something, something, something else";
            string csvLine = String.Format("19/04/2017^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something ,something else";
            string csvLine = String.Format("19/04/2017^^^{0}^{1}", textContainingCommas, inputAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amountContainingComma);
            credCard2InOutRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard2InOutRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("19/04/2017,\"{0}\",,\"Acme: Esmerelda's birthday\",,", amountContainingComma);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadUnreconciledAmountsContainingCommas()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amountContainingComma);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard2InOutRecord.UnreconciledAmount);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadReconciledAmountsContainingCommas()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", amountContainingComma);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard2InOutRecord.ReconciledAmount);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadAmountsPrecededByPoundSigns()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var amountWithPoundSign = "£1,234.55";
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", amountWithPoundSign);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var amountWithPoundSign = "£123.55";
            string csvLine = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amountWithPoundSign);
            credCard2InOutRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard2InOutRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("19/04/2017,{0},,\"Acme: Esmerelda's birthday\",,", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            var negativeAmount = "-£123.55";
            string csvLine = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", negativeAmount);

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(-123.55, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = String.Empty;

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard2InOutRecord.ReconciledAmount);
        }

        [Test]
        public void WillAddDefaultDescriptionIfDescriptionIsUnpopulated()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord();
            string csvLine = "19/12/2016^£123.55^^^";

            // Act 
            credCard2InOutRecord.Load(csvLine);

            // Assert
            Assert.AreEqual("Source record had no description", credCard2InOutRecord.Description);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var credCard2InOutRecord = new CredCard2InOutRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 12.34,
                Description = "Description",
                ReconciledAmount = 56.78,
                SourceLine = "SourceLine"
            };

            // Act 
            var copiedRecord = (CredCard2InOutRecord)credCard2InOutRecord.Copy();

            // Assert
            Assert.AreEqual(credCard2InOutRecord.Date, copiedRecord.Date);
            Assert.AreEqual(credCard2InOutRecord.UnreconciledAmount, copiedRecord.UnreconciledAmount);
            Assert.AreEqual(credCard2InOutRecord.Description, copiedRecord.Description);
            Assert.AreEqual(credCard2InOutRecord.ReconciledAmount, copiedRecord.ReconciledAmount);
            Assert.AreEqual(credCard2InOutRecord.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalUnreconciledAmount = 12.34;
            var originalDescription = "Description";
            var originalReconciledAmount = 56.78;
            var originalSourceLine = "SourceLine";
            var credCard2InOutRecord = new CredCard2InOutRecord
            {
                Date = originalDate,
                UnreconciledAmount = originalUnreconciledAmount,
                Description = originalDescription,
                ReconciledAmount = originalReconciledAmount,
                SourceLine = originalSourceLine
            };

            // Act 
            var copiedRecord = (CredCard2InOutRecord)credCard2InOutRecord.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.UnreconciledAmount = copiedRecord.UnreconciledAmount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.ReconciledAmount = copiedRecord.ReconciledAmount + 1;
            copiedRecord.SourceLine = copiedRecord.SourceLine + "something else";

            // Assert
            Assert.AreEqual(originalDate, credCard2InOutRecord.Date);
            Assert.AreEqual(originalUnreconciledAmount, credCard2InOutRecord.UnreconciledAmount);
            Assert.AreEqual(originalDescription, credCard2InOutRecord.Description);
            Assert.AreEqual(originalReconciledAmount, credCard2InOutRecord.ReconciledAmount);
            Assert.AreEqual(originalSourceLine, credCard2InOutRecord.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingCredCard2SpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var credCard2InOutRecord = new CredCard2InOutRecord
            {
                Match = new CredCard2Record
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Description = "match description"
                }
            };
            var mockCells = new Mock<ICellSet>();

            // Act 
            credCard2InOutRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.DateSpreadsheetIndex + 1, credCard2InOutRecord.Match.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.AmountSpreadsheetIndex + 1, credCard2InOutRecord.Match.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, CredCard2Record.DescriptionSpreadsheetIndex + 1, credCard2InOutRecord.Match.Description), "Desc");
        }
    }
}
