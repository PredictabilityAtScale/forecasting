using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VotingSystems
{
    public class VotingBase
    {

        public virtual IEnumerable<IGrouping<int,VoteCandidateResult>> Results(List<VoteTally> votes)
        {
            // base voting is just a number of primary votes for each candidate
            List<VoteCandidateResult> results = new List<VoteCandidateResult>();

            foreach (var vote in votes)
            {
                if (!string.IsNullOrEmpty(vote.PreferanceOrder))
                {
                    // get the winner, and increment the value in the dictionary if it exists
                    VoteCandidateResult vr = results
                        .Where(v => v.Candidate == vote.PreferanceOrder[0])
                        .FirstOrDefault();

                    // first time through - setup entry, add to the list
                    if (vr == null)
                    {
                       vr = new VoteCandidateResult();
                       vr.Candidate = vote.PreferanceOrder[0];
                       results.Add(vr);
                    }
                    
                    vr.Score += vote.Count;
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .GroupBy(r => r.Score);

        }


        public double RankOrderCentroid(int N, int k)
        {
            double result = 0.0;
            for (int i = k; i <= N; ++i)
                result += (1.0 / i);
            return result / N;
        }


        public virtual List<char> GetAllCandidates(List<VoteTally> votes)
        {
            // find all candidates from  all votes
            List<char> candidates = new List<char>();
            foreach (var vote in votes)
            {
                foreach (var candidate in vote.PreferanceOrder.Trim().ToUpper())
                {
                    // > is a special character, so is =
                    if (candidate != '>' && candidate != '=')
                    {
                        if (!candidates.Contains(candidate))
                            candidates.Add(candidate);
                    }
                }
            }

            return candidates.OrderBy(c => c).ToList() ;
        }
        
        public virtual int[,] GetPairWins(List<VoteTally> votes)
        {
            //todo:equal and skipped candidates
            // > and =
            //
            // if no symbols > and =, then all are considered a win sequence
            // if there is a single  > or = , then we need to do specific situations
            
            List<char> candidates = GetAllCandidates(votes);

            int[,] pairWins = new int[candidates.Count, candidates.Count];
            foreach (var vote in votes)
            {
                bool specialCharsPresent = (vote.PreferanceOrder.Contains('>') || vote.PreferanceOrder.Contains('='));
                bool greaterOn = false;
                bool equalOn = false;

                // grab each candidate pair and write the win/loss pairs
                for (int j = 0; j < vote.PreferanceOrder.Length - 1; j++) // candidate order (skip the last, we just want pairs)
                {
                    char jChar = vote.PreferanceOrder[j];

                    // skip special characters
                    if (jChar != '>' && jChar != '=')
                    {
                        int candidateWin = candidates.IndexOf(jChar);

                        bool equalFound = false;
                        bool greaterThanFound = false;

                        for (int k = j + 1; k < vote.PreferanceOrder.Length; k++)
                        {
                            char kChar = vote.PreferanceOrder[k];

                            switch (kChar)
                            {
                                case '>': 
                                    greaterThanFound = true; 
                                    equalFound = false; 
                                    break;
                                case '=':
                                    // we set this, but can just skip. when special symbols are in play, 
                                    // all candidates until a > sign are equal
                                    equalFound = true;
                                    break;
                                default:
                                    {
                                        // skip votes until the > symbol
                                        if (!specialCharsPresent || greaterThanFound)
                                        {
                                            int candidateLose = candidates.IndexOf(kChar);
                                            pairWins[candidateWin, candidateLose] += vote.Count;
                                        }

                                        // reset equals flag. just pertains to the next vote
                                        equalFound = false;

                                        break;
                                    }
                            }
                        }
                    }
                }

                // candidates not listed in ballot need to be "beaten" by all others listed in ballot
                // (but not those also skipped)
                var diffIndexes = candidates.Except(vote.Candidates).Select(c => candidates.IndexOf(c)).ToList();
                
                // vote for A, but missing B and C
                /*
                 * diff should have B and C (1 and 2)
                            0   1   2
                        0   -   +   +
                        1   0   -   0
                        2   0   0    -
                 */

                if (diffIndexes.Count > 0)
                {
                    for (int i = 0; i < pairWins.GetLength(0); i++)
                    {
                        // if the 'win' axis isn't in the skipped, we need to add a win for all those not in the skipped list
                        if (!diffIndexes.Contains(i))
                        {
                            for (int j = 0; j < pairWins.GetLength(1); j++)
                            {
                                if (diffIndexes.Contains(j))
                                {
                                    pairWins[i, j] += vote.Count;
                                }
                            }
                        }
                    }
                }
            }

            return pairWins;
        }

    }
}
