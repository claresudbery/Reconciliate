using System;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class CredCard1InOutRecordTests
    {
        [Test]
        public void Can_read_date_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string expected_date_as_string = "19/12/2016";
            string csv_line = String.Format("{0}^£7.99^^ZZZSpecialDescription017^", expected_date_as_string);
            var expected_date = Convert.ToDateTime(expected_date_as_string, StringHelper.Culture());

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_in_out_record.Date);
        }

        [Test]
        public void Can_read_unreconciled_amount_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            double expected_amount = 7888.99;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_amount_surrounded_by_quotes_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            double expected_amount = 7888.99;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/12/2016^\"{0}\"^^ZZZSpecialDescription017^", input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_amount_containing_comma_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = "19/12/2016^£5,678.99^^ZZZSpecialDescription017^";

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(5678.99, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Can_read_data_from_csv_with_extra_separator_at_end()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var expected_description = "ZZZSpecialDescription017";
            string csv_line = String.Format("19/12/2016^£7.99^^{0}^^", expected_description);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card1_in_out_record.Description);
        }

        [Test]
        public void Can_read_description_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var expected_description = "ZZZSpecialDescription017";
            string csv_line = String.Format("19/12/2016^£7.99^^{0}^", expected_description);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card1_in_out_record.Description);
        }

        [Test]
        public void Can_read_reconciled_amount_from_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var expected_amount = 238.92;
            string input_amount = "£" + expected_amount;
            string csv_line = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^{0}", input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Can_cope_with_empty_date()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format("^£7.99^^ZZZSpecialDescription017^");

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_in_out_record.Date);
        }

        [Test]
        public void Can_cope_with_empty_unreconciled_amount()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^^^ZZZSpecialDescription017^");

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Can_cope_with_empty_reconciled_amount()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^");

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Can_cope_with_bad_date()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0}^£7.99^^ZZZSpecialDescription017^", bad_date);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_in_out_record.Date);
        }

        [Test]
        public void Can_cope_with_bad_unreconciled_amount()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", bad_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Can_cope_with_bad_reconciled_amount()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var bad_amount = "not an amount";
            string csv_line = String.Format("19/12/2016^£7.99^^ZZZSpecialDescription017^{0}", bad_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Can_make_main_amount_positive()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var negative_amount = -23.23;
            string input_amount = "-£" + negative_amount * -1;
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", input_amount);
            cred_card1_in_out_record.Load(csv_line);

            // Act 
            cred_card1_in_out_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(negative_amount * -1, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void If_main_amount_already_positive_then_making_it_positive_has_no_effect()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var positive_amount = 23.23;
            string input_amount = "£" + positive_amount;
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", input_amount);
            cred_card1_in_out_record.Load(csv_line);

            // Act 
            cred_card1_in_out_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(positive_amount, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Csv_is_constructed_correctly_without_matched_record()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            cred_card1_in_out_record.Load(csv_line);
            cred_card1_in_out_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card1_in_out_record.To_csv();

            // Assert
            Assert.AreEqual("19/12/2016,£12.34,,\"Bantams\",£33.44,", constructed_csv_line);
        }

        [Test]
        public void Csv_is_constructed_correctly_with_matched_record()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            cred_card1_in_out_record.Load(csv_line);
            cred_card1_in_out_record.Matched = false;
            string matched_record_csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE\",12.33");
            var matched_record = new CredCard1Record();
            matched_record.Load(matched_record_csv_line);
            cred_card1_in_out_record.Match = matched_record;

            // Act 
            string constructed_csv_line = cred_card1_in_out_record.To_csv();

            // Assert
            Assert.AreEqual("19/12/2016,£12.34,,\"Bantams\",£33.44,,,17/02/2017,£12.33,\"ANY STORE\"", constructed_csv_line);
        }

        [Test]
        public void Empty_fields_are_output_as_nothing_for_csv()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Format("19/12/2016^^^Bantams^^");
            cred_card1_in_out_record.Load(csv_line);
            cred_card1_in_out_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card1_in_out_record.To_csv();

            // Assert
            Assert.AreEqual("19/12/2016,,,\"Bantams\",,", constructed_csv_line);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_surrounded_by_spaces()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something , something, something else";
            string csv_line = String.Format("19/12/2016^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Reconciled_amount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_followed_by_spaces()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something, something, something else";
            string csv_line = String.Format("19/12/2016^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Reconciled_amount);
        }

        // Note that these tests are arguably redundant, as the input uses ^ as a separator, instead of comma.
        // But still it's nice to know we can cope with commas.
        [Test]
        public void Can_cope_with_input_containing_commas_preceded_by_spaces()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            double expected_amount = 12.35;
            string input_amount = "£" + expected_amount;
            string text_containing_commas = "something ,something ,something else";
            string csv_line = String.Format("19/12/2016^^^{0}^{1}", text_containing_commas, input_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Amounts_containing_commas_should_be_encased_in_quotes()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/12/2016^{0}^^Bantams^", amount_containing_comma);
            cred_card1_in_out_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card1_in_out_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("19/12/2016,\"{0}\",,\"Bantams\",,", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_unreconciled_amounts_containing_commas()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", amount_containing_comma);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card1_in_out_record.Unreconciled_amount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_reconciled_amounts_containing_commas()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("19/12/2016^^^ZZZSpecialDescription017^{0}", amount_containing_comma);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card1_in_out_record.Reconciled_amount);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_amounts_preceded_by_pound_signs()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var amount_with_pound_sign = "£1,234.55";
            string csv_line = String.Format("19/12/2016^{0}^^ZZZSpecialDescription017^", amount_with_pound_sign);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(1234.55, cred_card1_in_out_record.Unreconciled_amount);
        }

        [Test]
        public void Amounts_should_be_written_using_pound_signs()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var amount_with_pound_sign = "£123.55";
            string csv_line = String.Format("19/12/2016^{0}^^Bantams^", amount_with_pound_sign);
            cred_card1_in_out_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card1_in_out_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("19/12/2016,{0},,\"Bantams\",,", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        // This doesn't apply to ActualBank and CredCard1 because their input does not have £ signs or commas
        [Test]
        public void Should_be_able_to_read_negative_amounts()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            var negative_amount = "-£123.55";
            string csv_line = String.Format("19/12/2016^^^ZZZSpecialDescription017^{0}", negative_amount);

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(-123.55, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Should_be_able_to_cope_with_empty_input()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = String.Empty;

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_in_out_record.Reconciled_amount);
        }

        [Test]
        public void Will_add_default_description_if_description_is_unpopulated()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord();
            string csv_line = "19/12/2016^£123.55^^^";

            // Act 
            cred_card1_in_out_record.Load(csv_line);

            // Assert
            Assert.AreEqual("Source record had no description", cred_card1_in_out_record.Description);
        }

        [Test]
        public void When_copying_record_will_copy_all_important_data()
        {
            // Arrange
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Date = DateTime.Today,
                Unreconciled_amount = 12.34,
                Description = "Description",
                Reconciled_amount = 56.78
            };
            cred_card1_in_out_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (CredCard1InOutRecord)cred_card1_in_out_record.Copy();

            // Assert
            Assert.AreEqual(cred_card1_in_out_record.Date, copied_record.Date);
            Assert.AreEqual(cred_card1_in_out_record.Unreconciled_amount, copied_record.Unreconciled_amount);
            Assert.AreEqual(cred_card1_in_out_record.Description, copied_record.Description);
            Assert.AreEqual(cred_card1_in_out_record.Reconciled_amount, copied_record.Reconciled_amount);
            Assert.AreEqual(cred_card1_in_out_record.Source_line, copied_record.Source_line);
        }

        [Test]
        public void When_copying_record_will_create_new_object()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_unreconciled_amount = 12.34;
            var original_description = "Description";
            var original_reconciled_amount = 56.78;
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Date = original_date,
                Unreconciled_amount = original_unreconciled_amount,
                Description = original_description,
                Reconciled_amount = original_reconciled_amount
            };
            cred_card1_in_out_record.Update_source_line_for_output(',');
            var original_source_line = cred_card1_in_out_record.Source_line;

            // Act 
            var copied_record = (CredCard1InOutRecord)cred_card1_in_out_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Unreconciled_amount = copied_record.Unreconciled_amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Reconciled_amount = copied_record.Reconciled_amount + 1;
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, cred_card1_in_out_record.Date);
            Assert.AreEqual(original_unreconciled_amount, cred_card1_in_out_record.Unreconciled_amount);
            Assert.AreEqual(original_description, cred_card1_in_out_record.Description);
            Assert.AreEqual(original_reconciled_amount, cred_card1_in_out_record.Reconciled_amount);
            Assert.AreEqual(original_source_line, cred_card1_in_out_record.Source_line);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillAddMatchData_WhenPopulatingCredCard1SpreadsheetRow()
        {
            // Arrange
            var row = 10;
            var cred_card1_in_out_record = new CredCard1InOutRecord
            {
                Match = new CredCard1Record
                {
                    Date = DateTime.Today,
                    Amount = 22.34,
                    Description = "match description"
                }
            };
            var mock_cells = new Mock<ICellSet>();

            // Act 
            cred_card1_in_out_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.DateSpreadsheetIndex + 1, cred_card1_in_out_record.Match.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.AmountSpreadsheetIndex + 1, cred_card1_in_out_record.Match.Main_amount()), "Amount");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.DescriptionSpreadsheetIndex + 1, cred_card1_in_out_record.Match.Description), "Desc");
        }
    }
}
