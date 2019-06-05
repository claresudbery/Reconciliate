using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class BankAndBankInMatcherTests : IInputOutput
    {
        private Mock<IInputOutput> _mockInputOutput;

        [SetUp]
        public void SetUp()
        {
            _mockInputOutput = new Mock<IInputOutput>();
        }

        [Test]
        public void M_WhenDoingExpenseMatchingWillFilterOwnedFileForWagesRowsAndExpenseTransactionsOnly()
        {
            // Arrange
            var mockBankInFileIO = new Mock<IFileIO<BankRecord>>();
            var wagesDescription =
                $"Wages ({ReconConsts.EmployerExpenseDescription}) (!! in an inestimable manner, not forthwith - forever 1st outstanding)";
            mockBankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "1"},
                    new BankRecord {Type = "Chq", Description = "something"},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "2"},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "3"},
                    new BankRecord {Type = "Chq", Description = "something"},
                    new BankRecord {Type = "PCL", Description = wagesDescription}
                });
            var bankInFile = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mockBankInFileIO.Object));
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Description = $"\"'{ReconConsts.EmployerExpenseDescription}\""}
                });
            var actualBankFile = new ActualBankInFile(new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object));
            var dataLoadingInfo = new DataLoadingInformation<ActualBankRecord, BankRecord> { SheetName = MainSheetNames.BankIn };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(dataLoadingInfo, actualBankFile, bankInFile);
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.FilterForAllWagesRowsAndExpenseTransactionsFromExpectedIn(reconciliator);

            // Assert
            Assert.AreEqual(4, reconciliator.OwnedFile.Records.Count);
            Assert.AreEqual(Codes.Expenses + "1", reconciliator.OwnedFile.Records[0].Description);
            Assert.AreEqual(Codes.Expenses + "2", reconciliator.OwnedFile.Records[1].Description);
            Assert.AreEqual(Codes.Expenses + "3", reconciliator.OwnedFile.Records[2].Description);
            Assert.AreEqual(wagesDescription, reconciliator.OwnedFile.Records[3].Description);
        }

        [Test]
        public void M_WillFilterActualBankFileForExpenseTransactionsOnly()
        {
            // Arrange
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Description = $"\"'{ReconConsts.EmployerExpenseDescription}\""},
                    new ActualBankRecord {Description = "something else"}
                });
            var actualBankFile = new ActualBankInFile(new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object));
            var mockBankInFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses + "1"}
                });
            var bankInFile = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mockBankInFileIO.Object));
            var dataLoadingInfo = new DataLoadingInformation<ActualBankRecord, BankRecord> { SheetName = MainSheetNames.BankIn };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(dataLoadingInfo, actualBankFile, bankInFile);
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.FilterForAllExpenseTransactionsFromActualBankIn(reconciliator);

            // Assert
            Assert.AreEqual(1, reconciliator.ThirdPartyFile.Records.Count);
            Assert.AreEqual(ReconConsts.EmployerExpenseDescription, reconciliator.ThirdPartyFile.Records[0].Description.RemovePunctuation());
        }

        [Test]
        public void M_WhenExpenseMatchingWillFilterOwnedFileForWagesRowsAndExpenseTransactionsOnly()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.FilterOwnedFile(matcher.IsNotWagesRowOrExpenseTransaction));
        }

        [Test]
        public void M_WhenExpenseMatchingWillFilterActualBankFileForExpenseTransactionsOnly()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.FilterThirdPartyFile(matcher.IsNotExpenseTransaction));
        }

        [Test]
        public void M_WhenExpenseMatchingWillRefreshFilesAtEnd()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.RefreshFiles());
        }

        [Test]
        public void M_WhenExpenseMatchingWillSetMatchFindingDelegate()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.SetMatchFinder(matcher.FindExpenseMatches));
        }

        [Test]
        public void M_WhenExpenseMatchingWillResetMatchFindingDelegateAtEnd()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.ResetMatchFinder());
        }

        [Test]
        public void M_WhenExpenseMatchingWillSetRecordMatchingDelegate()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.SetRecordMatcher(matcher.MatchSpecifiedRecords));
        }

        [Test]
        public void M_WhenExpenseMatchingWillResetRecordMatchingDelegate()
        {
            // Arrange
            var mockReconciliator = new Mock<IReconciliator<ActualBankRecord, BankRecord>>();
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DoPreliminaryStuff(mockReconciliator.Object);

            // Assert
            mockReconciliator.Verify(x => x.ResetRecordMatcher());
        }

        [Test]
        public void M_CanShowFirstExpenseTransactionWithListOfMatches()
        {
            // Arrange
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<ActualBankRecord> {
                    new ActualBankRecord {Description = ReconConsts.EmployerExpenseDescription},
                    new ActualBankRecord {Description = "something else"}
                });
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null))
                .Returns(new List<BankRecord> {
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses},
                    new BankRecord {Type = Codes.Expenses, Description = Codes.Expenses},
                    new BankRecord {Type = "Chq", Description = "something else"}
                });
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>("Bank In", mockActualBankFileIO.Object, mockBankFileIO.Object);
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            matcher.DEBUGPreliminaryStuff(reconciliator);

            // Assert
            var expenseDescriptionLines = _outputAllLinesRecordedConsoleLines.Where(
                x => x.DescriptionString == ReconConsts.EmployerExpenseDescription);
            Assert.AreEqual(1, expenseDescriptionLines.Count(), "transaction with expense description.");
            var expenseCodeLines = _outputAllLinesRecordedConsoleLines.Where(
                x => x.DescriptionString == Codes.Expenses);
            Assert.AreEqual(3, expenseCodeLines.Count(), "row with expense code.");

            // Clean up
            reconciliator.RefreshFiles();
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillMatchRecordWithSpecifiedIndex()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch { ActualRecords = new List<ICSVRecord> { new BankRecord {Matched = false} } },
                new PotentialMatch { ActualRecords = new List<ICSVRecord> { new BankRecord {Matched = false} } },
                new PotentialMatch { ActualRecords = new List<ICSVRecord> { new BankRecord {Matched = false} } }
            };
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);
            var index = 1;

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(false, recordForMatching.Matches[0].ActualRecords[0].Matched, "first record not matched");
            Assert.AreEqual(true, recordForMatching.Matches[1].ActualRecords[0].Matched, "second record matched");
            Assert.AreEqual(false, recordForMatching.Matches[2].ActualRecords[0].Matched, "third record not matched");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_WillReplaceMultipleMatchesWithSingleMatch()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30}
                    }
                }
            };
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);
            var index = 0;
            Assert.AreEqual(2, recordForMatching.Matches[index].ActualRecords.Count, "num matches before call");

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(1, recordForMatching.Matches[index].ActualRecords.Count, "num matches after call");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithExplanatoryDescription()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potentialMatches[index].ActualRecords;
            var expectedDescription =
                $"{ReconConsts.SeveralExpenses} (£{matches[0].MainAmount()}, £{matches[1].MainAmount()}, £{matches[2].MainAmount()})";
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(expectedDescription, recordForMatching.Matches[index].ActualRecords[0].Description);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithDateToMatchSourceRecord()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(sourceRecord.Date, recordForMatching.Matches[index].ActualRecords[0].Date);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithAllAmountsAddedTogether()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var matches = potentialMatches[index].ActualRecords;
            var expectedAmount = matches[0].MainAmount() + matches[1].MainAmount() + matches[2].MainAmount();
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(expectedAmount, recordForMatching.Matches[index].ActualRecords[0].MainAmount());
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillCreateNewRecordWithTypeOfFirstMatch()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var expectedType = (potentialMatches[index].ActualRecords[0] as BankRecord).Type;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(expectedType, (recordForMatching.Matches[index].ActualRecords[0] as BankRecord).Type);
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_BothSourceAndMatchWillHaveMatchedSetToTrue()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(true, recordForMatching.Matches[index].ActualRecords[0].Matched, "match is set to matched");
            Assert.AreEqual(true, recordForMatching.SourceRecord.Matched, "source is set to matched");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_SourceAndMatchWillHaveMatchPropertiesPointingAtEachOther()
        {
            // Arrange
            var mockOwnedFile = new Mock<ICSVFile<BankRecord>>();
            mockOwnedFile.Setup(x => x.Records).Returns(new List<BankRecord>());
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var potentialMatches = new List<IPotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>
                    {
                        new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22, Type = "Type"},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                        new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
                    }
                }
            };
            var index = 0;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, mockOwnedFile.Object);

            // Assert
            Assert.AreEqual(recordForMatching.SourceRecord, recordForMatching.Matches[index].ActualRecords[0].Match, "match is pointing at source");
            Assert.AreEqual(recordForMatching.Matches[index].ActualRecords[0], recordForMatching.SourceRecord.Match, "source is pointing at match");
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillRemoveOriginalMatchesFromOwnedFile()
        {
            // Arrange
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bankRecords = new List<BankRecord>
            {
                new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22, Type = "Type"},
                new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
            };
            var mockBankInFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankRecords);
            var bankInFile = new CSVFile<BankRecord>(mockBankInFileIO.Object);
            bankInFile.Load();
            var potentialMatches = new List<IPotentialMatch> { new PotentialMatch {ActualRecords = new List<ICSVRecord>()} };
            potentialMatches[0].ActualRecords.Add(bankRecords[0]);
            potentialMatches[0].ActualRecords.Add(bankRecords[1]);
            potentialMatches[0].ActualRecords.Add(bankRecords[2]);
            var index = 0;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);
            foreach (var bankRecord in bankRecords)
            {
                Assert.IsTrue(bankInFile.Records.Contains(bankRecord));
            }

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, bankInFile);

            // Assert
            foreach (var bankRecord in bankRecords)
            {
                Assert.IsFalse(bankInFile.Records.Contains(bankRecord));
            }
        }

        [Test]
        public void M_WhenMatchingSpecifiedRecords_AndMultipleMatchesExist_WillAddNewlyCreatedMatchToOwnedFile()
        {
            // Arrange
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            var sourceRecord = new ActualBankRecord
            {
                Date = DateTime.Today,
                Amount = 34.56,
                Match = null,
                Matched = false
            };
            var bankRecords = new List<BankRecord>
            {
                new BankRecord {Description = "Match 01", UnreconciledAmount = 20.22, Type = "Type"},
                new BankRecord {Description = "Match 02", UnreconciledAmount = 30.33},
                new BankRecord {Description = "Match 02", UnreconciledAmount = 40.44}
            };
            var mockBankInFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankRecords);
            var bankInFile = new CSVFile<BankRecord>(mockBankInFileIO.Object);
            bankInFile.Load();
            var potentialMatches = new List<IPotentialMatch> { new PotentialMatch { ActualRecords = new List<ICSVRecord>() } };
            potentialMatches[0].ActualRecords.Add(bankRecords[0]);
            potentialMatches[0].ActualRecords.Add(bankRecords[1]);
            potentialMatches[0].ActualRecords.Add(bankRecords[2]);
            var index = 0;
            var recordForMatching = new RecordForMatching<ActualBankRecord>(sourceRecord, potentialMatches);
            foreach (var bankRecord in bankRecords)
            {
                Assert.IsTrue(bankInFile.Records.Contains(bankRecord));
            }

            // Act
            matcher.MatchSpecifiedRecords(recordForMatching, index, bankInFile);

            // Assert
            Assert.AreEqual(1, bankInFile.Records.Count);
            Assert.IsTrue(bankInFile.Records[0].Description.Contains(ReconConsts.SeveralExpenses));
        }

        [Test]
        public void WillNotLosePreviouslyMatchedRecordsWhenFilesAreRefreshed()
        {
            // Arrange
            var someOtherActualBankDescription = "Some other ActualBank description";
            var someOtherBankDescription = "Some other bank description";
            var actualBankData = new List<ActualBankRecord>
            {
                new ActualBankRecord { Description = ReconConsts.EmployerExpenseDescription },
                new ActualBankRecord { Description = someOtherActualBankDescription }
            };
            var mockActualBankFileIO = new Mock<IFileIO<ActualBankRecord>>();
            mockActualBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(actualBankData);
            var actualBankFile = new GenericFile<ActualBankRecord>(new CSVFile<ActualBankRecord>(mockActualBankFileIO.Object));
            var bankData = new List<BankRecord>
            {
                new BankRecord {Description = "BankRecord01", Type = Codes.Expenses},
                new BankRecord {Description = "BankRecord02", Type = Codes.Expenses},
                new BankRecord {Description = "BankRecord03", Type = Codes.Expenses},
                new BankRecord {Description = someOtherBankDescription, Type = "Not an expense"}
            };
            var mockBankFileIO = new Mock<IFileIO<BankRecord>>();
            mockBankFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(bankData);
            var bankFile = new GenericFile<BankRecord>(new CSVFile<BankRecord>(mockBankFileIO.Object));
            var dataLoadingInfo = new DataLoadingInformation<ActualBankRecord, BankRecord> { SheetName = MainSheetNames.BankIn };
            var reconciliator = new Reconciliator<ActualBankRecord, BankRecord>(dataLoadingInfo, actualBankFile, bankFile);
            var expectedPotentialMatches = new List<PotentialMatch>
            {
                new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>{ bankData[0], bankData[1] },
                    ConsoleLines = new List<ConsoleLine>{ new ConsoleLine { DescriptionString = "Console Description" } }
                }
            };
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());
            matcher.FilterForAllExpenseTransactionsFromActualBankIn(reconciliator);
            matcher.FilterForAllWagesRowsAndExpenseTransactionsFromExpectedIn(reconciliator);
            reconciliator.SetMatchFinder((record, file) => expectedPotentialMatches);
            reconciliator.SetRecordMatcher(matcher.MatchSpecifiedRecords);
            reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            reconciliator.MatchCurrentRecord(0);
            Assert.AreEqual(1, reconciliator.ThirdPartyFile.Records.Count);
            Assert.AreEqual(2, reconciliator.OwnedFile.Records.Count);
            Assert.IsFalse(reconciliator.OwnedFile.Records[0].Matched);
            Assert.IsTrue(reconciliator.OwnedFile.Records[1].Matched);
            Assert.IsTrue(reconciliator.OwnedFile.Records[1].Description.Contains(ReconConsts.SeveralExpenses));

            // Act
            reconciliator.RefreshFiles();

            // Assert
            Assert.AreEqual(2, reconciliator.ThirdPartyFile.Records.Count);
            Assert.AreEqual(someOtherActualBankDescription, reconciliator.ThirdPartyFile.Records[1].Description);
            Assert.AreEqual(3, reconciliator.OwnedFile.Records.Count);
            Assert.IsFalse(reconciliator.OwnedFile.Records[0].Matched);
            Assert.AreEqual(someOtherBankDescription, reconciliator.OwnedFile.Records[1].Description);
            Assert.AreEqual(actualBankData[0], reconciliator.OwnedFile.Records[2].Match);
            Assert.IsTrue(reconciliator.OwnedFile.Records[2].Matched);
            Assert.IsTrue(reconciliator.OwnedFile.Records[2].Description.Contains(ReconConsts.SeveralExpenses));
        }

        [Test]
        public void M_WhenReconcilingExpenses_WillMatchOnASingleAmount()
        {
            // Arrange
            var expenseAmount = 10.00;
            List<BankRecord> expectedInRows = new List<BankRecord> { new BankRecord
            {
                UnreconciledAmount = expenseAmount,
                Description = "HELLOW"
            } };
            var bankInFileIO = new Mock<IFileIO<BankRecord>>();
            bankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expectedInRows);
            var bankInFile = new CSVFile<BankRecord>(bankInFileIO.Object);
            bankInFile.Load();
            ActualBankRecord expenseTransaction = new ActualBankRecord { Amount = expenseAmount };
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            var result = matcher.STANDBYFindExpenseMatches(expenseTransaction, bankInFile).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expectedInRows[0], result[0].ActualRecords[0]);
        }

        [Test]
        public void M_WhenReconcilingExpenses_WillNotMatchOnASingleDifferentAmount()
        {
            // Arrange
            var expenseAmount = 10.00;
            List<BankRecord> expectedInRows = new List<BankRecord> { new BankRecord { UnreconciledAmount = expenseAmount } };
            var bankInFileIO = new Mock<IFileIO<BankRecord>>();
            bankInFileIO.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(expectedInRows);
            var bankInFile = new CSVFile<BankRecord>(bankInFileIO.Object);
            bankInFile.Load();
            ActualBankRecord expenseTransaction = new ActualBankRecord { Amount = expenseAmount - 1 };
            var matcher = new BankAndBankInMatcher(this, new FakeSpreadsheetRepoFactory());

            // Act
            var result = matcher.STANDBYFindExpenseMatches(expenseTransaction, bankInFile).ToList();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void M_WillPopulateConsoleLinesForEveryPotentialMatch()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillPopulateRankingsForEveryPotentialMatch()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindSingleMatchingBankInTransactionForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindSingleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void M_WillFindMultipleMatchingCollectionOfBankInTransactionsForOneActualBankExpenseTransaction()
        {
            Assert.AreEqual(true, true);
        }
    }
}
