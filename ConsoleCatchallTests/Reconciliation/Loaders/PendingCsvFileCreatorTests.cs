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
            var currentPath = TestContext.CurrentContext.TestDirectory;
            var csvFilePath = TestHelper.FullyQualifiedCSVFilePath(currentPath);
            _pendingCsvFileCreator = new PendingCsvFileCreator(csvFilePath);
            _newBankOutPendingFilePath = csvFilePath + "/" + PendingCsvFileCreator.BankOutPendingFileName + ".csv";
            _newBankInPendingFilePath = csvFilePath + "/" + PendingCsvFileCreator.BankInPendingFileName + ".csv";
            _newCredCard2InOutPendingFilePath = csvFilePath + "/" + PendingCsvFileCreator.CredCard2InOutPendingFileName + ".csv";
            _testPendingSourceFilePath = csvFilePath + "/" + PendingCsvFileCreator.PendingSourceFileName;
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
            using (var fileStream = File.OpenRead(filePath))
            using (var reader = new StreamReader(fileStream))
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
            string[] pendingLines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,travel",
                "30/9/18,99.96,POS,,Divide your greene"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[1], newLines[0]);
            Assert.AreEqual(pendingLines[2], newLines[1]);
            Assert.AreEqual(2, newLines.Count);
        }

        [Test]
        public void BankInPendingCsvContainsDataAfterBankInHeader()
        {
            // Arrange
            string[] pendingLines =
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
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankInPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[4], newLines[0]);
            Assert.AreEqual(pendingLines[5], newLines[1]);
            Assert.AreEqual(pendingLines[6], newLines[2]);
            Assert.AreEqual(3, newLines.Count);
        }

        [Test]
        public void CredCard2InOutPendingCsvContainsDataAfterCredCard2Header()
        {
            // Arrange
            string[] pendingLines =
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
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newCredCard2InOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[7], newLines[0]);
            Assert.AreEqual(pendingLines[8], newLines[1]);
            Assert.AreEqual(2, newLines.Count);
        }

        [Test]
        public void PendingCsvDataStopsAfterBlankLine()
        {
            // Arrange
            string[] pendingLines =
            {
                ReconConsts.BankOutHeader,
                "30/9/18,77.10,till,,travel",
                "",
                "30/9/18,99.96,POS,,Divide your greene",
                "30/9/18,99.96,POS,,Divide your kangaroo-boo"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[1], newLines[0]);
            Assert.AreEqual(1, newLines.Count);
        }

        [Test]
        public void PendingCsvDataCanBeReadWhenThereIsUnrelatedPrecedingData()
        {
            // Arrange
            string[] pendingLines =
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
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[6], newLines[0]);
            Assert.AreEqual(pendingLines[7], newLines[1]);
            Assert.AreEqual(2, newLines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNoDataAfterHeader()
        {
            // Arrange
            string[] pendingLines =
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
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, newLines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNothingAfterAllAfterHeader()
        {
            // Arrange
            string[] pendingLines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3",
                ReconConsts.BankOutHeader
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, newLines.Count);
        }

        [Test]
        public void PendingCsvFileWillBeEmptyIfThereIsNoHeader()
        {
            // Arrange
            string[] pendingLines =
            {
                ReconConsts.BankInHeader,
                "30/9/18,77.10,till,,test1",
                "",
                "30/9/18,99.96,POS,,test2",
                "30/9/18,99.96,POS,,test3"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(0, newLines.Count);
        }

        [Test]
        public void ShouldThrowExceptionIfThereIsNoSourcePendingFile()
        {
            // Arrange
            bool exceptionThrown = false;

            // Act
            try
            {
                _pendingCsvFileCreator.CreateAndPopulateAllCsvs();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
        }

        [Test]
        public void WillStillLoadDataEvenIfHeaderHasInconsistentCapitalisation()
        {
            // Arrange
            string[] pendingLines =
            {
                "BAnK ouT",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[1], newLines[0]);
            Assert.AreEqual(1, newLines.Count);
        }

        [Test]
        public void WillStillLoadDataEvenIfHeaderIsFollowedByOtherCharacters()
        {
            // Arrange
            string[] pendingLines =
            {
                "Bank Out: ",
                "30/9/18,77.10,till,,travel"
            };
            File.WriteAllLines(_testPendingSourceFilePath, pendingLines);

            // Act
            _pendingCsvFileCreator.CreateAndPopulateAllCsvs();

            // Assert
            List<string> newLines = ReadLines(_newBankOutPendingFilePath).ToList();
            Assert.AreEqual(pendingLines[1], newLines[0]);
            Assert.AreEqual(1, newLines.Count);
        }
    }
}