using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using System.ComponentModel;
using FocusedObjective.Simulation.Extensions;

namespace FocusedObjective.Simulation
{
    internal class ExecuteForecastDateSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {

            switch (data.Execute.ForecastDate.Permutations)
            {
                case ForecastPermutationsEnum.Deliverables:
                        return ExecuteForecastDateSimulation.ByDeliverables_AsXML(data, workerThread);

                case ForecastPermutationsEnum.SequentialDeliverables:
                        return ExecuteForecastDateSimulation.BySequentialDeliverables_AsXML(data, workerThread);

                case ForecastPermutationsEnum.SequentialBacklog:
                        return ExecuteForecastDateSimulation.BySequentialBacklog_AsXML(data, workerThread);

                default:
                    if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                        return Kanban.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread);
                    else
                        return Scrum.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread);

            }
        }

        internal static XElement ByDeliverables_AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            XElement result = new XElement("forecastDatePermutations",
                new XAttribute("success", "false"),
                new XAttribute("permutations", "deliverables")
                );

            string originalDeliverables = data.Execute.Deliverables;

            try
            {
                // do them all
                if (data.Execute.Deliverables == "")
                    data.Execute.Deliverables = string.Join("|", data.Setup.Backlog.Deliverables.Select(d => d.Name));

                // find all permutations of deliverable's
                var deliverables = data.Execute.Deliverables.Split(new char[] { '|', ',' });

                var permutations = deliverables.ToList().GetPowerSet<string>();

                List<XElement> forecasts = new List<XElement>();

                // forecast each permutation
                foreach (var perm in permutations)
                {
                    // must be at least one non-skippable deliverable
                    bool valid = perm.Count() > 0 &&
                                 perm.Intersect(
                                    data.Setup.Backlog.Deliverables.Where(d => d.SkipPercentage == 0.0).Select(d => d.Name)
                                  ).Any();

                    if (valid)
                    {
                        // remember current skip percentages, and then zero them out. We want all of these deliverable to be in-scope
                        Dictionary<string, double> oldSkips = new Dictionary<string, double>();
                        foreach (var del in perm)
                        {
                            oldSkips.Add(del, data.Setup.Backlog.Deliverables.First(thisDel => thisDel.Name == del).SkipPercentage);
                            data.Setup.Backlog.Deliverables.First(thisDel => thisDel.Name == del).SkipPercentage = 0.0;
                        }

                        try
                        {
                            string d = string.Join("|", perm);
                            data.Execute.Deliverables = d;

                            string progressString = "Sim: " + d + " Cycle: {0}";

                            XElement thisForecast = null;
                            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                                thisForecast = Kanban.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);
                            else
                                thisForecast = Scrum.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);

                            XElement entry = new XElement("permutation",
                                new XAttribute("name", string.Format("{0}", d.Replace("|", " & "))),
                                new XAttribute("deliverables", d),
                                thisForecast
                                );

                            forecasts.Add(entry);
                        }
                        finally
                        {
                            // reset the skip percentages
                            foreach (var key in oldSkips.Keys)
                                data.Setup.Backlog.Deliverables.First(thisDel => thisDel.Name == key).SkipPercentage = oldSkips[key];
                        }
                    }
                }

                // write out the results.
                if (forecasts.Count > 0)
                {
                    result = new XElement("forecastDatePermutations",
                        new XAttribute("success", "true"),
                        new XAttribute("permutations", "deliverables"),
                        forecasts
                        );
                }
            }
            finally
            {
                // reset the data
                data.Execute.Deliverables = originalDeliverables;
            }
            return result;
        }

        internal static XElement BySequentialDeliverables_AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            XElement result = new XElement("forecastDatePermutations",
                new XAttribute("success", "false"),
                new XAttribute("permutations", "SequentialDeliverables")
                );

            string originalDeliverables = data.Execute.Deliverables;
            try
            {
                List<XElement> forecasts = new List<XElement>();

                if (!data.Setup.Backlog.Deliverables.Any())
                {
                    // no defined deliverables, return empty
                    return result;
                }
                else
                {
                    int count = data.Setup.Backlog.Deliverables.Count;
                    int index = 1;

                    // blan out the original deliverables
                    data.Execute.Deliverables = "";

                    foreach (var deliverable in data.Setup.Backlog.Deliverables.OrderBy(d => d.Order))
                    {
                        // set deliverable
                        data.Execute.Deliverables = string.Join("|", data.Execute.Deliverables.Trim(), deliverable.Name);

                        string progressString = "Sim " + index + " of " + count + " Cycle: {0}";

                        XElement thisForecast = null;
                        if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                            thisForecast = Kanban.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);
                        else
                            thisForecast = Scrum.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);


                        string name = "";
                        foreach (var d in data.Execute.Deliverables.Split('|'))
                        {
                            if (!string.IsNullOrWhiteSpace(d))
                            {
                                if (string.IsNullOrWhiteSpace(name))
                                    name += d;
                                else
                                    name += " & " + d;
                            }
                        }

                        XElement entry = new XElement("permutation",
                            new XAttribute("name", name),
                            new XAttribute("deliverables", data.Execute.Deliverables),
                            new XAttribute("deliverable", deliverable.Name),
                            thisForecast
                            );

                        forecasts.Add(entry);

                        index++;
                    }
                }

                // write out the results.
                if (forecasts.Count > 0)
                {
                    result = new XElement("forecastDatePermutations",
                        new XAttribute("success", "true"),
                        new XAttribute("permutations", "SequentialDeliverables"),
                        forecasts
                        );
                }
            }
            finally
            {
                // reset the data
                data.Execute.Deliverables = originalDeliverables;
            }
            return result;
        }

        internal static XElement BySequentialBacklog_AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            XElement result = new XElement("forecastDatePermutations",
                new XAttribute("success", "false"),
                new XAttribute("permutations", "SequentialBacklog")
                );

            string originalDeliverables = data.Execute.Deliverables;

            try
            {
                List<XElement> forecasts = new List<XElement>();

                var permulationDeliverable = new SetupBacklogDeliverableData();
                permulationDeliverable.Name = "FORECAST ONLY"; 
                
                if (!data.Setup.Backlog.Deliverables.Any())
                {
                    customBacklogSequentialForecast(permulationDeliverable, data.Setup.Backlog.CustomBacklog, "", data, workerThread, forecasts);
                }
                else
                {
                    foreach (var deliverable in data.Setup.Backlog.Deliverables.OrderBy(d => d.Order))
                        customBacklogSequentialForecast(permulationDeliverable, deliverable.CustomBacklog, deliverable.Name, data, workerThread, forecasts);
                }

                // write out the results.
                if (forecasts.Count > 0)
                {
                    result = new XElement("forecastDatePermutations",
                        new XAttribute("success", "true"),
                        new XAttribute("permutations", "SequentialBacklog"),
                        forecasts
                        );
                }
            }
            finally
            {
                // reset the data
                data.Execute.Deliverables = originalDeliverables;
            }
            return result;
        }

        private static void customBacklogSequentialForecast(SetupBacklogDeliverableData deliverable, List<SetupBacklogCustomData> customBacklog, string deliverableName, SimulationData data, BackgroundWorker workerThread, List<XElement> forecasts)
        {
            data.Setup.Backlog.Deliverables.Add(deliverable);
            string originalDeliverables = data.Execute.Deliverables;
            try
            {
                data.Execute.Deliverables = "FORECAST ONLY";

                int count = customBacklog.Count;
                int index = 1;

                // no defined deliverables, just do the base custom entries
                foreach (var custom in customBacklog.OrderBy(b => b.Order).ThenBy(b => b.SafeDueDate))
                {
                    deliverable.CustomBacklog.Add(custom);

                    string progressString = "Sim " + index + " of " + count + " Cycle: {0}";

                    XElement thisForecast = null;
                    if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                        thisForecast = Kanban.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);
                    else
                        thisForecast = Scrum.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);

                    XElement entry = new XElement("permutation",
                        new XAttribute("name", string.Format("{0} - {1} {2}", deliverableName.Replace("|", ""), custom.Name, custom.Order != int.MaxValue ? string.Format("(order: {0})", custom.Order) : "(no order defined)")),
                        new XAttribute("deliverable", deliverableName),
                        new XAttribute("custom", custom.Name),
                        thisForecast
                        );

                    forecasts.Add(entry);

                    index++;
                }
            }
            finally
            {
                data.Setup.Backlog.Deliverables.Remove(deliverable);
                data.Execute.Deliverables = originalDeliverables;
            }

        }


        internal static XElement FindStartDate_AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            XElement result = new XElement("forecastDateFindStartDate",
                new XAttribute("success", "false")
                );

            string originalDeliverables = data.Execute.Deliverables;

            try
            {
                List<XElement> forecasts = new List<XElement>();
                

                // test from today and see if it is even possible

                DateTime currentTestDate = DateTime.Now;

                data.Setup.ForecastDate.StartDate = currentTestDate.ToString();
                string progressString = "Baseline - Cycle: {0}";

                XElement thisForecast = null;
                if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
                    thisForecast = Kanban.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);
                else
                    thisForecast = Scrum.Execute.ExecuteForecastDateSimulation.AsXML(data, workerThread, progressString);

                var likelyCompletionAtTarget = (from date in thisForecast.Element("dates").Descendants()
                                               where date.Attribute("targetLikelihood").Value == "true"
                                               select date)
                                               .FirstOrDefault();

                if (likelyCompletionAtTarget != null)
                {
                    var date = likelyCompletionAtTarget.Attribute("date").Value;

                }


                


                // if possible, lets try a binary search (to what resolution?)

                // loop

                // find the start date half way between last and the target date

                // does it complete with time to spare

                // try again...

            }
            finally
            {
                // reset the data
                data.Execute.Deliverables = originalDeliverables;
            }
            
            return result;
        }





    }
}
