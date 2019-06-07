using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Reconciliators
{
    internal class CredCard1Reconciliator : IReconciliator
    {
        private readonly Reconciliator<CredCard1Record, CredCard1InOutRecord> _reconciliator;
        public ICSVFile<CredCard1Record> ThirdPartyFile { get; set; }
        public ICSVFile<CredCard1InOutRecord> OwnedFile { get; set; }

        public CredCard1Reconciliator(
            IFileIO<CredCard1Record> credCard1FileIO,
            IFileIO<CredCard1InOutRecord> credCard1InOutFileIO)
        {
            ThirdPartyFile = new CSVFile<CredCard1Record>(credCard1FileIO);
            ThirdPartyFile.Load();

            OwnedFile = new CSVFile<CredCard1InOutRecord>(credCard1InOutFileIO);
            OwnedFile.Load();

            _reconciliator = new Reconciliator<CredCard1Record, CredCard1InOutRecord>(
                ThirdPartyFile,
                OwnedFile,
                CredCard1AndCredCard1InOutData.LoadingInfo.ThirdPartyFileLoadAction,
                CredCard1AndCredCard1InOutData.LoadingInfo.SheetName);
        }

        public void FilterForNegativeRecordsOnly()
        {
            ThirdPartyFile.FilterForNegativeRecordsOnly();
        }

        public void FilterForPositiveRecordsOnly()
        {
            ThirdPartyFile.FilterForPositiveRecordsOnly();
        }

        public void SwapSignsOfAllAmounts()
        {
            ThirdPartyFile.SwapSignsOfAllAmounts();
        }

        public bool FindReconciliationMatchesForNextThirdPartyRecord()
        {
            return _reconciliator.FindReconciliationMatchesForNextThirdPartyRecord();
        }

        public bool MoveToNextUnmatchedThirdPartyRecord()
        {
            return _reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
        }

        public bool NotAtEnd()
        {
            return _reconciliator.NotAtEnd();
        }

        public string CurrentSourceRecordAsString()
        {
            return _reconciliator.CurrentSourceRecordAsString();
        }

        public ConsoleLine CurrentSourceRecordAsConsoleLine()
        {
            return _reconciliator.CurrentSourceRecordAsConsoleLine();
        }

        public string CurrentSourceDescription()
        {
            return _reconciliator.CurrentSourceDescription();
        }

        public void DeleteCurrentThirdPartyRecord()
        {
            _reconciliator.DeleteCurrentThirdPartyRecord();
        }

        public void DeleteSpecificThirdPartyRecord(int specifiedIndex)
        {
            _reconciliator.DeleteSpecificThirdPartyRecord(specifiedIndex);
        }

        public void DeleteSpecificOwnedRecordFromListOfMatches(int specifiedIndex)
        {
            _reconciliator.DeleteSpecificOwnedRecordFromListOfMatches(specifiedIndex);
        }

        public int NumPotentialMatches()
        {
            return _reconciliator.NumPotentialMatches();
        }

        public List<IPotentialMatch> CurrentPotentialMatches()
        {
            return _reconciliator.CurrentPotentialMatches();
        }

        public void MarkLatestMatchIndex(int matchIndex)
        {
            _reconciliator.MarkLatestMatchIndex(matchIndex);
        }

        public void MatchNonMatchingRecord(int matchIndex)
        {
            _reconciliator.MatchNonMatchingRecord(matchIndex);
        }

        public List<ConsoleLine> GetFinalMatchesForConsole()
        {
            return _reconciliator.GetFinalMatchesForConsole();
        }

        public void DoAutoMatching()
        {
            _reconciliator.ReturnAutoMatches();
        }

        public List<ConsoleLine> GetAutoMatchesForConsole()
        {
            return _reconciliator.GetAutoMatchesForConsole();
        }

        public void RemoveAutoMatch(int matchIndex)
        {
            _reconciliator.RemoveAutoMatch(matchIndex);
        }

        public void RemoveMultipleAutoMatches(List<int> matchIndices)
        {
            _reconciliator.RemoveMultipleAutoMatches(matchIndices);
        }

        public void RemoveFinalMatch(int matchIndex)
        {
            _reconciliator.RemoveFinalMatch(matchIndex);
        }

        public void RemoveMultipleFinalMatches(List<int> matchIndices)
        {
            _reconciliator.RemoveMultipleFinalMatches(matchIndices);
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

        public void Rewind()
        {
            _reconciliator.Rewind();
        }

        public void Finish(string fileSuffix)
        {
            _reconciliator.Finish(fileSuffix);
        }

        public void MatchCurrentRecord(int matchIndex)
        {
            _reconciliator.MatchCurrentRecord(matchIndex);
        }

        public bool MoveToNextUnmatchedThirdPartyRecordForManualMatching()
        {
            return _reconciliator.MoveToNextUnmatchedThirdPartyRecordForManualMatching();
        }

        public void RefreshFiles()
        {
            _reconciliator.RefreshFiles();
        }
    }
}