using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using FocusedObjective.Common;
using System.ComponentModel;

namespace FocusedObjective.Simulation.Scrum.Execute
{
    internal class ExecuteForecastDateSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker workerThread = null, string progressString = "")
        {
            XElement result = new XElement("forecastDate",
                                   new XAttribute("success", "false"));

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            // force return of only interval data as a perf improvement
            string origReturnResults = data.Execute.ReturnResults;
            data.Execute.ReturnResults = "Iterations";

            // do sim
            List<Scrum.SimulationResultSummary> results;

            try
            {
                results = ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, data.Execute.ForecastDate.Cycles, workerThread, progressString);
            }
            finally
            {
                data.Execute.ReturnResults = origReturnResults;
            }


            result = processResults(data, results);

            result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));

            return result;
        }

        internal static XElement AsXML(SimulationData data, List<SimulationResultSummary> runSummaries, BackgroundWorker workerThread = null, string progressString = "")
        {
            XElement result = new XElement("forecastDate",
                                   new XAttribute("success", "false"));

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            // use recycled results
            result = processResults(data, runSummaries);

            result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));

            return result;
        }

        private static double getLiklihoodForIterations(SimulationData data, List<Scrum.SimulationResultSummary> results, int iterations)
        {
            // the percentage of results that were < or = this number of intervals
            int below = results.Where(r => r.Iterations <= iterations).Count();
            int total = results.Count();

            return (below * 1.0) / (total * 1.0);
        }

        internal struct ScrumForecastDateData
        {
            internal int Iterations;
            internal int WorkDays;
            internal DateTime Date;
            internal double Likelihood;
            internal double Cost;
            internal bool TargetLikelihood;
            internal double CostOfDelay;
            internal int DelayInDays;
            internal DateTime LatestStartDate;
        }

        private static XElement processResults(SimulationData data, List<Scrum.SimulationResultSummary> results)
        {
            Scrum.MonteCarloResultSummary summary;
            summary = new Scrum.MonteCarloResultSummary(data, results);
            List<ScrumForecastDateData> dates = new List<ScrumForecastDateData>();

            // get the interval range
            int min = (int)Math.Truncate(summary.Iterations.Minimum);
            int max = (int)Math.Truncate(summary.Iterations.Maximum);

            int range = max - min;
            if (range == 0)
            {
                var entry = new ScrumForecastDateData
                {
                    Iterations = min,
                    WorkDays = DateHelpers.GetWorkDaysForIterationCount(data.Setup.ForecastDate.WorkDaysPerIteration, min),
                    Date =  DateHelpers.GetDateByWorkDays(DateHelpers.GetWorkDaysForIterationCount(data.Setup.ForecastDate.WorkDaysPerIteration, min), data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates),
                    Likelihood = getLiklihoodForIterations(data, results, min),
                    Cost = DateHelpers.GetCostByWorkDays(DateHelpers.GetWorkDaysForIterationCount( data.Setup.ForecastDate.WorkDaysPerIteration, min), data.Setup.ForecastDate.CostPerDay)
                };

                if (!string.IsNullOrWhiteSpace(data.Setup.ForecastDate.TargetDate))
                {
                    entry.DelayInDays = DateHelpers.GetDelayInDays(entry.Date, data.Setup.ForecastDate.TargetDate, data.Execute.DateFormat);
                    entry.LatestStartDate = DateHelpers.GetLatestStartDate(entry.DelayInDays, data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates);
                }

                // cost of delay
                if (!string.IsNullOrWhiteSpace(data.Setup.ForecastDate.TargetDate) && data.Setup.ForecastDate.Revenue > 0.0)
                    entry.CostOfDelay = DateHelpers.GetCostOfDelay(entry.Date, data.Setup.ForecastDate.TargetDate, data.Execute.DateFormat, data.Setup.ForecastDate.Revenue, data.Setup.ForecastDate.RevenueUnit);

                // mark the fist entry equal or above the target
                if (entry.Likelihood >= (data.Setup.ForecastDate.TargetLikelihood / 100.0))
                    entry.TargetLikelihood = true;

                dates.Add(entry);
            }
            else
            {
                // idea? get the histogram for intervals and work with it?
                bool firstLikelihoodAboveTarget = false;

                foreach (var row in summary.Iterations.Histogram)
                {
                    var entry = new ScrumForecastDateData
                    {
                        Iterations = (int)Math.Ceiling(row.Key),
                        WorkDays = DateHelpers.GetWorkDaysForIterationCount(data.Setup.ForecastDate.WorkDaysPerIteration, (int)Math.Ceiling(row.Key)),
                        Date = DateHelpers.GetDateByWorkDays(DateHelpers.GetWorkDaysForIterationCount(data.Setup.ForecastDate.WorkDaysPerIteration, (int)Math.Ceiling(row.Key)), data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates),
                        Likelihood = getLiklihoodForIterations(data, results, (int)Math.Ceiling(row.Key)),
                        Cost = DateHelpers.GetCostByWorkDays(DateHelpers.GetWorkDaysForIterationCount(data.Setup.ForecastDate.WorkDaysPerIteration, (int)Math.Ceiling(row.Key)), data.Setup.ForecastDate.CostPerDay)
                    };

                    if (!string.IsNullOrWhiteSpace(data.Setup.ForecastDate.TargetDate))
                    {
                        entry.DelayInDays = DateHelpers.GetDelayInDays(entry.Date, data.Setup.ForecastDate.TargetDate, data.Execute.DateFormat);
                        entry.LatestStartDate = DateHelpers.GetLatestStartDate(entry.DelayInDays, data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates);

                    }

                    // cost of delay
                    if (!string.IsNullOrWhiteSpace(data.Setup.ForecastDate.TargetDate) & data.Setup.ForecastDate.Revenue > 0.0)
                        entry.CostOfDelay = DateHelpers.GetCostOfDelay(entry.Date, data.Setup.ForecastDate.TargetDate, data.Execute.DateFormat, data.Setup.ForecastDate.Revenue, data.Setup.ForecastDate.RevenueUnit);

                    // mark the fist entry equal or above the target
                    if (!firstLikelihoodAboveTarget && entry.Likelihood >= (data.Setup.ForecastDate.TargetLikelihood / 100.0))
                    {
                        entry.TargetLikelihood = true;
                        firstLikelihoodAboveTarget = true;
                    }

                    dates.Add(entry);
                }
            }

            XElement result = new XElement("forecastDate");

            result.Add(new XAttribute("startDate", data.Setup.ForecastDate.StartDate));
            result.Add(new XAttribute("workDaysPerIteration", data.Setup.ForecastDate.WorkDaysPerIteration));
            result.Add(new XAttribute("workdays", data.Setup.ForecastDate.WorkDays));
            result.Add(new XAttribute("costPerDay", data.Setup.ForecastDate.CostPerDay));
            result.Add(new XAttribute("targetLikelihood", data.Setup.ForecastDate.TargetLikelihood));
            result.Add(new XAttribute("targetDate", data.Setup.ForecastDate.TargetDate));
            result.Add(new XAttribute("revenue", data.Setup.ForecastDate.Revenue.ToString(data.Execute.CurrencyFormat)));
            result.Add(new XAttribute("revenueUnit", data.Setup.ForecastDate.RevenueUnit));

            result.Add(new XElement("dates",
                dates.OrderByDescending(d => d.Likelihood).Select(d => new XElement("date",
                    new XAttribute("iterations", d.Iterations),
                    new XAttribute("workDays", d.WorkDays),
                    new XAttribute("date", d.Date.ToString(data.Execute.DateFormat)),
                    new XAttribute("likelihood", d.Likelihood.ToString(data.Execute.PercentageFormat)),
                    new XAttribute("cost", d.Cost.ToString(data.Execute.CurrencyFormat)),
                    new XAttribute("targetLikelihood", d.TargetLikelihood),
                    new XAttribute("delayInDays", d.DelayInDays),
                    new XAttribute("costOfDelay", d.CostOfDelay.ToString(data.Execute.CurrencyFormat)),
                    new XAttribute("latestStartDate", d.LatestStartDate.ToString(data.Execute.DateFormat))
                    ))
                        )
                    );
            
            result.Add(new XAttribute("success", "true"));
            return result;
        }


    }
}
