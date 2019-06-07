using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation
{
    internal class Reconciliator<TThirdPartyType, TOwnedType> : IReconciliator
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        public const string CannotDeleteCurrentRecordNoMatching = "Cannot delete current record if no matching has been done yet.";
        public const string CannotDeleteMatchedRecordNoMatching = "Cannot delete matched record if no matching has been done yet.";
        public const string CannotDeleteThirdPartyRecordDoesNotExist = "There is no third-party record to delete with the specified index.";
        public const string CannotDeleteOwnedRecordDoesNotExist = "There is no owned record to delete with the specified index.";
        public const string CannotDeleteMatchedOwnedRecord = "You can't delete an owned record that has already been matched with a third party record.";
        public const string BadMatchNumber = "Bad match number.";

        public ICSVFile<TThirdPartyType> ThirdPartyFile
        {
            get => _thirdPartyFile.File;
            set => _thirdPartyFile.File = value;
        }
        public ICSVFile<TOwnedType> OwnedFile
        {
            get => _ownedFile.File;
            set => _ownedFile.File = value;
        }
        public IDataFile<TThirdPartyType> _thirdPartyFile { get; set; }
        public IDataFile<TOwnedType> _ownedFile { get; set; }
        private readonly string _worksheetName;
        private RecordForMatching<TThirdPartyType> _latestRecordForMatching = null;
        private List<AutoMatchedRecord<TThirdPartyType>> _autoMatches = null;
        private int _currentIndex = -1;
        private readonly List<string> _reservedWords = new List<string>{"AND","THE","OR","WITH"};

        public Reconciliator(
            ICSVFile<TThirdPartyType> thirdPartyCSVFile,
            ICSVFile<TOwnedType> ownedCSVFile,
            ThirdPartyFileLoadAction thirdPartyFileLoadAction = ThirdPartyFileLoadAction.NoAction,
            string worksheetName = "")
        {
            _thirdPartyFile = new GenericFile<TThirdPartyType>(thirdPartyCSVFile);
            _ownedFile = new GenericFile<TOwnedType>(ownedCSVFile);

            PerformThirdPartyFileLoadAction(thirdPartyFileLoadAction, thirdPartyCSVFile);

            _worksheetName = worksheetName;

            Reset();
        }

        private void PerformThirdPartyFileLoadAction<TThirdPartyType>(
                ThirdPartyFileLoadAction thirdPartyFileLoadAction,
                ICSVFile<TThirdPartyType> thirdPartyCSVFile)
            where TThirdPartyType : ICSVRecord, new()
        {
            switch (thirdPartyFileLoadAction)
            {
                case ThirdPartyFileLoadAction.FilterForPositiveRecordsOnly:
                    thirdPartyCSVFile.FilterForPositiveRecordsOnly();
                    break;
                case ThirdPartyFileLoadAction.FilterForNegativeRecordsOnly:
                    thirdPartyCSVFile.FilterForNegativeRecordsOnly();
                    break;
                case ThirdPartyFileLoadAction.SwapSignsOfAllAmounts:
                    thirdPartyCSVFile.SwapSignsOfAllAmounts();
                    break;
                case ThirdPartyFileLoadAction.NoAction:
                default: break;
            }
        }

        public bool FindReconciliationMatchesForNextThirdPartyRecord()
        {
            bool foundUnmatchedThirdPartyRecord = false;

            while (NotAtEnd() && !foundUnmatchedThirdPartyRecord)
            {
                _currentIndex++;
                TThirdPartyType sourceRecord = ThirdPartyFile.Records[_currentIndex];
                var matches = FindLatestOrderedMatches(sourceRecord, OwnedFile).ToList();

                if (!sourceRecord.Matched && matches.Count > 0)
                {
                    _latestRecordForMatching = new RecordForMatching<TThirdPartyType>(
                        sourceRecord,
                        matches);

                    foundUnmatchedThirdPartyRecord = true;
                }
            }

            return foundUnmatchedThirdPartyRecord;
        }

        public bool MoveToNextUnmatchedThirdPartyRecordForManualMatching()
        {
            bool foundUnmatchedThirdPartyRecord = false;

            while (NotAtEnd() && !foundUnmatchedThirdPartyRecord)
            {
                _currentIndex++;
                TThirdPartyType sourceRecord = ThirdPartyFile.Records[_currentIndex];
                var unmatchedOwnedRecords = CurrentUnmatchedOwnedRecords(sourceRecord).ToList();

                if (!sourceRecord.Matched && unmatchedOwnedRecords.Count > 0)
                {
                    _latestRecordForMatching = new RecordForMatching<TThirdPartyType>(
                        sourceRecord,
                        unmatchedOwnedRecords);

                    foundUnmatchedThirdPartyRecord = true;
                }
            }

            return foundUnmatchedThirdPartyRecord;
        }

        private double GetDateRanking(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            return candidateRecord.Date.ProximityScore(masterRecord.Date);
        }

        private double GetAmountRanking(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            return candidateRecord.MainAmount().ProximityScore(masterRecord.MainAmount());
        }

        private IEnumerable<IPotentialMatch> FindLatestOrderedMatches(
            TThirdPartyType sourceRecord,
            ICSVFile<TOwnedType> ownedFile)
        {
            return ConvertUnmatchedOwnedRecordsToPotentialMatches(sourceRecord, ownedFile)
                .Where(b => b.FullTextMatch 
                    || b.PartialTextMatch 
                    || b.Rankings.Amount <= PotentialMatch.PartialAmountMatchThreshold)
                .OrderByDescending(c => c.AmountMatch)
                .ThenByDescending(c => c.FullTextMatch)
                .ThenByDescending(c => c.PartialTextMatch)
                .ThenBy(c => c.Rankings.Date)
                .ThenBy(c => c.Rankings.Amount);
        }

        private IEnumerable<IPotentialMatch> ConvertUnmatchedOwnedRecordsToPotentialMatches(
            TThirdPartyType sourceRecord,
            ICSVFile<TOwnedType> ownedFile)
        {
            return ownedFile
                .Records
                .Where(x => x.Matched == false)
                .Select(ownedRecord => new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord>{ownedRecord},
                    AmountMatch = ownedRecord.MainAmount() == sourceRecord.MainAmount(),
                    FullTextMatch = CheckForFullTextMatch(sourceRecord.Description, ownedRecord.Description),
                    PartialTextMatch = CheckForPartialTextMatch(sourceRecord.Description, ownedRecord.Description),
                    Rankings = new Rankings
                    {
                        Amount = GetAmountRanking(ownedRecord, sourceRecord),
                        Date = GetDateRanking(ownedRecord, sourceRecord)
                    },
                    ConsoleLines = new List<ConsoleLine> { ownedRecord.ToConsole() }
                });
        }

        private bool CheckForFullTextMatch(string sourceDescription, string targetDescription)
        {
            var sourceDescriptionTransformed = sourceDescription.RemovePunctuation().ToUpper();
            var targetDescriptionTransformed = targetDescription.RemovePunctuation().ToUpper();
            return (targetDescriptionTransformed == sourceDescriptionTransformed)
                || targetDescriptionTransformed.Contains(sourceDescriptionTransformed);
        }

        public bool CheckForPartialTextMatch(string sourceDescription, string targetDescription)
        {
            bool foundPartialMatch = false;
            var sourceWords = sourceDescription
                .ReplacePunctuationWithSpaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var targetWords = targetDescription
                .ReplacePunctuationWithSpaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            int sourceCount = 0;
            while (!foundPartialMatch && sourceCount < sourceWords.Count)
            {
                var sourceWord = sourceWords[sourceCount];
                if (sourceWord.Length > 1)
                {
                    int targetCount = 0;
                    while (!foundPartialMatch && targetCount < targetWords.Count)
                    {
                        var targetWord = targetWords[targetCount];
                        if ((targetWord == sourceWord || targetWord.StartsWith(sourceWord)) && !_reservedWords.Contains(targetWord))
                        {
                            foundPartialMatch = true;
                        }
                        targetCount++;
                    }
                }
                sourceCount++;
            }
            return foundPartialMatch;
        }

        private IEnumerable<IPotentialMatch> CurrentUnmatchedOwnedRecords(TThirdPartyType sourceRecord)
        {
            var unmatchedOwnedRecords = OwnedFile
                .Records
                .Where(x => x.Matched == false)
                .Select( ownedRecord => new PotentialMatch
                {
                    ActualRecords = new List<ICSVRecord> { ownedRecord },
                    AmountMatch = ownedRecord.MainAmount() == sourceRecord.MainAmount(),
                    FullTextMatch = CheckForFullTextMatch(sourceRecord.Description, ownedRecord.Description),
                    PartialTextMatch = CheckForPartialTextMatch(sourceRecord.Description, ownedRecord.Description),
                    Rankings = GetRankings(ownedRecord, sourceRecord),
                    ConsoleLines = new List<ConsoleLine> { ownedRecord.ToConsole() }
                })
                .OrderBy(c => c.Rankings.Combined);

            return unmatchedOwnedRecords;
        }

        private Rankings GetRankings(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            var amountRanking = GetAmountRanking(candidateRecord, masterRecord);
            var dateRanking = GetDateRanking(candidateRecord, masterRecord);
            return new Rankings
            {
                Amount = amountRanking,
                Date = dateRanking,
                Combined = Math.Min(amountRanking, dateRanking)
            };
        }

        public void MatchCurrentRecord(int matchIndex)
        {
            MatchSpecifiedRecords(_latestRecordForMatching, matchIndex, OwnedFile);
        }

        private void MatchSpecifiedRecords(
            RecordForMatching<TThirdPartyType> recordForMatching, 
            int matchIndex,
            ICSVFile<TOwnedType> ownedFile)
        { 
            try
            {
                MatchRecords(recordForMatching.SourceRecord,
                    recordForMatching.Matches[matchIndex].ActualRecords.ElementAt(0));

                if (recordForMatching.SourceRecord.MainAmount() !=
                    recordForMatching.SourceRecord.Match.MainAmount())
                {
                    ChangeAmountAndDescriptionToMatchThirdPartyRecord(
                        recordForMatching.SourceRecord,
                        recordForMatching.Matches[matchIndex].ActualRecords.ElementAt(0));
                }
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        private void MatchRecords(TThirdPartyType source, ICSVRecord match)
        {
            source.Matched = true;
            source.Match = match;

            match.Matched = true;
            match.Match = source;
        }

        private void UnmatchRecords(TThirdPartyType source, ICSVRecord match)
        {
            source.Matched = false;
            source.Match = null;

            match.Matched = false;
            match.Match = null;
        }

        private void ChangeAmountAndDescriptionToMatchThirdPartyRecord(TThirdPartyType sourceRecord, ICSVRecord matchedRecord)
        {
            matchedRecord.Description = matchedRecord.Description 
                                        + ReconConsts.OriginalAmountWas
                                        + string.Format(StringHelper.Culture(), "{0:C}", matchedRecord.MainAmount());

            matchedRecord.ChangeMainAmount(sourceRecord.MainAmount());
        }

        public void Reset()
        {
            _currentIndex = -1;
            ThirdPartyFile.ResetAllMatches();
            OwnedFile.ResetAllMatches();
        }

        public void RefreshFiles()
        {
            _thirdPartyFile.RefreshFileContents();
            _ownedFile.RefreshFileContents();
        }

        public void Rewind()
        {
            _currentIndex = -1;
        }

        public void Finish(string fileSuffix)
        {
            AddUnmatchedSourceItemsToOwnedFile();
            ReconcileAllMatchedItems();
            
            OwnedFile.WriteToCsvFile(fileSuffix);
            if ("" != _worksheetName)
            {
                OwnedFile.WriteBackToMainSpreadsheet(_worksheetName);
            }
        }

        private void ReconcileAllMatchedItems()
        {
            foreach (var matchedOwnedRecord in OwnedFile.Records.Where(x => x.Matched))
            {
                matchedOwnedRecord.Reconcile();
            }
        }

        private void AddUnmatchedSourceItemsToOwnedFile()
        {
            foreach (var unmatchedSource in ThirdPartyFile.Records.Where(x => !x.Matched))
            {
                var newOwnedRecord = new TOwnedType();
                newOwnedRecord.CreateFromMatch(
                    unmatchedSource.Date,
                    unmatchedSource.MainAmount(),
                    unmatchedSource.TransactionType(),
                    "!! Unmatched from 3rd party: " + unmatchedSource.Description,
                    unmatchedSource.ExtraInfo(),
                    unmatchedSource);
                OwnedFile.Records.Add(newOwnedRecord);
            }
        }

        public bool NotAtEnd()
        {
            return _currentIndex < (ThirdPartyFile.Records.Count - 1);
        }

        public List<IPotentialMatch> CurrentPotentialMatches()
        {
            // !! Don't re-order items here !! If you do, they will be displayed in a different order than they are stored.
            // This will mean that if a user selects Item with index 4, it might actually be stored with index 9, 
            // and the wrong record will be matched.
            // Instead, if you want to change the ordering, look at CurrentUnmatchedOwnedRecords() and FindLatestOrderedMatches()
            return _latestRecordForMatching != null 
                ? Indexed(_latestRecordForMatching.Matches)
                : null;
        }

        public static List<IPotentialMatch> Indexed(List<IPotentialMatch> sourceList)
        {
            int index = 0;
            foreach (var item in sourceList)
            {
                foreach (var consoleLine in item.ConsoleLines)
                {
                    consoleLine.Index = index;
                }
                index++;
            }

            return sourceList;
        }

        public string CurrentSourceRecordAsString()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.ToCsv()
                : "No record currently stored";
        }

        public ConsoleLine CurrentSourceRecordAsConsoleLine()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.ToConsole()
                : new ConsoleLine();
        }

        public string CurrentSourceDescription()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.Description
                : "No current record";
        }

        public RecordForMatching<TThirdPartyType> CurrentRecordForMatching()
        {
            return _latestRecordForMatching;
        }

        public List<TOwnedType> OwnedFileRecords()
        {
            return OwnedFile.Records;
        }

        public void DeleteCurrentThirdPartyRecord()
        {
            try
            {
                if (ThirdPartyFile.Records[_currentIndex].Matched)
                {
                    ThirdPartyFile.Records[_currentIndex].Match.Matched = false;
                    ThirdPartyFile.Records[_currentIndex].Match.Match = null;
                }
                ThirdPartyFile.Records.RemoveAt(_currentIndex);
                _currentIndex--;
                _latestRecordForMatching = null;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception(CannotDeleteCurrentRecordNoMatching);
            }
        }

        public void DeleteSpecificThirdPartyRecord(int specifiedIndex)
        {
            try
            {
                if (ThirdPartyFile.Records[specifiedIndex].Matched)
                {
                    ThirdPartyFile.Records[specifiedIndex].Match.Matched = false;
                    ThirdPartyFile.Records[specifiedIndex].Match.Match = null;
                }
                ThirdPartyFile.Records.RemoveAt(specifiedIndex);
                if (specifiedIndex == _currentIndex)
                {
                    _currentIndex--;
                    _latestRecordForMatching = null;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception(CannotDeleteThirdPartyRecordDoesNotExist);
            }
        }

        public void DeleteSpecificOwnedRecordFromListOfMatches(int specifiedIndex)
        {
            if (_latestRecordForMatching == null)
            {
                throw new Exception(CannotDeleteMatchedRecordNoMatching);
            }
            else
            {
                try
                {
                    foreach (var actualRecord in _latestRecordForMatching.Matches[specifiedIndex].ActualRecords)
                    {
                        if (actualRecord.Matched)
                        {
                            throw new Exception(CannotDeleteMatchedOwnedRecord);
                        }
                        else
                        {
                            OwnedFile.Records.Remove((TOwnedType)(actualRecord));
                        }
                    }
                    _latestRecordForMatching.Matches.RemoveAt(specifiedIndex);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new ArgumentOutOfRangeException(CannotDeleteOwnedRecordDoesNotExist, exception);
                }
            }
        }

        public int NumPotentialMatches()
        {
            return _latestRecordForMatching != null ? 
                _latestRecordForMatching.Matches.Count
                : 0;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> ReturnAutoMatches()
        {
            _autoMatches = new List<AutoMatchedRecord<TThirdPartyType>>();
            var index = 0;

            foreach (var thirdPartyRecord in ThirdPartyFile.Records)
            {
                var singleMatch = FindSingleMatchByAmountAndTextAndNearDate(thirdPartyRecord);
                if (null != singleMatch)
                {
                    var newRecordForMatching = new AutoMatchedRecord<TThirdPartyType>(
                        thirdPartyRecord,
                        FindSingleMatchByAmountAndTextAndNearDate(thirdPartyRecord),
                        index);

                    MatchRecords(thirdPartyRecord, singleMatch.ActualRecords.ElementAt(0));

                    _autoMatches.Add(newRecordForMatching);
                    index++;
                }
            }

            return _autoMatches;
        }

        private IPotentialMatch FindSingleMatchByAmountAndTextAndNearDate(TThirdPartyType sourceRecord)
        {
            var allMatches = ConvertUnmatchedOwnedRecordsToPotentialMatches(sourceRecord, OwnedFile)
                .Where(b => 
                    (b.FullTextMatch || b.PartialTextMatch)
                    && b.Rankings.Amount == 0 
                    && b.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold);

            return SingleMatchOnly(allMatches);
        }

        private IPotentialMatch SingleMatchOnly(IEnumerable<IPotentialMatch> allMatches)
        {
            return allMatches.Count() == 1
                ? allMatches.ToList()[0]
                : null;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> GetAutoMatches()
        {
            return _autoMatches;
        }

        public List<ConsoleLine> GetAutoMatchesForConsole()
        {
            List<ConsoleLine> consoleLines = new List<ConsoleLine>();

            var autoMatchesWithMatchedItemsOnly = _autoMatches.Where(x => x.Match != null);
            foreach (var autoMatch in autoMatchesWithMatchedItemsOnly)
            {
                consoleLines.Add(new ConsoleLine().AsSeparator(autoMatch.Index));
                consoleLines.Add(autoMatch.SourceRecord.ToConsole(autoMatch.Index));
                consoleLines.Add(autoMatch.Match.ActualRecords.ElementAt(0).ToConsole(autoMatch.Index));
            }

            return consoleLines;
        }

        public List<ConsoleLine> GetFinalMatchesForConsole()
        {
            List<ConsoleLine> consoleLines = new List<ConsoleLine>();

            var matchedRecords = ThirdPartyFile.Records.Where(x => x.Matched);
            foreach (var record in matchedRecords)
            {
                var index = ThirdPartyFile.Records.IndexOf(record);
                consoleLines.Add(new ConsoleLine().AsSeparator(index));
                consoleLines.Add(record.ToConsole(index));
                consoleLines.Add(record.Match.ToConsole(index));
            }

            return consoleLines;
        }

        public void RemoveAutoMatch(int matchIndex)
        {
            try
            {
                UnmatchRecords(_autoMatches[matchIndex].SourceRecord, _autoMatches[matchIndex].Match.ActualRecords.ElementAt(0));
                _autoMatches[matchIndex].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void RemoveFinalMatch(int thirdPartyIndex)
        {
            try
            {
                UnmatchRecords(ThirdPartyFile.Records[thirdPartyIndex], ThirdPartyFile.Records[thirdPartyIndex].Match);
                ThirdPartyFile.Records[thirdPartyIndex].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void RemoveMultipleAutoMatches(List<int> matchIndices)
        {
            foreach (var matchIndex in matchIndices)
            {
                RemoveAutoMatch(matchIndex);
            }
        }

        public void RemoveMultipleFinalMatches(List<int> thirdPartyIndices)
        {
            foreach (var thirdPartyIndex in thirdPartyIndices)
            {
                RemoveFinalMatch(thirdPartyIndex);
            }
        }

        public List<TThirdPartyType> ThirdPartyRecords()
        {
            return ThirdPartyFile.Records;
        }

        public int NumThirdPartyRecords()
        {
            return ThirdPartyFile.Records.Count;
        }

        public int NumOwnedRecords()
        {
            return OwnedFile.Records.Count;
        }

        public int NumMatchedThirdPartyRecords()
        {
            return ThirdPartyFile.NumMatchedRecords();
        }

        public int NumMatchedOwnedRecords()
        {
            return OwnedFile.NumMatchedRecords();
        }

        public int NumUnmatchedThirdPartyRecords()
        {
            return ThirdPartyFile.NumUnmatchedRecords();
        }

        public int NumUnmatchedOwnedRecords()
        {
            return OwnedFile.NumUnmatchedRecords();
        }

        public List<string> UnmatchedThirdPartyRecords()
        {
            return ThirdPartyFile.UnmatchedRecordsAsCsv();
        }

        public List<string> UnmatchedOwnedRecords()
        {
            return OwnedFile.UnmatchedRecordsAsCsv();
        }

        public void MarkLatestMatchIndex(int matchIndex)
        {
            try
            {
                MatchRecords(_latestRecordForMatching.SourceRecord,
                    _latestRecordForMatching.Matches[matchIndex].ActualRecords[0]);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void MatchNonMatchingRecord(int matchIndex)
        {
            MarkLatestMatchIndex(matchIndex);

            ChangeAmountAndDescriptionToMatchThirdPartyRecord(
                _latestRecordForMatching.SourceRecord,
                _latestRecordForMatching.Matches[matchIndex].ActualRecords[0]);
        }

        public void DoAutoMatching()
        {
            ReturnAutoMatches();
        }
    }
}