using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using Troschuetz.Random;

namespace FocusedObjective.Simulation
{
    internal class ExecuteModelAuditSimulation
    {
        internal static XElement AsXML(SimulationData data)
        {
            ModelAudit auditor = new ModelAudit(data);
            return auditor.AsXml();
        }
    }
}
