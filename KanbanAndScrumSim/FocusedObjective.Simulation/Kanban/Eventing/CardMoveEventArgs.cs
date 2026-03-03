using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Kanban
{
    internal class CardMoveEventArgs : EventArgs
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

        internal SetupColumnData ToColumn
        {
            get;
            set;
        }

        internal int ToPosition
        {
            get;
            set;
        }
    }
}
