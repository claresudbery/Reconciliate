using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class CSVFileTests
    {
        private string _csv_file_path;

        public CSVFileTests()
        {
            Populate_csv_file_path();
            TestHelper.Set_correct_date_formatting();
        }

        private void Populate_csv_file_path()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            _csv_file_path = TestHelper.Fully_qualified_csv_file_path(current_path);
        }

        [Test]
        public void M_CanCreateEmptyCSVFile()
        {
            // Act
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory());
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(false);

            // Assert
            Assert.AreEqual(0, csv_file.Records.Count);
        }

        [Test]
        public void Can_load_csv_file()
        {
            // Act & Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankOut-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();

            // Assert
            Assert.AreEqual("01/03/2017^£5.00^^TILL^DescriptionBank013^^^^^", csv_file.File_contents[0]);
        }

        [Test]
        public void Will_order_records_by_date_on_loading_by_default()
        {
            // Act & Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();

            // Assert
            Assert.AreEqual("01/02/2017", csv_file.Records[0].Date.ToShortDateString());
            Assert.AreEqual("01/03/2017", csv_file.Records[1].Date.ToShortDateString());
            Assert.AreEqual("24/03/2017", csv_file.Records[2].Date.ToShortDateString());
            Assert.AreEqual("01/04/2017", csv_file.Records[3].Date.ToShortDateString());
            Assert.AreEqual("03/04/2017", csv_file.Records[4].Date.ToShortDateString());
        }

        [Test]
        public void WillNotOrderRecordsByDateOnLoading_WhenSpecified()
        {
            // Arrange
            bool order_by_date = false;

            // Act
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                order_by_date);

            // Assert
            Assert.AreEqual("24/03/2017", csv_file.Records[0].Date.ToShortDateString());
            Assert.AreEqual("03/04/2017", csv_file.Records[1].Date.ToShortDateString());
            Assert.AreEqual("01/04/2017", csv_file.Records[2].Date.ToShortDateString());
            Assert.AreEqual("01/03/2017", csv_file.Records[3].Date.ToShortDateString());
            Assert.AreEqual("01/02/2017", csv_file.Records[4].Date.ToShortDateString());
        }

        [Test]
        public void Can_load_bank_out_records()
        {
            // Act & Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankOut-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();

            // Assert
            Assert.AreEqual("TILL", csv_file.Records[0].Type);
            Assert.AreEqual(47, csv_file.Records.Count);
            Assert.AreEqual("ZZZEsterene plinkle (Inestimable plarts)", csv_file.Records[46].Description);
        }

        [Test]
        public void Can_load_bank_in_records()
        {
            // Act & Arrange
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);

            // Assert
            Assert.AreEqual("PCL", csv_file.Records[0].Type);
            Assert.AreEqual(5, csv_file.Records.Count);
            Assert.AreEqual("ZZZThing3", csv_file.Records[4].Description);
        }

        [Test]
        public void Can_load_cred_card1_records()
        {
            // Act & Arrange
            var file_io = new FileIO<CredCard1Record>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "CredCard1-Statement");
            var csv_file = new CSVFile<CredCard1Record>(file_io);
            csv_file.Load();

            // Assert
            Assert.AreEqual("22223333", csv_file.Records[0].Reference);
            Assert.AreEqual(50, csv_file.Records.Count);
            Assert.AreEqual("Description-CredCard1-001", csv_file.Records[49].Description);
        }

        [Test]
        public void Can_load_cred_card1_in_out_records()
        {
            // Act & Arrange
            var file_io = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "CredCard1InOut-formatted-date-only");
            var csv_file = new CSVFile<CredCard1InOutRecord>(file_io);
            csv_file.Load();

            // Assert
            Assert.AreEqual("ZZZSpecialDescription017", csv_file.Records[0].Description);
            Assert.AreEqual(32, csv_file.Records.Count);
            Assert.AreEqual("Description-CredCard1-form-007", csv_file.Records[31].Description);
        }

        [Test]
        public void Can_load_actual_bank_records()
        {
            // Act & Arrange
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "ActualBank-sample");
            var csv_file = new CSVFile<ActualBankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);

            // Assert
            Assert.AreEqual("'ZZZSpecialDescription001", csv_file.Records[0].Description);
            Assert.AreEqual(67, csv_file.Records.Count);
            Assert.AreEqual(4554.18, csv_file.Records[66].Balance);
        }

        [Test]
        public void Can_filter_for_positive_records_only()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "ActualBank-sample");
            var csv_file = new CSVFile<ActualBankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);

            // Act
            csv_file.Filter_for_positive_records_only();

            // Assert
            Assert.AreEqual("'ZZZSpecialDescription001", csv_file.Records[0].Description);
            Assert.AreEqual(16, csv_file.Records.Count);
            Assert.AreEqual(4669.48, csv_file.Records[15].Balance);
        }

        [Test]
        public void Can_filter_for_negative_records_only()
        {
            // Arrange
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "ActualBank-sample");
            var csv_file = new CSVFile<ActualBankRecord>(file_io);
            csv_file.Load();

            // Act
            csv_file.Filter_for_negative_records_only();

            // Assert
            Assert.AreEqual("'DIVIDE YOUR GREEN", csv_file.Records[0].Description);
            Assert.AreEqual(51, csv_file.Records.Count);
            Assert.AreEqual(4554.18, csv_file.Records[50].Balance);
        }

        [Test]
        public void M_CanRemoveRecordsUsingSpecifiedFilter()
        {
            // Arrange
            var mock_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(
                new List<ActualBankRecord>
                {
                    new ActualBankRecord {Matched = true, Description = "Matched"},
                    new ActualBankRecord {Matched = false, Description = "Not Matched"},
                    new ActualBankRecord {Matched = true, Description = "Matched"},
                    new ActualBankRecord {Matched = false, Description = "Not Matched"}
                }
            );
            var csv_file = new CSVFile<ActualBankRecord>(mock_file_io.Object);
            csv_file.Load();

            // Act
            csv_file.Remove_records(x => x.Matched);

            // Assert
            Assert.AreEqual(2, csv_file.Records.Count);
            Assert.IsFalse(csv_file.Records.Any(x => x.Matched), "None with matched flag");
            Assert.IsTrue(csv_file.Records.All(x => !x.Matched), "All with matched flag = false");
            Assert.IsFalse(csv_file.Records.Any(x => x.Description == "Matched"), "None with matched description");
            Assert.IsTrue(csv_file.Records.All(x => x.Description == "Not Matched"), "All with 'Not Matched' description");
        }

        [Test]
        public void When_filtering_for_negative_records_all_are_converted_to_positive()
        {
            // Arrange
            var file_io = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "ActualBank-sample");
            var csv_file = new CSVFile<ActualBankRecord>(file_io);
            csv_file.Load();

            // Act
            csv_file.Filter_for_negative_records_only();

            // Assert
            Assert.AreEqual(115.30, csv_file.Records[50].Amount);
        }

        [Test]
        public void Can_output_unmatched_records_as_csv_in_date_order()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            csv_file.Reset_all_matches();

            // Act
            List<string> csv_lines = csv_file.Unmatched_records_as_csv();

            // Assert
            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csv_lines[0]);
            Assert.AreEqual("01/03/2017,£350.00,,ABC,\"ZZZThing2\",,,,,", csv_lines[1]);
            Assert.AreEqual("24/03/2017,£200.12,,PCL,\"ZZZSpecialDescription001\",,,,,", csv_lines[2]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csv_lines[3]);
            Assert.AreEqual("03/04/2017,£350.00,,ABC,\"ZZZThing1\",,,,,", csv_lines[4]);
        }

        [Test]
        public void Can_output_matched_records_as_csv_in_date_order()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            foreach (var record in csv_file.Records)
            {
                record.Matched = true;
            }

            // Act
            List<string> csv_lines = csv_file.Matched_records_as_csv();

            // Assert
            Assert.AreEqual("01/02/2017,£350.00,x,ABC,\"ZZZThing3\",,,,,", csv_lines[0]);
            Assert.AreEqual("01/03/2017,£350.00,x,ABC,\"ZZZThing2\",,,,,", csv_lines[1]);
            Assert.AreEqual("24/03/2017,£200.12,x,PCL,\"ZZZSpecialDescription001\",,,,,", csv_lines[2]);
            Assert.AreEqual("01/04/2017,£261.40,x,PCL,\"ZZZSpecialDescription005\",,,,,", csv_lines[3]);
            Assert.AreEqual("03/04/2017,£350.00,x,ABC,\"ZZZThing1\",,,,,", csv_lines[4]);
        }

        [Test]
        public void Can_output_all_records_as_csv_ordered_by_matched_and_then_by_date()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);
            csv_file.Records[0].Matched = true;
            csv_file.Records[1].Matched = true;
            csv_file.Records[3].Matched = true;

            // Act
            List<string> csv_lines = csv_file.All_records_as_csv();

            // Assert
            Assert.AreEqual("01/03/2017,£350.00,x,ABC,\"ZZZThing2\",,,,,", csv_lines[0]);
            Assert.AreEqual("24/03/2017,£200.12,x,PCL,\"ZZZSpecialDescription001\",,,,,", csv_lines[1]);
            Assert.AreEqual("03/04/2017,£350.00,x,ABC,\"ZZZThing1\",,,,,", csv_lines[2]);
            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csv_lines[3]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csv_lines[4]);
        }

        [Test]
        public void Will_order_records_for_spreadsheet_by_matched_and_then_by_date_with_divider_between()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(
                true,
                null,
                doNotOrderByDate);
            csv_file.Records[0].Matched = true;
            csv_file.Records[1].Matched = true;
            csv_file.Records[3].Matched = true;

            // Act
            var records = csv_file.Records_ordered_for_spreadsheet();

            // Assert
            Assert.AreEqual("ZZZThing2", records[0].Description);
            Assert.AreEqual("ZZZSpecialDescription001", records[1].Description);
            Assert.AreEqual("ZZZThing1", records[2].Description);
            Assert.AreEqual(true, records[3].Divider);
            Assert.AreEqual("ZZZThing3", records[4].Description);
            Assert.AreEqual("ZZZSpecialDescription005", records[5].Description);
        }

        [Test]
        public void When_ordering_records_for_spreadsheet_will_insert_divider_between_matched_and_non_matched()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            csv_file.Records[0].Matched = true;
            csv_file.Records[3].Matched = true;

            // Act
            var records = csv_file.Records_ordered_for_spreadsheet();

            // Assert
            Assert.AreEqual(6, records.Count);
            Assert.AreEqual(true, records[1].Matched);
            Assert.AreEqual(false, records[1].Divider);

            Assert.AreEqual(true, records[2].Divider);

            Assert.AreEqual(false, records[3].Matched);
            Assert.AreEqual(false, records[3].Divider);
        }

        [Test]
        public void When_ordering_records_for_spreadsheet_if_no_matched_then_no_divider()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();

            // Act
            var records = csv_file.Records_ordered_for_spreadsheet();

            // Assert
            Assert.AreEqual(5, records.Count);
            Assert.AreEqual(false, records[0].Divider);
            Assert.AreEqual(false, records[1].Divider);
            Assert.AreEqual(false, records[2].Divider);
            Assert.AreEqual(false, records[3].Divider);
            Assert.AreEqual(false, records[4].Divider);
        }

        [Test]
        public void When_ordering_records_for_spreadsheet_if_no_unmatched_then_divider_is_at_end()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            csv_file.Records[0].Matched = true;
            csv_file.Records[1].Matched = true;
            csv_file.Records[2].Matched = true;
            csv_file.Records[3].Matched = true;
            csv_file.Records[4].Matched = true;

            // Act
            var records = csv_file.Records_ordered_for_spreadsheet();

            // Assert
            Assert.AreEqual(6, records.Count);
            Assert.AreEqual(true, records[4].Matched);
            Assert.AreEqual(false, records[4].Divider);

            Assert.AreEqual(false, records[5].Matched);
            Assert.AreEqual(true, records[5].Divider);
        }

        [Test]
        public void Can_output_all_records_as_source_line_ordered_by_date()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();

            // Act
            List<string> csv_lines = csv_file.All_records_as_output_source_lines();

            // Assert
            Assert.AreEqual("01/02/2017^£350.00^^ABC^ZZZThing3^^^^^", csv_lines[0]);
            Assert.AreEqual("01/03/2017^£350.00^^ABC^ZZZThing2^^^^^", csv_lines[1]);
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", csv_lines[2]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", csv_lines[3]);
            Assert.AreEqual("03/04/2017^£350.00^^ABC^ZZZThing1^^^^^", csv_lines[4]);
        }

        [Test]
        public void Can_convert_commas_to_little_hats_for_bank_in()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-with-commas");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load(true, ',');

            // Act
            csv_file.Convert_source_line_separators(',', '^');

            // Assert
            List<string> csv_lines = csv_file.All_records_as_output_source_lines();
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", csv_lines[0]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", csv_lines[1]);
            Assert.AreEqual("03/10/2018^£350.00^^ABC^ZZZThing1^^^^^", csv_lines[2]);
        }

        [Test]
        public void Can_convert_commas_to_little_hats_for_cred_card2_in_out()
        {
            // Arrange
            var file_io = new FileIO<CredCard2InOutRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "CredCard2InOut-with-commas");
            var csv_file = new CSVFile<CredCard2InOutRecord>(file_io);
            csv_file.Load(true, ',');

            // Act
            csv_file.Convert_source_line_separators(',', '^');

            // Assert
            List<string> csv_lines = csv_file.All_records_as_output_source_lines();
            Assert.AreEqual("09/04/2017^£8.33^^ZZZSpecialDescription021^", csv_lines[0]);
            Assert.AreEqual("01/05/2017^£3.16^^ZZZSpecialDescription022^", csv_lines[1]);
            Assert.AreEqual("06/05/2017^£11.94^^ZZZSpecialDescription023^", csv_lines[2]);
            Assert.AreEqual("20/05/2017^£158.32^^ZZZSpecialDescription024^", csv_lines[3]);
            Assert.AreEqual("21/10/2018^£16.05^^ZZZSpecialDescription025^", csv_lines[4]);
        }

        [Test]
        public void Can_convert_commas_to_little_hats_for_cred_card1_in_out()
        {
            // Arrange
            var file_io = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "CredCard1InOut-with-commas");
            var csv_file = new CSVFile<CredCard1InOutRecord>(file_io);
            csv_file.Load(true, ',');

            // Act
            csv_file.Convert_source_line_separators(',', '^');

            // Assert
            List<string> csv_lines = csv_file.All_records_as_output_source_lines();
            Assert.AreEqual("19/12/2016^£7.99^^ZZZSpecialDescription017^", csv_lines[0]);
            Assert.AreEqual("02/01/2017^£6.29^^ZZZSpecialDescription018^", csv_lines[1]);
            Assert.AreEqual("15/02/2017^£1.99^^ZZZSpecialDescription019^", csv_lines[2]);
            Assert.AreEqual("17/10/2018^£1.94^^ZZZSpecialDescription020^", csv_lines[3]);
        }

        [Test]
        public void Can_write_new_contents_to_csv()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var new_bank_record = new BankRecord();
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            var todays_date = DateTime.Today.ToString("dd/MM/yyyy");
            new_bank_record.Load(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todays_date));
            csv_file.Records.Add(new_bank_record);

            // Act
            csv_file.Write_to_csv_file("testing");

            // Assert
            var file_io_test_file = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only-testing");
            var new_csv_file = new CSVFile<BankRecord>(file_io_test_file);
            new_csv_file.Load(
                true,
                ',');
            List<string> csv_lines = new_csv_file.All_records_as_csv();

            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csv_lines[0]);
            Assert.AreEqual("01/03/2017,£350.00,,ABC,\"ZZZThing2\",,,,,", csv_lines[1]);
            Assert.AreEqual("24/03/2017,£200.12,,PCL,\"ZZZSpecialDescription001\",,,,,", csv_lines[2]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csv_lines[3]);
            Assert.AreEqual("03/04/2017,£350.00,,ABC,\"ZZZThing1\",,,,,", csv_lines[4]);
            Assert.AreEqual(String.Format("{0},£12.34,,POS,\"Purchase\",,,,,",todays_date), csv_lines[5]);
        }

        [Test]
        public void Can_write_new_contents_as_source_lines()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var new_bank_record = new BankRecord();
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            var todays_date = DateTime.Today.ToString("dd/MM/yyyy");
            new_bank_record.Load(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todays_date));
            csv_file.Records.Add(new_bank_record);

            // Act
            csv_file.Write_to_file_as_source_lines("BankIn-formatted-date-only-testing");

            // Assert
            List<string> new_file_lines = new List<string>();
            using (var file_stream = File.OpenRead(_csv_file_path + "/" + "BankIn-formatted-date-only-testing" + ".csv"))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    var new_line = reader.ReadLine();
                    new_file_lines.Add(new_line);
                }
            }

            Assert.AreEqual("01/02/2017^£350.00^^ABC^ZZZThing3^^^^^", new_file_lines[0]);
            Assert.AreEqual("01/03/2017^£350.00^^ABC^ZZZThing2^^^^^", new_file_lines[1]);
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", new_file_lines[2]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", new_file_lines[3]);
            Assert.AreEqual("03/04/2017^£350.00^^ABC^ZZZThing1^^^^^", new_file_lines[4]);
            Assert.AreEqual(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todays_date), new_file_lines[5]);
        }

        // Note that this only applies to CredCard1InOutRecord, BankRecord, CredCard2InOutRecord - because commas are stripped from input in third party records.
        // But not from owned records, because we use ^ as a separator instead of comma.
        [Test]
        public void Can_write_descriptions_containing_commas()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var new_bank_record = new BankRecord();
            var description_containing_comma = "something, something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            new_bank_record.Load(string.Format("01/05/2017^12.34^^POS^{0}^^^^^", description_containing_comma));
            csv_file.Records.Add(new_bank_record);

            // Act
            csv_file.Write_to_csv_file("testing");

            // Assert
            var file_io_test_file = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only-testing");
            var new_csv_file = new CSVFile<BankRecord>(file_io_test_file);
            new_csv_file.Load(
                true,
                ',');

            Assert.AreEqual("01/05/2017,£12.34,,POS,\"something, something, something else\",,,,,", new_csv_file.Records[5].OutputSourceLine);
        }

        [Test]
        public void Can_write_amounts_containing_commas()
        {
            // Arrange
            var file_io = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only");
            var csv_file = new CSVFile<BankRecord>(file_io);
            csv_file.Load();
            var new_bank_record = new BankRecord();
            var amount_containing_comma = "£1,234.55";
            new_bank_record.Load(string.Format("01/05/2017^{0}^^POS^Purchase^^^^^", amount_containing_comma));
            csv_file.Records.Add(new_bank_record);

            // Act
            csv_file.Write_to_csv_file("testing");

            // Assert
            var file_io_test_file = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csv_file_path, "BankIn-formatted-date-only-testing");
            var new_csv_file = new CSVFile<BankRecord>(file_io_test_file);
            new_csv_file.Load(
                true,
                ',');

            Assert.AreEqual(1234.55, new_csv_file.Records[5].Unreconciled_amount);
            Assert.AreEqual("Purchase", new_csv_file.Records[5].Description);
        }

        [Test]
        public void M_WillCopyRecordsToAnotherFile()
        {
            // Arrange
            var mock_source_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var source_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = "First record"},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mock_source_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(source_records);
            var source_file = new CSVFile<ExpectedIncomeRecord>(mock_source_file_io.Object);
            source_file.Load();
            var mock_target_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            mock_target_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ExpectedIncomeRecord>());
            var target_file = new CSVFile<ExpectedIncomeRecord>(mock_target_file_io.Object);
            target_file.Load();
            Assert.AreEqual(0, target_file.Records.Count);

            // Act
            source_file.Copy_records_to_csv_file(target_file);

            // Assert
            Assert.AreEqual(source_file.Records.Count, target_file.Records.Count);
            Assert.AreEqual(source_file.Records[0].Description, target_file.Records[0].Description);
            Assert.AreEqual(source_file.Records[1].Description, target_file.Records[1].Description);
        }

        [Test]
        public void M_WhenRemovingRecordPermanentlyItDoesNotComeBackAfterRefreshingFileContents()
        {
            // Arrange
            var mock_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var new_description = "Third record";
            var source_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = "First record"},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mock_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(source_records);
            var file = new CSVFile<ExpectedIncomeRecord>(mock_file_io.Object);
            file.Load();
            Assert.AreEqual(2, file.Records.Count);

            // Act
            file.Add_record_permanently(new ExpectedIncomeRecord {Description = new_description});
            file.Populate_records_from_original_file_load();

            // Assert
            Assert.AreEqual(3, file.Records.Count);
            Assert.AreEqual(new_description, file.Records[2].Description);
        }

        [Test]
        public void M_WhenAddingRecordPermanentlyItIsStillThereAfterRefreshingFileContents()
        {
            // Arrange
            var mock_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var lost_description = "First record";
            var source_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = lost_description},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mock_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(source_records);
            var file = new CSVFile<ExpectedIncomeRecord>(mock_file_io.Object);
            file.Load();
            Assert.AreEqual(2, file.Records.Count);

            // Act
            file.Remove_record_permanently(source_records[0]);
            file.Populate_records_from_original_file_load();

            // Assert
            Assert.AreEqual(1, file.Records.Count);
            Assert.IsFalse(file.Records.Any(x => x.Description == lost_description));
        }
    }
}
