using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using FocusedObjective.Common;
using System.ComponentModel;
using System.Threading;
using Troschuetz.Random;
using FocusedObjective.Distributions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FocusedObjective.Simulation.Kanban.Execute
{
    internal class ExecuteMonteCarloSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker workerThread = null)
        {
            

            XElement result = new XElement("monteCarlo",
                                   new XAttribute("success", "false"));

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            // limit cycles if license demands
            int allowedCycles = data.Execute.MonteCarlo.Cycles;

            List<Kanban.SimulationResultSummary> runSummaries;
            runSummaries = performMonteCarloSimulation(data, data.Execute.MonteCarlo.Cycles, workerThread);

            if (runSummaries.Count < allowedCycles && !workerThread.CancellationPending)
            {
                // at least one run didn't finish... report the error
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 2, string.Format(Strings.Error2, data.Execute.LimitIntervalsTo));
                result.Add(data.Errors);
            }
            else
            {
                // process the run summaries into a result...
                result = Kanban.ResultsMonteCarlo.AsXML(data, runSummaries);
                result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));

                //recycle runs for forecast date? Add an overload to forecast date that takes data and run summaries...
                if (data.Execute.ForecastDate != null)
                {
                    result.Add(ExecuteForecastDateSimulation.AsXML(data, runSummaries, workerThread));
                }
            }


            return result;
        }

        internal static List<Kanban.SimulationResultSummary> performMonteCarloSimulation(SimulationData data, int cycles, BackgroundWorker workerThread = null, string progressFormat = "")
        {
            System.Collections.Concurrent.ConcurrentBag<Kanban.SimulationResultSummary> runSummaries = new System.Collections.Concurrent.ConcurrentBag<Kanban.SimulationResultSummary>();

            // if any distributions need modelling, so them now
            foreach (var distData in data.Setup.Distributions)
            {
                if (distData.Shape.ToLower() == "model")
                {
                    var dist = DistributionHelper.CreateDistribution(distData);
                    ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(dist, workerThread);

                    // change to a SIP
                    distData.Shape = "sip";
                    distData.Data = ((ModelDistribution)dist).Sip.ToString();
                }
            }

            
            Parallel.For(0, cycles, (i, loopState) =>
            {
                if (workerThread == null || 
                    !workerThread.WorkerSupportsCancellation || 
                    workerThread.CancellationPending == false)
                {
                     Kanban.KanbanSimulation sim = new Kanban.KanbanSimulation(data);

                    try
                    {
                        if (sim.RunSimulation())
                            runSummaries.Add(new Kanban.SimulationResultSummary(sim));
                    }
                    finally
                    {
                        sim.Dispose();
                    }

                    if (workerThread != null 
                        && workerThread.WorkerReportsProgress)
                    {
                        int count = runSummaries.Count;

                        if (count % 25 == 0)
                            workerThread.ReportProgress(
                                (int)Math.Round(((100.0 / cycles) * count), 0),  
                                progressFormat == "" ? count.ToString() : string.Format(progressFormat, count));
                    }
                }
                else
                {
                    loopState.Stop();
                }
            });

            return runSummaries.ToList();
        }
    }
}
