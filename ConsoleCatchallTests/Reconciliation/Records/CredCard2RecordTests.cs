using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Moq;
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
            var cred_card2_record = new CredCard2Record();
            string expected_date_as_string = "08/06/2017";
            string csv_line = String.Format("{0},ref,13.49,ACME UK HOLDINGS", expected_date_as_string);
            var expected_date = DateTime.ParseExact(expected_date_as_string, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_description = "ACME UK HOLDINGS";
            string csv_line = String.Format("08/06/2017,ref,13.49,{0},", expected_description);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card2_record.Description);
        }

        [Test]
        public void CanReadAmountFromCSVWithQuotesAndLeadingSpace()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_amount = 1.94;
            string input_amount = "\" " + expected_amount + "\"";
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS,General Purchases", input_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_record.Amount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format(",ref,13.49,ACME UK HOLDINGS");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void CanCopeWithEmptyDescription()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,ref,13.49,");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual("", cred_card2_record.Description);
        }

        [Test]
        public void CanCopeWithEmptyAmount()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,ref,,ACME UK HOLDINGS");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_record.Amount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0},ref,13.49,ACME UK HOLDINGS", bad_date);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void CanCopeWithBadAmount()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var bad_amount = "not an amount";
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", bad_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_record.Amount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var negative_amount = -23.23;
            string input_amount = "-" + negative_amount * -1;
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", input_amount);
            cred_card2_record.Load(csv_line);

            // Act 
            cred_card2_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negative_amount * -1, cred_card2_record.Amount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var positive_amount = 23.23;
            string input_amount = positive_amount.ToString();
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", input_amount);
            cred_card2_record.Load(csv_line);

            // Act 
            cred_card2_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positive_amount, cred_card2_record.Amount);
        }

        [Test]
        public void CsvIsConstructedCorrectly()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,ref,13.49,ACME UK HOLDINGS");
            cred_card2_record.Load(csv_line);
            cred_card2_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card2_record.ToCsv();

            // Assert
            Assert.AreEqual("08/06/2017,£13.49,\"ACME UK HOLDINGS\"", constructed_csv_line);
        }

        // This doesn't apply to CredCard2Record and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void CommasInInputAreReplacedBySemiColons()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string text_containing_commas = "\"something, something, something else\"";
            string csv_line = String.Format("08/06/2017,ref,13.49,{0}", text_containing_commas);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual("\"something; something; something else\"", cred_card2_record.Description);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var amount = "123.55";
            var amount_with_pound_sign = "£" + amount;
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", amount);
            cred_card2_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card2_record.ToCsv();

            // Assert
            string expected_csv_line = String.Format("08/06/2017,{0},\"ACME UK HOLDINGS\"", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var negative_amount = "-123.55";
            string csv_line = String.Format("08/06/2017,ref,{0},ACME UK HOLDINGS", negative_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(-123.55, cred_card2_record.Amount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Empty;

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_record.Amount);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description"
            };
            cred_card2_record.UpdateSourceLineForOutput(',');

            // Act 
            var copied_record = (CredCard2Record)cred_card2_record.Copy();

            // Assert
            Assert.AreEqual(cred_card2_record.Date, copied_record.Date);
            Assert.AreEqual(cred_card2_record.Amount, copied_record.Amount);
            Assert.AreEqual(cred_card2_record.Description, copied_record.Description);
            Assert.AreEqual(cred_card2_record.SourceLine, copied_record.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_amount = 12.34;
            var original_description = "Description";
            var cred_card2_record = new CredCard2Record
            {
                Date = original_date,
                Amount = original_amount,
                Description = original_description,
            };
            cred_card2_record.UpdateSourceLineForOutput(',');
            var original_source_line = cred_card2_record.SourceLine;

            // Act 
            var copied_record = (CredCard2Record)cred_card2_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Amount = copied_record.Amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(original_date, cred_card2_record.Date);
            Assert.AreEqual(original_amount, cred_card2_record.Amount);
            Assert.AreEqual(original_description, cred_card2_record.Description);
            Assert.AreEqual(original_source_line, cred_card2_record.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateCredCard2RecordCells()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Amount = 13.48,
                Description = "Acme: Esmerelda's birthday"
            };
            var row = 10;
            var mock_cells = new Mock<ICellSet>();

            // Act 
            cred_card2_record.PopulateSpreadsheetRow(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.DateSpreadsheetIndex + 1, cred_card2_record.Date), "Date");
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.AmountSpreadsheetIndex + 1, cred_card2_record.MainAmount()), "Amount");
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.DescriptionSpreadsheetIndex + 1, cred_card2_record.Description), "Desc");
        }
    }
}
