using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.IO;
using System.ComponentModel;

namespace FocusedObjective.Simulation
{
    internal class ExecuteVisualSimulation
    {
        internal static XElement AsXML(SimulationData data)
        {
            Object blank = null;

            return ExecuteVisualSimulation.AsXML(data, ref blank);
        }

        internal static XElement AsXML(SimulationData data, ref dynamic simulator, BackgroundWorker workerThread = null)
        {
            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                return Kanban.Execute.ExecuteVisualSimulation.AsXML(data, ref simulator, workerThread);
            else
                return Scrum.Execute.ExecuteVisualSimulation.AsXML(data, ref simulator, workerThread);
        }
    }
}
