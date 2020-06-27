using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class MultipleAmountMatcher<TThirdPartyType, TOwnedType> 
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        private const int DateTolerance = 5;

        public IEnumerable<IPotentialMatch> Find_matches(TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
        {
            var result = new List<PotentialMatch>();

            var match_lists = Find_match_lists(
                source_record.Main_amount(),
                owned_file.Records.Where(
                    x => !x.Matched && Date_is_within_tolerance(source_record, x)
                ) as IEnumerable<ICSVRecord>).ToList();

            var debug = match_lists.Select(x => x.Actual_amount()).ToList();

            bool no_matches_found = match_lists.Count == 1 
                                    && match_lists[0].Matches.Count == 0;

            if (!no_matches_found)
            {
                result = match_lists.Select(y => new PotentialMatch
                {
                    Actual_records = y.Matches,
                    Console_lines = y.Matches.Select(z => z.To_console()).ToList(),
                    Rankings = new Rankings
                    {
                        Amount = y.Distance_from_target(), 
                        Date = y.Matches.Average(z => z.Date.Proximity_score(source_record.Date)), 
                        Combined = 0
                    },
                    Amount_match = y.Exact_match(),
                    Full_text_match = true,
                    Partial_text_match = true
                })
                .OrderByDescending(x => x.Amount_match)
                .ThenBy(x => x.Rankings.Date)
                .ThenBy(x => x.Rankings.Amount)
                .ToList();
            }

            return result;
        }

        public bool Date_is_within_tolerance(TThirdPartyType source_record, TOwnedType target_record)
        {
            return target_record.Date >= source_record.Date.AddDays(-DateTolerance)
                   && target_record.Date <= source_record.Date.AddDays(DateTolerance);
        }

        private IEnumerable<MatchList> Find_match_lists(double target_amount, IEnumerable<ICSVRecord> candidates)
        {
            List<MatchList> results = new List<MatchList>();

            var concrete_candidates = candidates
                .OrderBy(x => x.Description)
                .ThenBy(x => x.Date)
                .ThenBy(x => x.Main_amount())
                .ToList();
            concrete_candidates.RemoveAll(x => x.Main_amount().Double_greater_than(target_amount));
            double candidate_total = concrete_candidates.Sum(x => x.Main_amount());

            if (candidate_total.Double_equals(target_amount))
            {
                results.Add(new MatchList
                {
                    TargetAmount = target_amount,
                    Matches = new List<ICSVRecord>(concrete_candidates)
                });
            }
            else if (candidate_total.Double_less_than(target_amount))
            {
                // !! We need this to sometimes be an empty list!
                results.Add(new MatchList
                {
                    TargetAmount = target_amount,
                    Matches = new List<ICSVRecord>(concrete_candidates)
                });
            }
            else
            {
                foreach (ICSVRecord candidate in concrete_candidates)
                {
                    List<ICSVRecord> new_candidates = new List<ICSVRecord>(concrete_candidates);
                    new_candidates.Remove(candidate);
                    double new_target = target_amount - candidate.Main_amount();
                    IEnumerable<MatchList> new_match_lists = Find_match_lists(new_target, new_candidates);
                    foreach (MatchList match_list in new_match_lists)
                    {
                        var new_result = new MatchList
                        {
                            TargetAmount = target_amount,
                            Matches = match_list.Matches.Concat(new List<ICSVRecord> {candidate})
                                .OrderBy(x => x.Description)
                                .ThenBy(x => x.Date)
                                .ThenBy(x => x.Main_amount())
                                .ToList()
                        };
                        if (!results.Any(x => x.Matches.SequenceEqual(new_result.Matches, new CsvRecordComparer())))
                        {
                            results.Add(new_result);
                        }
                    }
                }
            }

            return results;
        }
    }
}