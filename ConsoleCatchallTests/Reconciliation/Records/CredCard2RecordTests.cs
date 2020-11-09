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
        public void Can_read_date_from_csv()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string expected_date_as_string = "08/06/2017";
            string csv_line = String.Format("{0},ACME UK HOLDINGS,Ms Pippi Long,ref,13.49", expected_date_as_string);
            var expected_date = DateTime.ParseExact(expected_date_as_string, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void Can_read_description_from_csv()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_description = "ACME UK HOLDINGS";
            string csv_line = String.Format("08/06/2017,{0},Ms Pippi Long,ref,13.49", expected_description);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card2_record.Description);
        }

        [Test]
        public void Can_read_amount_from_csv_with_quotes_and_leading_space()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_amount = 1.94;
            string input_amount = "\" " + expected_amount + "\"";
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", input_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card2_record.Amount);
        }

        [Test]
        public void Can_cope_with_empty_date()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format(",ACME UK HOLDINGS,Ms Pippi Long,ref,13.49");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void Can_cope_with_empty_description()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,,Ms Pippi Long,ref,13.49");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual("", cred_card2_record.Description);
        }

        [Test]
        public void Can_cope_with_empty_amount()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,");

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_record.Amount);
        }

        [Test]
        public void Can_cope_with_bad_date()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0},ACME UK HOLDINGS,Ms Pippi Long,ref,13.49", bad_date);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card2_record.Date);
        }

        [Test]
        public void Can_cope_with_bad_amount()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var bad_amount = "not an amount";
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", bad_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card2_record.Amount);
        }

        [Test]
        public void Can_make_main_amount_positive()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var negative_amount = -23.23;
            string input_amount = "-" + negative_amount * -1;
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", input_amount);
            cred_card2_record.Load(csv_line);

            // Act 
            cred_card2_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(negative_amount * -1, cred_card2_record.Amount);
        }

        [Test]
        public void If_main_amount_already_positive_then_making_it_positive_has_no_effect()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var positive_amount = 23.23;
            string input_amount = positive_amount.ToString();
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", input_amount);
            cred_card2_record.Load(csv_line);

            // Act 
            cred_card2_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(positive_amount, cred_card2_record.Amount);
        }

        [Test]
        public void Csv_is_constructed_correctly()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,13.49");
            cred_card2_record.Load(csv_line);
            cred_card2_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card2_record.To_csv();

            // Assert
            Assert.AreEqual("08/06/2017,£13.49,\"ACME UK HOLDINGS\"", constructed_csv_line);
        }

        // This doesn't apply to CredCard2Record and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void Commas_in_input_are_replaced_by_semi_colons()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            string text_containing_commas = "something, something, something else";
            string csv_line = String.Format("08/06/2017,{0},Ms Pippi Long,ref,13.49", text_containing_commas);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual("something; something; something else", cred_card2_record.Description);
        }

        [Test]
        public void Amounts_should_be_written_using_pound_signs()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var amount = "123.55";
            var amount_with_pound_sign = "£" + amount;
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", amount);
            cred_card2_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card2_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("08/06/2017,{0},\"ACME UK HOLDINGS\"", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_negative_amounts()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record();
            var negative_amount = "-123.55";
            string csv_line = String.Format("08/06/2017,ACME UK HOLDINGS,Ms Pippi Long,ref,{0}", negative_amount);

            // Act 
            cred_card2_record.Load(csv_line);

            // Assert
            Assert.AreEqual(-123.55, cred_card2_record.Amount);
        }

        [Test]
        public void Should_be_able_to_cope_with_empty_input()
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
        public void When_copying_record_will_copy_all_important_data()
        {
            // Arrange
            var cred_card2_record = new CredCard2Record
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description"
            };
            cred_card2_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (CredCard2Record)cred_card2_record.Copy();

            // Assert
            Assert.AreEqual(cred_card2_record.Date, copied_record.Date);
            Assert.AreEqual(cred_card2_record.Amount, copied_record.Amount);
            Assert.AreEqual(cred_card2_record.Description, copied_record.Description);
            Assert.AreEqual(cred_card2_record.OutputSourceLine, copied_record.OutputSourceLine);
        }

        [Test]
        public void When_copying_record_will_create_new_object()
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
            cred_card2_record.Update_source_line_for_output(',');
            var original_source_line = cred_card2_record.OutputSourceLine;

            // Act 
            var copied_record = (CredCard2Record)cred_card2_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Amount = copied_record.Amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, cred_card2_record.Date);
            Assert.AreEqual(original_amount, cred_card2_record.Amount);
            Assert.AreEqual(original_description, cred_card2_record.Description);
            Assert.AreEqual(original_source_line, cred_card2_record.OutputSourceLine);
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
            cred_card2_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, CredCard2Record.DateSpreadsheetIndex + 1, cred_card2_record.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard2Record.AmountSpreadsheetIndex + 1, cred_card2_record.Main_amount()), "Amount");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard2Record.DescriptionSpreadsheetIndex + 1, cred_card2_record.Description), "Desc");
        }
    }
}
