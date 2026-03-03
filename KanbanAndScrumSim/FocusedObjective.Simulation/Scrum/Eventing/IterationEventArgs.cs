using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{
    internal class IterationEventArgs : EventArgs
    {
        internal Iteration Iteration { get; set; }
    }
}
