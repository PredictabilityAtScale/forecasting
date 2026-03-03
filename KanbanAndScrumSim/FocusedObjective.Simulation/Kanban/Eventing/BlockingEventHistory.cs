using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Kanban
{
    internal class BlockingEventHistory
    {
        internal Card Card { get; set; }
        internal double TotalBlockedTime { get; set; }
    }
}
