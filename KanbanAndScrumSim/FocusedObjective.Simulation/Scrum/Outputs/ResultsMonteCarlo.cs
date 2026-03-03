using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{
    internal static class ResultsMonteCarlo
    {
        internal static XElement AsXML(SimulationData data, List<SimulationResultSummary> results)
        {
            XElement monteCarlo = new XElement("monteCarlo");
            monteCarlo.Add(new MonteCarloResultSummary(data, results, true).AsXML());
            monteCarlo.Add(new XAttribute("success", "true"));
            return monteCarlo;
        }

    }
}
