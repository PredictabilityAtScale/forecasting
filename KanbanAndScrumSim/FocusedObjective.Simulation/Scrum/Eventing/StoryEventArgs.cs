using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{
    internal class StoryEventArgs : EventArgs
    {
        internal Story Story { get; set; }
    }
}
