using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VotingSystems
{
    public class Schulze : VotingBase
    {
        public override IEnumerable<IGrouping<int, VoteCandidateResult>> Results(List<VoteTally> votes)
        {
            List<VoteCandidateResult> results = new List<VoteCandidateResult>();

            List<char> candidates = GetAllCandidates(votes);
            int[,] pairWins = GetPairWins(votes);
            int[,] pathStrengths = FindPathStrength(pairWins);

            bool[] winners;
            int rank = 1;

            winners = MakeWinners(pathStrengths);
            while (AnyWinners(winners))
            {
                WinnerIterative(results, candidates, pathStrengths, winners, rank);
                
                // go again. until there are no more winners
                winners = MakeWinners(pathStrengths);
                rank++;
            }

            return results
                .OrderBy(r => r.Rank)
                .GroupBy(r => r.Rank);
        }

        private void WinnerIterative(List<VoteCandidateResult> results, List<char> candidates, int[,] pathStrengths, bool[] winners, int rank)
        {
            for (int i = 0; i < winners.Length; i++)
            {
                if (winners[i] == true)
                {
                    int score = 0;

                    for (int j = 0; j < pathStrengths.GetLength(1); j++)
                        if ((pathStrengths[j, i] > -1) && (pathStrengths[i, j] > pathStrengths[j,i]))
                            score += 1;

                    results.Add(new VoteCandidateResult { Candidate = candidates[i], Rank = rank, Score = score });

                    // remove the path strength of the winner(s) this round
                    for (int j = 0; j < pathStrengths.GetLength(0); j++)
                        pathStrengths[i, j] = -1;
                }
            }
        }

        private bool AnyWinners(bool[] winners)
        {
            // there must be at least one false, and at least one true;
            bool foundFalse = false;
            bool foundTrue = false;

            foreach (bool b in winners)
            {
                if (b) foundTrue = true;
                if (!b) foundFalse = true;
            }

            return foundTrue && foundFalse;
        }

        public int[,] FindPathStrength(int[,] pairWins)
        {
            int[,] p = new int[pairWins.GetLength(0), pairWins.GetLength(1)];

            // take the winner of the pairwise contest, set the others to zero
            for (int i = 0; i < pairWins.GetLength(0); i++)
            {

                for (int j = 0; j < pairWins.GetLength(1); j++)
                {
                    if (i != j)
                    {
                        if (pairWins[i, j] > pairWins[j, i])
                            p[i, j] = pairWins[i, j];
                        else
                            p[i, j] = 0;
                    }
                }

                // floyd-Warshall algorithm to find strengths
                for (int i1 = 0; i1 < p.GetLength(0); i1++)
                {
                    for (int j1 = 0; j1 < p.GetLength(0); j1++)
                    {
                        if (i1 != j1)
                        {
                            for (int k1 = 0; k1 < p.GetLength(1); k1++)
                            {
                                if (i1 != k1)
                                {
                                    if (j1 != k1)
                                    {
                                        p[j1, k1] = Math.Max(p[j1, k1], Math.Min(p[j1, i1], p[i1, k1]));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return p;
        }

        public bool[] MakeWinners(int[,] pathStrengthMatrix)
        {
            bool[] result = new bool[pathStrengthMatrix.GetLength(0)];

            // default everyone to true
            for (int i = 0; i < pathStrengthMatrix.GetLength(0); ++i)
                result[i] = true;

            for (int i = 0; i < pathStrengthMatrix.GetLength(0); ++i)
            {
                for (int j = 0; j < pathStrengthMatrix.GetLength(0); ++j)
                {
                    if (pathStrengthMatrix[i, j] < 0)
                        result[i] = false;

                    if (pathStrengthMatrix[i, j] < pathStrengthMatrix[j, i])
                            result[i] = false;
                }
            }

            return result;
        }


        public override XElement AsXML (List<VoteTally> votes)
        {
            IEnumerable<IGrouping<int, VoteCandidateResult>> results = this.Results(votes);
            int[,] pairWins = this.GetPairWins(votes);
            int[,] pathStrength = this.FindPathStrength(pairWins);


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
                                select new XElement("ballot", new XAttribute("preference", g.Key), new XAttribute("count", g.Sum(v => v.Count)))
                      
                            
                            )
                
                ,

                new XElement("pairWins", 
                    // pairWins
                    from i in Enumerable.Range(0, pairWins.GetLength(0))
                                
                    let candidates = this.GetAllCandidates(votes)

                    select new XElement("C" + candidates[i].ToString(),
                        from j in Enumerable.Range(0, pairWins.GetLength(1))
                        select new XAttribute("C" + candidates[j].ToString(), pairWins[i,j].ToString())
                                        
                                        
                    )),
                            
                            
                new XElement("pathStrength", 
                    // pairWins
                    from i in Enumerable.Range(0, pathStrength.GetLength(0))
                                
                    let candidates = this.GetAllCandidates(votes)

                    select new XElement("C" + candidates[i].ToString(),
                        from j in Enumerable.Range(0, pathStrength.GetLength(1))
                        select new XAttribute("C" + candidates[j].ToString(), pathStrength[i,j].ToString())
                                        
                    ))

                

                            )

                        );

            return doc;
        }
    }
}
