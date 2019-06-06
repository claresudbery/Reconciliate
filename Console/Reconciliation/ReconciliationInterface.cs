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
        private readonly IInputOutput _inputOutput;

        public IReconciliator Reconciliator { get; private set; }
        public string ThirdPartyDescriptor { get; private set; }
        public string OwnedFileDescriptor { get; private set; }

        private bool _doingSemiAutomaticMatching = false;

        public ReconciliationInterface(
            IInputOutput inputOutput,
            IReconciliator reconciliator,
            string thirdPartyDescriptor,
            string ownedFileDescriptor)
        {
            _inputOutput = inputOutput;
            Reconciliator = reconciliator;
            ThirdPartyDescriptor = thirdPartyDescriptor;
            OwnedFileDescriptor = ownedFileDescriptor;
        }

        public void DoTheMatching()
        {
            DoAutomaticMatching();
            
            DoSemiAutomaticMatching();
            ShowFinalResultsOfMatching();
            ProceedAfterShowingMatchingResults();
        }

        public void DoMatchingForNonMatchingRecords()
        {
            DoManualMatching();
            ShowFinalResultsOfMatching();
            ProceedAfterShowingMatchingResults();
        }

        private void DoAutomaticMatching()
        {
            Reconciliator.DoAutoMatching();
            RecursivelyShowAutoMatchesAndGetChoices();
        }

        public void DoSemiAutomaticMatching()
        {
            _doingSemiAutomaticMatching = true;
            bool unmatchedRecordFound = Reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();

            while (unmatchedRecordFound)
            {
                ShowCurrentRecordAndSemiAutoMatches();
                GetAndRecordChoiceOfMatching();
                unmatchedRecordFound = Reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
            }
        }

        private void DoManualMatching()
        {
            _doingSemiAutomaticMatching = false;
            bool unmatchedRecordFound = Reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();

            while (unmatchedRecordFound)
            {
                ShowCurrentRecordAndManualMatches();
                GetAndRecordChoiceOfMatching();
                unmatchedRecordFound = Reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
            }
        }

        private void RecursivelyShowAutoMatchesAndGetChoices()
        {
            List<ConsoleLine> autoMatches = Reconciliator.GetAutoMatchesForConsole();
            if (autoMatches.Count > 0)
            {
                ShowAutoMatches(autoMatches);
                ActOnChoicesForAutoMatching();
            }
            else
            {
                _inputOutput.OutputLine("Couldn't find any automatic matches for you, sorry.");
                _inputOutput.OutputLine("");
            }
        }

        private void ShowAutoMatches(List<ConsoleLine> autoMatches)
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"Here are the automatic matches based on amount, text and date within {PotentialMatch.PartialDateMatchThreshold} days:");
            _inputOutput.OutputLine("");
            _inputOutput.OutputAllLines(autoMatches);
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("You can reverse one match by entering the index of the match...");
            _inputOutput.OutputLine("...or you can reverse multiple matches, by entering a comma-separated list of indices.");
            _inputOutput.OutputLine("Like this: '0,3,23,5,24'");
        }

        public void ShowCurrentRecordAndSemiAutoMatches()
        {
            var potentialMatches = Reconciliator.CurrentPotentialMatches();
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("*****************************************************************************");
            _inputOutput.OutputLine("*****************************************************************************");
            _inputOutput.OutputLine($"Source record for {ThirdPartyDescriptor}, with best match immediately afterwards:");
            _inputOutput.Output("   ");
            _inputOutput.OutputLine(Reconciliator.CurrentSourceRecordAsConsoleLine());
            foreach (var consoleLine in potentialMatches[0].ConsoleLines)
            {
                _inputOutput.OutputLine(consoleLine.GetConsoleSnippets(potentialMatches[0]));
            }

            _inputOutput.OutputLine("..............");
            _inputOutput.OutputLine($"Other matches found from {OwnedFileDescriptor}:");
            _inputOutput.OutputLine("***********");
            _inputOutput.OutputLine("Enter the number next to the option you want.");
            _inputOutput.OutputAllLinesExceptTheFirst(potentialMatches);
            _inputOutput.OutputLine("***********");
        }

        public void ShowCurrentRecordAndManualMatches()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"Source record for {ThirdPartyDescriptor}:");
            _inputOutput.OutputLine(Reconciliator.CurrentSourceRecordAsString());

            _inputOutput.OutputLine("..............");
            _inputOutput.OutputLine($"Matches found from {OwnedFileDescriptor}:");
            _inputOutput.OutputLine("***********");
            _inputOutput.OutputAllLines(Reconciliator.CurrentPotentialMatches());
            _inputOutput.OutputLine("***********");
        }

        private void ShowFinalResultsOfMatching()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"We started with {Reconciliator.NumThirdPartyRecords()} records from {ThirdPartyDescriptor}");
            _inputOutput.OutputLine($"We started with {Reconciliator.NumOwnedRecords()} records from {OwnedFileDescriptor}");
            _inputOutput.OutputLine($"Number of matched records from {ThirdPartyDescriptor}: {Reconciliator.NumMatchedThirdPartyRecords()}");
            _inputOutput.OutputLine($"Number of matched records from {OwnedFileDescriptor}: {Reconciliator.NumMatchedOwnedRecords()}");

            RecursivelyShowFinalMatchesAndGetChoices();
        }

        private void RecursivelyShowFinalMatchesAndGetChoices()
        {
            List<ConsoleLine> finalMatches = Reconciliator.GetFinalMatchesForConsole();
            if (finalMatches.Count > 0)
            {
                ShowFinalMatches(finalMatches);
                ActOnChoicesForFinalMatching();
            }
            else
            {
                _inputOutput.OutputLine("Couldn't find any final matches for you, sorry.");
                _inputOutput.OutputLine("");
            }
        }

        private void ShowFinalMatches(List<ConsoleLine> finalMatches)
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"There are {Reconciliator.NumUnmatchedThirdPartyRecords()} unmatched records from {ThirdPartyDescriptor}:");
            _inputOutput.OutputStringList(Reconciliator.UnmatchedThirdPartyRecords());

            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"There are {Reconciliator.NumUnmatchedOwnedRecords()} unmatched records from {OwnedFileDescriptor}:");
            _inputOutput.OutputStringList(Reconciliator.UnmatchedOwnedRecords());
            
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine($"{Reconciliator.NumMatchedThirdPartyRecords()} records have been matched:");
            _inputOutput.OutputAllLines(finalMatches);
            _inputOutput.OutputLine("");
            
            _inputOutput.OutputLine("You can reverse one match by entering the index of the match...");
            _inputOutput.OutputLine("...or you can reverse multiple matches, by entering a comma-separated list of indices.");
            _inputOutput.OutputLine("Like this: '0,3,23,5,24'");
            _inputOutput.OutputLine("");
        }

        private void ProceedAfterShowingMatchingResults()
        {
            _inputOutput.OutputOptions(new List<string>
            {
                "1. Go again! (this means you can match any item, regardless of amount)",
                "2. Write csv and finish.",
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.GoAgainFinish);

            switch (input)
            {
                case "1": GoAgain(); break;
                case "2": Finish(); break;
            }
        }

        private void GoAgain()
        {
            Reconciliator.Rewind();
            DoMatchingForNonMatchingRecords();
        }

        private void ShowWarnings()
        {
            var numMatchedThirdPartyRecords = Reconciliator.NumMatchedThirdPartyRecords();
            var numMatchedOwnedRecords = Reconciliator.NumMatchedOwnedRecords();
            var numThirdPartyRecords = Reconciliator.NumThirdPartyRecords();
            var numOwnedRecords = Reconciliator.NumOwnedRecords();

            if (numMatchedThirdPartyRecords != numMatchedOwnedRecords)
            {
                _inputOutput.OutputLine(ReconConsts.BadTallyMatchedItems);
                _inputOutput.GetInput(ReconConsts.EnterAnyKeyToContinue);
            }

            if (numMatchedThirdPartyRecords > numOwnedRecords)
            {
                _inputOutput.OutputLine(ReconConsts.BadTallyNumMatchedThirdParty);
                _inputOutput.GetInput(ReconConsts.EnterAnyKeyToContinue);
            }

            if (numMatchedOwnedRecords > numThirdPartyRecords)
            {
                _inputOutput.OutputLine(ReconConsts.BadTallyNumMatchedOwned);
                _inputOutput.GetInput(ReconConsts.EnterAnyKeyToContinue);
            }
        }

        public void Finish()
        {
            ShowWarnings();
            _inputOutput.Output(ReconConsts.WritingNewData);
            Reconciliator.Finish("recon");
        }

        private void ActOnChoicesForAutoMatching()
        {
            var input = _inputOutput.GetInput(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.AutoMatches);

            if (!string.IsNullOrEmpty(input) && char.IsDigit(input[0]))
            {
                try
                {
                    RemoveAutoMatches(input);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    _inputOutput.OutputLine(exception.Message);
                    ActOnChoicesForAutoMatching();
                }
            }
        }

        private void RemoveAutoMatches(string input)
        {
            if (input.Contains(","))
            {
                Reconciliator.RemoveMultipleAutoMatches(
                    input.Split(',')
                        .Select(x => Convert.ToInt32(x))
                        .ToList());
            }
            else
            {
                Reconciliator.RemoveAutoMatch(Convert.ToInt32(input));
            }
            RecursivelyShowAutoMatchesAndGetChoices();
        }

        private void ActOnChoicesForFinalMatching()
        {
            var input = _inputOutput.GetInput(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.FinalMatches);

            if (!string.IsNullOrEmpty(input) && char.IsDigit(input[0]))
            {
                try
                {
                    RemoveFinalMatches(input);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    _inputOutput.OutputLine(exception.Message);
                    ActOnChoicesForFinalMatching();
                }
            }
        }

        private void RemoveFinalMatches(string input)
        {
            if (input.Contains(","))
            {
                Reconciliator.RemoveMultipleFinalMatches(
                    input
                        .Split(',')
                        .Select(x => Convert.ToInt32(x))
                        .ToList());
            }
            else
            {
                Reconciliator.RemoveFinalMatch(Convert.ToInt32(input));
            }
            RecursivelyShowFinalMatchesAndGetChoices();
        }

        private void GetAndRecordChoiceOfMatching()
        {
            string input = _inputOutput.GetInput(ReconConsts.EnterNumberOfMatch, Reconciliator.CurrentSourceDescription());

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    if (Char.IsDigit(input[0]))
                    {
                        MarkTheMatch(input);
                    }
                    else if (input.ToUpper() == "D")
                    {
                        ProcessDeletion();
                    }
                }
                catch (Exception exception)
                {
                    _inputOutput.OutputLine(exception.Message);
                    GetAndRecordChoiceOfMatching();
                }
            }
        }

        private void MarkTheMatch(string input)
        {
            Reconciliator.MatchCurrentRecord(Convert.ToInt16(input));
        }

        private void ProcessDeletion()
        {
            string input = _inputOutput.GetInput(ReconConsts.WhetherToDeleteThirdParty, Reconciliator.CurrentSourceDescription());

            try
            {
                if (!string.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    Reconciliator.DeleteCurrentThirdPartyRecord();
                }
                else
                {
                    DeleteRecordFromListOfMatches();
                }
            }
            catch (Exception e)
            {
                _inputOutput.OutputLine(e.Message);
                GetAndRecordChoiceOfMatching();
            }
        }

        private void DeleteRecordFromListOfMatches()
        {
            string input = _inputOutput.GetInput(ReconConsts.EnterDeletionIndex, Reconciliator.CurrentSourceDescription());
            if (!string.IsNullOrEmpty(input) && Char.IsDigit(input[0]))
            {
                Reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(Convert.ToInt16(input));

                if (Reconciliator.NumPotentialMatches() == 0)
                {
                    _inputOutput.OutputLine(ReconConsts.NoMatchesLeft);
                }
                else
                {
                    // Update the display.
                    if (_doingSemiAutomaticMatching)
                    {
                        ShowCurrentRecordAndSemiAutoMatches();
                    }
                    else
                    {
                        ShowCurrentRecordAndManualMatches();
                    }
                    GetAndRecordChoiceOfMatching();
                }
            }
        }
    }
}