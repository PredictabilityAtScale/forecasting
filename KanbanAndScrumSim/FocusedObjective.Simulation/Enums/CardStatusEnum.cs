using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.Simulation.Enums
{
    internal enum CardStatusEnum
    {
        InBacklog,
        NewStatusThisInterval,
        SameStatusThisInterval,
        Blocked,
        CompletedButWaitingForFreePosition,
        Completed,
        None
    }
}
