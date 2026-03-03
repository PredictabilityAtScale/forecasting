using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.IO;

namespace FocusedObjective.Simulation
{
    internal class ExecuteBallot
    {
        internal static XElement AsXML(SimulationData data)
        {
            XElement result = new XElement("ballot");

            if (!string.IsNullOrWhiteSpace(data.Execute.Ballot.Data))
            {
                VotingSystems.VotingBase b = null;

                if (data.Execute.Ballot.BallotType == BallotTypeEnum.Borda)
                    b = new VotingSystems.BordaCounting();
                else
                    b = new VotingSystems.Schulze();

                string[] rawVoteLines = data.Execute.Ballot.Data.Split(new char[] { '\n' });

                var votes = VotingSystems.VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(rawVoteLines);

                result = b.AsXML(votes);
            }

            return result;
        }

    }
}
