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
            var loadingInfo = new BankAndBankInLoader().LoadingInfo();
            loadingInfo.FilePaths = mainFilePaths;
            var reconciliationIntro = new ReconciliationIntro(_inputOutput);
            ReconciliationInterface<ActualBankRecord, BankRecord> reconciliationInterface
                = reconciliationIntro.LoadCorrectFiles<ActualBankRecord, BankRecord>(loadingInfo, _spreadsheetFactory);
            reconciliationInterface?.DoTheMatching();
        }

        public void FilterForAllExpenseTransactionsFromActualBankIn<TThirdPartyType, TOwnedType>(IReconciliator<TThirdPartyType, TOwnedType> reconciliator)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
        }

        public bool IsNotExpenseTransaction<TThirdPartyType>(TThirdPartyType actualBankRecord) where TThirdPartyType : ICSVRecord, new()
        {
            return actualBankRecord.Description.RemovePunctuation().ToUpper()
                   != ReconConsts.EmployerExpenseDescription;
        }

        public bool IsNotWagesRowOrExpenseTransaction<TOwnedType>(TOwnedType bankRecord) where TOwnedType : ICSVRecord, new()
        {
            return (bankRecord as BankRecord).Type != Codes.Expenses
                && !bankRecord.Description.Contains(ReconConsts.EmployerExpenseDescription);
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
                    CreateExpectedIncomeRecord(recordForMatching.SourceRecord, (TOwnedType)actualRecord);
                    ownedFile.RemoveRecordPermanently((TOwnedType)actualRecord);
                }
                recordForMatching.Matches[matchIndex].ActualRecords.Clear();
                recordForMatching.Matches[matchIndex].ActualRecords.Add(newMatch);
                ownedFile.AddRecordPermanently(newMatch);
            }
            else
            {
                CreateExpectedIncomeRecord(
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

        private void CreateExpectedIncomeRecord<TThirdPartyType, TOwnedType>(
            TThirdPartyType sourceRecord,
            TOwnedType match)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            MatchedExpectedIncomeRecords.Add(new ExpectedIncomeRecord
            {
                Description = match.Description,
                UnreconciledAmount = match.MainAmount(),
                Match = sourceRecord,
                Matched = true,
                Code = Codes.Expenses,
                Date = match.Date,
                DatePaid = sourceRecord.Date,
                TotalPaid = sourceRecord.MainAmount()
            });
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
            return new List<IPotentialMatch>();
        }
    }
}