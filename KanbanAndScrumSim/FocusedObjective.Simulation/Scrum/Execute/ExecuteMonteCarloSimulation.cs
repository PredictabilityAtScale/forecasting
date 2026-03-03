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

namespace FocusedObjective.Simulation.Scrum.Execute
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
            List<Scrum.SimulationResultSummary> runSummaries = performMonteCarloSimulation(data, data.Execute.MonteCarlo.Cycles, workerThread);

            if (runSummaries.Count < allowedCycles && !workerThread.CancellationPending)
            {
                // at least one run didn't finish... report the error
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 2, string.Format(Strings.Error2, data.Execute.LimitIntervalsTo));
                result.Add(data.Errors);
            }
            else
            {
                // process the run summaries into a result...
                result = Scrum.ResultsMonteCarlo.AsXML(data, runSummaries);
                result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));

                //recycle runs for forecast date? Add an overload to forecast date that takes data and run summaries...
                if (data.Execute.ForecastDate != null)
                {
                    result.Add(ExecuteForecastDateSimulation.AsXML(data, runSummaries, workerThread));
                }

            }

            return result;
        }

        internal static List<Scrum.SimulationResultSummary> performMonteCarloSimulation(SimulationData data, int cycles, BackgroundWorker workerThread = null, string progressFormat = "")
        {
            System.Collections.Concurrent.ConcurrentBag<Scrum.SimulationResultSummary> runSummaries = new System.Collections.Concurrent.ConcurrentBag<Scrum.SimulationResultSummary>();

            Parallel.For(0, cycles, (i, loopState) =>
            {
                if (workerThread == null ||
                    !workerThread.WorkerSupportsCancellation ||
                    workerThread.CancellationPending == false)
                {
                    Scrum.ScrumSimulation sim = new Scrum.ScrumSimulation(data);

                    try
                    {
                        if (sim.RunSimulation())
                            runSummaries.Add(new Scrum.SimulationResultSummary(sim));
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
