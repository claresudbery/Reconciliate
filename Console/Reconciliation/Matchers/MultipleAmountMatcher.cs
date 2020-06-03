using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class MultipleAmountMatcher<TThirdPartyType, TOwnedType> 
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {

        public IEnumerable<IPotentialMatch> Find_expense_matches(TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
        {
            return Debug_find_expense_matches(source_record, owned_file);
        }

        public IEnumerable<IPotentialMatch> Standby_find_expense_matches(TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
        {
            var result = new List<PotentialMatch>();
            /*if (owned_file.Records[0].Main_amount() == source_record.Main_amount())
            {
                var actual_records = new List<ICSVRecord>();
                actual_records.Add(owned_file.Records[0]);
                result.Add(new PotentialMatch {Actual_records = actual_records});
            }*/

            result = Find_match_lists(
                owned_file.Records[0].Main_amount(), 
                owned_file.Records as IEnumerable<ICSVRecord>)
            .Select(y => new PotentialMatch
            {
                Actual_records = y.Matches
            }).ToList();

            return result;
        }

        public IEnumerable<IPotentialMatch> Debug_find_expense_matches(TThirdPartyType source_record, ICSVFile<TOwnedType> owned_file)
        {
            var result = new List<IPotentialMatch>();
            var random_number_generator = new Random();

            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 2);
            Add_set_of_overlapping_matches(random_number_generator, owned_file, result, 3);

            return result;
        }

        private static void Add_set_of_overlapping_matches(
            Random random_number_generator,
            ICSVFile<TOwnedType> owned_file, 
            List<IPotentialMatch> result, 
            int num_matches)
        {
            var unmatched_records = owned_file.Records.Where(x => !x.Matched).ToList();
            var max_rand = unmatched_records.Count - 1;
            if (max_rand >= 0)
            {
                var new_match = new PotentialMatch
                {
                    Actual_records = new List<ICSVRecord>(),
                    Console_lines = new List<ConsoleLine>(),
                    Rankings = new Rankings { Amount = 0, Date = 0, Combined = 0 },
                    Amount_match = true,
                    Full_text_match = true,
                    Partial_text_match = true
                };
                for (int count = 1; count <= num_matches; count++)
                {
                    if (max_rand >= 0)
                    {
                        var random_index = random_number_generator.Next(0, max_rand);
                        var next_record = unmatched_records[random_index];
                        new_match.Actual_records.Add(next_record);
                        new_match.Console_lines.Add(next_record.To_console());
                        unmatched_records.Remove(next_record);
                        max_rand--;
                    }
                }
                result.Add(new_match);
            }
        }

        private IEnumerable<MatchList> Find_match_lists(double target_amount, IEnumerable<ICSVRecord> candidates)
        {
            List<MatchList> results = new List<MatchList>();

            var concrete_candidates = candidates.ToList();
            concrete_candidates.RemoveAll(x => x.Main_amount() > target_amount);
            double candidate_total = concrete_candidates.Sum(x => x.Main_amount());

            if (candidate_total.Equals(target_amount))
            {
                results.Add(new MatchList
                {
                    TargetAmount = target_amount,
                    Matches = new List<ICSVRecord>(concrete_candidates)
                });
            }
            else if (candidate_total < target_amount)
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
                            Matches = match_list.Matches.Concat(new List<ICSVRecord> {candidate}).ToList()
                        };
                        if (!results.Any(x => x.Matches.SequenceEqual(new_result.Matches)))
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