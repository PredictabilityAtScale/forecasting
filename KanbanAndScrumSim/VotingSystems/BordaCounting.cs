using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VotingSystems
{
    public class BordaCounting : VotingBase
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

        public override XElement AsXML(List<VoteTally> votes)
        {
            IEnumerable<IGrouping<int, VoteCandidateResult>> results = this.Results(votes);
            
            XElement doc = new XElement(

                new XElement("ballot",

                    // results
                    new XElement("results",
                        from g in results
                        select new XElement("rank", new XAttribute("rank", g.Key),

                            from r in g
                            select new XElement("candidate",
                                new XAttribute("candidate", r.Candidate),
                                new XAttribute("score", r.Score)))
                                )
                            ,

                           new XElement("ballotSummary",

                                from v in votes
                                group v by v.PreferanceOrder.Trim().ToUpper() into g
                                orderby g.Sum(v => v.Count) descending
                                select new XElement("ballot", new XAttribute("preferance", g.Key), new XAttribute("count", g.Sum(v => v.Count)))


                            )
                            )
                        );

            return doc;
        }

    }
}
