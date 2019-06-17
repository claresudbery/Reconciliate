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
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string expected_date_as_string = "01/04/2017";
            string csv_line = String.Format("{0}^£13.48^^Acme: Esmerelda's birthday^", expected_date_as_string);
            var expected_date = Convert.ToDateTime(expected_date_as_string, StringHelper.Culture());

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
        }

        [Test]
        public void CanReadUnreconciledAmountFromCSV()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_amount = 13.95;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountSurroundedByQuotesFromCSV()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            double expected_amount = 7888.99;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/12/2016^\"{0}\"^^ZZZSpecialDescription017^", input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountContainingCommaFromCSV()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = "19/12/2016^£5,678.99^^ZZZSpecialDescription017^";

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(5678.99, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadDataFromCSVWithExtraSeparatorAtEnd()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_description = "ZZZSpecialDescription017";
            string csv_line = String.Format("19/12/2016^£7.99^^{0}^^", expected_description);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_description = "Acme: Esmerelda's birthday";
            string csv_line = String.Format("19/04/2017^£13.48^^{0}^", expected_description);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card2_in_out_record.Description);
        }

        [Test]
        public void CanReadReconciledAmountFromCSV()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_amount = 238.92;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyDate()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format("^£13.48^^Acme: Esmerelda's birthday^");

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
        }

        [Test]
        public void CanCopeWithEmptyUnreconciledAmount()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyReconciledAmount()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithBadDate()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0}^£13.48^^Acme: Esmerelda's birthday^", bad_date);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_in_out_record.Date);
        }

        [Test]
        public void CanCopeWithBadUnreconciledAmount()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", bad_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CanCopeWithBadReconciledAmount()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", bad_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var negative_amount = -23.23;
            string input_amount = "-£" + negative_amount * -1;
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", input_amount);
            cred_card2_in_out_record.Load(csv_line);

            // Act 
            cred_card2_in_out_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negative_amount * -1, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var positive_amount = 23.23;
            string input_amount = "£" + positive_amount;
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", input_amount);
            cred_card2_in_out_record.Load(csv_line);

            // Act 
            cred_card2_in_out_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positive_amount, cred_card2_in_out_record.UnreconciledAmount);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithoutMatchedRecord()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^£33.44");
            cred_card2_in_out_record.Load(csv_line);
            cred_card2_in_out_record.Matched = true;

            // Act 
            string constructed_csv_line = cred_card2_in_out_record.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,£13.48,x,\"Acme: Esmerelda's birthday\",£33.44,", constructed_csv_line);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithMatchedRecord()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^£33.44");
            cred_card2_in_out_record.Load(csv_line);
            cred_card2_in_out_record.Matched = true;
            string matched_record_csv_line = String.Format("08/06/2017,ref,13.49,ACME UK HOLDINGS");
            var matched_record = new CredCard2Record();
            matched_record.Load(matched_record_csv_line);
            cred_card2_in_out_record.Match = matched_record;

            // Act 
            string constructed_csv_line = cred_card2_in_out_record.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,£13.48,x,\"Acme: Esmerelda's birthday\",£33.44,,,08/06/2017,£13.49,\"ACME UK HOLDINGS\"", constructed_csv_line);
        }

        [Test]
        public void EmptyFieldsAreOutputAsNothingForCsv()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Format("19/04/2017^^^Acme: Esmerelda's birthday^");
            cred_card2_in_out_record.Load(csv_line);
            cred_card2_in_out_record.Matched = true;

            // Act 
            string constructed_csv_line = cred_card2_in_out_record.ToCsv();

            // Assert
            Assert.AreEqual("19/04/2017,,x,\"Acme: Esmerelda's birthday\",,", constructed_csv_line);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something , something, something else";
            string csv_line = String.Format("19/04/2017^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something, something, something else";
            string csv_line = String.Format("19/04/2017^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something ,something else";
            string csv_line = String.Format("19/04/2017^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amount_containing_comma);
            cred_card2_in_out_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card2_in_out_record.ToCsv();

            // Assert
            string expected_csv_line = String.Format("19/04/2017,\"{0}\",,\"Acme: Esmerelda's birthday\",,", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadUnreconciledAmountsContainingCommas()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amount_containing_comma);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card2_in_out_record.UnreconciledAmount);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadReconciledAmountsContainingCommas()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", amount_containing_comma);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card2_in_out_record.ReconciledAmount);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadAmountsPrecededByPoundSigns()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var amount_with_pound_sign = "£1,234.55";
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", amount_with_pound_sign);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var amount_with_pound_sign = "£123.55";
            string csv_line = String.Format("19/04/2017^{0}^^Acme: Esmerelda's birthday^", amount_with_pound_sign);
            cred_card2_in_out_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card2_in_out_record.ToCsv();

            // Assert
            string expected_csv_line = String.Format("19/04/2017,{0},,\"Acme: Esmerelda's birthday\",,", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to CredCard2 and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            var negative_amount = "-£123.55";
            string csv_line = String.Format("19/04/2017^£13.48^^Acme: Esmerelda's birthday^{0}", negative_amount);

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(-123.55, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithEmptyInput()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = String.Empty;

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_in_out_record.ReconciledAmount);
        }

        [Test]
        public void WillAddDefaultDescriptionIfDescriptionIsUnpopulated()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord();
            string csv_line = "19/12/2016^£123.55^^^";

            // Act 
            cred_card2_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual("Source record had no description", cred_card2_in_out_record.Description);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 12.34,
                Description = "Description",
                ReconciledAmount = 56.78,
                SourceLine = "SourceLine"
            };

            // Act 
            var copied_record = (CredCard2InOutRecord)cred_card2_in_out_record.Copy();

            // Assert
            Assert.AreEqual(cred_card2_in_out_record.Date, copied_record.Date);
            Assert.AreEqual(cred_card2_in_out_record.UnreconciledAmount, copied_record.UnreconciledAmount);
            Assert.AreEqual(cred_card2_in_out_record.Description, copied_record.Description);
            Assert.AreEqual(cred_card2_in_out_record.ReconciledAmount, copied_record.ReconciledAmount);
            Assert.AreEqual(cred_card2_in_out_record.SourceLine, copied_record.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_unreconciled_amount = 12.34;
            var original_description = "Description";
            var original_reconciled_amount = 56.78;
            var original_source_line = "SourceLine";
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Date = original_date,
                UnreconciledAmount = original_unreconciled_amount,
                Description = original_description,
                ReconciledAmount = original_reconciled_amount,
                SourceLine = original_source_line
            };

            // Act 
            var copied_record = (CredCard2InOutRecord)cred_card2_in_out_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.UnreconciledAmount = copied_record.UnreconciledAmount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.ReconciledAmount = copied_record.ReconciledAmount + 1;
            copied_record.SourceLine = copied_record.SourceLine + "something else";

            // Assert
            Assert.AreEqual(original_date, cred_card2_in_out_record.Date);
            Assert.AreEqual(original_unreconciled_amount, cred_card2_in_out_record.UnreconciledAmount);
            Assert.AreEqual(original_description, cred_card2_in_out_record.Description);
            Assert.AreEqual(original_reconciled_amount, cred_card2_in_out_record.ReconciledAmount);
            Assert.AreEqual(original_source_line, cred_card2_in_out_record.SourceLine);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingCredCard2SpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var cred_card2_in_out_record = new CredCard2InOutRecord
            {
                Match = new CredCard2Record
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Description = "match description"
                }
            };
            var mock_cells = new Mock<ICellSet>();

            // Act 
            cred_card2_in_out_record.PopulateSpreadsheetRow(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.DateSpreadsheetIndex + 1, cred_card2_in_out_record.Match.Date), "Date");
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.AmountSpreadsheetIndex + 1, cred_card2_in_out_record.Match.MainAmount()), "Amount");
            mock_cells.Verify(x => x.PopulateCell(row, CredCard2Record.DescriptionSpreadsheetIndex + 1, cred_card2_in_out_record.Match.Description), "Desc");
        }
    }
}
