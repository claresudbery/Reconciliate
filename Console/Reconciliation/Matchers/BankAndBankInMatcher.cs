using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class BankAndBankInMatcher : IMatcher
    {
        public List<ExpectedIncomeRecord> MatchedExpectedIncomeRecords;

        private readonly IInputOutput _inputOutput;
        private readonly ISpreadsheetRepoFactory _spreadsheetFactory;
        private readonly BankAndBankInLoader _bankAndBankInLoader = new BankAndBankInLoader();

        public BankAndBankInMatcher(
            IInputOutput inputOutput,
            ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _inputOutput = inputOutput;
            _spreadsheetFactory = spreadsheetFactory;
            MatchedExpectedIncomeRecords = new List<ExpectedIncomeRecord>();
        }

        public void DoMatching(FilePaths mainFilePaths)
        {
            var loadingInfo = _bankAndBankInLoader.LoadingInfo();
            loadingInfo.FilePaths = mainFilePaths;
            var reconciliationIntro = new ReconciliationIntro(_inputOutput);
            ReconciliationInterface<ActualBankRecord, BankRecord> reconciliationInterface
                = reconciliationIntro.LoadCorrectFiles<ActualBankRecord, BankRecord>(loadingInfo, _spreadsheetFactory, this);
            reconciliationInterface?.DoTheMatching();
        }

        public void DoPreliminaryStuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliationInterface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            DoEmployerExpenseMatching(reconciliator, reconciliationInterface);
        }

        private void DoEmployerExpenseMatching<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliationInterface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            FilterForAllExpenseTransactionsFromActualBankIn(reconciliator);
            FilterForAllWagesRowsAndExpenseTransactionsFromExpectedIn(reconciliator);
            reconciliator.SetMatchFinder(FindExpenseMatches);
            reconciliator.SetRecordMatcher(MatchSpecifiedRecords);

            reconciliationInterface.DoSemiAutomaticMatching();

            reconciliator.RefreshFiles();
            reconciliator.ResetMatchFinder();
            reconciliator.ResetRecordMatcher();
        }

        public void FilterForAllExpenseTransactionsFromActualBankIn<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.FilterThirdPartyFile(IsNotExpenseTransaction);
        }

        public bool IsNotExpenseTransaction<TThirdPartyType>(TThirdPartyType actualBankRecord) where TThirdPartyType : ICSVRecord, new()
        {
            return actualBankRecord.Description.RemovePunctuation().ToUpper()
                   != ReconConsts.EmployerExpenseDescription;
        }

        public void FilterForAllWagesRowsAndExpenseTransactionsFromExpectedIn<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            reconciliator.FilterOwnedFile(IsNotWagesRowOrExpenseTransaction);
        }

        public bool IsNotWagesRowOrExpenseTransaction<TOwnedType>(TOwnedType bankRecord) where TOwnedType : ICSVRecord, new()
        {
            return (bankRecord as BankRecord).Type != Codes.Expenses
                && !bankRecord.Description.Contains(ReconConsts.EmployerExpenseDescription);
        }

        public List<ConsoleLine> GetAllExpenseTransactionsFromActualBankIn<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            FilterForAllExpenseTransactionsFromActualBankIn(reconciliator);
            return reconciliator.ThirdPartyFile.Records.Select(x => x.ToConsole()).ToList();
        }

        public List<ConsoleLine> GetAllWagesRowsAndExpenseTransactionsFromExpectedIn<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            FilterForAllWagesRowsAndExpenseTransactionsFromExpectedIn(reconciliator);
            return reconciliator.OwnedFile.Records.Select(x => x.ToConsole()).ToList();
        }

        public void MatchSpecifiedRecords<TThirdPartyType, TOwnedType>(
                RecordForMatching<TThirdPartyType> recordForMatching,
                int matchIndex,
                ICSVFile<TOwnedType> ownedFile)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            if (recordForMatching.Matches[matchIndex].ActualRecords.Count > 1)
            {
                var newMatch = new TOwnedType
                {
                    Date = recordForMatching.SourceRecord.Date,
                    Description = CreateNewDescription(recordForMatching.Matches[matchIndex])
                };
                (newMatch as BankRecord).UnreconciledAmount = recordForMatching.Matches[matchIndex].ActualRecords.Sum(x => x.MainAmount());
                (newMatch as BankRecord).Type = (recordForMatching.Matches[matchIndex].ActualRecords[0] as BankRecord).Type;
                foreach (var actualRecord in recordForMatching.Matches[matchIndex].ActualRecords)
                {
                    _bankAndBankInLoader.UpdateExpectedIncomeRecordWhenMatched(recordForMatching.SourceRecord, (TOwnedType)actualRecord);
                    ownedFile.RemoveRecordPermanently((TOwnedType)actualRecord);
                }
                recordForMatching.Matches[matchIndex].ActualRecords.Clear();
                recordForMatching.Matches[matchIndex].ActualRecords.Add(newMatch);
                ownedFile.AddRecordPermanently(newMatch);
            }
            else
            {
                _bankAndBankInLoader.UpdateExpectedIncomeRecordWhenMatched(
                    recordForMatching.SourceRecord,
                    (TOwnedType)recordForMatching.Matches[matchIndex].ActualRecords[0]);
            }
            MatchRecords(recordForMatching.SourceRecord, recordForMatching.Matches[matchIndex].ActualRecords[0]);
        }

        private void MatchRecords<TThirdPartyType>(TThirdPartyType source, ICSVRecord match) where TThirdPartyType : ICSVRecord, new()
        {
            match.Matched = true;
            (source as ICSVRecord).Matched = true;
            match.Match = source;
            (source as ICSVRecord).Match = match;
        }

        private string CreateNewDescription(IPotentialMatch potentialMatch)
        {
            var combinedAmounts = potentialMatch.ActualRecords[0].MainAmount().ToCsvString(true);
            for (int count = 1; count < potentialMatch.ActualRecords.Count; count++)
            {
                combinedAmounts += $", {potentialMatch.ActualRecords[count].MainAmount().ToCsvString(true)}";
            }
            return $"{ReconConsts.SeveralExpenses} ({combinedAmounts})";
        }

        public IEnumerable<IPotentialMatch> FindExpenseMatches<TThirdPartyType, TOwnedType>
                (TThirdPartyType sourceRecord, ICSVFile<TOwnedType> ownedFile)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            return DEBUGFindExpenseMatches(sourceRecord as ActualBankRecord, ownedFile as ICSVFile<BankRecord>);
        }

        public IEnumerable<IPotentialMatch> STANDBYFindExpenseMatches<TThirdPartyType, TOwnedType>
                (TThirdPartyType sourceRecord, ICSVFile<TOwnedType> ownedFile)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            var result = new List<PotentialMatch>();
            if (ownedFile.Records[0].MainAmount() == sourceRecord.MainAmount())
            {
                var actualRecords = new List<ICSVRecord>();
                actualRecords.Add(ownedFile.Records[0]);
                result.Add(new PotentialMatch {ActualRecords = actualRecords});
            }
            return result;
        }

        public void DEBUGPreliminaryStuff<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            List<ConsoleLine> allExpenseTransactionsFromActualBankIn = GetAllExpenseTransactionsFromActualBankIn(reconciliator);
            _inputOutput.OutputLine("***********");
            _inputOutput.OutputLine("All Expense Transactions From ActualBank In:");
            _inputOutput.OutputAllLines(allExpenseTransactionsFromActualBankIn);

            List<ConsoleLine> allExpenseTransactionsFromExpectedIn = GetAllWagesRowsAndExpenseTransactionsFromExpectedIn(reconciliator);
            _inputOutput.OutputLine("***********");
            _inputOutput.OutputLine("All Expense Transactions From Expected In:");
            _inputOutput.OutputAllLines(allExpenseTransactionsFromExpectedIn);

            reconciliator.RefreshFiles();

            _inputOutput.GetInput(ReconConsts.EnterAnyKeyToContinue);
        }

        private IEnumerable<IPotentialMatch> DEBUGFindExpenseMatches(ActualBankRecord sourceRecord, ICSVFile<BankRecord> ownedFile)
        {
            var result = new List<IPotentialMatch>();
            var randomNumberGenerator = new Random();

            AddSetOfOverlappingMatches(randomNumberGenerator, ownedFile, result, 3);
            AddSetOfOverlappingMatches(randomNumberGenerator, ownedFile, result, 2);
            AddSetOfOverlappingMatches(randomNumberGenerator, ownedFile, result, 2);
            AddSetOfOverlappingMatches(randomNumberGenerator, ownedFile, result, 3);

            return result;
        }

        private static void AddSetOfOverlappingMatches(
            Random randomNumberGenerator,
            ICSVFile<BankRecord> ownedFile, 
            List<IPotentialMatch> result, 
            int numMatches)
        {
            var unmatchedRecords = ownedFile.Records.Where(x => !x.Matched).ToList();
            var maxRand = unmatchedRecords.Count - 1;
            if (maxRand >= 0)
            {
                var newMatch = new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>(),
                    ConsoleLines = new List<ConsoleLine>(),
                    Rankings = new Rankings { Amount = 0, Date = 0, Combined = 0 },
                    AmountMatch = true,
                    FullTextMatch = true,
                    PartialTextMatch = true
                };
                for (int count = 1; count <= numMatches; count++)
                {
                    if (maxRand >= 0)
                    {
                        var randomIndex = randomNumberGenerator.Next(0, maxRand);
                        var nextRecord = unmatchedRecords[randomIndex];
                        newMatch.ActualRecords.Add(nextRecord);
                        newMatch.ConsoleLines.Add(nextRecord.ToConsole());
                        unmatchedRecords.Remove(nextRecord);
                        maxRand--;
                    }
                }
                result.Add(newMatch);
            }
        }
    }
}