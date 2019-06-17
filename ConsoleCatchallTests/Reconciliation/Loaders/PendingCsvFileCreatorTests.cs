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
        public void OneTimeSetUp()
        {
            var current_path = TestContext.CurrentContext.TestDirectory;
            var csv_file_path = TestHelper.FullyQualifiedCSVFilePath(current_path);
            _pendingCsvFileCreator = new PendingCsvFileCreator(csv_file_path);
            _newBankOutPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.BankOutPendingFileName + ".csv";
            _newBankInPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.BankInPendingFileName + ".csv";
            _newCredCard2InOutPendingFilePath = csv_file_path + "/" + PendingCsvFileCreator.CredCard2InOutPendingFileName + ".csv";
            _testPendingSourceFilePath = csv_file_path + "/" + PendingCsvFileCreator.PendingSourceFileName;
        }

        [TearDown]
        public void TearDown()
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
                new InputOutput().OutputLine(e.Message);
            }
        }

        private IEnumerable<string> ReadLines(string filePath)
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
        public void BankOutPendingCsvIsCreatedByCreateAllCsvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newBankOutPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[] { });

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            Assert.IsTrue(File.Exists(_newBankOutPendingFilePath));
        }

        [Test]
        public void BankInPendingCsvIsCreatedByCreateAllCsvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newBankInPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[] { });

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            Assert.IsTrue(File.Exists(_newBankInPendingFilePath));
        }

        [Test]
        public void CredCard2InOutPendingCsvIsCreatedByCreateAllCsvs()
        {
            // Arrange
            Assert.IsFalse(File.Exists(_newCredCard2InOutPendingFilePath));
            File.WriteAllLines(_testPendingSourceFilePath, new string[]{});

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            Assert.IsTrue(File.Exists(_newCredCard2InOutPendingFilePath));
        }

        [Test]
        public void BankOutPendingCsvContainsDataAfterBankOutHeader()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your greene"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(pending_lines[2], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void BankInPendingCsvContainsDataAfterBankInHeader()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,test1",
                "",
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your pintipoplication",
                ""
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankInPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[4], new_lines[0]);
            Assert.AreEqual(pending_lines[5], new_lines[1]);
            Assert.AreEqual(pending_lines[6], new_lines[2]);
            Assert.AreEqual(3, new_lines.Count);
        }

        [Test]
        public void CredCard2InOutPendingCsvContainsDataAfterCredCard2Header()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,test1",
                "",
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test2",
                "",
                ReconConsts.CredCard2Header,
                "30/9/18,99.96,POS,,Divide your kangaroo",
                "30/9/18,99.96,POS,,Divide your pintipoplication",
                ""
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newCredCard2InOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[7], new_lines[0]);
            Assert.AreEqual(pending_lines[8], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void PendingCsvDataStopsAfterBlankLine()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,travel",
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }

        [Test]
        public void PendingCsvDataCanBeReadWhenThereIsUnrelatedPrecedingData()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your pickle pock",
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[6], new_lines[0]);
            Assert.AreEqual(pending_lines[7], new_lines[1]);
            Assert.AreEqual(2, new_lines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNoDataAfterHeader()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.BankOutHeader,
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNothingAfterAllAfterHeader()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.BankOutHeader
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNoHeader()
        {
            // Arrange
            string[] pending_lines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, new_lines.Count);
        }

        [Test]
        public void ShouldThrowExceptionIfThereIsNoSourcePendingFile()
        {
            // Arrange
            bool exception_thrown = false;

            // Act
            try
            {
                _pendingCsvFileCreator.CreateAndPopulateAllCsvs();
            }
            catch (Exception)
            {
                exception_thrown = true;
            }

            // Assert
            Assert.IsTrue(exception_thrown);
        }

        [Test]
        public void WillStillLoadDataEvenIfHeaderHasInconsistentCapitalisation()
        {
            // Arrange
            string[] pending_lines =
            {
                "BAnK ouT",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }

        [Test]
        public void WillStillLoadDataEvenIfHeaderIsFollowedByOtherCharacters()
        {
            // Arrange
            string[] pending_lines =
            {
                "Bank Out: ",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pending_lines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> new_lines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pending_lines[1], new_lines[0]);
            Assert.AreEqual(1, new_lines.Count);
        }
    }
}