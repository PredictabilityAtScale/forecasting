using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VotingSystems
{
    public class VoteTally
    {
        public int Count = 1;
        public string PreferanceOrder = string.Empty;

        public List<char> Candidates
        {
            get
            {
                return PreferanceOrder
                    .Trim()
                    .ToUpper()
                    .Where(c => c != '>' && c != '=') // special characters
                    .Distinct() // should always be anyway
                    .OrderBy(c => c)
                    .ToList();
            }
        }

        public bool Valid
        {
            get
            {
                return InvalidVoteReasons.Count == 0;
            }
        }

        public List<string> InvalidVoteReasons
        {
            get
            {
                List<string> result = new List<string>();

                // count needs to be greater than zero
                if (this.Count <= 0)
                    result.Add("Vote has a 0 or negative count. Each vote must have a count of 1 or greater");

                // not null or empty list of preferences
                if (PreferanceOrder == null ||( !PreferanceOrder.Trim().Where(c => c!='>' && c!='=').Any()))
                    result.Add("Vote contains no candidates (empty, or just whitespace characters)");

                if (PreferanceOrder == null)
                    return result;

                // each candidate only appears once (compare a distinct list versus all candidates
                var allCandidates = PreferanceOrder // all candidates, not just distinct ones (as returned by Candidates)
                       .Trim()
                       .ToUpper()
                       .Where(c => c != '>' && c != '=');
                   
                if (this.Candidates.Count() != allCandidates.Count())
                {
                    result.Add("One or more candidates appear more than once in this vote. Note: All candidates are converted to upper case, so A = a.");
                }

                return result;
            }

        }
    }

    public class VoteCandidate
    {
        public char Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    
    public static class VotingHelpers
    {
        public static List<VoteTally> VoteOrderPreferencesAsAlphabeticalVoteTally(IEnumerable<string> inputs)
        {
            List<VoteTally> results = new List<VoteTally>();

            foreach (var s in inputs)
            {
                // prepare and format string.
                string str = s.ToUpper();

                // if there is a count? then extract and remove from string
                // default to 1 if missing.
                int count = 1;

                // find the first non-numeric character
                int i = 0;
                while ((i < str.Count() - 1) && (char.IsDigit(str[i])))
                {
                    i++;
                }

                if (i > 0)
                {
                    // grab the number
                    count = int.Parse(str.Substring(0, i));

                    // delete the count (plus the first non-numeric character (hope it is a space!)
                    str = str.Substring(i, str.Count()-i).Trim();
                }

                results.Add(
                    new VoteTally { Count = count, PreferanceOrder = str });
            }

            return results;
        }

    }
}

