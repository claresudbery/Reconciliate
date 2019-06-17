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
            var bank_record = new BankRecord();
            string expected_date_as_string = "01/04/2017";
            string csv_line = String.Format("{0}^£13.95^^POS^Purchase^^^^^", expected_date_as_string);
            var expected_date = Convert.ToDateTime(expected_date_as_string, StringHelper.Culture());

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, bank_record.Date);
        }

        [Test]
        public void CanReadUnreconciledAmountFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 13.95;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountSurroundedByQuotesFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 13.95;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^\"{0}\"^^POS^Purchase^^^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadAmountContainingCommaFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£4,567.89^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(4567.89, bank_record.UnreconciledAmount);
        }

        [Test]
        public void CanReadTypeFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_type = "POS";
            string csv_line = String.Format("01/04/2017^£13.95^^{0}^Purchase^^^^^", expected_type);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_type, bank_record.Type);
        }

        [Test]
        public void CanReadDescriptionFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_description = "Purchase";
            string csv_line = String.Format("01/04/2017^£13.95^^POS^{0}^^^^^", expected_description);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, bank_record.Description);
        }

        [Test]
        public void CanReadChequeNumberFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            int expected_cheque_number = 1395;
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", expected_cheque_number);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_cheque_number, bank_record.ChequeNumber);
        }

        [Test]
        public void CanReadReconciledAmountFromCSV()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 238.92;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyReconciledAmount()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.ReconciledAmount);
        }

        [Test]
        public void CanCopeWithEmptyChequeNumber()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.ChequeNumber);
        }

        [Test]
        public void ThrowsExceptionForBadDate()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_date = "not a date";
            string csv_line = String.Format("{0}^£13.95^^POS^Purchase^^^^^", bad_date);
            bool exception_thrown = false;
            string expected_message_snippet = "empty";
            string error_message = String.Empty;

            // Act 
            try
            {
                bank_record.Load(csv_line);
            }
            catch (Exception ex)
            {
                exception_thrown = true;
                error_message = ex.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(expected_message_snippet));
        }

        [Test]
        public void ThrowsExceptionForBadUnreconciledAmount()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", bad_amount);
            bool exception_thrown = false;
            string expected_message_snippet = "empty";
            string error_message = String.Empty;

            // Act 
            try
            {
                bank_record.Load(csv_line);
            }
            catch (Exception ex)
            {
                exception_thrown = true;
                error_message = ex.Message;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
            Assert.IsTrue(error_message.Contains(expected_message_snippet));
        }

        [Test]
        public void CanCopeWithBadChequeNumber()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_cheque_number = "not a cheque number";
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", bad_cheque_number);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.ChequeNumber);
        }

        [Test]
        public void CanCopeWithBadReconciledAmount()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", bad_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.ReconciledAmount);
        }

        [Test]
        public void CanMakeMainAmountPositive()
        {
            // Arrange
            var bank_record = new BankRecord();
            var negative_amount = -23.23;
            string input_amount = "-£" + negative_amount * -1;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);
            bank_record.Load(csv_line);

            // Act 
            bank_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(negative_amount * -1, bank_record.UnreconciledAmount);
        }

        [Test]
        public void IfMainAmountAlreadyPositiveThenMakingItPositiveHasNoEffect()
        {
            // Arrange
            var bank_record = new BankRecord();
            var positive_amount = 23.23;
            string input_amount = "£" + positive_amount;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);
            bank_record.Load(csv_line);

            // Act 
            bank_record.MakeMainAmountPositive();

            // Assert
            Assert.AreEqual(positive_amount, bank_record.UnreconciledAmount);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithoutMatchedRecord()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bank_record.Load(csv_line);
            bank_record.Matched = true;

            // Act 
            string constructed_csv_line = bank_record.ToCsv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,", constructed_csv_line);
        }

        [Test]
        public void CsvIsConstructedCorrectlyWithMatchedRecord()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bank_record.Load(csv_line);
            bank_record.Matched = true;
            string matched_record_csv_line = String.Format("06/03/2017,BAC,\"'Some , description\",127.69,261.40,\"'Envelope\",\"'228822-99933422\",");
            var matched_record = new ActualBankRecord();
            matched_record.Load(matched_record_csv_line);
            bank_record.Match = matched_record;

            // Act 
            string constructed_csv_line = bank_record.ToCsv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,,,06/03/2017,£127.69,BAC,\"'Some ; description\"", constructed_csv_line);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasSurroundedBySpaces()
        {
            // Arrange
            var bank_record = new BankRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something , something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", text_containing_commas, input_amount);

            // Act 
            
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasFollowedBySpaces()
        {
            // Arrange
            var bank_record = new BankRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something, something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", text_containing_commas, input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.ReconciledAmount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void CanCopeWithInputContainingCommasPrecededBySpaces()
        {
            // Arrange
            var bank_record = new BankRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something ,something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^{0}^1234^{1}^^^", text_containing_commas, input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.ReconciledAmount);
        }

        [Test]
        public void AmountsContainingCommasShouldBeEncasedInQuotes()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_containing_comma);
            bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = bank_record.ToCsv();

            // Assert
            string expected_csv_line = String.Format("01/04/2017,\"{0}\",,POS,\"Purchase\",1234,,,,", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadUnreconciledAmountsContainingCommas()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_containing_comma);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, bank_record.UnreconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadReconciledAmountsContainingCommas()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", amount_containing_comma);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, bank_record.ReconciledAmount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadAmountsPrecededByPoundSigns()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_with_pound_sign = "£1,234.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", amount_with_pound_sign);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, bank_record.ReconciledAmount);
        }

        [Test]
        public void AmountsShouldBeWrittenUsingPoundSigns()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_with_pound_sign = "£123.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_with_pound_sign);
            bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = bank_record.ToCsv();

            // Assert
            string expected_csv_line = String.Format("01/04/2017,{0},,POS,\"Purchase\",1234,,,,", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void ShouldBeAbleToReadNegativeAmounts()
        {
            // Arrange
            var bank_record = new BankRecord();
            var negative_amount = "-£123.55";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            string csv_line = String.Format("01/04/2017^12.34^^POS^Purchase^1234^{0}^^^", negative_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(-123.55, bank_record.ReconciledAmount);
        }

        [Test]
        public void ShouldBeAbleToCopeWithTooFewInputFields()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = "01/04/2017^12.34^^POS^Purchase";

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.ReconciledAmount);
        }

        [Test]
        public void WillThrowExceptionIfDateIsUnpopulated()
        {
            // Arrange
            var bank_record = new BankRecord();
            bool exception_thrown = false;
            string csv_line = String.Format("^£13.95^^POS^Purchase^^^^^");

            // Act 
            try
            {
                bank_record.Load(csv_line);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
        }

        [Test]
        public void WillThrowExceptionIfUnreconciledAmountIsUnpopulated()
        {
            // Arrange
            var bank_record = new BankRecord();
            bool exception_thrown = false;
            string csv_line = String.Format("01/04/2017^^^POS^Purchase^^^^^");

            // Act 
            try
            {
                bank_record.Load(csv_line);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
        }

        [Test]
        public void WillThrowExceptionIfTypeIsUnpopulated()
        {
            // Arrange
            var bank_record = new BankRecord();
            bool exception_thrown = false;
            string csv_line = String.Format("01/04/2017^£13.95^^^Purchase^^^^^");

            // Act 
            try
            {
                bank_record.Load(csv_line);
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
        }

        [Test]
        public void WillAssumeMissingTypeFieldIfDescriptionIsUnpopulated()
        {
            // Arrange
            var bank_record = new BankRecord();
            var type_is_actually_description = "Purchase";
            string csv_line = String.Format("01/04/2017^£13.95^^{0}^^^^^^", type_is_actually_description);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(BankRecord.EmptyType, bank_record.Type);
            Assert.AreEqual(type_is_actually_description, bank_record.Description);
        }

        [Test]
        public void WhenCopyingRecordWillCopyAllImportantData()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                Date = DateTime.Today,
                UnreconciledAmount = 12.34,
                Description = "Description",
                ReconciledAmount = 56.78,
                Type = "Type",
                ChequeNumber = 10
            };
            bank_record.UpdateSourceLineForOutput(',');

            // Act 
            var copied_record = (BankRecord)bank_record.Copy();

            // Assert
            Assert.AreEqual(bank_record.Date, copied_record.Date);
            Assert.AreEqual(bank_record.UnreconciledAmount, copied_record.UnreconciledAmount);
            Assert.AreEqual(bank_record.Description, copied_record.Description);
            Assert.AreEqual(bank_record.ReconciledAmount, copied_record.ReconciledAmount);
            Assert.AreEqual(bank_record.Type, copied_record.Type);
            Assert.AreEqual(bank_record.ChequeNumber, copied_record.ChequeNumber);
            Assert.AreEqual(bank_record.SourceLine, copied_record.SourceLine);
        }

        [Test]
        public void WhenCopyingRecordWillCreateNewObject()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_unreconciled_amount = 12.34;
            var original_description = "Description";
            var original_reconciled_amount = 56.78;
            var original_type = "Type";
            var original_cheque_number = 19;
            var bank_record = new BankRecord
            {
                Date = original_date,
                UnreconciledAmount = original_unreconciled_amount,
                Description = original_description,
                ReconciledAmount = original_reconciled_amount,
                Type = original_type,
                ChequeNumber = original_cheque_number
            };
            bank_record.UpdateSourceLineForOutput(',');
            var original_source_line = bank_record.SourceLine;

            // Act 
            var copied_record = (BankRecord)bank_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.UnreconciledAmount = copied_record.UnreconciledAmount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.ReconciledAmount = copied_record.ReconciledAmount + 1;
            copied_record.Type = copied_record.Type + "something else";
            copied_record.ChequeNumber = copied_record.ChequeNumber + 1;
            copied_record.UpdateSourceLineForOutput(',');

            // Assert
            Assert.AreEqual(original_date, bank_record.Date);
            Assert.AreEqual(original_unreconciled_amount, bank_record.UnreconciledAmount);
            Assert.AreEqual(original_description, bank_record.Description);
            Assert.AreEqual(original_reconciled_amount, bank_record.ReconciledAmount);
            Assert.AreEqual(original_source_line, bank_record.SourceLine);
            Assert.AreEqual(original_type, bank_record.Type);
            Assert.AreEqual(original_cheque_number, bank_record.ChequeNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingBankSpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var bank_record = new BankRecord
            {
                Match = new ActualBankRecord
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Type = "POS",
                    Description = "match description"
                }
            };
            var mock_cells = new Mock<ICellSet>();

            // Act 
            bank_record.PopulateSpreadsheetRow(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.PopulateCell(row, ActualBankRecord.DateSpreadsheetIndex + 1, bank_record.Match.Date), "Date");
            mock_cells.Verify(x => x.PopulateCell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, bank_record.Match.MainAmount()), "Amount");
            mock_cells.Verify(x => x.PopulateCell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, ((ActualBankRecord)bank_record.Match).Type), "Type");
            mock_cells.Verify(x => x.PopulateCell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, bank_record.Match.Description), "Desc");
        }
    }
}
