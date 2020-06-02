using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Files
{
    [TestFixture]
    public class ExpectedIncomeFileTests
    {
        [Test]
        public void Will_filter_for_employer_expense_records_when_loading()
        {
            // Arrange
            var mock_expected_income_file = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expected_income_file = new ExpectedIncomeFile(mock_expected_income_file.Object);

            // Act
            expected_income_file.Load();

            // Assert
            mock_expected_income_file.Verify(x => x.Remove_records(It.IsAny<System.Predicate<ExpectedIncomeRecord>>()));
        }

        [Test]
        public void Can_filter_for_employer_expense_records_only()
        {
            // Arrange
            var mock_expected_income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(mock_expected_income_file_io.Object);
            var expected_description = "description2";
            var expected_income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = expected_description,
                    Code = Codes.Expenses
                },
                new ExpectedIncomeRecord
                {
                    Description = "description1",
                    Code = "other"
                }
            };
            mock_expected_income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_income_records);
            expected_income_csv_file.Load();
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);

            // Act
            expected_income_file.Filter_for_employer_expenses_only();

            // Assert
            Assert.AreEqual(1, expected_income_csv_file.Records.Count);
            Assert.AreEqual(expected_description, expected_income_csv_file.Records[0].Description);
        }

        [Test]
        public void Will_copy_all_records_to_pending_file()
        {
            // Arrange
            var mock_expected_income_file = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            var expected_income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = "description1"
                },
                new ExpectedIncomeRecord
                {
                    Description = "description2"
                }
            };
            mock_expected_income_file.Setup(x => x.Records).Returns(expected_income_records);
            var expected_income_file = new ExpectedIncomeFile(mock_expected_income_file.Object);
            var mock_pending_file_io = new Mock<IFileIO<BankRecord>>();
            mock_pending_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var pending_file = new CSVFile<BankRecord>(mock_pending_file_io.Object);
            pending_file.Load();
            Assert.AreEqual(0, pending_file.Records.Count);

            // Act
            expected_income_file.Copy_to_pending_file(pending_file);

            // Assert
            Assert.AreEqual(2, pending_file.Records.Count);
            Assert.AreEqual(expected_income_file.File.Records[0].Description, pending_file.Records[0].Description);
            Assert.AreEqual(expected_income_file.File.Records[1].Description, pending_file.Records[1].Description);
        }

        [Test]
        public void Will_update_expected_income_record_when_matched()
        {
            // Arrange
            var match_desc = "matchDesc";
            var match_date = DateTime.Today.AddDays(2);
            var match_amount = 22.22;
            var source_date = DateTime.Today;
            var source_amount = 22.22;
            var income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = match_desc,
                    Date = match_date,
                    Unreconciled_amount = match_amount
                }
            };
            income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(income_records);
            var income_file = new CSVFile<ExpectedIncomeRecord>(income_file_io.Object);
            income_file.Load();
            var expected_income_file = new ExpectedIncomeFile(income_file);
            var actual_bank_record = new ActualBankRecord
            {
                Date = source_date,
                Amount = source_amount
            };
            var bank_record = new BankRecord
            {
                Description = match_desc,
                Date = match_date,
                Unreconciled_amount = match_amount
            };

            // Act
            expected_income_file.Update_expected_income_record_when_matched(actual_bank_record, bank_record);

            // Assert
            Assert.AreEqual(match_desc, income_records[0].Description);
            Assert.AreEqual(match_amount, income_records[0].Unreconciled_amount);
            Assert.AreEqual(actual_bank_record, income_records[0].Match);
            Assert.AreEqual(true, income_records[0].Matched);
            Assert.AreEqual(match_date, income_records[0].Date);
            Assert.AreEqual(source_date, income_records[0].Date_paid);
            Assert.AreEqual(source_amount, income_records[0].Total_paid);
        }

        [Test]
        public void Will_ignore_punctuation_when_updating_expected_income_record()
        {
            // Arrange
            var match_desc = "matchDesc";
            var match_date = DateTime.Today.AddDays(2);
            var match_amount = 22.22;
            var source_date = DateTime.Today;
            var source_amount = 22.22;
            var income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord
                {
                    Description = match_desc,
                    Date = match_date,
                    Unreconciled_amount = match_amount
                }
            };
            income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(income_records);
            var income_file = new CSVFile<ExpectedIncomeRecord>(income_file_io.Object);
            income_file.Load();
            var expected_income_file = new ExpectedIncomeFile(income_file);
            var actual_bank_record = new ActualBankRecord
            {
                Date = source_date,
                Amount = source_amount
            };
            var bank_record = new BankRecord
            {
                Description = $"\"{match_desc}\"",
                Date = match_date,
                Unreconciled_amount = match_amount
            };

            // Act
            expected_income_file.Update_expected_income_record_when_matched(actual_bank_record, bank_record);

            // Assert
            Assert.AreEqual(match_desc, income_records[0].Description);
            Assert.AreEqual(match_amount, income_records[0].Unreconciled_amount);
            Assert.AreEqual(actual_bank_record, income_records[0].Match);
            Assert.AreEqual(true, income_records[0].Matched);
            Assert.AreEqual(match_date, income_records[0].Date);
            Assert.AreEqual(source_date, income_records[0].Date_paid);
            Assert.AreEqual(source_amount, income_records[0].Total_paid);
        }

        [Test]
        public void WhenFinishing_WillWriteBackToMainSpreadsheet()
        {
            // Arrange
            var mock_expected_income_file = new Mock<ICSVFile<ExpectedIncomeRecord>>();
            mock_expected_income_file.Setup(x => x.Records).Returns(new List<ExpectedIncomeRecord>());
            var expected_income_file = new ExpectedIncomeFile(mock_expected_income_file.Object);

            // Act
            expected_income_file.Finish();

            // Assert
            mock_expected_income_file.Verify(x => x.Write_back_to_main_spreadsheet(MainSheetNames.Expected_in));
        }

        [Test]
        public void WhenFinishing_WillReconcileAllRecords()
        {
            // Arrange
            var mock_income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var desc1 = "desc1";
            var desc2 = "desc2";
            var amount = 10;
            var expected_income_records = new List<ExpectedIncomeRecord>
            {
                new ExpectedIncomeRecord { Matched = false, Unreconciled_amount = amount, Reconciled_amount = 0, Description = desc1 },
                new ExpectedIncomeRecord { Matched = true, Unreconciled_amount = amount, Reconciled_amount = 0, Description = desc2 }
            };
            mock_income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expected_income_records);
            var csv_file = new CSVFile<ExpectedIncomeRecord>(mock_income_file_io.Object);
            csv_file.Load();
            var expected_income_file = new ExpectedIncomeFile(csv_file);

            // Act
            expected_income_file.Finish();

            // Assert
            Assert.AreEqual(amount, csv_file.Records[0].Unreconciled_amount);
            Assert.AreEqual(0, csv_file.Records[0].Reconciled_amount);
            Assert.AreEqual(desc1, csv_file.Records[0].Description);
            Assert.AreEqual(0, csv_file.Records[1].Unreconciled_amount);
            Assert.AreEqual(amount, csv_file.Records[1].Reconciled_amount);
            Assert.AreEqual(desc2, csv_file.Records[1].Description);
        }

        [Test]
        public void Will_create_new_expenses_record_to_match_balance()
        {
            // Arrange
            var income_file_io = new Mock<IFileIO<ExpectedIncomeRecord>>();
            var income_records = new List<ExpectedIncomeRecord>();
            income_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(income_records);
            var income_file = new CSVFile<ExpectedIncomeRecord>(income_file_io.Object);
            income_file.Load();
            var expected_income_file = new ExpectedIncomeFile(income_file);
            var actual_bank_record = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 50
            };
            double balance = actual_bank_record.Amount - 10;

            // Act
            expected_income_file.Create_new_expenses_record_to_match_balance(actual_bank_record, balance);

            // Assert
            Assert.AreEqual(1, expected_income_file.File.Records.Count);
            Assert.AreEqual(ReconConsts.UnknownExpense, expected_income_file.File.Records[0].Description);
            Assert.AreEqual(balance, expected_income_file.File.Records[0].Unreconciled_amount);
            Assert.AreEqual(actual_bank_record, expected_income_file.File.Records[0].Match);
            Assert.AreEqual(true, expected_income_file.File.Records[0].Matched);
            Assert.AreEqual(actual_bank_record.Date, expected_income_file.File.Records[0].Date);
            Assert.AreEqual(actual_bank_record.Date, expected_income_file.File.Records[0].Date_paid);
            Assert.AreEqual(actual_bank_record.Amount, expected_income_file.File.Records[0].Total_paid);
            Assert.AreEqual(Codes.Expenses, expected_income_file.File.Records[0].Code);
        }
    }
}
