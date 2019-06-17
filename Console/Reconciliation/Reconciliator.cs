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
            get => ThirdPartyDataFile.File;
            set => ThirdPartyDataFile.File = value;
        }
        public ICSVFile<TOwnedType> OwnedFile
        {
            get => OwnedDataFile.File;
            set => OwnedDataFile.File = value;
        }
        public IDataFile<TThirdPartyType> ThirdPartyDataFile { get; set; }
        public IDataFile<TOwnedType> OwnedDataFile { get; set; }
        private readonly string _worksheetName;
        private RecordForMatching<TThirdPartyType> _latestRecordForMatching = null;
        private List<AutoMatchedRecord<TThirdPartyType>> _autoMatches = null;
        private int _currentIndex = -1;
        private readonly List<string> _reservedWords = new List<string>{"AND","THE","OR","WITH"};

        public Reconciliator(
            ICSVFile<TThirdPartyType> thirdPartyCSVFile,
            ICSVFile<TOwnedType> ownedCSVFile,
            ThirdPartyFileLoadAction thirdPartyFileLoadAction,
            string worksheetName = "")
        {
            ThirdPartyDataFile = new GenericFile<TThirdPartyType>(thirdPartyCSVFile);
            OwnedDataFile = new GenericFile<TOwnedType>(ownedCSVFile);

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
            bool found_unmatched_third_party_record = false;

            while (NotAtEnd() && !found_unmatched_third_party_record)
            {
                _currentIndex++;
                TThirdPartyType source_record = ThirdPartyFile.Records[_currentIndex];
                var matches = FindLatestOrderedMatches(source_record, OwnedFile).ToList();

                if (!source_record.Matched && matches.Count > 0)
                {
                    _latestRecordForMatching = new RecordForMatching<TThirdPartyType>(
                        source_record,
                        matches);

                    found_unmatched_third_party_record = true;
                }
            }

            return found_unmatched_third_party_record;
        }

        public bool MoveToNextUnmatchedThirdPartyRecordForManualMatching()
        {
            bool found_unmatched_third_party_record = false;

            while (NotAtEnd() && !found_unmatched_third_party_record)
            {
                _currentIndex++;
                TThirdPartyType source_record = ThirdPartyFile.Records[_currentIndex];
                var unmatched_owned_records = CurrentUnmatchedOwnedRecords(source_record).ToList();

                if (!source_record.Matched && unmatched_owned_records.Count > 0)
                {
                    _latestRecordForMatching = new RecordForMatching<TThirdPartyType>(
                        source_record,
                        unmatched_owned_records);

                    found_unmatched_third_party_record = true;
                }
            }

            return found_unmatched_third_party_record;
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
            var source_description_transformed = sourceDescription.RemovePunctuation().ToUpper();
            var target_description_transformed = targetDescription.RemovePunctuation().ToUpper();
            return (target_description_transformed == source_description_transformed)
                || target_description_transformed.Contains(source_description_transformed);
        }

        public bool CheckForPartialTextMatch(string sourceDescription, string targetDescription)
        {
            bool found_partial_match = false;
            var source_words = sourceDescription
                .ReplacePunctuationWithSpaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var target_words = targetDescription
                .ReplacePunctuationWithSpaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            int source_count = 0;
            while (!found_partial_match && source_count < source_words.Count)
            {
                var source_word = source_words[source_count];
                if (source_word.Length > 1)
                {
                    int target_count = 0;
                    while (!found_partial_match && target_count < target_words.Count)
                    {
                        var target_word = target_words[target_count];
                        if ((target_word == source_word || target_word.StartsWith(source_word)) && !_reservedWords.Contains(target_word))
                        {
                            found_partial_match = true;
                        }
                        target_count++;
                    }
                }
                source_count++;
            }
            return found_partial_match;
        }

        private IEnumerable<IPotentialMatch> CurrentUnmatchedOwnedRecords(TThirdPartyType sourceRecord)
        {
            var unmatched_owned_records = OwnedFile
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

            return unmatched_owned_records;
        }

        private Rankings GetRankings(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            var amount_ranking = GetAmountRanking(candidateRecord, masterRecord);
            var date_ranking = GetDateRanking(candidateRecord, masterRecord);
            return new Rankings
            {
                Amount = amount_ranking,
                Date = date_ranking,
                Combined = Math.Min(amount_ranking, date_ranking)
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
            ThirdPartyDataFile.RefreshFileContents();
            OwnedDataFile.RefreshFileContents();
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
            foreach (var matched_owned_record in OwnedFile.Records.Where(x => x.Matched))
            {
                matched_owned_record.Reconcile();
            }
        }

        private void AddUnmatchedSourceItemsToOwnedFile()
        {
            foreach (var unmatched_source in ThirdPartyFile.Records.Where(x => !x.Matched))
            {
                var new_owned_record = new TOwnedType();
                new_owned_record.CreateFromMatch(
                    unmatched_source.Date,
                    unmatched_source.MainAmount(),
                    unmatched_source.TransactionType(),
                    "!! Unmatched from 3rd party: " + unmatched_source.Description,
                    unmatched_source.ExtraInfo(),
                    unmatched_source);
                OwnedFile.Records.Add(new_owned_record);
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
                foreach (var console_line in item.ConsoleLines)
                {
                    console_line.Index = index;
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
                    foreach (var actual_record in _latestRecordForMatching.Matches[specifiedIndex].ActualRecords)
                    {
                        if (actual_record.Matched)
                        {
                            throw new Exception(CannotDeleteMatchedOwnedRecord);
                        }
                        else
                        {
                            OwnedFile.Records.Remove((TOwnedType)(actual_record));
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

            foreach (var third_party_record in ThirdPartyFile.Records)
            {
                var single_match = FindSingleMatchByAmountAndTextAndNearDate(third_party_record);
                if (null != single_match)
                {
                    var new_record_for_matching = new AutoMatchedRecord<TThirdPartyType>(
                        third_party_record,
                        FindSingleMatchByAmountAndTextAndNearDate(third_party_record),
                        index);

                    MatchRecords(third_party_record, single_match.ActualRecords.ElementAt(0));

                    _autoMatches.Add(new_record_for_matching);
                    index++;
                }
            }

            return _autoMatches;
        }

        private IPotentialMatch FindSingleMatchByAmountAndTextAndNearDate(TThirdPartyType sourceRecord)
        {
            var all_matches = ConvertUnmatchedOwnedRecordsToPotentialMatches(sourceRecord, OwnedFile)
                .Where(b => 
                    (b.FullTextMatch || b.PartialTextMatch)
                    && b.Rankings.Amount == 0 
                    && b.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold);

            return SingleMatchOnly(all_matches);
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
            List<ConsoleLine> console_lines = new List<ConsoleLine>();

            var auto_matches_with_matched_items_only = _autoMatches.Where(x => x.Match != null);
            foreach (var auto_match in auto_matches_with_matched_items_only)
            {
                console_lines.Add(new ConsoleLine().AsSeparator(auto_match.Index));
                console_lines.Add(auto_match.SourceRecord.ToConsole(auto_match.Index));
                console_lines.Add(auto_match.Match.ActualRecords.ElementAt(0).ToConsole(auto_match.Index));
            }

            return console_lines;
        }

        public List<ConsoleLine> GetFinalMatchesForConsole()
        {
            List<ConsoleLine> console_lines = new List<ConsoleLine>();

            var matched_records = ThirdPartyFile.Records.Where(x => x.Matched);
            foreach (var record in matched_records)
            {
                var index = ThirdPartyFile.Records.IndexOf(record);
                console_lines.Add(new ConsoleLine().AsSeparator(index));
                console_lines.Add(record.ToConsole(index));
                console_lines.Add(record.Match.ToConsole(index));
            }

            return console_lines;
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
            foreach (var match_index in matchIndices)
            {
                RemoveAutoMatch(match_index);
            }
        }

        public void RemoveMultipleFinalMatches(List<int> thirdPartyIndices)
        {
            foreach (var third_party_index in thirdPartyIndices)
            {
                RemoveFinalMatch(third_party_index);
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