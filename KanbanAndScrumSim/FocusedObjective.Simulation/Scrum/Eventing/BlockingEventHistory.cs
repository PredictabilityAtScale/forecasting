using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{
    internal class BlockingEventHistory
    {
        internal Story Story { get; set; }
        internal double TotalBlockedPoints { get; set; }
    }
}
