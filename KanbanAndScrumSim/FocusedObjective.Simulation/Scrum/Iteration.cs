using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{
    internal class Iteration
    {
        private List<Story> _stories = new List<Story>();

        internal int Sequence { get; set; }

        internal Iteration PreviousIteration { get; set; }

        internal double PointsAllocatableThisIteration { get; set; }

        internal int CountStoriesInBacklog { get; set; }
        internal int CountStoriesInComplete { get; set; }
        internal double ValueDeliveredSoFar { get; set; }
        internal DateTime? CurrentDate { get; set; }

        internal List<Story> Stories
        {
            get { return _stories; }
        }

        internal SetupPhaseData Phase { get; set; }
    }
}
