using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class BankRecordTests
    {
        [Test]
        public void CanReadDateFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string expectedDateAsString = "01/04/2017";
            string csvLine = String.Format("{0}^£13.95^^POS^Purchase^^^^^", expectedDateAsString);
            var expectedDate = Convert.ToDateTime(expectedDateAsString, StringHelper.Culture());

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDate, bankRecord.Date);
        }

        [Test]
        public void CanReadUnreconciledAmountFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var expectedAmount = 13.95;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", inputAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountSurroundedByQuotesFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var expectedAmount = 13.95;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("01/04/2017^\"{0}\"^^POS^Purchase^^^^^", inputAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountContainingCommaFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = String.Format("01/04/2017^£4,567.89^^POS^Purchase^^^^^");

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(4567.89, bankRecord.UnreconciledAmount);
        }

        [Test]
        public void CanReadTypeFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var expectedType = "POS";
            string csvLine = String.Format("01/04/2017^£13.95^^{0}^Purchase^^^^^", expectedType);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedType, bankRecord.Type);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var expectedDescription = "Purchase";
            string csvLine = String.Format("01/04/2017^£13.95^^POS^{0}^^^^^", expectedDescription);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedDescription, bankRecord.Description);
        }

        [Test]
        public void CanReadChequeNumberFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            int expectedChequeNumber = 1395;
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", expectedChequeNumber);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedChequeNumber, bankRecord.ChequeNumber);
        }

        [Test]
        public void CanReadReconciledAmountFromCSV()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var expectedAmount = 238.92;
            string inputAmount = "£" + expectedAmount;
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", inputAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyReconciledAmount()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, bankRecord.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyChequeNumber()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, bankRecord.ChequeNumber);
        }

        [Test]
        public void ThrowsExceptionForBadDate()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var badDate = "not a date";
            string csvLine = String.Format("{0}^£13.95^^POS^Purchase^^^^^", badDate);
            bool exceptionThrown = false;
            string expectedMessageSnippet = "empty";
            string errorMessage = String.Empty;

            // Act 
            try
            {
                bankRecord.Load(csvLine);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                errorMessage = ex.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(expectedMessageSnippet));
        }

        [Test]
        public void ThrowsExceptionForBadUnreconciledAmount()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", badAmount);
            bool exceptionThrown = false;
            string expectedMessageSnippet = "empty";
            string errorMessage = String.Empty;

            // Act 
            try
            {
                bankRecord.Load(csvLine);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                errorMessage = ex.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(expectedMessageSnippet));
        }

        [Test]
        public void CanCopeWithBadChequeNumber()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var badChequeNumber = "not a cheque number";
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", badChequeNumber);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, bankRecord.ChequeNumber);
        }

        [Test]
        public void CanCopeWithBadReconciledAmount()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var badAmount = "not an amount";
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", badAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, bankRecord.ReconciledAmount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var negativeAmount = -23.23;
            string inputAmount = "-£" + negativeAmount * -1;
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", inputAmount);
            bankRecord.Load(csvLine);

            // Act 
            bankRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negativeAmount * -1, bankRecord.UnreconciledAmount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var positiveAmount = 23.23;
            string inputAmount = "£" + positiveAmount;
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", inputAmount);
            bankRecord.Load(csvLine);

            // Act 
            bankRecord.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positiveAmount, bankRecord.UnreconciledAmount);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithoutMatchedRecord()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bankRecord.Load(csvLine);
            bankRecord.Matched = true;

            // Act 
            string constructedCsvLine = bankRecord.ToCsv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,", constructedCsvLine);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithMatchedRecord()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bankRecord.Load(csvLine);
            bankRecord.Matched = true;
            string matchedRecordCsvLine = String.Format("06/03/2017,BAC,\"'Some , description\",127.69,261.40,\"'Envelope\",\"'228822-99933422\",");
            var matchedRecord = new ActualBankRecord();
            matchedRecord.Load(matchedRecordCsvLine);
            bankRecord.Match = matchedRecord;

            // Act 
            string constructedCsvLine = bankRecord.ToCsv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,,,06/03/2017,£127.69,BAC,\"'Some ; description\"", constructedCsvLine);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var bankRecord = new BankRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something , something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", textContainingCommas, inputAmount);

            // Act 
            
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var bankRecord = new BankRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something, something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", textContainingCommas, inputAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var bankRecord = new BankRecord();
            double expectedAmount = 12.35;
            string inputAmount = "£" + expectedAmount;
            string textContainingCommas = "something ,something ,something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", textContainingCommas, inputAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(expectedAmount, bankRecord.ReconciledAmount);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amountContainingComma);
            bankRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = bankRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("01/04/2017,\"{0}\",,POS,\"Purchase\",1234,,,,", amountContainingComma);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadUnreconciledAmountsContainingCommas()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var amountContainingComma = "£1,234.55";
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amountContainingComma);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, bankRecord.UnreconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadReconciledAmountsContainingCommas()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var amountContainingComma = "£1,234.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", amountContainingComma);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, bankRecord.ReconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadAmountsPrecededByPoundSigns()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var amountWithPoundSign = "£1,234.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", amountWithPoundSign);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(1234.55, bankRecord.ReconciledAmount);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var amountWithPoundSign = "£123.55";
            string csvLine = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amountWithPoundSign);
            bankRecord.Load(csvLine);

            // Act 
            string constructedCsvLine = bankRecord.ToCsv();

            // Assert
            string expectedCsvLine = String.Format("01/04/2017,{0},,POS,\"Purchase\",1234,,,,", amountWithPoundSign);
            Assert.AreEqual(expectedCsvLine, constructedCsvLine);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var negativeAmount = "-£123.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csvLine = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", negativeAmount);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(-123.55, bankRecord.ReconciledAmount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithTooFewInputFields()
        {
            // Arrange
            var bankRecord = new BankRecord();
            string csvLine = "01/04/2017^12.34^^POS^Purchase";

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(0, bankRecord.ReconciledAmount);
        }

        [Test]
        public void WillThrowExceptionIfDateIsUnpopulated()
        {
            // Arrange
            var bankRecord = new BankRecord();
            bool exceptionThrown = false;
            string csvLine = String.Format("^£13.95^^POS^Purchase^^^^^");

            // Act 
            try
            {
                bankRecord.Load(csvLine);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.AreEqual(true, exceptionThrown);
        }

        [Test]
        public void WillThrowExceptionIfUnreconciledAmountIsUnpopulated()
        {
            // Arrange
            var bankRecord = new BankRecord();
            bool exceptionThrown = false;
            string csvLine = String.Format("01/04/2017^^^POS^Purchase^^^^^");

            // Act 
            try
            {
                bankRecord.Load(csvLine);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.AreEqual(true, exceptionThrown);
        }

        [Test]
        public void WillThrowExceptionIfTypeIsUnpopulated()
        {
            // Arrange
            var bankRecord = new BankRecord();
            bool exceptionThrown = false;
            string csvLine = String.Format("01/04/2017^£13.95^^^Purchase^^^^^");

            // Act 
            try
            {
                bankRecord.Load(csvLine);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.AreEqual(true, exceptionThrown);
        }

        [Test]
        public void WillAssumeMissingTypeFieldIfDescriptionIsUnpopulated()
        {
            // Arrange
            var bankRecord = new BankRecord();
            var typeIsActuallyDescription = "Purchase";
            string csvLine = String.Format("01/04/2017^£13.95^^{0}^^^^^^", typeIsActuallyDescription);

            // Act 
            bankRecord.Load(csvLine);

            // Assert
            Assert.AreEqual(BankRecord.EmptyType, bankRecord.Type);
            Assert.AreEqual(typeIsActuallyDescription, bankRecord.Description);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var bankRecord = new BankRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 12.34,
                Description = "Description",
                ReconciledAmount = 56.78,
                Type = "Type",
                ChequeNumber = 10
            };
            bankRecord.UpdateSourceLineForOutput(',');

            // Act 
            var copiedRecord = (BankRecord)bankRecord.Copy();

            // Assert
            Assert.AreEqual(bankRecord.Date, copiedRecord.Date);
            Assert.AreEqual(bankRecord.UnreconciledAmount, copiedRecord.UnreconciledAmount);
            Assert.AreEqual(bankRecord.Description, copiedRecord.Description);
            Assert.AreEqual(bankRecord.ReconciledAmount, copiedRecord.ReconciledAmount);
            Assert.AreEqual(bankRecord.Type, copiedRecord.Type);
            Assert.AreEqual(bankRecord.ChequeNumber, copiedRecord.ChequeNumber);
            Assert.AreEqual(bankRecord.SourceLine, copiedRecord.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var originalUnreconciledAmount = 12.34;
            var originalDescription = "Description";
            var originalReconciledAmount = 56.78;
            var originalType = "Type";
            var originalChequeNumber = 19;
            var bankRecord = new BankRecord
            {
                Date = originalDate,
                UnreconciledAmount = originalUnreconciledAmount,
                Description = originalDescription,
                ReconciledAmount = originalReconciledAmount,
                Type = originalType,
                ChequeNumber = originalChequeNumber
            };
            bankRecord.UpdateSourceLineForOutput(',');
            var originalSourceLine = bankRecord.SourceLine;

            // Act 
            var copiedRecord = (BankRecord)bankRecord.Copy();
            copiedRecord.Date = copiedRecord.Date.AddDays(1);
            copiedRecord.UnreconciledAmount = copiedRecord.UnreconciledAmount + 1;
            copiedRecord.Description = copiedRecord.Description + "something else";
            copiedRecord.ReconciledAmount = copiedRecord.ReconciledAmount + 1;
            copiedRecord.Type = copiedRecord.Type + "something else";
            copiedRecord.ChequeNumber = copiedRecord.ChequeNumber + 1;
            copiedRecord.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(originalDate, bankRecord.Date);
            Assert.AreEqual(originalUnreconciledAmount, bankRecord.UnreconciledAmount);
            Assert.AreEqual(originalDescription, bankRecord.Description);
            Assert.AreEqual(originalReconciledAmount, bankRecord.ReconciledAmount);
            Assert.AreEqual(originalSourceLine, bankRecord.SourceLine);
            Assert.AreEqual(originalType, bankRecord.Type);
            Assert.AreEqual(originalChequeNumber, bankRecord.ChequeNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingBankSpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var bankRecord = new BankRecord
            {
                Match = new ActualBankRecord
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Type = "POS",
                    Description = "match description"
                }
            };
            var mockCells = new Mock<ICellSet>();

            // Act 
            bankRecord.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DateSpreadsheetIndex + 1, bankRecord.Match.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, bankRecord.Match.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, ((ActualBankRecord)bankRecord.Match).Type), "Type");
            mockCells.Verify(x => x.PopulateCell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, bankRecord.Match.Description), "Desc");
        }
    }
}
