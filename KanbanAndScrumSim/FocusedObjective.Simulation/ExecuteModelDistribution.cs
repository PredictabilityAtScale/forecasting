using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Troschuetz.Random;

namespace FocusedObjective.Simulation
{
    internal static class ExecuteModelDistribution
    {
        internal static void RunModelForDistributionDataIfNeeded(Troschuetz.Random.Distribution distribution, BackgroundWorker workerThread = null)
        {
            if (distribution.Data.Shape != "model" && distribution.Data.Shape != "fromModel")
                return;
            
            try
            {

                if (workerThread != null
                    && workerThread.WorkerReportsProgress)
                {
                    workerThread.ReportProgress(0, string.Format("#Simulating '{0}'", distribution.Data.Name));
                }

                string path = distribution.Data.Path;

                // was going to combine base path here, but did it in the validate of distribution instead so it could be validated there.
                XDocument doc = XDocument.Load(path);
                
                // set the parameters

                //loop: locate the parameters by name and set the value
                //add validation logic in the contract...

                //split the parameters string as csv
                if (!string.IsNullOrWhiteSpace(distribution.Data.Parameters))
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>(); 
                    MatchCollection matches = new Regex("((?<=\")[^\"]*(?=\"(,|$)+)|(?<=,|^)[^,\"]*(?=,|$))").Matches(distribution.Data.Parameters);
                    for (int i = 0; i < matches.Count-1; i+=2)
                    {
                        string arg = matches[i].ToString();
                        string val = "";

                        if (i+1 <= matches.Count-1)
                            val = matches[i+1].ToString();

                        parameters.Add(arg, val);
                    }

                    ProcessVariablesRecursive(doc.FirstNode, parameters);

                }


                // load sim data
                FocusedObjective.Contract.SimulationData data = new Contract.SimulationData(doc);

                // delete all the execute commands
                data.Execute.Visual = null;
                data.Execute.SummaryStatistics = null;
                data.Execute.Sensitivity = null;
                data.Execute.AddStaff = null;
                

                // add a monteCarlo with cycles = count, and executeData only to intervals and iterations
                if (data.Execute.MonteCarlo == null)
                    data.Execute.MonteCarlo = new Contract.ExecuteMonteCarloData();
                
                data.Execute.MonteCarlo.Cycles = distribution.Data.Count;
                data.Execute.ReturnResults = "Intervals|Iterations";



               // perform sim
               Simulator sim = new Simulator(data.AsXML(data.Execute.SimulationType).ToString());
               bool success = sim.Execute();
               if (success)
               {
                   XElement sipElement = null;

                   //grab the sip and assign it to the distribution.
                   if (data.Execute.SimulationType == Contract.SimulationTypeEnum.Kanban)
                        sipElement = sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Element("sip");
                   else
                       sipElement = sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Element("sip");

                   ((ModelDistribution)distribution).Sip = sipElement;
               }

            }
            catch
            {
                // choosing to return an empty list as an indicator of an invalid model. 
                // model could fail because its invalid xml or invalid simml, or other.
                // we test the path is accessible earlier, but not that it sims correctly. 
                // testing validity could mean that creating the distribution could take hours and fail, and still not get a result.
                // so, we just run the sim when we need too.
            }

        }


        internal static void ProcessVariablesRecursive(XNode node, Dictionary<string,string> variables)
        {
            if (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.ProcessingInstruction)
                {
                    XProcessingInstruction pi = (XProcessingInstruction)node;
                    if (!string.IsNullOrEmpty(pi.Target) && (pi.Target == "variable" || pi.Target == "parameter"))
                    {
                        if (!string.IsNullOrEmpty(pi.Data))
                        {
                            Match m = Regex.Match(pi.Data, "name=\"(.*?)\"", RegexOptions.IgnoreCase);
                            if (m != null && m.Groups.Count > 1)
                            {
                                string name = m.Groups[1].Value;
                                Match mval = Regex.Match(pi.Data, "value=\"(.*?)\"", RegexOptions.IgnoreCase);
                                if (mval != null && mval.Groups.Count > 1)
                                {
                                    //string oldVal = mval.Groups[1].Value;
                                    if (variables.ContainsKey(name))
                                    {
                                        pi.Data = pi.Data.Remove(mval.Groups[1].Index, mval.Groups[1].Length);
                                        pi.Data = pi.Data.Insert(mval.Groups[1].Index, variables[name]);
                                    }
                                }
                            }
                        }
                    }
                }

                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    foreach (var child in ((XElement)node).Nodes())
                        ProcessVariablesRecursive(child, variables);
                }
            }
        }

    }
}
