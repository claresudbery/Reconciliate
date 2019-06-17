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

        public ICSVFile<TThirdPartyType> Third_party_file
        {
            get => Third_party_data_file.File;
            set => Third_party_data_file.File = value;
        }
        public ICSVFile<TOwnedType> Owned_file
        {
            get => Owned_data_file.File;
            set => Owned_data_file.File = value;
        }
        public IDataFile<TThirdPartyType> Third_party_data_file { get; set; }
        public IDataFile<TOwnedType> Owned_data_file { get; set; }
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
            Third_party_data_file = new GenericFile<TThirdPartyType>(thirdPartyCSVFile);
            Owned_data_file = new GenericFile<TOwnedType>(ownedCSVFile);

            Perform_third_party_file_load_action(thirdPartyFileLoadAction, thirdPartyCSVFile);

            _worksheetName = worksheetName;

            Reset();
        }

        private void Perform_third_party_file_load_action<TThirdPartyType>(
                ThirdPartyFileLoadAction thirdPartyFileLoadAction,
                ICSVFile<TThirdPartyType> thirdPartyCSVFile)
            where TThirdPartyType : ICSVRecord, new()
        {
            switch (thirdPartyFileLoadAction)
            {
                case ThirdPartyFileLoadAction.FilterForPositiveRecordsOnly:
                    thirdPartyCSVFile.Filter_for_positive_records_only();
                    break;
                case ThirdPartyFileLoadAction.FilterForNegativeRecordsOnly:
                    thirdPartyCSVFile.Filter_for_negative_records_only();
                    break;
                case ThirdPartyFileLoadAction.SwapSignsOfAllAmounts:
                    thirdPartyCSVFile.Swap_signs_of_all_amounts();
                    break;
                case ThirdPartyFileLoadAction.NoAction:
                default: break;
            }
        }

        public bool Find_reconciliation_matches_for_next_third_party_record()
        {
            bool found_unmatched_third_party_record = false;

            while (Not_at_end() && !found_unmatched_third_party_record)
            {
                _currentIndex++;
                TThirdPartyType source_record = Third_party_file.Records[_currentIndex];
                var matches = Find_latest_ordered_matches(source_record, Owned_file).ToList();

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

        public bool Move_to_next_unmatched_third_party_record_for_manual_matching()
        {
            bool found_unmatched_third_party_record = false;

            while (Not_at_end() && !found_unmatched_third_party_record)
            {
                _currentIndex++;
                TThirdPartyType source_record = Third_party_file.Records[_currentIndex];
                var unmatched_owned_records = Current_unmatched_owned_records(source_record).ToList();

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

        private double Get_date_ranking(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            return candidateRecord.Date.Proximity_score(masterRecord.Date);
        }

        private double Get_amount_ranking(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            return candidateRecord.Main_amount().Proximity_score(masterRecord.Main_amount());
        }

        private IEnumerable<IPotentialMatch> Find_latest_ordered_matches(
            TThirdPartyType sourceRecord,
            ICSVFile<TOwnedType> ownedFile)
        {
            return Convert_unmatched_owned_records_to_potential_matches(sourceRecord, ownedFile)
                .Where(b => b.Full_text_match 
                    || b.Partial_text_match 
                    || b.Rankings.Amount <= PotentialMatch.PartialAmountMatchThreshold)
                .OrderByDescending(c => c.Amount_match)
                .ThenByDescending(c => c.Full_text_match)
                .ThenByDescending(c => c.Partial_text_match)
                .ThenBy(c => c.Rankings.Date)
                .ThenBy(c => c.Rankings.Amount);
        }

        private IEnumerable<IPotentialMatch> Convert_unmatched_owned_records_to_potential_matches(
            TThirdPartyType sourceRecord,
            ICSVFile<TOwnedType> ownedFile)
        {
            return ownedFile
                .Records
                .Where(x => x.Matched == false)
                .Select(ownedRecord => new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>{ownedRecord},
                    Amount_match = ownedRecord.Main_amount() == sourceRecord.Main_amount(),
                    Full_text_match = Check_for_full_text_match(sourceRecord.Description, ownedRecord.Description),
                    Partial_text_match = Check_for_partial_text_match(sourceRecord.Description, ownedRecord.Description),
                    Rankings = new Rankings
                    {
                        Amount = Get_amount_ranking(ownedRecord, sourceRecord),
                        Date = Get_date_ranking(ownedRecord, sourceRecord)
                    },
                    Console_lines = new List<ConsoleLine> { ownedRecord.To_console() }
                });
        }

        private bool Check_for_full_text_match(string sourceDescription, string targetDescription)
        {
            var source_description_transformed = sourceDescription.Remove_punctuation().ToUpper();
            var target_description_transformed = targetDescription.Remove_punctuation().ToUpper();
            return (target_description_transformed == source_description_transformed)
                || target_description_transformed.Contains(source_description_transformed);
        }

        public bool Check_for_partial_text_match(string sourceDescription, string targetDescription)
        {
            bool found_partial_match = false;
            var source_words = sourceDescription
                .Replace_punctuation_with_spaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var target_words = targetDescription
                .Replace_punctuation_with_spaces()
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

        private IEnumerable<IPotentialMatch> Current_unmatched_owned_records(TThirdPartyType sourceRecord)
        {
            var unmatched_owned_records = Owned_file
                .Records
                .Where(x => x.Matched == false)
                .Select( ownedRecord => new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord> { ownedRecord },
                    Amount_match = ownedRecord.Main_amount() == sourceRecord.Main_amount(),
                    Full_text_match = Check_for_full_text_match(sourceRecord.Description, ownedRecord.Description),
                    Partial_text_match = Check_for_partial_text_match(sourceRecord.Description, ownedRecord.Description),
                    Rankings = Get_rankings(ownedRecord, sourceRecord),
                    Console_lines = new List<ConsoleLine> { ownedRecord.To_console() }
                })
                .OrderBy(c => c.Rankings.Combined);

            return unmatched_owned_records;
        }

        private Rankings Get_rankings(TOwnedType candidateRecord, TThirdPartyType masterRecord)
        {
            var amount_ranking = Get_amount_ranking(candidateRecord, masterRecord);
            var date_ranking = Get_date_ranking(candidateRecord, masterRecord);
            return new Rankings
            {
                Amount = amount_ranking,
                Date = date_ranking,
                Combined = Math.Min(amount_ranking, date_ranking)
            };
        }

        public void Match_current_record(int matchIndex)
        {
            Match_specified_records(_latestRecordForMatching, matchIndex, Owned_file);
        }

        private void Match_specified_records(
            RecordForMatching<TThirdPartyType> recordForMatching, 
            int matchIndex,
            ICSVFile<TOwnedType> ownedFile)
        { 
            try
            {
                Match_records(recordForMatching.SourceRecord,
                    recordForMatching.Matches[matchIndex].Actual_records.ElementAt(0));

                if (recordForMatching.SourceRecord.Main_amount() !=
                    recordForMatching.SourceRecord.Match.Main_amount())
                {
                    Change_amount_and_description_to_match_third_party_record(
                        recordForMatching.SourceRecord,
                        recordForMatching.Matches[matchIndex].Actual_records.ElementAt(0));
                }
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        private void Match_records(TThirdPartyType source, ICSVRecord match)
        {
            source.Matched = true;
            source.Match = match;

            match.Matched = true;
            match.Match = source;
        }

        private void Unmatch_records(TThirdPartyType source, ICSVRecord match)
        {
            source.Matched = false;
            source.Match = null;

            match.Matched = false;
            match.Match = null;
        }

        private void Change_amount_and_description_to_match_third_party_record(TThirdPartyType sourceRecord, ICSVRecord matchedRecord)
        {
            matchedRecord.Description = matchedRecord.Description 
                                        + ReconConsts.OriginalAmountWas
                                        + string.Format(StringHelper.Culture(), "{0:C}", matchedRecord.Main_amount());

            matchedRecord.Change_main_amount(sourceRecord.Main_amount());
        }

        public void Reset()
        {
            _currentIndex = -1;
            Third_party_file.Reset_all_matches();
            Owned_file.Reset_all_matches();
        }

        public void Refresh_files()
        {
            Third_party_data_file.Refresh_file_contents();
            Owned_data_file.Refresh_file_contents();
        }

        public void Rewind()
        {
            _currentIndex = -1;
        }

        public void Finish(string fileSuffix)
        {
            Add_unmatched_source_items_to_owned_file();
            Reconcile_all_matched_items();
            
            Owned_file.Write_to_csv_file(fileSuffix);
            if ("" != _worksheetName)
            {
                Owned_file.Write_back_to_main_spreadsheet(_worksheetName);
            }
        }

        private void Reconcile_all_matched_items()
        {
            foreach (var matched_owned_record in Owned_file.Records.Where(x => x.Matched))
            {
                matched_owned_record.Reconcile();
            }
        }

        private void Add_unmatched_source_items_to_owned_file()
        {
            foreach (var unmatched_source in Third_party_file.Records.Where(x => !x.Matched))
            {
                var new_owned_record = new TOwnedType();
                new_owned_record.Create_from_match(
                    unmatched_source.Date,
                    unmatched_source.Main_amount(),
                    unmatched_source.Transaction_type(),
                    "!! Unmatched from 3rd party: " + unmatched_source.Description,
                    unmatched_source.Extra_info(),
                    unmatched_source);
                Owned_file.Records.Add(new_owned_record);
            }
        }

        public bool Not_at_end()
        {
            return _currentIndex < (Third_party_file.Records.Count - 1);
        }

        public List<IPotentialMatch> Current_potential_matches()
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
                foreach (var console_line in item.Console_lines)
                {
                    console_line.Index = index;
                }
                index++;
            }

            return sourceList;
        }

        public string Current_source_record_as_string()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.To_csv()
                : "No record currently stored";
        }

        public ConsoleLine Current_source_record_as_console_line()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.To_console()
                : new ConsoleLine();
        }

        public string Current_source_description()
        {
            return _latestRecordForMatching != null
                ? _latestRecordForMatching.SourceRecord.Description
                : "No current record";
        }

        public RecordForMatching<TThirdPartyType> Current_record_for_matching()
        {
            return _latestRecordForMatching;
        }

        public List<TOwnedType> Owned_file_records()
        {
            return Owned_file.Records;
        }

        public void Delete_current_third_party_record()
        {
            try
            {
                if (Third_party_file.Records[_currentIndex].Matched)
                {
                    Third_party_file.Records[_currentIndex].Match.Matched = false;
                    Third_party_file.Records[_currentIndex].Match.Match = null;
                }
                Third_party_file.Records.RemoveAt(_currentIndex);
                _currentIndex--;
                _latestRecordForMatching = null;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception(CannotDeleteCurrentRecordNoMatching);
            }
        }

        public void Delete_specific_third_party_record(int specifiedIndex)
        {
            try
            {
                if (Third_party_file.Records[specifiedIndex].Matched)
                {
                    Third_party_file.Records[specifiedIndex].Match.Matched = false;
                    Third_party_file.Records[specifiedIndex].Match.Match = null;
                }
                Third_party_file.Records.RemoveAt(specifiedIndex);
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

        public void Delete_specific_owned_record_from_list_of_matches(int specifiedIndex)
        {
            if (_latestRecordForMatching == null)
            {
                throw new Exception(CannotDeleteMatchedRecordNoMatching);
            }
            else
            {
                try
                {
                    foreach (var actual_record in _latestRecordForMatching.Matches[specifiedIndex].Actual_records)
                    {
                        if (actual_record.Matched)
                        {
                            throw new Exception(CannotDeleteMatchedOwnedRecord);
                        }
                        else
                        {
                            Owned_file.Records.Remove((TOwnedType)(actual_record));
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

        public int Num_potential_matches()
        {
            return _latestRecordForMatching != null ? 
                _latestRecordForMatching.Matches.Count
                : 0;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> Return_auto_matches()
        {
            _autoMatches = new List<AutoMatchedRecord<TThirdPartyType>>();
            var index = 0;

            foreach (var third_party_record in Third_party_file.Records)
            {
                var single_match = Find_single_match_by_amount_and_text_and_near_date(third_party_record);
                if (null != single_match)
                {
                    var new_record_for_matching = new AutoMatchedRecord<TThirdPartyType>(
                        third_party_record,
                        Find_single_match_by_amount_and_text_and_near_date(third_party_record),
                        index);

                    Match_records(third_party_record, single_match.Actual_records.ElementAt(0));

                    _autoMatches.Add(new_record_for_matching);
                    index++;
                }
            }

            return _autoMatches;
        }

        private IPotentialMatch Find_single_match_by_amount_and_text_and_near_date(TThirdPartyType sourceRecord)
        {
            var all_matches = Convert_unmatched_owned_records_to_potential_matches(sourceRecord, Owned_file)
                .Where(b => 
                    (b.Full_text_match || b.Partial_text_match)
                    && b.Rankings.Amount == 0 
                    && b.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold);

            return Single_match_only(all_matches);
        }

        private IPotentialMatch Single_match_only(IEnumerable<IPotentialMatch> allMatches)
        {
            return allMatches.Count() == 1
                ? allMatches.ToList()[0]
                : null;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> Get_auto_matches()
        {
            return _autoMatches;
        }

        public List<ConsoleLine> Get_auto_matches_for_console()
        {
            List<ConsoleLine> console_lines = new List<ConsoleLine>();

            var auto_matches_with_matched_items_only = _autoMatches.Where(x => x.Match != null);
            foreach (var auto_match in auto_matches_with_matched_items_only)
            {
                console_lines.Add(new ConsoleLine().As_separator(auto_match.Index));
                console_lines.Add(auto_match.SourceRecord.To_console(auto_match.Index));
                console_lines.Add(auto_match.Match.Actual_records.ElementAt(0).To_console(auto_match.Index));
            }

            return console_lines;
        }

        public List<ConsoleLine> Get_final_matches_for_console()
        {
            List<ConsoleLine> console_lines = new List<ConsoleLine>();

            var matched_records = Third_party_file.Records.Where(x => x.Matched);
            foreach (var record in matched_records)
            {
                var index = Third_party_file.Records.IndexOf(record);
                console_lines.Add(new ConsoleLine().As_separator(index));
                console_lines.Add(record.To_console(index));
                console_lines.Add(record.Match.To_console(index));
            }

            return console_lines;
        }

        public void Remove_auto_match(int matchIndex)
        {
            try
            {
                Unmatch_records(_autoMatches[matchIndex].SourceRecord, _autoMatches[matchIndex].Match.Actual_records.ElementAt(0));
                _autoMatches[matchIndex].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Remove_final_match(int thirdPartyIndex)
        {
            try
            {
                Unmatch_records(Third_party_file.Records[thirdPartyIndex], Third_party_file.Records[thirdPartyIndex].Match);
                Third_party_file.Records[thirdPartyIndex].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Remove_multiple_auto_matches(List<int> matchIndices)
        {
            foreach (var match_index in matchIndices)
            {
                Remove_auto_match(match_index);
            }
        }

        public void Remove_multiple_final_matches(List<int> thirdPartyIndices)
        {
            foreach (var third_party_index in thirdPartyIndices)
            {
                Remove_final_match(third_party_index);
            }
        }

        public List<TThirdPartyType> Third_party_records()
        {
            return Third_party_file.Records;
        }

        public int Num_third_party_records()
        {
            return Third_party_file.Records.Count;
        }

        public int Num_owned_records()
        {
            return Owned_file.Records.Count;
        }

        public int Num_matched_third_party_records()
        {
            return Third_party_file.Num_matched_records();
        }

        public int Num_matched_owned_records()
        {
            return Owned_file.Num_matched_records();
        }

        public int Num_unmatched_third_party_records()
        {
            return Third_party_file.Num_unmatched_records();
        }

        public int Num_unmatched_owned_records()
        {
            return Owned_file.Num_unmatched_records();
        }

        public List<string> Unmatched_third_party_records()
        {
            return Third_party_file.Unmatched_records_as_csv();
        }

        public List<string> Unmatched_owned_records()
        {
            return Owned_file.Unmatched_records_as_csv();
        }

        public void Mark_latest_match_index(int matchIndex)
        {
            try
            {
                Match_records(_latestRecordForMatching.SourceRecord,
                    _latestRecordForMatching.Matches[matchIndex].Actual_records[0]);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Match_non_matching_record(int matchIndex)
        {
            Mark_latest_match_index(matchIndex);

            Change_amount_and_description_to_match_third_party_record(
                _latestRecordForMatching.SourceRecord,
                _latestRecordForMatching.Matches[matchIndex].Actual_records[0]);
        }

        public void Do_auto_matching()
        {
            Return_auto_matches();
        }
    }
}