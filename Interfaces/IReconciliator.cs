using System.Collections.Generic;
using Interfaces.Delegates;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IReconciliator<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        ICSVFile<TThirdPartyType> ThirdPartyFile { get; set; }
        ICSVFile<TOwnedType> OwnedFile { get; set; }
        bool FindReconciliationMatchesForNextThirdPartyRecord();
        bool MoveToNextUnmatchedThirdPartyRecordForManualMatching();
        bool NotAtEnd();
        string CurrentSourceRecordAsString();
        List<IPotentialMatch> CurrentPotentialMatches();
        void MatchCurrentRecord(int matchIndex);
        int NumThirdPartyRecords();
        int NumOwnedRecords();
        int NumMatchedThirdPartyRecords();
        int NumMatchedOwnedRecords();
        int NumUnmatchedThirdPartyRecords();
        int NumUnmatchedOwnedRecords();
        List<string> UnmatchedThirdPartyRecords();
        List<string> UnmatchedOwnedRecords();
        void Rewind();
        void Finish(string fileSuffix);
        ConsoleLine CurrentSourceRecordAsConsoleLine();
        string CurrentSourceDescription();
        void DeleteCurrentThirdPartyRecord();
        void DeleteSpecificThirdPartyRecord(int specifiedIndex);
        void DeleteSpecificOwnedRecordFromListOfMatches(int specifiedIndex);
        int NumPotentialMatches();
        List<ConsoleLine> GetFinalMatchesForConsole();
        List<AutoMatchedRecord<TThirdPartyType>> DoAutoMatching();
        List<ConsoleLine> GetAutoMatchesForConsole();
        void RemoveAutoMatch(int matchIndex);
        void RemoveMultipleAutoMatches(List<int> matchIndices);
        void RemoveFinalMatch(int matchIndex);
        void RemoveMultipleFinalMatches(List<int> matchIndices);
        void RefreshFiles();
        void FilterOwnedFile(System.Predicate<TOwnedType> filterPredicate);
        void FilterThirdPartyFile(System.Predicate<TThirdPartyType> filterPredicate);
        void SetMatchFinder(MatchFindingDelegate<TThirdPartyType, TOwnedType> matchFindingDelegate);
        void ResetMatchFinder();
        void SetRecordMatcher(RecordMatchingDelegate<TThirdPartyType, TOwnedType> recordMatcher);
        void ResetRecordMatcher();
    }
}