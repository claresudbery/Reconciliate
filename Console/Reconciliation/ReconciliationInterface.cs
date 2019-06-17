using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation
{
    internal class ReconciliationInterface
    {
        private readonly IInputOutput _input_output;

        public IReconciliator Reconciliator { get; private set; }
        public string Third_party_descriptor { get; private set; }
        public string Owned_file_descriptor { get; private set; }

        private bool _doing_semi_automatic_matching = false;

        public ReconciliationInterface(
            IInputOutput input_output,
            IReconciliator reconciliator,
            string third_party_descriptor,
            string owned_file_descriptor)
        {
            _input_output = input_output;
            Reconciliator = reconciliator;
            Third_party_descriptor = third_party_descriptor;
            Owned_file_descriptor = owned_file_descriptor;
        }

        public void Do_the_matching()
        {
            Do_automatic_matching();
            
            Do_semi_automatic_matching();
            Show_final_results_of_matching();
            Proceed_after_showing_matching_results();
        }

        public void Do_matching_for_non_matching_records()
        {
            Do_manual_matching();
            Show_final_results_of_matching();
            Proceed_after_showing_matching_results();
        }

        private void Do_automatic_matching()
        {
            Reconciliator.Do_auto_matching();
            Recursively_show_auto_matches_and_get_choices();
        }

        public void Do_semi_automatic_matching()
        {
            _doing_semi_automatic_matching = true;
            bool unmatched_record_found = Reconciliator.Find_reconciliation_matches_for_next_third_party_record();

            while (unmatched_record_found)
            {
                Show_current_record_and_semi_auto_matches();
                Get_and_record_choice_of_matching();
                unmatched_record_found = Reconciliator.Find_reconciliation_matches_for_next_third_party_record();
            }
        }

        private void Do_manual_matching()
        {
            _doing_semi_automatic_matching = false;
            bool unmatched_record_found = Reconciliator.Move_to_next_unmatched_third_party_record_for_manual_matching();

            while (unmatched_record_found)
            {
                Show_current_record_and_manual_matches();
                Get_and_record_choice_of_matching();
                unmatched_record_found = Reconciliator.Move_to_next_unmatched_third_party_record_for_manual_matching();
            }
        }

        private void Recursively_show_auto_matches_and_get_choices()
        {
            List<ConsoleLine> auto_matches = Reconciliator.Get_auto_matches_for_console();
            if (auto_matches.Count > 0)
            {
                Show_auto_matches(auto_matches);
                Act_on_choices_for_auto_matching();
            }
            else
            {
                _input_output.Output_line("Couldn't find any automatic matches for you, sorry.");
                _input_output.Output_line("");
            }
        }

        private void Show_auto_matches(List<ConsoleLine> auto_matches)
        {
            _input_output.Output_line("");
            _input_output.Output_line($"Here are the automatic matches based on amount, text and date within {PotentialMatch.PartialDateMatchThreshold} days:");
            _input_output.Output_line("");
            _input_output.Output_all_lines(auto_matches);
            _input_output.Output_line("");
            _input_output.Output_line("You can reverse one match by entering the index of the match...");
            _input_output.Output_line("...or you can reverse multiple matches, by entering a comma-separated list of indices.");
            _input_output.Output_line("Like this: '0,3,23,5,24'");
        }

        public void Show_current_record_and_semi_auto_matches()
        {
            var potential_matches = Reconciliator.Current_potential_matches();
            _input_output.Output_line("");
            _input_output.Output_line("*****************************************************************************");
            _input_output.Output_line("*****************************************************************************");
            _input_output.Output_line($"Source record for {Third_party_descriptor}, with best match immediately afterwards:");
            _input_output.Output("   ");
            _input_output.Output_line(Reconciliator.Current_source_record_as_console_line());
            foreach (var console_line in potential_matches[0].Console_lines)
            {
                _input_output.Output_line(console_line.Get_console_snippets(potential_matches[0]));
            }

            _input_output.Output_line("..............");
            _input_output.Output_line($"Other matches found from {Owned_file_descriptor}:");
            _input_output.Output_line("***********");
            _input_output.Output_line("Enter the number next to the option you want.");
            _input_output.Output_all_lines_except_the_first(potential_matches);
            _input_output.Output_line("***********");
        }

        public void Show_current_record_and_manual_matches()
        {
            _input_output.Output_line("");
            _input_output.Output_line($"Source record for {Third_party_descriptor}:");
            _input_output.Output_line(Reconciliator.Current_source_record_as_string());

            _input_output.Output_line("..............");
            _input_output.Output_line($"Matches found from {Owned_file_descriptor}:");
            _input_output.Output_line("***********");
            _input_output.Output_all_lines(Reconciliator.Current_potential_matches());
            _input_output.Output_line("***********");
        }

        private void Show_final_results_of_matching()
        {
            _input_output.Output_line("");
            _input_output.Output_line($"We started with {Reconciliator.Num_third_party_records()} records from {Third_party_descriptor}");
            _input_output.Output_line($"We started with {Reconciliator.Num_owned_records()} records from {Owned_file_descriptor}");
            _input_output.Output_line($"Number of matched records from {Third_party_descriptor}: {Reconciliator.Num_matched_third_party_records()}");
            _input_output.Output_line($"Number of matched records from {Owned_file_descriptor}: {Reconciliator.Num_matched_owned_records()}");

            Recursively_show_final_matches_and_get_choices();
        }

        private void Recursively_show_final_matches_and_get_choices()
        {
            List<ConsoleLine> final_matches = Reconciliator.Get_final_matches_for_console();
            if (final_matches.Count > 0)
            {
                Show_final_matches(final_matches);
                Act_on_choices_for_final_matching();
            }
            else
            {
                _input_output.Output_line("Couldn't find any final matches for you, sorry.");
                _input_output.Output_line("");
            }
        }

        private void Show_final_matches(List<ConsoleLine> final_matches)
        {
            _input_output.Output_line("");
            _input_output.Output_line($"There are {Reconciliator.Num_unmatched_third_party_records()} unmatched records from {Third_party_descriptor}:");
            _input_output.Output_string_list(Reconciliator.Unmatched_third_party_records());

            _input_output.Output_line("");
            _input_output.Output_line($"There are {Reconciliator.Num_unmatched_owned_records()} unmatched records from {Owned_file_descriptor}:");
            _input_output.Output_string_list(Reconciliator.Unmatched_owned_records());
            
            _input_output.Output_line("");
            _input_output.Output_line($"{Reconciliator.Num_matched_third_party_records()} records have been matched:");
            _input_output.Output_all_lines(final_matches);
            _input_output.Output_line("");
            
            _input_output.Output_line("You can reverse one match by entering the index of the match...");
            _input_output.Output_line("...or you can reverse multiple matches, by entering a comma-separated list of indices.");
            _input_output.Output_line("Like this: '0,3,23,5,24'");
            _input_output.Output_line("");
        }

        private void Proceed_after_showing_matching_results()
        {
            _input_output.Output_options(new List<string>
            {
                "1. Go again! (this means you can match any item, regardless of amount)",
                "2. Write csv and finish.",
            });

            string input = _input_output.Get_generic_input(ReconConsts.GoAgainFinish);

            switch (input)
            {
                case "1": Go_again(); break;
                case "2": Finish(); break;
            }
        }

        private void Go_again()
        {
            Reconciliator.Rewind();
            Do_matching_for_non_matching_records();
        }

        private void Show_warnings()
        {
            var num_matched_third_party_records = Reconciliator.Num_matched_third_party_records();
            var num_matched_owned_records = Reconciliator.Num_matched_owned_records();
            var num_third_party_records = Reconciliator.Num_third_party_records();
            var num_owned_records = Reconciliator.Num_owned_records();

            if (num_matched_third_party_records != num_matched_owned_records)
            {
                _input_output.Output_line(ReconConsts.BadTallyMatchedItems);
                _input_output.Get_input(ReconConsts.EnterAnyKeyToContinue);
            }

            if (num_matched_third_party_records > num_owned_records)
            {
                _input_output.Output_line(ReconConsts.BadTallyNumMatchedThirdParty);
                _input_output.Get_input(ReconConsts.EnterAnyKeyToContinue);
            }

            if (num_matched_owned_records > num_third_party_records)
            {
                _input_output.Output_line(ReconConsts.BadTallyNumMatchedOwned);
                _input_output.Get_input(ReconConsts.EnterAnyKeyToContinue);
            }
        }

        public void Finish()
        {
            Show_warnings();
            _input_output.Output_line(ReconConsts.WritingNewData);
            Reconciliator.Finish("recon");
            _input_output.Output_line(ReconConsts.Finished);
            _input_output.Output_line("");
        }

        private void Act_on_choices_for_auto_matching()
        {
            var input = _input_output.Get_input(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.AutoMatches);

            if (!string.IsNullOrEmpty(input) && char.IsDigit(input[0]))
            {
                try
                {
                    Remove_auto_matches(input);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    _input_output.Output_line(exception.Message);
                    Act_on_choices_for_auto_matching();
                }
            }
        }

        private void Remove_auto_matches(string input)
        {
            if (input.Contains(","))
            {
                Reconciliator.Remove_multiple_auto_matches(
                    input.Split(',')
                        .Select(x => Convert.ToInt32(x))
                        .ToList());
            }
            else
            {
                Reconciliator.Remove_auto_match(Convert.ToInt32(input));
            }
            Recursively_show_auto_matches_and_get_choices();
        }

        private void Act_on_choices_for_final_matching()
        {
            var input = _input_output.Get_input(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.FinalMatches);

            if (!string.IsNullOrEmpty(input) && char.IsDigit(input[0]))
            {
                try
                {
                    Remove_final_matches(input);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    _input_output.Output_line(exception.Message);
                    Act_on_choices_for_final_matching();
                }
            }
        }

        private void Remove_final_matches(string input)
        {
            if (input.Contains(","))
            {
                Reconciliator.Remove_multiple_final_matches(
                    input
                        .Split(',')
                        .Select(x => Convert.ToInt32(x))
                        .ToList());
            }
            else
            {
                Reconciliator.Remove_final_match(Convert.ToInt32(input));
            }
            Recursively_show_final_matches_and_get_choices();
        }

        private void Get_and_record_choice_of_matching()
        {
            string input = _input_output.Get_input(ReconConsts.EnterNumberOfMatch, Reconciliator.Current_source_description());

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    if (Char.IsDigit(input[0]))
                    {
                        Mark_the_match(input);
                    }
                    else if (input.ToUpper() == "D")
                    {
                        Process_deletion();
                    }
                }
                catch (Exception exception)
                {
                    _input_output.Output_line(exception.Message);
                    Get_and_record_choice_of_matching();
                }
            }
        }

        private void Mark_the_match(string input)
        {
            Reconciliator.Match_current_record(Convert.ToInt16(input));
        }

        private void Process_deletion()
        {
            string input = _input_output.Get_input(ReconConsts.WhetherToDeleteThirdParty, Reconciliator.Current_source_description());

            try
            {
                if (!string.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    Reconciliator.Delete_current_third_party_record();
                }
                else
                {
                    Delete_record_from_list_of_matches();
                }
            }
            catch (Exception e)
            {
                _input_output.Output_line(e.Message);
                Get_and_record_choice_of_matching();
            }
        }

        private void Delete_record_from_list_of_matches()
        {
            string input = _input_output.Get_input(ReconConsts.EnterDeletionIndex, Reconciliator.Current_source_description());
            if (!string.IsNullOrEmpty(input) && Char.IsDigit(input[0]))
            {
                Reconciliator.Delete_specific_owned_record_from_list_of_matches(Convert.ToInt16(input));

                if (Reconciliator.Num_potential_matches() == 0)
                {
                    _input_output.Output_line(ReconConsts.NoMatchesLeft);
                }
                else
                {
                    // Update the display.
                    if (_doing_semi_automatic_matching)
                    {
                        Show_current_record_and_semi_auto_matches();
                    }
                    else
                    {
                        Show_current_record_and_manual_matches();
                    }
                    Get_and_record_choice_of_matching();
                }
            }
        }
    }
}