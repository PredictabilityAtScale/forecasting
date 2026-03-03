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

using FocusedObjective.Simulation.Extensions;

namespace FocusedObjective.Simulation.Kanban.Execute
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

            // this may not be needed for forecasting engines who just need speed! so it needs to be explicitly asked for...
            if (data.Execute.ForecastDate.ReturnProgressData || data.Execute.ReturnResults.Contains("IntervalCompletedCards"))
                data.Execute.ReturnResults = "Intervals|IntervalCompletedCards|TotalCost";
            else
                data.Execute.ReturnResults = "Intervals|TotalCost";
            
            // do sim
            List<Kanban.SimulationResultSummary> results;
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

        // fast version recycling the monte carlo runs for a forecast
        internal static XElement AsXML(SimulationData data, List<SimulationResultSummary> runSummaries, BackgroundWorker workerThread = null, string progressString = "")
        {
            XElement result = new XElement("forecastDate",
                                   new XAttribute("success", "false"));

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            result = processResults(data, runSummaries);

            result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));


            return result;
        }



        private static double getLiklihoodForIntervals(IEnumerable<int> intervalsData, int intervals)
        {
            // the percentage of results that were < or = this number of intervals
            int below = intervalsData.Where(i => i <= intervals).Count();
            int total = intervalsData.Count();

            return (below * 1.0) / (total * 1.0);
        }

        internal struct KanbanForecastDateData
        {
            internal int Intervals;
            internal int WorkDays;
            internal DateTime Date;
            internal double Likelihood;
            internal int Count;
            internal double Cost;
            internal bool TargetLikelihood;
            internal double CostOfDelay;
            internal int DelayInDays;
            internal DateTime LatestStartDate;
        }

        internal static XElement processResults(SimulationData data, List<SimulationResultSummary> results /*IEnumerable<int> intervals*/)
        {
            if (data.Execute.ForecastDate == null && data.Setup.ForecastDate == null)
                return new XElement("forecastDate");

            List<KanbanForecastDateData> dates = new List<KanbanForecastDateData>();
            StatisticResults<int> intervalResults = new StatisticResults<int>(results.Select(r => r.Intervals));
            bool firstLikelihoodAboveTarget = false;
            KanbanForecastDateData firstEntryAboveTarget = new KanbanForecastDateData();

            // get the interval range

            int min = (int)Math.Truncate(intervalResults.Minimum);
            int max = (int)Math.Truncate(intervalResults.Maximum);

            int range = max - min;
            if (range == 0)
            {
                var entry = new KanbanForecastDateData
                {
                    Intervals = min,
                    WorkDays = DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, min - 1)),
                    Date = DateHelpers.GetDateByWorkDays(DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, min - 1)), data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates),
                    Likelihood = getLiklihoodForIntervals(results.Select(r => r.Intervals), min),
                    Count = results.Count,
                    Cost = 0.0

                };


                //DONE: get the cost from one entry that has this interval result...
                //Cost = DateHelpers.GetCostByWorkDays(DateHelpers.GetWorkDaysForIntervalCount( data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, min - 1)), data.Setup.ForecastDate.CostPerDay)
                if (results.Any(r => r.Intervals == min))
                    entry.Cost = results.Where(r => r.Intervals == min).Average(i => i.TotalCost);


                if (!string.IsNullOrWhiteSpace(data.Setup.ForecastDate.TargetDate))
                {
                    entry.DelayInDays = DateHelpers.GetDelayInDays(entry.Date, data.Setup.ForecastDate.TargetDate, data.Execute.DateFormat);
                    entry.LatestStartDate = DateHelpers.GetLatestStartDate(entry.DelayInDays,data.Execute.DateFormat, data.Setup.ForecastDate.StartDate,   data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates);
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
                // get the histogram for intervals and work with it

                foreach (var row in intervalResults.Histogram)
                {
                    int thisIntervals = (int)Math.Ceiling(row.Key);

                    var entry = new KanbanForecastDateData
                    {
                        Intervals = thisIntervals,
                        WorkDays = DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, thisIntervals - 1)),
                        Date = DateHelpers.GetDateByWorkDays(DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, thisIntervals - 1)), data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates),
                        Likelihood = getLiklihoodForIntervals(results.Select(r => r.Intervals), thisIntervals),
                        Count = row.Value,
                        Cost = 0.0

                    };

                    //DONE: get the cost from one entry that has this interval result...or the average cost for the last entry at this interval
                    //Cost = DateHelpers.GetCostByWorkDays(DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, Math.Max(0, thisIntervals - 1)), data.Setup.ForecastDate.CostPerDay),
                    if (results.Any(r => r.Intervals == thisIntervals))
                        entry.Cost = results.Where(r => r.Intervals == thisIntervals).Average(i => i.TotalCost);

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
                        firstEntryAboveTarget = entry;
                    }

                    dates.Add(entry);
                }
            }

            XElement result = new XElement("forecastDate");

            result.Add(new XAttribute("startDate", data.Setup.ForecastDate.StartDate));
            result.Add(new XAttribute("intervalsToOneDay", data.Setup.ForecastDate.IntervalsToOneDay));
            result.Add(new XAttribute("workdays", data.Setup.ForecastDate.WorkDays));
            result.Add(new XAttribute("costPerDay", data.Setup.ForecastDate.CostPerDay));
            result.Add(new XAttribute("targetLikelihood", data.Setup.ForecastDate.TargetLikelihood));
            result.Add(new XAttribute("targetDate", data.Setup.ForecastDate.TargetDate)); 
            result.Add(new XAttribute("revenue", data.Setup.ForecastDate.Revenue.ToString(data.Execute.CurrencyFormat)));
            result.Add(new XAttribute("revenueUnit", data.Setup.ForecastDate.RevenueUnit));


            result.Add(new XElement("dates",
                dates.OrderByDescending(d => d.Likelihood).Select(d => new XElement("date",
                    new XAttribute("intervals", d.Intervals),
                    new XAttribute("workDays", d.WorkDays),
                    new XAttribute("date", d.Date.ToString(data.Execute.DateFormat)),
                    new XAttribute("likelihood", d.Likelihood.ToString(data.Execute.PercentageFormat)),
                    new XAttribute("cost", d.Cost.ToString(data.Execute.CurrencyFormat)),
                    new XAttribute("targetLikelihood", d.TargetLikelihood),
                    new XAttribute("delayInDays", d.DelayInDays),
                    new XAttribute("costOfDelay", d.CostOfDelay.ToString(data.Execute.CurrencyFormat)),
                    new XAttribute("latestStartDate", d.LatestStartDate.ToString(data.Execute.DateFormat)),
                    new XAttribute("count", d.Count)
                        )))
                    );

            result.Add(intervalResults.AsXML("intervals"));

            if (     (data.Execute.ForecastDate != null && data.Execute.ForecastDate.ReturnProgressData)
                  || (data.Execute.ReturnResults.Contains("IntervalCompletedCards")) )
            {
                // add progress variance chart data 

                if (results != null && results.Any()
                    && results[0].IntervalCompletedCards != null && results[0].IntervalCompletedCards.Any())
                {
                    XElement completedProgress = new XElement("progress");

                    completedProgress.Add(new XAttribute("dateFormat", data.Execute.DateFormat));

                    var maxInts = results.Max(r => r.IntervalCompletedCards.Last().Key);

                    for (var d = 0; d < maxInts; d++)
                    {
                        StatisticResults<int> r = new StatisticResults<int>(
                            results
                            .Where(v => v.IntervalCompletedCards.ContainsKey(d))
                            .Select(x => x.IntervalCompletedCards[d]));

                        //TODO: add weekend and skipped dates to keep axis complete?

                        var date = DateHelpers.GetDateByWorkDays( DateHelpers.GetWorkDaysForIntervalCount(data.Setup.ForecastDate.IntervalsToOneDay, d), data.Execute.DateFormat, data.Setup.ForecastDate.StartDate, data.Setup.ForecastDate.WorkDays, data.Setup.ForecastDate.ExcludedDates);
                        
                        //TODO get count of completed count by date... 
                        
                        //intervalResults.Histogram


                        XElement dateXML = new XElement("date",
                                new XAttribute("interval", d),
                                
                                new XAttribute("likelihood",
                                    getLiklihoodForIntervals(results.Select(i => i.Intervals), d)),

                                new XAttribute("targetLikelihood", firstLikelihoodAboveTarget == false ? false : d >= firstEntryAboveTarget.Intervals),

                                new XAttribute("date", date.ToString(data.Execute.DateFormat)),
                                r.AsXML("forecast"));


                        //TODO:check if there is an actual matching this date
                        var actualData =
                            data.Setup.ForecastDate.Actuals
                            .OrderBy(x => x.Date)
                            .ThenBy(y => y.Count)
                            .LastOrDefault(z => date.Date == z.Date.Date);

                        if (actualData != null)
                        {
                            // add actual count
                            if (actualData.Count > 0.0)
                                dateXML.Add(new XAttribute("actual", actualData.Count));

                            if (!string.IsNullOrWhiteSpace(actualData.Annotation))
                                dateXML.Add(new XAttribute("annotation", actualData.Annotation));
                        }

                        //TODO:add likelihood flags to put background in place        


                        completedProgress.Add(dateXML);
                    }

                    result.Add(completedProgress);

                }
            }

            result.Add(new XAttribute("success", "true"));
            return result;
        }


        
    }
}
