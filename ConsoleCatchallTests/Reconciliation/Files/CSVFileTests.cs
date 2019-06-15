using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class CSVFileTests
    {
        private string _csvFilePath;

        public CSVFileTests()
        {
            PopulateCsvFilePath();
            TestHelper.SetCorrectDateFormatting();
        }

        private void PopulateCsvFilePath()
        {
            string currentPath = TestContext.CurrentContext.TestDirectory;
            _csvFilePath = TestHelper.FullyQualifiedCSVFilePath(currentPath);
        }

        [Test]
        public void M_CanCreateEmptyCSVFile()
        {
            // Act
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory());
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(false);

            // Assert
            Assert.AreEqual(0, csvFile.Records.Count);
        }

        [Test]
        public void CanLoadCSVFile()
        {
            // Act & Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankOut-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();

            // Assert
            Assert.AreEqual("01/03/2017^£5.00^^TILL^DescriptionBank013^^^^^", csvFile.FileContents[0]);
        }

        [Test]
        public void WillOrderRecordsByDateOnLoadingByDefault()
        {
            // Act & Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();

            // Assert
            Assert.AreEqual("01/02/2017", csvFile.Records[0].Date.ToShortDateString());
            Assert.AreEqual("01/03/2017", csvFile.Records[1].Date.ToShortDateString());
            Assert.AreEqual("24/03/2017", csvFile.Records[2].Date.ToShortDateString());
            Assert.AreEqual("01/04/2017", csvFile.Records[3].Date.ToShortDateString());
            Assert.AreEqual("03/04/2017", csvFile.Records[4].Date.ToShortDateString());
        }

        [Test]
        public void WillNotOrderRecordsByDateOnLoading_WhenSpecified()
        {
            // Arrange
            bool orderByDate = false;

            // Act
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                orderByDate);

            // Assert
            Assert.AreEqual("24/03/2017", csvFile.Records[0].Date.ToShortDateString());
            Assert.AreEqual("03/04/2017", csvFile.Records[1].Date.ToShortDateString());
            Assert.AreEqual("01/04/2017", csvFile.Records[2].Date.ToShortDateString());
            Assert.AreEqual("01/03/2017", csvFile.Records[3].Date.ToShortDateString());
            Assert.AreEqual("01/02/2017", csvFile.Records[4].Date.ToShortDateString());
        }

        [Test]
        public void CanLoadBankOutRecords()
        {
            // Act & Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankOut-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();

            // Assert
            Assert.AreEqual("TILL", csvFile.Records[0].Type);
            Assert.AreEqual(47, csvFile.Records.Count);
            Assert.AreEqual("ZZZEsterene plinkle (Inestimable plarts)", csvFile.Records[46].Description);
        }

        [Test]
        public void CanLoadBankInRecords()
        {
            // Act & Arrange
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);

            // Assert
            Assert.AreEqual("PCL", csvFile.Records[0].Type);
            Assert.AreEqual(5, csvFile.Records.Count);
            Assert.AreEqual("ZZZThing3", csvFile.Records[4].Description);
        }

        [Test]
        public void CanLoadCredCard1Records()
        {
            // Act & Arrange
            var fileIO = new FileIO<CredCard1Record>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "CredCard1-Statement");
            var csvFile = new CSVFile<CredCard1Record>(fileIO);
            csvFile.Load();

            // Assert
            Assert.AreEqual("22223333", csvFile.Records[0].Reference);
            Assert.AreEqual(50, csvFile.Records.Count);
            Assert.AreEqual("Description-CredCard1-001", csvFile.Records[49].Description);
        }

        [Test]
        public void CanLoadCredCard1InOutRecords()
        {
            // Act & Arrange
            var fileIO = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "CredCard1InOut-formatted-date-only");
            var csvFile = new CSVFile<CredCard1InOutRecord>(fileIO);
            csvFile.Load();

            // Assert
            Assert.AreEqual("ZZZSpecialDescription017", csvFile.Records[0].Description);
            Assert.AreEqual(32, csvFile.Records.Count);
            Assert.AreEqual("Description-CredCard1-form-007", csvFile.Records[31].Description);
        }

        [Test]
        public void CanLoadActualBankRecords()
        {
            // Act & Arrange
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "ActualBank-sample");
            var csvFile = new CSVFile<ActualBankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);

            // Assert
            Assert.AreEqual("'ZZZSpecialDescription001", csvFile.Records[0].Description);
            Assert.AreEqual(67, csvFile.Records.Count);
            Assert.AreEqual(4554.18, csvFile.Records[66].Balance);
        }

        [Test]
        public void CanFilterForPositiveRecordsOnly()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "ActualBank-sample");
            var csvFile = new CSVFile<ActualBankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);

            // Act
            csvFile.FilterForPositiveRecordsOnly();

            // Assert
            Assert.AreEqual("'ZZZSpecialDescription001", csvFile.Records[0].Description);
            Assert.AreEqual(16, csvFile.Records.Count);
            Assert.AreEqual(4669.48, csvFile.Records[15].Balance);
        }

        [Test]
        public void CanFilterForNegativeRecordsOnly()
        {
            // Arrange
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "ActualBank-sample");
            var csvFile = new CSVFile<ActualBankRecord>(fileIO);
            csvFile.Load();

            // Act
            csvFile.FilterForNegativeRecordsOnly();

            // Assert
            Assert.AreEqual("'DIVIDE YOUR GREEN", csvFile.Records[0].Description);
            Assert.AreEqual(51, csvFile.Records.Count);
            Assert.AreEqual(4554.18, csvFile.Records[50].Balance);
        }

        [Test]
        public void M_CanRemoveRecordsUsingSpecifiedFilter()
        {
            // Arrange
            var mockFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(
                new List<ActualBankRecord>
                {
                    new ActualBankRecord {Matched = true, Description = "Matched"},
                    new ActualBankRecord {Matched = false, Description = "Not Matched"},
                    new ActualBankRecord {Matched = true, Description = "Matched"},
                    new ActualBankRecord {Matched = false, Description = "Not Matched"}
                }
            );
            var csvFile = new CSVFile<ActualBankRecord>(mockFileIO.Object);
            csvFile.Load();

            // Act
            csvFile.RemoveRecords(x => x.Matched);

            // Assert
            Assert.AreEqual(2, csvFile.Records.Count);
            Assert.IsFalse(csvFile.Records.Any(x => x.Matched), "None with matched flag");
            Assert.IsTrue(csvFile.Records.All(x => !x.Matched), "All with matched flag = false");
            Assert.IsFalse(csvFile.Records.Any(x => x.Description == "Matched"), "None with matched description");
            Assert.IsTrue(csvFile.Records.All(x => x.Description == "Not Matched"), "All with 'Not Matched' description");
        }

        [Test]
        public void WhenFilteringForNegativeRecordsAllAreConvertedToPositive()
        {
            // Arrange
            var fileIO = new FileIO<ActualBankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "ActualBank-sample");
            var csvFile = new CSVFile<ActualBankRecord>(fileIO);
            csvFile.Load();

            // Act
            csvFile.FilterForNegativeRecordsOnly();

            // Assert
            Assert.AreEqual(115.30, csvFile.Records[50].Amount);
        }

        [Test]
        public void CanOutputUnmatchedRecordsAsCsvInDateOrder()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            csvFile.ResetAllMatches();

            // Act
            List<string> csvLines = csvFile.UnmatchedRecordsAsCsv();

            // Assert
            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csvLines[0]);
            Assert.AreEqual("01/03/2017,£350.00,,ABC,\"ZZZThing2\",,,,,", csvLines[1]);
            Assert.AreEqual("24/03/2017,£200.12,,PCL,\"ZZZSpecialDescription001\",,,,,", csvLines[2]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csvLines[3]);
            Assert.AreEqual("03/04/2017,£350.00,,ABC,\"ZZZThing1\",,,,,", csvLines[4]);
        }

        [Test]
        public void CanOutputMatchedRecordsAsCsvInDateOrder()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            foreach (var record in csvFile.Records)
            {
                record.Matched = true;
            }

            // Act
            List<string> csvLines = csvFile.MatchedRecordsAsCsv();

            // Assert
            Assert.AreEqual("01/02/2017,£350.00,x,ABC,\"ZZZThing3\",,,,,", csvLines[0]);
            Assert.AreEqual("01/03/2017,£350.00,x,ABC,\"ZZZThing2\",,,,,", csvLines[1]);
            Assert.AreEqual("24/03/2017,£200.12,x,PCL,\"ZZZSpecialDescription001\",,,,,", csvLines[2]);
            Assert.AreEqual("01/04/2017,£261.40,x,PCL,\"ZZZSpecialDescription005\",,,,,", csvLines[3]);
            Assert.AreEqual("03/04/2017,£350.00,x,ABC,\"ZZZThing1\",,,,,", csvLines[4]);
        }

        [Test]
        public void CanOutputAllRecordsAsCsvOrderedByMatchedAndThenByDate()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);
            csvFile.Records[0].Matched = true;
            csvFile.Records[1].Matched = true;
            csvFile.Records[3].Matched = true;

            // Act
            List<string> csvLines = csvFile.AllRecordsAsCsv();

            // Assert
            Assert.AreEqual("01/03/2017,£350.00,x,ABC,\"ZZZThing2\",,,,,", csvLines[0]);
            Assert.AreEqual("24/03/2017,£200.12,x,PCL,\"ZZZSpecialDescription001\",,,,,", csvLines[1]);
            Assert.AreEqual("03/04/2017,£350.00,x,ABC,\"ZZZThing1\",,,,,", csvLines[2]);
            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csvLines[3]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csvLines[4]);
        }

        [Test]
        public void WillOrderRecordsForSpreadsheetByMatchedAndThenByDateWithDividerBetween()
        {
            // Arrange
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);
            csvFile.Records[0].Matched = true;
            csvFile.Records[1].Matched = true;
            csvFile.Records[3].Matched = true;

            // Act
            var records = csvFile.RecordsOrderedForSpreadsheet();

            // Assert
            Assert.AreEqual("ZZZThing2", records[0].Description);
            Assert.AreEqual("ZZZSpecialDescription001", records[1].Description);
            Assert.AreEqual("ZZZThing1", records[2].Description);
            Assert.AreEqual(true, records[3].Divider);
            Assert.AreEqual("ZZZThing3", records[4].Description);
            Assert.AreEqual("ZZZSpecialDescription005", records[5].Description);
        }

        [Test]
        public void WhenOrderingRecordsForSpreadsheetWillInsertDividerBetweenMatchedAndNonMatched()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            csvFile.Records[0].Matched = true;
            csvFile.Records[3].Matched = true;

            // Act
            var records = csvFile.RecordsOrderedForSpreadsheet();

            // Assert
            Assert.AreEqual(6, records.Count);
            Assert.AreEqual(true, records[1].Matched);
            Assert.AreEqual(false, records[1].Divider);

            Assert.AreEqual(true, records[2].Divider);

            Assert.AreEqual(false, records[3].Matched);
            Assert.AreEqual(false, records[3].Divider);
        }

        [Test]
        public void WhenOrderingRecordsForSpreadsheetIfNoMatchedThenNoDivider()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();

            // Act
            var records = csvFile.RecordsOrderedForSpreadsheet();

            // Assert
            Assert.AreEqual(5, records.Count);
            Assert.AreEqual(false, records[0].Divider);
            Assert.AreEqual(false, records[1].Divider);
            Assert.AreEqual(false, records[2].Divider);
            Assert.AreEqual(false, records[3].Divider);
            Assert.AreEqual(false, records[4].Divider);
        }

        [Test]
        public void WhenOrderingRecordsForSpreadsheetIfNoUnmatchedThenDividerIsAtEnd()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            csvFile.Records[0].Matched = true;
            csvFile.Records[1].Matched = true;
            csvFile.Records[2].Matched = true;
            csvFile.Records[3].Matched = true;
            csvFile.Records[4].Matched = true;

            // Act
            var records = csvFile.RecordsOrderedForSpreadsheet();

            // Assert
            Assert.AreEqual(6, records.Count);
            Assert.AreEqual(true, records[4].Matched);
            Assert.AreEqual(false, records[4].Divider);

            Assert.AreEqual(false, records[5].Matched);
            Assert.AreEqual(true, records[5].Divider);
        }

        [Test]
        public void CanOutputAllRecordsAsSourceLineOrderedByDate()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();

            // Act
            List<string> csvLines = csvFile.AllRecordsAsSourceLines();

            // Assert
            Assert.AreEqual("01/02/2017^£350.00^^ABC^ZZZThing3^^^^^", csvLines[0]);
            Assert.AreEqual("01/03/2017^£350.00^^ABC^ZZZThing2^^^^^", csvLines[1]);
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", csvLines[2]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", csvLines[3]);
            Assert.AreEqual("03/04/2017^£350.00^^ABC^ZZZThing1^^^^^", csvLines[4]);
        }

        [Test]
        public void CanConvertCommasToLittleHatsForBankIn()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-with-commas");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(true, ',');

            // Act
            csvFile.ConvertSourceLineSeparators(',', '^');

            // Assert
            List<string> csvLines = csvFile.AllRecordsAsSourceLines();
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", csvLines[0]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", csvLines[1]);
            Assert.AreEqual("03/10/2018^£350.00^^ABC^ZZZThing1^^^^^", csvLines[2]);
        }

        [Test]
        public void CanConvertCommasToLittleHatsForCredCard2InOut()
        {
            // Arrange
            var fileIO = new FileIO<CredCard2InOutRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "CredCard2InOut-with-commas");
            var csvFile = new CSVFile<CredCard2InOutRecord>(fileIO);
            csvFile.Load(true, ',');

            // Act
            csvFile.ConvertSourceLineSeparators(',', '^');

            // Assert
            List<string> csvLines = csvFile.AllRecordsAsSourceLines();
            Assert.AreEqual("09/04/2017^£8.33^^ZZZSpecialDescription021^", csvLines[0]);
            Assert.AreEqual("01/05/2017^£3.16^^ZZZSpecialDescription022^", csvLines[1]);
            Assert.AreEqual("06/05/2017^£11.94^^ZZZSpecialDescription023^", csvLines[2]);
            Assert.AreEqual("20/05/2017^£158.32^^ZZZSpecialDescription024^", csvLines[3]);
            Assert.AreEqual("21/10/2018^£16.05^^ZZZSpecialDescription025^", csvLines[4]);
        }

        [Test]
        public void CanConvertCommasToLittleHatsForCredCard1InOut()
        {
            // Arrange
            var fileIO = new FileIO<CredCard1InOutRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "CredCard1InOut-with-commas");
            var csvFile = new CSVFile<CredCard1InOutRecord>(fileIO);
            csvFile.Load(true, ',');

            // Act
            csvFile.ConvertSourceLineSeparators(',', '^');

            // Assert
            List<string> csvLines = csvFile.AllRecordsAsSourceLines();
            Assert.AreEqual("19/12/2016^£7.99^^ZZZSpecialDescription017^", csvLines[0]);
            Assert.AreEqual("02/01/2017^£6.29^^ZZZSpecialDescription018^", csvLines[1]);
            Assert.AreEqual("15/02/2017^£1.99^^ZZZSpecialDescription019^", csvLines[2]);
            Assert.AreEqual("17/10/2018^£1.94^^ZZZSpecialDescription020^", csvLines[3]);
        }

        [Test]
        public void CanWriteNewContentsToCsv()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            var newBankRecord = new BankRecord();
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            var todaysDate = DateTime.Today.ToString("dd/MM/yyyy");
            newBankRecord.Load(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todaysDate));
            csvFile.Records.Add(newBankRecord);

            // Act
            csvFile.WriteToCsvFile("testing");

            // Assert
            var fileIOTestFile = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only-testing");
            var newCsvFile = new CSVFile<BankRecord>(fileIOTestFile);
            newCsvFile.Load(
                true,
                ',');
            List<string> csvLines = newCsvFile.AllRecordsAsCsv();

            Assert.AreEqual("01/02/2017,£350.00,,ABC,\"ZZZThing3\",,,,,", csvLines[0]);
            Assert.AreEqual("01/03/2017,£350.00,,ABC,\"ZZZThing2\",,,,,", csvLines[1]);
            Assert.AreEqual("24/03/2017,£200.12,,PCL,\"ZZZSpecialDescription001\",,,,,", csvLines[2]);
            Assert.AreEqual("01/04/2017,£261.40,,PCL,\"ZZZSpecialDescription005\",,,,,", csvLines[3]);
            Assert.AreEqual("03/04/2017,£350.00,,ABC,\"ZZZThing1\",,,,,", csvLines[4]);
            Assert.AreEqual(String.Format("{0},£12.34,,POS,\"Purchase\",,,,,",todaysDate), csvLines[5]);
        }

        [Test]
        public void CanWriteNewContentsAsSourceLines()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            var newBankRecord = new BankRecord();
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            var todaysDate = DateTime.Today.ToString("dd/MM/yyyy");
            newBankRecord.Load(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todaysDate));
            csvFile.Records.Add(newBankRecord);

            // Act
            csvFile.WriteToFileAsSourceLines("BankIn-formatted-date-only-testing");

            // Assert
            List<string> newFileLines = new List<string>();
            using (var fileStream = File.OpenRead(_csvFilePath + "/" + "BankIn-formatted-date-only-testing" + ".csv"))
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    var newLine = reader.ReadLine();
                    newFileLines.Add(newLine);
                }
            }

            Assert.AreEqual("01/02/2017^£350.00^^ABC^ZZZThing3^^^^^", newFileLines[0]);
            Assert.AreEqual("01/03/2017^£350.00^^ABC^ZZZThing2^^^^^", newFileLines[1]);
            Assert.AreEqual("24/03/2017^£200.12^^PCL^ZZZSpecialDescription001^^^^^", newFileLines[2]);
            Assert.AreEqual("01/04/2017^£261.40^^PCL^ZZZSpecialDescription005^^^^^", newFileLines[3]);
            Assert.AreEqual("03/04/2017^£350.00^^ABC^ZZZThing1^^^^^", newFileLines[4]);
            Assert.AreEqual(String.Format("{0}^£12.34^^POS^Purchase^^^^^", todaysDate), newFileLines[5]);
        }

        // Note that this only applies to CredCard1InOutRecord, BankRecord, CredCard2InOutRecord - because commas are stripped from input in third party records.
        // But not from owned records, because we use ^ as a separator instead of comma.
        [Test]
        public void CanWriteDescriptionsContainingCommas()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            var newBankRecord = new BankRecord();
            var descriptionContainingComma = "something, something, something else";
            // Date, unreconciled amount, type and description are mandatory.
            // string csvLine = String.Format("DATE^UNRECONCILED_AMT^^TYPE^DESCRIPTION^^^^^");
            newBankRecord.Load(string.Format("01/05/2017^12.34^^POS^{0}^^^^^", descriptionContainingComma));
            csvFile.Records.Add(newBankRecord);

            // Act
            csvFile.WriteToCsvFile("testing");

            // Assert
            var fileIOTestFile = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only-testing");
            var newCsvFile = new CSVFile<BankRecord>(fileIOTestFile);
            newCsvFile.Load(
                true,
                ',');

            Assert.AreEqual("01/05/2017,£12.34,,POS,\"something, something, something else\",,,,,", newCsvFile.Records[5].SourceLine);
        }

        [Test]
        public void CanWriteAmountsContainingCommas()
        {
            // Arrange
            var fileIO = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            var newBankRecord = new BankRecord();
            var amountContainingComma = "£1,234.55";
            newBankRecord.Load(string.Format("01/05/2017^{0}^^POS^Purchase^^^^^", amountContainingComma));
            csvFile.Records.Add(newBankRecord);

            // Act
            csvFile.WriteToCsvFile("testing");

            // Assert
            var fileIOTestFile = new FileIO<BankRecord>(new FakeSpreadsheetRepoFactory(), _csvFilePath, "BankIn-formatted-date-only-testing");
            var newCsvFile = new CSVFile<BankRecord>(fileIOTestFile);
            newCsvFile.Load(
                true,
                ',');

            Assert.AreEqual(1234.55, newCsvFile.Records[5].UnreconciledAmount);
            Assert.AreEqual("\"Purchase\"", newCsvFile.Records[5].Description);
        }

        [Test]
        public void M_WillCopyRecordsToAnotherFile()
        {
            // Arrange
            var mockSourceFileIO = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var sourceRecords = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = "First record"},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mockSourceFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(sourceRecords);
            var sourceFile = new CSVFile<ExpectedIncomeRecord>(mockSourceFileIO.Object);
            sourceFile.Load();
            var mockTargetFileIO = new Mock<IFileIO<ExpectedIncomeRecord>>();
            mockTargetFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ExpectedIncomeRecord>());
            var targetFile = new CSVFile<ExpectedIncomeRecord>(mockTargetFileIO.Object);
            targetFile.Load();
            Assert.AreEqual(0, targetFile.Records.Count);

            // Act
            sourceFile.CopyRecordsToCsvFile(targetFile);

            // Assert
            Assert.AreEqual(sourceFile.Records.Count, targetFile.Records.Count);
            Assert.AreEqual(sourceFile.Records[0].Description, targetFile.Records[0].Description);
            Assert.AreEqual(sourceFile.Records[1].Description, targetFile.Records[1].Description);
        }

        [Test]
        public void M_WhenRemovingRecordPermanentlyItDoesNotComeBackAfterRefreshingFileContents()
        {
            // Arrange
            var mockFileIO = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var newDescription = "Third record";
            var sourceRecords = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = "First record"},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mockFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(sourceRecords);
            var file = new CSVFile<ExpectedIncomeRecord>(mockFileIO.Object);
            file.Load();
            Assert.AreEqual(2, file.Records.Count);

            // Act
            file.AddRecordPermanently(new ExpectedIncomeRecord {Description = newDescription});
            file.PopulateRecordsFromOriginalFileLoad();

            // Assert
            Assert.AreEqual(3, file.Records.Count);
            Assert.AreEqual(newDescription, file.Records[2].Description);
        }

        [Test]
        public void M_WhenAddingRecordPermanentlyItIsStillThereAfterRefreshingFileContents()
        {
            // Arrange
            var mockFileIO = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var lostDescription = "First record";
            var sourceRecords = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord {Description = lostDescription},
                new ExpectedIncomeRecord {Description = "Second record"}
            };
            mockFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(sourceRecords);
            var file = new CSVFile<ExpectedIncomeRecord>(mockFileIO.Object);
            file.Load();
            Assert.AreEqual(2, file.Records.Count);

            // Act
            file.RemoveRecordPermanently(sourceRecords[0]);
            file.PopulateRecordsFromOriginalFileLoad();

            // Assert
            Assert.AreEqual(1, file.Records.Count);
            Assert.IsFalse(file.Records.Any(x => x.Description == lostDescription));
        }
    }
}
