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
    internal class ExecuteMonteCarloSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                return Kanban.Execute.ExecuteMonteCarloSimulation.AsXML(data, workerThread);
            else
                return Scrum.Execute.ExecuteMonteCarloSimulation.AsXML(data, workerThread);
        }
    }
}
