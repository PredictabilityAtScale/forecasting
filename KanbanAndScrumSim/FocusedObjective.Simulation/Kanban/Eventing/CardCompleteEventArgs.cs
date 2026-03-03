using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Kanban
{
    internal class CardCompleteEventArgs : EventArgs
    {

        internal Card Card
        {
            get;
            set;
        }

        internal SetupColumnData FromColumn
        {
            get;
            set;
        }

        internal int FromPosition
        {
            get;
            set;
        }
    }
}
