using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Kanban
{
    internal class TimeIntervalTickEventArgs : EventArgs
    {
        internal TimeInterval TimeInterval { get; set; }
        internal double IntervalTime { get; set; }
    }
}
