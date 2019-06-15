using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class CredCard1RecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string expectedDateAsString = "17/02/2017";
            string csvLine = String.Format("{0},23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",1.94", expectedDateAsString);
            var expectedDate = Convert.ToDateTime(expectedDateAsString, StringHelper.Culture());

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1Record.Date);
        }

        [Test]
        public void CanReadReferenceFromCSV()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string expectedReference = "22223333";
            string csvLine = String.Format("17/02/2017,23/11/2018,{0},\"ANY STORE 8888        ANYWHERE\",1.94", expectedReference);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedReference, credCard1Record.Reference);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedDescription = "\"ANY STORE 8888        ANYWHERE\"";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},1.94", expectedDescription);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, credCard1Record.Description);
        }

        [Test]
        public void CanReadAmountFromCSVWithoutPoundSign()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedAmount = -1.94;
            var csvAmount = expectedAmount * -1;
            string csvLine = $"19/10/2018,23/11/2018,11112222,SQ *HAPPY BOOK STORE,{csvAmount},";

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        [Test]
        public void When_Description_Ends_With_Spaces_Amount_Can_Still_Be_Read()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedAmount = -1.94;
            var csvAmount = expectedAmount * -1;
            string csvLine = $"19/10/2018,23/11/2018,11112222,SQ *HAPPY BOOK STORE   ,{csvAmount},";

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        [Test]
        public void CanReadAmountFromCSVWithPoundSign()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedAmount = 1.94;
            var inputAmount = "£" + expectedAmount*-1;
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", inputAmount);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedDate = new DateTime(9999, 9, 9);
            string csvLine = String.Format(",23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",1.94");

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1Record.Date);
        }

        [Test]
        public void CanCopeWithEmptyReference()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string csvLine = String.Format("17/02/2017,23/11/2018,,\"ANY STORE 8888        ANYWHERE\",1.94");

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(String.Empty, credCard1Record.Reference);
        }

        [Test]
        public void CanCopeWithEmptyDescription()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,,1.94");

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual("", credCard1Record.Description);
        }

        [Test]
        public void CanCopeWithEmptyAmount()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",");

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1Record.Amount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var expectedDate = new DateTime(9999, 9, 9);
            var badDate = "not a date";
            string csvLine = String.Format("{0},23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",", badDate);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, credCard1Record.Date);
        }

        [Test]
        public void CanCopeWithBadAmount()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var badAmount = "not an amount";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", badAmount);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1Record.Amount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string csvLine = String.Empty;

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(0, credCard1Record.Amount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var negativeAmount = -23.23;
            var csvNegativeAmount = negativeAmount * -1;
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", csvNegativeAmount);
            credCard1Record.Load(csvLine);

            // Act 
            credCard1Record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, credCard1Record.Amount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var positiveAmount = 23.23;
            var csvPositiveAmount = positiveAmount * -1;
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", csvPositiveAmount);
            credCard1Record.Load(csvLine);

            // Act 
            credCard1Record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, credCard1Record.Amount);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,-12.33");
            credCard1Record.Load(csvLine);
            credCard1Record.Matched = false;

            // Act 
            string constructedCsvLine = credCard1Record.ToCsv();

            // Assert
            Assert.AreEqual("17/02/2017,£12.33,\"ANY STORE\"", constructedCsvLine);
        }

        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            double expectedAmount = 12.35;
            double csvExpectedAmount = expectedAmount * -1;
            string textContainingCommas = "something ,something , something, something else";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", textContainingCommas, csvExpectedAmount);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            double expectedAmount = 12.35;
            double csvExpectedAmount = expectedAmount * -1;
            string textContainingCommas = "something, something, something else";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", textContainingCommas, csvExpectedAmount);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            double expectedAmount = 12.35;
            double csvExpectedAmount = expectedAmount * -1;
            string textContainingCommas = "something ,something ,something else";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", textContainingCommas, csvExpectedAmount);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void CommasInInputAreReplacedBySemiColons()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            string textContainingCommas = "\"something, something, something else\"";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},12.35", textContainingCommas);

            // Act 
            credCard1Record.Load(csvLine);

            // Assert
            Assert.AreEqual("\"something; something; something else\"", credCard1Record.Description);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord and CredCard2Record because the input is never encased in quotes.
        [Test]
        public void IfInputIsEncasedInQuotesThenOutputOnlyHasOneEncasingSetOfQuotes()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var descriptionEncasedInOneSetOfQuotes = "\"ANY STORE\"";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,{0},12.33", descriptionEncasedInOneSetOfQuotes);
            credCard1Record.Load(csvLine);
            credCard1Record.Matched = false;

            // Act 
            string constructedCsvLine = credCard1Record.ToCsv();

            // Assert
            var expectedCsvLine = String.Format("17/02/2017,-£12.33,{0}", descriptionEncasedInOneSetOfQuotes);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var amount = 1234.55;
            double csvAmount = amount * -1;
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,{0}", csvAmount);
            credCard1Record.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard1Record.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("17/02/2017,\"{0}\",\"ANY STORE\"", amountContainingComma);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var credCard1Record = new CredCard1Record();
            var amount = "123.55";
            string csvAmount = $"-{amount}";
            var amountWithPoundSign = "£" + amount;
            string csvLine = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,{0}", csvAmount);
            credCard1Record.Load(csvLine);

            // Act 
            string constructedCsvLine = credCard1Record.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("17/02/2017,{0},\"ANY STORE\"", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var credCard1Record = new CredCard1Record
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description",
                Reference = "33334444"
            };
            credCard1Record.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (CredCard1Record)credCard1Record.Copy();

            // Assert
            Assert.AreEqual(credCard1Record.Date, copiedRecord.Date);
            Assert.AreEqual(credCard1Record.Amount, copiedRecord.Amount);
            Assert.AreEqual(credCard1Record.Description, copiedRecord.Description);
            Assert.AreEqual(credCard1Record.Reference, copiedRecord.Reference);
            Assert.AreEqual(credCard1Record.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalAmount = 12.34;
            var originalDescription = "Description";
            var originalReference = "33334444";
            var credCard1Record = new CredCard1Record
            {
                Date = originalDate,
                Amount = originalAmount,
                Description = originalDescription,
                Reference = originalReference
            };
            credCard1Record.UpdateSourceLineForOutput(',');
            var originalSourceLine = credCard1Record.SourceLine;

            // Act 
            var copiedRecord = (CredCard1Record)credCard1Record.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.Amount = copiedRecord.Amount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.Reference = copiedRecord.Reference + 1;
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, credCard1Record.Date);
            Assert.AreEqual(originalAmount, credCard1Record.Amount);
            Assert.AreEqual(originalDescription, credCard1Record.Description);
            Assert.AreEqual(originalReference, credCard1Record.Reference);
            Assert.AreEqual(originalSourceLine, credCard1Record.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateCredCard1RecordCells()
        {
            // Arrange
            var credCard1Record = new CredCard1Record
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Amount = 1234.56,
                Description = "Acme: Esmerelda's birthday"
            };
            var row = 10;
            var mockCells = new Mock<ICellSet>();

            // Act 
            credCard1Record.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DateSpreadsheetIndex + 1, credCard1Record.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.AmountSpreadsheetIndex + 1, credCard1Record.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DescriptionSpreadsheetIndex + 1, credCard1Record.Description), "Desc");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillReadFromCredCard1RecordCells()
        {
            // Arrange
            var sheetName = "MockSheet";
            DateTime expectedDate = new DateTime(year: 2018, month: 6, day: 1);
            string expectedReference = "55556666";
            String expectedDescription = "description3";
            Double expectedAmount = 37958.90;
            var excelDate = expectedDate.ToOADate();
            var excelDate2 = expectedDate.AddDays(1).ToOADate();
            var fakeCellRow = new FakeCellRow().WithFakeData(new List<object>
            {
                excelDate,
                excelDate2,
                expectedReference,
                expectedDescription,
                expectedAmount
            });
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.ReadLastRow(sheetName)).Returns(fakeCellRow);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var credCard1Record = new CredCard1Record();
            var cells = spreadsheet.ReadLastRow(sheetName);

            // Act 
            credCard1Record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard1Record.Date);
            Assert.AreEqual(expectedReference, credCard1Record.Reference);
            Assert.AreEqual(expectedDescription, credCard1Record.Description);
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }
    }
}
