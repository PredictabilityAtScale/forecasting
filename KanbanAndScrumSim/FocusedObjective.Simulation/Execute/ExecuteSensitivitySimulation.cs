using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FocusedObjective.Simulation
{
    internal class ExecuteSensitivitySimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker worker = null)
        {
            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                return Kanban.Execute.ExecuteSensitivitySimulation.AsXML(data, worker);
            else
                return Scrum.Execute.ExecuteSensitivitySimulation.AsXML(data, worker);
        }
    }
}
