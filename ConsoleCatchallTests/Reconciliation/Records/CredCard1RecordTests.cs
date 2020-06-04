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
        public void Can_read_date_from_csv()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string expected_date_as_string = "17/02/2017";
            string csv_line = String.Format("{0},23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",1.94", expected_date_as_string);
            var expected_date = Convert.ToDateTime(expected_date_as_string, StringHelper.Culture());

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_record.Date);
        }

        [Test]
        public void Can_read_reference_from_csv()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string expected_reference = "22223333";
            string csv_line = String.Format("17/02/2017,23/11/2018,{0},\"ANY STORE 8888        ANYWHERE\",1.94", expected_reference);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_reference, cred_card1_record.Reference);
        }

        [Test]
        public void Can_read_description_from_csv()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_description = "ANY STORE 8888        ANYWHERE";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},1.94", expected_description);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_description, cred_card1_record.Description);
        }

        [Test]
        public void Can_read_amount_from_csv_without_pound_sign()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_amount = -1.94;
            string csv_line = $"19/10/2018,23/11/2018,11112222,SQ *HAPPY BOOK STORE,{expected_amount},";

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void When_Description_Ends_With_Spaces_Amount_Can_Still_Be_Read()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_amount = -1.94;
            string csv_line = $"19/10/2018,23/11/2018,11112222,SQ *HAPPY BOOK STORE   ,{expected_amount},";

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Can_read_amount_from_csv_with_pound_sign()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_amount = 1.94;
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", expected_amount);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Can_cope_with_empty_date()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_date = new DateTime(9999, 9, 9);
            string csv_line = String.Format(",23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",1.94");

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_record.Date);
        }

        [Test]
        public void Can_cope_with_empty_reference()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string csv_line = String.Format("17/02/2017,23/11/2018,,\"ANY STORE 8888        ANYWHERE\",1.94");

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(String.Empty, cred_card1_record.Reference);
        }

        [Test]
        public void Can_cope_with_empty_description()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,,1.94");

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual("", cred_card1_record.Description);
        }

        [Test]
        public void Can_cope_with_empty_amount()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",");

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_record.Amount);
        }

        [Test]
        public void Can_cope_with_bad_date()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expected_date = new DateTime(9999, 9, 9);
            var bad_date = "not a date";
            string csv_line = String.Format("{0},23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",", bad_date);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_record.Date);
        }

        [Test]
        public void Can_cope_with_bad_amount()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var bad_amount = "not an amount";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", bad_amount);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_record.Amount);
        }

        [Test]
        public void Should_be_able_to_cope_with_empty_input()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string csv_line = String.Empty;

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(0, cred_card1_record.Amount);
        }

        [Test]
        public void Can_make_main_amount_positive()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var negative_amount = -23.23;
            var csv_negative_amount = negative_amount * -1;
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", csv_negative_amount);
            cred_card1_record.Load(csv_line);

            // Act 
            cred_card1_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(negative_amount * -1, cred_card1_record.Amount);
        }

        [Test]
        public void If_main_amount_already_positive_then_making_it_positive_has_no_effect()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var positive_amount = 23.23;
            var csv_positive_amount = positive_amount * -1;
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,\"ANY STORE 8888        ANYWHERE\",{0}", csv_positive_amount);
            cred_card1_record.Load(csv_line);

            // Act 
            cred_card1_record.Make_main_amount_positive();

            // Assert
            Assert.AreEqual(positive_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Csv_is_constructed_correctly()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,12.33");
            cred_card1_record.Load(csv_line);
            cred_card1_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card1_record.To_csv();

            // Assert
            Assert.AreEqual("17/02/2017,£12.33,\"ANY STORE\"", constructed_csv_line);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_surrounded_by_spaces()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            double expected_amount = 12.35;
            string text_containing_commas = "something ,something , something, something else";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", text_containing_commas, expected_amount);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_followed_by_spaces()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            double expected_amount = 12.35;
            string text_containing_commas = "something, something, something else";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", text_containing_commas, expected_amount);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Can_cope_with_input_containing_commas_preceded_by_spaces()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            double expected_amount = 12.35;
            string text_containing_commas = "something ,something ,something else";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},{1}", text_containing_commas, expected_amount);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord because the input uses ^ as a separator, instead of comma.
        [Test]
        public void Commas_in_input_are_replaced_by_semi_colons()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            string text_containing_commas = "something, something, something else";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},12.35", text_containing_commas);

            // Act 
            cred_card1_record.Load(csv_line);

            // Assert
            Assert.AreEqual("something; something; something else", cred_card1_record.Description);
        }

        // This doesn't apply to CredCard1InOutRecord and BankRecord and CredCard2Record because the input is never encased in quotes.
        [Test]
        public void If_input_is_encased_in_quotes_then_output_only_has_one_encasing_set_of_quotes()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var description_encased_in_one_set_of_quotes = "\"ANY STORE\"";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,{0},12.33", description_encased_in_one_set_of_quotes);
            cred_card1_record.Load(csv_line);
            cred_card1_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card1_record.To_csv();

            // Assert
            var expected_csv_line = String.Format("17/02/2017,£12.33,{0}", description_encased_in_one_set_of_quotes);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void Amounts_containing_commas_should_be_encased_in_quotes()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var amount = 1234.55;
            var amount_containing_comma = "£1,234.55";
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,{0}", amount);
            cred_card1_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card1_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("17/02/2017,\"{0}\",\"ANY STORE\"", amount_containing_comma);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void Amounts_should_be_written_using_pound_signs()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var amount = "123.55";
            var amount_with_pound_sign = "£" + amount;
            string csv_line = String.Format("17/02/2017,23/11/2018,22223333,ANY STORE,{0}", amount);
            cred_card1_record.Load(csv_line);

            // Act 
            string constructed_csv_line = cred_card1_record.To_csv();

            // Assert
            string expected_csv_line = String.Format("17/02/2017,{0},\"ANY STORE\"", amount_with_pound_sign);
            Assert.AreEqual(expected_csv_line, constructed_csv_line);
        }

        [Test]
        public void When_copying_record_will_copy_all_important_data()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record
            {
                Date = DateTime.Today,
                Amount = 12.34,
                Description = "Description",
                Reference = "33334444"
            };
            cred_card1_record.Update_source_line_for_output(',');

            // Act 
            var copied_record = (CredCard1Record)cred_card1_record.Copy();

            // Assert
            Assert.AreEqual(cred_card1_record.Date, copied_record.Date);
            Assert.AreEqual(cred_card1_record.Amount, copied_record.Amount);
            Assert.AreEqual(cred_card1_record.Description, copied_record.Description);
            Assert.AreEqual(cred_card1_record.Reference, copied_record.Reference);
            Assert.AreEqual(cred_card1_record.Source_line, copied_record.Source_line);
        }

        [Test]
        public void When_copying_record_will_create_new_object()
        {
            // Arrange
            var original_date = DateTime.Today;
            var original_amount = 12.34;
            var original_description = "Description";
            var original_reference = "33334444";
            var cred_card1_record = new CredCard1Record
            {
                Date = original_date,
                Amount = original_amount,
                Description = original_description,
                Reference = original_reference
            };
            cred_card1_record.Update_source_line_for_output(',');
            var original_source_line = cred_card1_record.Source_line;

            // Act 
            var copied_record = (CredCard1Record)cred_card1_record.Copy();
            copied_record.Date = copied_record.Date.AddDays(1);
            copied_record.Amount = copied_record.Amount + 1;
            copied_record.Description = copied_record.Description + "something else";
            copied_record.Reference = copied_record.Reference + 1;
            copied_record.Update_source_line_for_output(',');

            // Assert
            Assert.AreEqual(original_date, cred_card1_record.Date);
            Assert.AreEqual(original_amount, cred_card1_record.Amount);
            Assert.AreEqual(original_description, cred_card1_record.Description);
            Assert.AreEqual(original_reference, cred_card1_record.Reference);
            Assert.AreEqual(original_source_line, cred_card1_record.Source_line);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateCredCard1RecordCells()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Amount = 1234.56,
                Description = "Acme: Esmerelda's birthday"
            };
            var row = 10;
            var mock_cells = new Mock<ICellSet>();

            // Act 
            cred_card1_record.Populate_spreadsheet_row(mock_cells.Object, row);

            // Assert
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.DateSpreadsheetIndex + 1, cred_card1_record.Date), "Date");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.AmountSpreadsheetIndex + 1, cred_card1_record.Main_amount()), "Amount");
            mock_cells.Verify(x => x.Populate_cell(row, CredCard1Record.DescriptionSpreadsheetIndex + 1, cred_card1_record.Description), "Desc");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillReadFromCredCard1RecordCells()
        {
            // Arrange
            var sheet_name = "MockSheet";
            DateTime expected_date = new DateTime(year: 2018, month: 6, day: 1);
            string expected_reference = "55556666";
            String expected_description = "description3";
            Double expected_amount = 37958.90;
            var excel_date = expected_date.ToOADate();
            var excel_date2 = expected_date.AddDays(1).ToOADate();
            var fake_cell_row = new FakeCellRow().With_fake_data(new List<object>
            {
                excel_date,
                excel_date2,
                expected_reference,
                expected_description,
                expected_amount
            });
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Read_last_row(sheet_name)).Returns(fake_cell_row);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var cred_card1_record = new CredCard1Record();
            var cells = spreadsheet.Read_last_row(sheet_name);

            // Act 
            cred_card1_record.Read_from_spreadsheet_row(cells);

            // Assert
            Assert.AreEqual(expected_date, cred_card1_record.Date);
            Assert.AreEqual(expected_reference, cred_card1_record.Reference);
            Assert.AreEqual(expected_description, cred_card1_record.Description);
            Assert.AreEqual(expected_amount, cred_card1_record.Amount);
        }

        [Test]
        public void Amounts_will_come_in_positive_and_stay_positive()
        {
            // Arrange
            var cred_card1_record = new CredCard1Record();
            var expectedAmount = 12.33;
            string csv_line = String.Format($"17/02/2017,23/11/2018,22223333,ANY STORE,{expectedAmount}");
            cred_card1_record.Load(csv_line);
            cred_card1_record.Matched = false;

            // Act 
            string constructed_csv_line = cred_card1_record.To_csv();

            // Assert
            Assert.AreEqual(expectedAmount, cred_card1_record.Amount);
        }
    }
}
