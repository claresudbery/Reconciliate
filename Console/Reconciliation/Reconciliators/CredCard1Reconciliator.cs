using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Reconciliators
{
    internal class CredCard1Reconciliator<TThirdPartyType, TOwnedType> : IReconciliator<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        private readonly Reconciliator<TThirdPartyType, TOwnedType> _reconciliator;
        public ICSVFile<TThirdPartyType> ThirdPartyFile { get; set; }
        public ICSVFile<TOwnedType> OwnedFile { get; set; }
        private string _worksheetName = "CredCard1";

        public CredCard1Reconciliator(
            IFileIO<TThirdPartyType> credCard1FileIO,
            IFileIO<TOwnedType> credCard1InOutFileIO)
        {
            ThirdPartyFile = new CSVFile<TThirdPartyType>(credCard1FileIO);
            ThirdPartyFile.Load();

            OwnedFile = new CSVFile<TOwnedType>(credCard1InOutFileIO);
            OwnedFile.Load();

            _reconciliator = new Reconciliator<TThirdPartyType, TOwnedType>(ThirdPartyFile, OwnedFile);
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
            _reconciliator.DoAutoMatching();
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

        List<AutoMatchedRecord<TThirdPartyType>> IReconciliator<TThirdPartyType, TOwnedType>.DoAutoMatching()
        {
            return _reconciliator.DoAutoMatching();
        }

        public void RefreshFiles()
        {
            _reconciliator.RefreshFiles();
        }
    }
}