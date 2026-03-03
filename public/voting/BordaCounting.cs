using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VotingSystems
{
    class BordaCounting : VotingBase
    {
        public override IEnumerable<IGrouping<int, VoteCandidateResult>> Results(List<VoteTally> votes)
        {
            List<VoteCandidateResult> results = new List<VoteCandidateResult>();

            foreach (var vote in votes)
            {
                int numberOfCandidates = vote.PreferanceOrder.Length;

                for (int i = 0; i < numberOfCandidates; i++)
                {
                    int score = numberOfCandidates - i -1;
                    char candidate = vote.PreferanceOrder[i];

                    // grab the current result for the candidate if it exists
                    VoteCandidateResult vr = results
                        .Where(v => v.Candidate == candidate)
                        .FirstOrDefault();

                    // first time through? - setup entry, add to the list
                    if (vr == null)
                    {
                       vr = new VoteCandidateResult();
                       vr.Candidate = candidate;
                       results.Add(vr);
                    }
                    
                    vr.Score += score * vote.Count;
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .GroupBy(r => r.Score);
        }

    }
}
