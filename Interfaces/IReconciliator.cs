using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IReconciliator
    {
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
        void DoAutoMatching();
        List<ConsoleLine> GetFinalMatchesForConsole();
        List<ConsoleLine> GetAutoMatchesForConsole();
        void RemoveAutoMatch(int matchIndex);
        void RemoveMultipleAutoMatches(List<int> matchIndices);
        void RemoveFinalMatch(int matchIndex);
        void RemoveMultipleFinalMatches(List<int> matchIndices);
        void RefreshFiles();
    }
}