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
        public void Can_read_date_from_csv()
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
        public void Can_read_unreconciled_amount_from_csv()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 13.95;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_amount_surrounded_by_quotes_from_csv()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 13.95;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^\"{0}\"^^POS^Purchase^^^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_amount_containing_comma_from_csv()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£4,567.89^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(4567.89, bank_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_type_from_csv()
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
        public void Can_read_description_from_csv()
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
        public void Can_read_cheque_number_from_csv()
        {
            // Arrange
            var bank_record = new BankRecord();
            int expected_cheque_number = 1395;
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", expected_cheque_number);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_cheque_number, bank_record.Cheque_number);
        }

        [Test]
        public void Can_read_reconciled_amount_from_csv()
        {
            // Arrange
            var bank_record = new BankRecord();
            var expected_amount = 238.92;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", input_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, bank_record.Reconciled_amount);
        }

        [Test]
        public void Can_cope_with_empty_reconciled_amount()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.Reconciled_amount);
        }

        [Test]
        public void Can_cope_with_empty_cheque_number()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^^^^");

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.Cheque_number);
        }

        [Test]
        public void Throws_exception_for_bad_date()
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
        public void Throws_exception_for_bad_unreconciled_amount()
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
        public void Can_cope_with_bad_cheque_number()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_cheque_number = "not a cheque number";
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^{0}^^^^", bad_cheque_number);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.Cheque_number);
        }

        [Test]
        public void Can_cope_with_bad_reconciled_amount()
        {
            // Arrange
            var bank_record = new BankRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^^{0}^^^", bad_amount);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.Reconciled_amount);
        }

        [Test]
        public void Can_make_main_amount_positive()
        {
            // Arrange
            var bank_record = new BankRecord();
            var negative_amount = -23.23;
            string input_amount = "-£" + negative_amount * -1;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);
            bank_record.Load(csv_line);

            // Act 
            bank_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(negative_amount * -1, bank_record.Unreconciled_amount);
        }

        [Test]
        public void If_main_amount_already_positive_then_making_it_positive_has_no_effect()
        {
            // Arrange
            var bank_record = new BankRecord();
            var positive_amount = 23.23;
            string input_amount = "£" + positive_amount;
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^^^^^", input_amount);
            bank_record.Load(csv_line);

            // Act 
            bank_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(positive_amount, bank_record.Unreconciled_amount);
        }

        [Test]
        public void Csv_is_constructed_correctly_without_matched_record()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bank_record.Load(csv_line);
            bank_record.Matched = true;

            // Act 
            string constructed_csv_line = bank_record.To_csv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,", constructed_csv_line);
        }

        [Test]
        public void Csv_is_constructed_correctly_with_matched_record()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = String.Format("01/04/2017^£13.95^^POS^Purchase^1234^£14.22^^^");
            bank_record.Load(csv_line);
            bank_record.Matched = true;
            string matched_record_csv_line = String.Format("06/03/2017,BAC,'Some , description,127.69,261.40,'Envelope,'228822-99933422,");
            var matched_record = new ActualBankRecord();
            matched_record.Load(matched_record_csv_line);
            bank_record.Match = matched_record;

            // Act 
            string constructed_csv_line = bank_record.To_csv();

            // Assert
            Assert.AreEqual("01/04/2017,£13.95,x,POS,\"Purchase\",1234,£14.22,,,,,06/03/2017,£127.69,BAC,'Some ; description", constructed_csv_line);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_surrounded_by_spaces()
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
            Assert.AreEqual(expected_amount, bank_record.Reconciled_amount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_followed_by_spaces()
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
            Assert.AreEqual(expected_amount, bank_record.Reconciled_amount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_preceded_by_spaces()
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
            Assert.AreEqual(expected_amount, bank_record.Reconciled_amount);
        }

        [Test]
        public void Amounts_containing_commas_should_be_encased_in_quotes()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_containing_comma);
            bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = bank_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("01/04/2017,\"{0}\",,POS,\"Purchase\",1234,,,,", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_unreconciled_amounts_containing_commas()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_containing_comma);

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, bank_record.Unreconciled_amount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_reconciled_amounts_containing_commas()
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
            Assert.AreEqual(1234.55, bank_record.Reconciled_amount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_amounts_preceded_by_pound_signs()
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
            Assert.AreEqual(1234.55, bank_record.Reconciled_amount);
        }

        [Test]
        public void Amounts_should_be_written_using_pound_signs()
        {
            // Arrange
            var bank_record = new BankRecord();
            var amount_with_pound_sign = "£123.55";
            string csv_line = String.Format("01/04/2017^{0}^^POS^Purchase^1234^^^^", amount_with_pound_sign);
            bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = bank_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("01/04/2017,{0},,POS,\"Purchase\",1234,,,,", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_negative_amounts()
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
            Assert.AreEqual(-123.55, bank_record.Reconciled_amount);
        }

        [Test]
        public void Should_be_able_to_cope_with_too_few_input_fields()
        {
            // Arrange
            var bank_record = new BankRecord();
            string csv_line = "01/04/2017^12.34^^POS^Purchase";

            // Act 
            bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, bank_record.Reconciled_amount);
        }

        [Test]
        public void Will_throw_exception_if_date_is_unpopulated()
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
        public void Will_throw_exception_if_unreconciled_amount_is_unpopulated()
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
        public void Will_throw_exception_if_type_is_unpopulated()
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
        public void Will_assume_missing_type_field_if_description_is_unpopulated()
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
        public void When_copying_record_will_copy_all_important_data()
        {
            // Arrange
            var bank_record = new BankRecord
            {
                Date = DateTime.Today,
                Unreconciled_amount = 12.34,
                Description = "Description",
                Reconciled_amount = 56.78,
                Type = "Type",
                Cheque_number = 10
            };
            bank_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (BankRecord)bank_record.Copy();

            // Assert
            Assert.AreEqual(bank_record.Date, copied_record.Date);
            Assert.AreEqual(bank_record.Unreconciled_amount, copied_record.Unreconciled_amount);
            Assert.AreEqual(bank_record.Description, copied_record.Description);
            Assert.AreEqual(bank_record.Reconciled_amount, copied_record.Reconciled_amount);
            Assert.AreEqual(bank_record.Type, copied_record.Type);
            Assert.AreEqual(bank_record.Cheque_number, copied_record.Cheque_number);
            Assert.AreEqual(bank_record.OutputSourceLine, copied_record.OutputSourceLine);
        }

        [Test]
        public void When_copying_record_will_create_new_object()
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
                Unreconciled_amount = original_unreconciled_amount,
                Description = original_description,
                Reconciled_amount = original_reconciled_amount,
                Type = original_type,
                Cheque_number = original_cheque_number
            };
            bank_record.Update_source_line_for_output(',');
            var original_source_line = bank_record.OutputSourceLine;

            // Act 
            var copied_record = (BankRecord)bank_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Unreconciled_amount = copied_record.Unreconciled_amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Reconciled_amount = copied_record.Reconciled_amount + 1;
            copied_record.Type = copied_record.Type + "something else";
            copied_record.Cheque_number = copied_record.Cheque_number + 1;
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, bank_record.Date);
            Assert.AreEqual(original_unreconciled_amount, bank_record.Unreconciled_amount);
            Assert.AreEqual(original_description, bank_record.Description);
            Assert.AreEqual(original_reconciled_amount, bank_record.Reconciled_amount);
            Assert.AreEqual(original_source_line, bank_record.OutputSourceLine);
            Assert.AreEqual(original_type, bank_record.Type);
            Assert.AreEqual(original_cheque_number, bank_record.Cheque_number);
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
            bank_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.DateSpreadsheetIndex + 1, bank_record.Match.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, bank_record.Match.Main_amount()), "Amount");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, ((ActualBankRecord)bank_record.Match).Type), "Type");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, bank_record.Match.Description), "Desc");
        }
    }
}
