using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class ActualBankRecordTests
    {
        [Test]
        public void Can_read_date_from_csv()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string expected_date_as_string = "06/03/2017";
            string csv_line = string.Format("{0},BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expected_date_as_string);
            var expected_date = Convert.ToDateTime(expected_date_as_string, StringHelper.Culture());

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, actual_bank_record.Date);
        }

        [Test]
        public void Can_read_type_from_csv()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_type = "BAC";
            string csv_line = String.Format("06/03/2017,{0},\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expected_type);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_type, actual_bank_record.Type);
        }

        [Test]
        public void Can_read_description_from_csv()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_description = "\"'99999999-BFGH\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", expected_description);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, actual_bank_record.Description);
        }

        [Test]
        public void Can_read_amount_from_csv_without_pound_sign()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_amount = 261.40;
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", expected_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, actual_bank_record.Amount);
        }

        [Test]
        public void Can_read_amount_from_csv_with_pound_sign()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_amount = 261.40;
            var input_amount = "£" + expected_amount;
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", input_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, actual_bank_record.Amount);
        }

        [Test]
        public void Can_read_balance_from_csv_without_pound_sign()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_amount = 4273.63;
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", expected_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, actual_bank_record.Balance);
        }

        [Test]
        public void Can_read_balance_from_csv_with_pound_sign()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_amount = 427.63;
            var input_amount = "£" + expected_amount;
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", input_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, actual_bank_record.Balance);
        }

        [Test]
        public void Can_cope_with_empty_date()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format(",BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, actual_bank_record.Date);
        }

        [Test]
        public void Can_cope_with_empty_type()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Format("06/03/2017,,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual("", actual_bank_record.Type);
        }

        [Test]
        public void Can_cope_with_empty_description()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Format("06/03/2017,BAC,,261.40,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual("", actual_bank_record.Description);
        }

        [Test]
        public void Can_cope_with_empty_amount()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",,4273.63,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, actual_bank_record.Amount);
        }

        [Test]
        public void Can_cope_with_empty_balance()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,,\"'Envelope\",\"'228822-99933422\",");

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, actual_bank_record.Balance);
        }

        [Test]
        public void Can_cope_with_bad_date()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0},BAC,\"'99999999-BFGH\",261.40,4273.63,\"'Envelope\",\"'228822-99933422\",", bad_date);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, actual_bank_record.Date);
        }

        [Test]
        public void Can_cope_with_bad_amount()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",{0},4273.63,\"'Envelope\",\"'228822-99933422\",", bad_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, actual_bank_record.Amount);
        }

        [Test]
        public void Can_cope_with_bad_balance()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("06/03/2017,BAC,\"'99999999-BFGH\",261.40,{0},\"'Envelope\",\"'228822-99933422\",", bad_amount);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, actual_bank_record.Balance);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_surrounded_by_spaces()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            double expected_balance = 12.35;
            string text_containing_commas = "\"'0363 23MAR17 C ,TFL.GOV.UK/CP , TFL TRAVEL CH GB\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", text_containing_commas, expected_balance);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_balance, actual_bank_record.Balance);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_followed_by_spaces()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            double expected_balance = 12.35;
            string text_containing_commas = "\"'0363 23MAR17 C, TFL.GOV.UK/CP, TFL TRAVEL CH GB\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", text_containing_commas, expected_balance);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_balance, actual_bank_record.Balance);
        }

        [Test]
        public void Should_be_able_to_cope_with_empty_input()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Empty;

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, actual_bank_record.Amount);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_preceded_by_spaces()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            double expected_balance = 12.35;
            string text_containing_commas = "\"'0363 23MAR17 C ,TFL.GOV.UK/CP ,TFL TRAVEL CH GB\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},261.40,{1},\"'Envelope\",\"'228822-99933422\",", text_containing_commas, expected_balance);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_balance, actual_bank_record.Balance);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void Commas_in_input_are_replaced_by_semi_colons()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string text_containing_commas = "\"something, something, something else\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},261.40,12.35,\"'Envelope\",\"'228822-99933422\",", text_containing_commas);

            // Act 
            actual_bank_record.Load(csv_line);

            // Assert
            Assert.AreEqual("\"something; something; something else\"", actual_bank_record.Description);
        }

        [Test]
        public void Can_make_main_amount_positive()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var negative_amount = -23.23;
            string csv_line = String.Format("06/03/2017,BAC,\"'0363 23MAR17 C , TFL.GOV.UK/CP , TFL TRAVEL CH GB\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", negative_amount);
            actual_bank_record.Load(csv_line);

            // Act 
            actual_bank_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(negative_amount * -1, actual_bank_record.Amount);
        }

        [Test]
        public void If_main_amount_already_positive_then_making_it_positive_has_no_effect()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var positive_amount = 23.23;
            string csv_line = String.Format("06/03/2017,BAC,\"'0363 23MAR17 C , TFL.GOV.UK/CP , TFL TRAVEL CH GB\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", positive_amount);
            actual_bank_record.Load(csv_line);

            // Act 
            actual_bank_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(positive_amount, actual_bank_record.Amount);
        }

        [Test]
        public void Csv_is_constructed_correctly()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            string csv_line = String.Format("06/03/2017,BAC,\"'Some , description\",127.69,261.40,\"'Envelope\",\"'228822-99933422\",");
            actual_bank_record.Load(csv_line);
            actual_bank_record.Matched = false;

            // Act 
            string constructed_csv_line = actual_bank_record.To_csv();

            // Assert
            Assert.AreEqual("06/03/2017,£127.69,BAC,\"'Some ; description\"", constructed_csv_line);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord and CredCard2Record because the input is never encased in quotes.
        [Test]
        public void If_input_is_encased_in_quotes_then_output_only_has_one_encasing_set_of_quotes()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var description_encased_in_one_set_of_quotes = "\"'Some description\"";
            string csv_line = String.Format("06/03/2017,BAC,{0},127.69,261.40,\"'Envelope\",\"'228822-99933422\",", description_encased_in_one_set_of_quotes);
            actual_bank_record.Load(csv_line);
            actual_bank_record.Matched = false;

            // Act 
            string constructed_csv_line = actual_bank_record.To_csv();

            // Assert
            var expected_csv_line = String.Format("06/03/2017,£127.69,BAC,{0}", description_encased_in_one_set_of_quotes);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void Amounts_containing_commas_should_be_encased_in_quotes()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var amount = 1234.55;
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("06/03/2017,BAC,\"description\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", amount);
            actual_bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = actual_bank_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("06/03/2017,\"{0}\",BAC,\"description\"", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void Amounts_should_be_written_using_pound_signs()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord();
            var amount = "123.55";
            var amount_with_pound_sign = "£" + amount;
            string csv_line = String.Format("06/03/2017,BAC,\"description\",{0},261.40,\"'Envelope\",\"'228822-99933422\",", amount);
            actual_bank_record.Load(csv_line);

            // Act 
            string constructed_csv_line = actual_bank_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("06/03/2017,{0},BAC,\"description\"", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void When_copying_cred_card2_record_will_copy_all_important_data()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description",
                Type = "Type"
            };
            actual_bank_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (ActualBankRecord)actual_bank_record.Copy();

            // Assert
            Assert.AreEqual(actual_bank_record.Date, copied_record.Date);
            Assert.AreEqual(actual_bank_record.Amount, copied_record.Amount);
            Assert.AreEqual(actual_bank_record.Description, copied_record.Description);
            Assert.AreEqual(actual_bank_record.Type, copied_record.Type);
            Assert.AreEqual(actual_bank_record.Source_line, copied_record.Source_line);
        }

        [Test]
        public void When_copying_cred_card2_record_will_create_new_object()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_amount = 12.34;
            var original_description = "Description";
            var original_type = "Type";
            var actual_bank_record = new ActualBankRecord
            {
                Date = original_date,
                Amount = original_amount,
                Description = original_description,
                Type = original_type
            };
            actual_bank_record.Update_source_line_for_output(',');
            var original_source_line = actual_bank_record.Source_line;

            // Act 
            var copied_record = (ActualBankRecord)actual_bank_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Amount = copied_record.Amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Type = copied_record.Type + "something else";
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, actual_bank_record.Date);
            Assert.AreEqual(original_amount, actual_bank_record.Amount);
            Assert.AreEqual(original_description, actual_bank_record.Description);
            Assert.AreEqual(original_type, actual_bank_record.Type);
            Assert.AreEqual(original_source_line, actual_bank_record.Source_line);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateActualBankRecordCells()
        {
            // Arrange
            var actual_bank_record = new ActualBankRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Type = "Chq",
                Description = "Acme: Esmerelda's birthday",
                Amount = 1234.56
            };
            var row = 10;
            var mock_cells = new Mock<ICellSet>();

            // Act 
            actual_bank_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.DateSpreadsheetIndex + 1, actual_bank_record.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.AmountSpreadsheetIndex + 1, actual_bank_record.Main_amount()), "Amount");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.TypeSpreadsheetIndex + 1, actual_bank_record.Type), "Type");
            mock_cells.Verify(x => x.Populate_cell(row, ActualBankRecord.DescriptionSpreadsheetIndex + 1, actual_bank_record.Description), "Desc");
        }
    }
}
