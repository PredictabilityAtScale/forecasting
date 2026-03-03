using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.ComponentModel;

namespace FocusedObjective.Simulation
{
    internal class ExecuteAddStaffSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker worker = null)
        {
            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                return Kanban.Execute.ExecuteAddStaffSimulation.AsXML(data, worker);
            else
                return null;
        }
    }
}
