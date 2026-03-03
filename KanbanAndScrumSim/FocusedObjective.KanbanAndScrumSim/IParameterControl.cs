using FocusedObjective.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.KanbanSim
{
    public interface IParameterControl
    {
        MoveableParameter Parameter { get; set; }
    }
}
