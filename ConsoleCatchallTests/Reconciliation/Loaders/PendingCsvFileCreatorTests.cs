using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleCatchall.Console;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces.Constants;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class PendingCsvFileCreatorTests
    {
        private static PendingCsvFileCreator _pendingCsvFileCreator;
        private static string _newBankOutPendingFilePath;
        private static string _newBankInPendingFilePath;
        private static string _newCredCard2InOutPendingFilePath;
        private static string _testPendingSourceFilePath;

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            var current_path = TestContext.CurrentContext.TestDirectory;
            var csv_file_path = TestHelper.Fully_qualified_csv_file_path(current_path);
            _pendingCsvFileCreator = new PendingCsvFileCreator(csv_file_path);
            _newBankOutPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.BankOutPendingFileName + ".csv";
            _newBankInPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.BankInPendingFileName + ".csv";
            _newCredCard2InOutPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.CredCard2InOutPendingFileName + ".csv";
            _testPendingSourceFilePath = csv_file_path + "/" + PendingCsvFileCreator.PendingSourceFileName;
        }

        [TearDown]
        public void Tear_down()
        {
            try
            {
                File.Delete(_newBankOutPendingFilePath);
                File.Delete(_newBankInPendingFilePath);
                File.Delete(_newCredCard2InOutPendingFilePath);
                File.Delete(_testPendingSourceFilePath);
            }
            catch (Exception e)
            {
                // not all files are created for all tests. Just log the error and carry on.
                new InputOutput().Output_line(e.Message);
            }
        }

        private IEnumerable<string> Read_lines(string filePath)
        {
            var lines = new List<string>();
            using (var file_stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(file_stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return lines;
        }

        [Test]
        public void Bank_out_pending_csv_is_created_by_create_all_csvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newBankOutPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[] { });

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            Assert.IsTrue(File.Exists(_newBankOutPendingFilePath));
        }

        [Test]
        public void Bank_in_pending_csv_is_created_by_create_all_csvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newBankInPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[] { });

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            Assert.IsTrue(File.Exists(_newBankInPendingFilePath));
        }

        [Test]
        public void Cred_card2_in_out_pending_csv_is_created_by_create_all_csvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newCredCard2InOutPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[]{});

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            Assert.IsTrue(File.Exists(_newCredCard2InOutPendingFilePath));
        }

        [Test]
        public void Bank_out_pending_csv_contains_data_after_bank_out_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_out_header,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your greene"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(pending_lines[2], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void Bank_in_pending_csv_contains_data_after_bank_in_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_out_header,
                "30/9/18,77.10,till,,test1",
                "",
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your pintipoplication",
                ""
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankInPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[4], new_lines[0]);
            Assert.AreEqual(pending_lines[5], new_lines[1]);
            Assert.AreEqual(pending_lines[6], new_lines[2]);
            Assert.AreEqual(3, new_lines.Count);
        }

        [Test]
        public void Cred_card2_in_out_pending_csv_contains_data_after_cred_card2_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_out_header,
                "30/9/18,77.10,till,,test1",
                "",
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,test2",
                "",
                ReconConsts.Cred_card2_header,
                "30/9/18,99.96,POS,,Divide your kangaroo",
                "30/9/18,99.96,POS,,Divide your pintipoplication",
                ""
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newCredCard2InOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[7], new_lines[0]);
            Assert.AreEqual(pending_lines[8], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void Pending_csv_data_stops_after_blank_line()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_out_header,
                "30/9/18,77.10,till,,travel",
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }

        [Test]
        public void Pending_csv_data_can_be_read_when_there_is_unrelated_preceding_data()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.Bank_out_header,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your pickle pock",
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[6], new_lines[0]);
            Assert.AreEqual(pending_lines[7], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void Pending_csv_file_will_be_empty_if_there_is_no_data_after_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.Bank_out_header,
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void Pending_csv_file_will_be_empty_if_there_is_nothing_after_all_after_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.Bank_out_header
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void Pending_csv_file_will_be_empty_if_there_is_no_header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.Bank_in_header,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void Should_throw_exception_if_there_is_no_source_pending_file()
        {
            // Arrange
            bool exception_thrown = false;

            // Act
            try
            {
                _pendingCsvFileCreator.Create_and_populate_all_csvs();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
        }

        [Test]
        public void Will_still_load_data_even_if_header_has_inconsistent_capitalisation()
        {
            // Arrange
            string[] pending_lines =
            {
                "BAnK ouT",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }

        [Test]
        public void Will_still_load_data_even_if_header_is_followed_by_other_characters()
        {
            // Arrange
            string[] pending_lines =
            {
                "Bank Out: ",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.Create_and_populate_all_csvs();

            // Assert
            List<string> new_lines = Read_lines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }
    }
}