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
        private readonly string _worksheet_name;
        private RecordForMatching<TThirdPartyType> _latest_record_for_matching = null;
        private List<AutoMatchedRecord<TThirdPartyType>> _auto_matches = null;
        private int _current_index = -1;
        private readonly List<string> _reserved_words = new List<string>{"AND","THE","OR","WITH"};

        public Reconciliator(
            IFileIO<TThirdPartyType> third_party_file_io,
            IFileIO<TOwnedType> owned_file_io,
            DataLoadingInformation loading_info)
        {
            _worksheet_name = loading_info.Sheet_name;

            var third_party_file = new CSVFile<TThirdPartyType>(third_party_file_io);
            third_party_file.Load();

            var owned_file = new CSVFile<TOwnedType>(owned_file_io);
            owned_file.Load();
            
            Initialise(
                third_party_file,
                owned_file,
                loading_info.Third_party_file_load_action);
        }

        public Reconciliator(
            ICSVFile<TThirdPartyType> third_party_csv_file,
            ICSVFile<TOwnedType> owned_csv_file,
            ThirdPartyFileLoadAction third_party_file_load_action,
            string worksheet_name = "")
        {
            _worksheet_name = worksheet_name;

            Initialise(
                third_party_csv_file,
                owned_csv_file,
                third_party_file_load_action);
        }

        private void Initialise(
            ICSVFile<TThirdPartyType> third_party_csv_file,
            ICSVFile<TOwnedType> owned_csv_file,
            ThirdPartyFileLoadAction third_party_file_load_action)
        {
            Third_party_data_file = new GenericFile<TThirdPartyType>(third_party_csv_file);
            Owned_data_file = new GenericFile<TOwnedType>(owned_csv_file);

            Perform_third_party_file_load_action(third_party_file_load_action, third_party_csv_file);

            Reset();
        }

        private void Perform_third_party_file_load_action<TThirdPartyType>(
                ThirdPartyFileLoadAction third_party_file_load_action,
                ICSVFile<TThirdPartyType> third_party_csv_file)
            where TThirdPartyType : ICSVRecord, new()
        {
            switch (third_party_file_load_action)
            {
                case ThirdPartyFileLoadAction.FilterForPositiveRecordsOnly:
                    third_party_csv_file.Filter_for_positive_records_only();
                    break;
                case ThirdPartyFileLoadAction.FilterForNegativeRecordsOnly:
                    third_party_csv_file.Filter_for_negative_records_only();
                    break;
                case ThirdPartyFileLoadAction.SwapSignsOfAllAmounts:
                    third_party_csv_file.Swap_signs_of_all_amounts();
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
                _current_index++;
                TThirdPartyType source_record = Third_party_file.Records[_current_index];
                var matches = Find_latest_ordered_matches(source_record, Owned_file).ToList();

                if (!source_record.Matched && matches.Count > 0)
                {
                    _latest_record_for_matching = new RecordForMatching<TThirdPartyType>(
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
                _current_index++;
                TThirdPartyType source_record = Third_party_file.Records[_current_index];
                var unmatched_owned_records = Current_unmatched_owned_records(source_record).ToList();

                if (!source_record.Matched && unmatched_owned_records.Count > 0)
                {
                    _latest_record_for_matching = new RecordForMatching<TThirdPartyType>(
                        source_record,
                        unmatched_owned_records);

                    found_unmatched_third_party_record = true;
                }
            }

            return found_unmatched_third_party_record;
        }

        private double Get_date_ranking(TOwnedType candidate_record, TThirdPartyType master_record)
        {
            return candidate_record.Date.Proximity_score(master_record.Date);
        }

        private double Get_amount_ranking(TOwnedType candidate_record, TThirdPartyType master_record)
        {
            return candidate_record.Main_amount().Proximity_score(master_record.Main_amount());
        }

        private IEnumerable<IPotentialMatch> Find_latest_ordered_matches(
            TThirdPartyType source_record,
            ICSVFile<TOwnedType> owned_file)
        {
            return Convert_unmatched_owned_records_to_potential_matches(source_record, owned_file)
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
            TThirdPartyType source_record,
            ICSVFile<TOwnedType> owned_file)
        {
            return owned_file
                .Records
                .Where(x => x.Matched == false)
                .Select(owned_record => new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>{owned_record},
                    Amount_match = owned_record.Main_amount() == source_record.Main_amount(),
                    Full_text_match = Check_for_full_text_match(source_record.Description, owned_record.Description),
                    Partial_text_match = Check_for_partial_text_match(source_record.Description, owned_record.Description),
                    Rankings = new Rankings
                    {
                        Amount = Get_amount_ranking(owned_record, source_record),
                        Date = Get_date_ranking(owned_record, source_record)
                    },
                    Console_lines = new List<ConsoleLine> { owned_record.To_console() }
                });
        }

        private bool Check_for_full_text_match(string source_description, string target_description)
        {
            var source_description_transformed = source_description.Remove_punctuation().ToUpper();
            var target_description_transformed = target_description.Remove_punctuation().ToUpper();
            return (target_description_transformed == source_description_transformed)
                || target_description_transformed.Contains(source_description_transformed);
        }

        public bool Check_for_partial_text_match(string source_description, string target_description)
        {
            bool found_partial_match = false;
            var source_words = source_description
                .Replace_punctuation_with_spaces()
                .ToUpper()
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var target_words = target_description
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
                        if ((target_word == source_word || target_word.StartsWith(source_word)) && !_reserved_words.Contains(target_word))
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

        private IEnumerable<IPotentialMatch> Current_unmatched_owned_records(TThirdPartyType source_record)
        {
            var unmatched_owned_records = Owned_file
                .Records
                .Where(x => x.Matched == false)
                .Select( owned_record => new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord> { owned_record },
                    Amount_match = owned_record.Main_amount() == source_record.Main_amount(),
                    Full_text_match = Check_for_full_text_match(source_record.Description, owned_record.Description),
                    Partial_text_match = Check_for_partial_text_match(source_record.Description, owned_record.Description),
                    Rankings = Get_rankings(owned_record, source_record),
                    Console_lines = new List<ConsoleLine> { owned_record.To_console() }
                })
                .OrderBy(c => c.Rankings.Combined);

            return unmatched_owned_records;
        }

        private Rankings Get_rankings(TOwnedType candidate_record, TThirdPartyType master_record)
        {
            var amount_ranking = Get_amount_ranking(candidate_record, master_record);
            var date_ranking = Get_date_ranking(candidate_record, master_record);
            return new Rankings
            {
                Amount = amount_ranking,
                Date = date_ranking,
                Combined = Math.Min(amount_ranking, date_ranking)
            };
        }

        public void Match_current_record(int match_index)
        {
            Match_specified_records(_latest_record_for_matching, match_index, Owned_file);
        }

        private void Match_specified_records(
            RecordForMatching<TThirdPartyType> record_for_matching, 
            int match_index,
            ICSVFile<TOwnedType> owned_file)
        { 
            try
            {
                Match_records(record_for_matching.SourceRecord,
                    record_for_matching.Matches[match_index].Actual_records.ElementAt(0));

                if (record_for_matching.SourceRecord.Main_amount() !=
                    record_for_matching.SourceRecord.Match.Main_amount())
                {
                    Change_amount_and_description_to_match_third_party_record(
                        record_for_matching.SourceRecord,
                        record_for_matching.Matches[match_index].Actual_records.ElementAt(0));
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

        private void Change_amount_and_description_to_match_third_party_record(TThirdPartyType source_record, ICSVRecord matched_record)
        {
            matched_record.Description = matched_record.Description 
                                        + ReconConsts.OriginalAmountWas
                                        + string.Format(StringHelper.Culture(), "{0:C}", matched_record.Main_amount());

            matched_record.Change_main_amount(source_record.Main_amount());
        }

        public void Reset()
        {
            _current_index = -1;
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
            _current_index = -1;
        }

        public void Finish(string file_suffix)
        {
            Add_unmatched_source_items_to_owned_file();
            Reconcile_all_matched_items();
            
            Owned_file.Write_to_csv_file(file_suffix);
            if ("" != _worksheet_name)
            {
                Owned_file.Write_back_to_main_spreadsheet(_worksheet_name);
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
            return _current_index < (Third_party_file.Records.Count - 1);
        }

        public List<IPotentialMatch> Current_potential_matches()
        {
            // !! Don't re-order items here !! If you do, they will be displayed in a different order than they are stored.
            // This will mean that if a user selects Item with index 4, it might actually be stored with index 9, 
            // and the wrong record will be matched.
            // Instead, if you want to change the ordering, look at CurrentUnmatchedOwnedRecords() and FindLatestOrderedMatches()
            return _latest_record_for_matching != null 
                ? Indexed(_latest_record_for_matching.Matches)
                : null;
        }

        public static List<IPotentialMatch> Indexed(List<IPotentialMatch> source_list)
        {
            int index = 0;
            foreach (var item in source_list)
            {
                foreach (var console_line in item.Console_lines)
                {
                    console_line.Index = index;
                }
                index++;
            }

            return source_list;
        }

        public string Current_source_record_as_string()
        {
            return _latest_record_for_matching != null
                ? _latest_record_for_matching.SourceRecord.To_csv()
                : "No record currently stored";
        }

        public ConsoleLine Current_source_record_as_console_line()
        {
            return _latest_record_for_matching != null
                ? _latest_record_for_matching.SourceRecord.To_console()
                : new ConsoleLine();
        }

        public string Current_source_description()
        {
            return _latest_record_for_matching != null
                ? _latest_record_for_matching.SourceRecord.Description
                : "No current record";
        }

        public RecordForMatching<TThirdPartyType> Current_record_for_matching()
        {
            return _latest_record_for_matching;
        }

        public List<TOwnedType> Owned_file_records()
        {
            return Owned_file.Records;
        }

        public void Delete_current_third_party_record()
        {
            try
            {
                if (Third_party_file.Records[_current_index].Matched)
                {
                    Third_party_file.Records[_current_index].Match.Matched = false;
                    Third_party_file.Records[_current_index].Match.Match = null;
                }
                Third_party_file.Records.RemoveAt(_current_index);
                _current_index--;
                _latest_record_for_matching = null;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception(CannotDeleteCurrentRecordNoMatching);
            }
        }

        public void Delete_specific_third_party_record(int specified_index)
        {
            try
            {
                if (Third_party_file.Records[specified_index].Matched)
                {
                    Third_party_file.Records[specified_index].Match.Matched = false;
                    Third_party_file.Records[specified_index].Match.Match = null;
                }
                Third_party_file.Records.RemoveAt(specified_index);
                if (specified_index == _current_index)
                {
                    _current_index--;
                    _latest_record_for_matching = null;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception(CannotDeleteThirdPartyRecordDoesNotExist);
            }
        }

        public void Delete_specific_owned_record_from_list_of_matches(int specified_index)
        {
            if (_latest_record_for_matching == null)
            {
                throw new Exception(CannotDeleteMatchedRecordNoMatching);
            }
            else
            {
                try
                {
                    foreach (var actual_record in _latest_record_for_matching.Matches[specified_index].Actual_records)
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
                    _latest_record_for_matching.Matches.RemoveAt(specified_index);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new ArgumentOutOfRangeException(CannotDeleteOwnedRecordDoesNotExist, exception);
                }
            }
        }

        public int Num_potential_matches()
        {
            return _latest_record_for_matching != null ? 
                _latest_record_for_matching.Matches.Count
                : 0;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> Return_auto_matches()
        {
            _auto_matches = new List<AutoMatchedRecord<TThirdPartyType>>();
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

                    _auto_matches.Add(new_record_for_matching);
                    index++;
                }
            }

            return _auto_matches;
        }

        private IPotentialMatch Find_single_match_by_amount_and_text_and_near_date(TThirdPartyType source_record)
        {
            var all_matches = Convert_unmatched_owned_records_to_potential_matches(source_record, Owned_file)
                .Where(b => 
                    (b.Full_text_match || b.Partial_text_match)
                    && b.Rankings.Amount == 0 
                    && b.Rankings.Date <= PotentialMatch.PartialDateMatchThreshold);

            return Single_match_only(all_matches);
        }

        private IPotentialMatch Single_match_only(IEnumerable<IPotentialMatch> all_matches)
        {
            return all_matches.Count() == 1
                ? all_matches.ToList()[0]
                : null;
        }

        public List<AutoMatchedRecord<TThirdPartyType>> Get_auto_matches()
        {
            return _auto_matches;
        }

        public List<ConsoleLine> Get_auto_matches_for_console()
        {
            List<ConsoleLine> console_lines = new List<ConsoleLine>();

            var auto_matches_with_matched_items_only = _auto_matches.Where(x => x.Match != null);
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

        public void Remove_auto_match(int match_index)
        {
            try
            {
                Unmatch_records(_auto_matches[match_index].SourceRecord, _auto_matches[match_index].Match.Actual_records.ElementAt(0));
                _auto_matches[match_index].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Remove_final_match(int third_party_index)
        {
            try
            {
                Unmatch_records(Third_party_file.Records[third_party_index], Third_party_file.Records[third_party_index].Match);
                Third_party_file.Records[third_party_index].Match = null;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Remove_multiple_auto_matches(List<int> match_indices)
        {
            foreach (var match_index in match_indices)
            {
                Remove_auto_match(match_index);
            }
        }

        public void Remove_multiple_final_matches(List<int> third_party_indices)
        {
            foreach (var third_party_index in third_party_indices)
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

        public void Mark_latest_match_index(int match_index)
        {
            try
            {
                Match_records(_latest_record_for_matching.SourceRecord,
                    _latest_record_for_matching.Matches[match_index].Actual_records[0]);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException(BadMatchNumber, exception);
            }
        }

        public void Match_non_matching_record(int match_index)
        {
            Mark_latest_match_index(match_index);

            Change_amount_and_description_to_match_third_party_record(
                _latest_record_for_matching.SourceRecord,
                _latest_record_for_matching.Matches[match_index].Actual_records[0]);
        }

        public void Do_auto_matching()
        {
            Return_auto_matches();
        }
    }
}