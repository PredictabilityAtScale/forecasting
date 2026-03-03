using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VotingSystems
{
    public class VoteCandidateResult
    {
        public int Rank { get; set; }
        public char Candidate { get; set; }
        public int Score { get; set; }
        public int[] PairWins { get; set; }
    }
}
