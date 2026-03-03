using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using FocusedObjective.Common;
using FocusedObjective.Simulation.Extensions;
using System.ComponentModel;

namespace FocusedObjective.Simulation.Kanban.Execute
{
    internal class ExecuteAddStaffSimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker worker = null)
        {
            XElement result = new XElement("addStaff");

            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                // get baseline
                var baselineSim = ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, data.Execute.AddStaff.Cycles, worker, "Baseline Test. Cycle {0}");
                Kanban.MonteCarloResultSummary baseline = new Kanban.MonteCarloResultSummary(data, baselineSim);
                result.Add(ExecuteForecastDateSimulation.processResults(data, baselineSim /*.Select(r => r.Intervals)*/));

                List<Kanban.MonteCarloResultSummary> summaries = new List<Kanban.MonteCarloResultSummary>();
                summaries.Add(baseline);

                string legend = "baseline";

                // loop count times...
                for (int i = 0; i < Math.Abs(data.Execute.AddStaff.Count); i++)
                {
                    Kanban.MonteCarloResultSummary summary = null;

                    XElement suggestion = findAddStaffSuggestion(data, baseline, i, Math.Abs(data.Execute.AddStaff.Count), worker, out summary);
                    result.Add(suggestion);

                    if (suggestion.Attribute("columnId") != null)
                    {
                        int columnId = int.Parse(suggestion.Attribute("columnId").Value);

                        // update to the new Wip in the column setup
                        SetupColumnData setupColumn = data.Setup.Columns.Where(c => c.Id == columnId).First();
                        setupColumn.WipLimit = int.Parse(suggestion.Attribute("newWip").Value);

                        // , AND any phase column override WIPs
                        foreach (var phase in data.Setup.Phases)
                        {
                            if (phase.Columns.Any())
                            {
                                var col = phase.Columns.FirstOrDefault(p => p.ColumnId == setupColumn.Id);

                                if (col != null)
                                {
                                    if (data.Execute.AddStaff.Count > 0)
                                    {
                                        col.WipLimit += 1;
                                    }
                                    else
                                    {
                                        // never go below 1
                                        if (col.WipLimit > 1)
                                            col.WipLimit -= 1;
                                    }
                                }
                            }
                        }

                        summaries.Add(summary);
                        if (data.Execute.AddStaff.Count > 0)
                            legend += "|" + setupColumn.Name + "%2B1";
                        else
                            legend += "|" + setupColumn.Name + "%2D1";
                    }
                }

                result.Add(buildImprovementChart(data, legend, summaries));

                result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds));
                result.Add(new XAttribute("success", "true"));


            }
            else
            {
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 1, string.Format(Strings.Error1, "Scrum simulation"));
                result.Add(new XAttribute("success", "false"));
            }

            return result;
        }

        private static XElement buildImprovementChart(SimulationData data, string legend, List<Kanban.MonteCarloResultSummary> summaries)
        {
            string chartURL = data.Execute.AddStaff.GoogleImprovementUrlFormat;
            
            // @"http://chart.apis.google.com/chart?chxt=y&chbh=a&chs=600x400&cht=bvg&chco={1}&chd=t:{2}&chdl={3}&chds={4},{5}&chxr=0,{4},{5}&chg=0,10&chtt={0}+Improvement";

            string[] colors = new string[] { "3072F3", "FF9900", "FF0000", "00FF00", "0000FF", "FF9900", "FFFF88", "C2BDDD", "008000", "000000" }; 
            string barColors = string.Join(",", summaries.Select ( (x,i) => colors[i < 10 ? i : 9]));

            MonteCarloResultSummary first = summaries.FirstOrDefault();

                       
            string intervalsData = first.Intervals != null ? string.Join("|", summaries.Select(s => Math.Round(s.Intervals.Average).ToString())) : "0";
            string intervalsURL = string.Format(chartURL, "Intervals", barColors, intervalsData, legend, 0, first.Intervals != null ? Math.Round(summaries.Max(x=>x.Intervals.Average)) : 0.0); 
            
            string cycleTimeData = first.WorkCycleTime != null ? string.Join("|", summaries.Select(s => Math.Round(s.WorkCycleTime.Average).ToString())) : "0";
            string cycleTimeURL = string.Format(chartURL, "Cycle+Time", barColors, cycleTimeData, legend, 0, first.WorkCycleTime != null ? Math.Round(summaries.Max(x => x.WorkCycleTime.Average)) : 0.0); 
            
            string queuedData = first.QueuedPositions != null ?  string.Join("|", summaries.Select(s => Math.Round(s.QueuedPositions.Average).ToString())) : "0";
            string queuedPositionsURL = string.Format(chartURL, "Queued+Positions", barColors, queuedData, legend, 0, first.QueuedPositions != null ? Math.Round(summaries.Max(x => x.QueuedPositions.Average)) : 0.0); 
            
            string emptyData =  first.EmptyPositions != null ? string.Join("|", summaries.Select(s => Math.Round(s.EmptyPositions.Average).ToString())) : "0";
            string emptyPositionsURL = string.Format(chartURL, "Empty+Positions", barColors, emptyData, legend, 0, first.EmptyPositions != null ? Math.Round(summaries.Max(x => x.EmptyPositions.Average)) : 0.0); 

            return new XElement("chart",
                new XElement("intervals",
                    new XCData(intervalsURL)),
                    
                new XElement("cycleTime",
                    new XCData(cycleTimeURL)),
                    
                new XElement("queuedPositions",
                    new XCData(queuedPositionsURL)),

                new XElement("emptyPositions",
                    new XCData(emptyPositionsURL))
                    
                    );
        }

        private static XElement findAddStaffSuggestion(SimulationData data, Kanban.MonteCarloResultSummary baseline, int index, int totalCount, BackgroundWorker worker, out Kanban.MonteCarloResultSummary summary)
        {
            summary = null;

            // simulate combinations
            Dictionary<ExecuteAddStaffColumnsData, List<SimulationResultSummary>> testSims =
                new Dictionary<ExecuteAddStaffColumnsData, List<SimulationResultSummary>>();
            
            Dictionary<ExecuteAddStaffColumnsData, StatisticResults<double>> staffing = 
                new Dictionary<ExecuteAddStaffColumnsData, StatisticResults<double>>();

            if (!data.Execute.AddStaff.Columns.Any())
            {
                // No columns specified, do all of them! Except infinite WIP columns
                // No need to do  WIP more than count (even if this column was chosen every time)
                // These bounds consider any column phase overrides, the minimum and maximum of any WIP limit (column or phase) for that column is used
                foreach (var col in data.Setup.Columns.Where(c => c.WipLimit > 0))
                {
                    data.Execute.AddStaff.Columns.Add(
                        new ExecuteAddStaffColumnsData
                        {
                            Id = col.Id,
                            MinWip = Math.Max(1, col.FindMinimumColumnWip(data.Setup.Phases) - Math.Abs(data.Execute.AddStaff.Count)),
                            MaxWip = col.FindMaximumColumnWip(data.Setup.Phases) + Math.Abs(data.Execute.AddStaff.Count)
                        });
                }
            }

            int numberOfTests = totalCount * data.Execute.AddStaff.Columns.Count;
            int currentTest = (index * data.Execute.AddStaff.Columns.Count) + 1;
            string progressString = "";

            foreach (var addStaffColumn in data.Execute.AddStaff.Columns)
            {

                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;

                SetupColumnData setupColumn = data.Setup.Columns.Where(c => c.Id == addStaffColumn.Id).First();

                // we don't test buffer columns? do we?
                // yes, leave it in there hands!
                //if (setupColumn.IsBuffer == false)
                //{
                    int originalWIP = setupColumn.WipLimit;

                    int testWip = originalWIP;

                    if (data.Execute.AddStaff.Count > 0)
                        testWip = originalWIP + 1;
                    else
                        testWip = originalWIP - 1;


                    if (testWip <= addStaffColumn.MaxWip && testWip >= addStaffColumn.MinWip)
                    {
                        // increase/decrease the wip temporarily
                        setupColumn.WipLimit = testWip;

                        //BUG:What if phases account for the entire simulation, and the WIP limit for the column never applies? We might skip this test....

                        // increase/decrease any phase column overrides temporarily...
                        // stay within the min/max wip boundaries though
                        Dictionary<SetupPhaseColumnData, int> originalWipLimits = new Dictionary<SetupPhaseColumnData, int>();
                        foreach (var phase in data.Setup.Phases)
                        {
                            foreach (var col in phase.Columns)
                            {
                                if (col.ColumnId == setupColumn.Id)
                                {
                                    int phaseColTestWip = col.WipLimit;

                                    if (data.Execute.AddStaff.Count > 0)
                                        phaseColTestWip = col.WipLimit + 1;
                                    else
                                        phaseColTestWip = col.WipLimit - 1;

                                    if (phaseColTestWip <= addStaffColumn.MaxWip && phaseColTestWip >= addStaffColumn.MinWip)
                                    {
                                        originalWipLimits.Add(col, col.WipLimit);
                                        col.WipLimit = phaseColTestWip;
                                    }
                                }
                            }
                        }

                        // simulate
                        List<SimulationResultSummary> testResults = ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, data.Execute.AddStaff.Cycles, worker, progressString);

                        // remember this columns results
                        testSims.Add(addStaffColumn, testResults);

                        // return the wip to the original
                        setupColumn.WipLimit = originalWIP;

                        //return any PHASE column wip tests
                        foreach (var col in originalWipLimits.Keys)
                            col.WipLimit = originalWipLimits[col];

                        originalWipLimits.Clear();
                    }
                //}
            }

            // determine which result made the most impact
            Kanban.MonteCarloResultSummary lowestValue;

            int lowestIndex = -1;
            

            switch (data.Execute.AddStaff.OptimizeForLowest)
            {
                case OptimizeForLowestEnum.CycleTime:
                    //lowestValue = testSims.Values.Select(v => new MonteCarloResultSummary(data, v)).OrderBy(t => t.WorkCycleTime.Average).FirstOrDefault();
                    lowestIndex = testSims.Values.Select((v,i) => new { index = i, summary = new MonteCarloResultSummary(data, v) }).OrderBy(t => t.summary.WorkCycleTime.Average).FirstOrDefault().index;
                    break;
                case OptimizeForLowestEnum.Empty:
                    //lowestValue = testSims.Values.Select(v => new MonteCarloResultSummary(data, v)).OrderBy(t => t.EmptyPositions.Average).FirstOrDefault();
                    lowestIndex = testSims.Values.Select((v,i) => new { index = i, summary = new MonteCarloResultSummary(data, v) }).OrderBy(t => t.summary.EmptyPositions.Average).FirstOrDefault().index;
                    break;
                case OptimizeForLowestEnum.Intervals:
                    //lowestValue = testSims.Values.Select(v => new MonteCarloResultSummary(data, v)).OrderBy(t => t.Intervals.Average).FirstOrDefault();
                    lowestIndex = testSims.Values.Select((v,i) => new { index = i, summary = new MonteCarloResultSummary(data, v) }).OrderBy(t => t.summary.Intervals.Average).FirstOrDefault().index;
                    break;
                case OptimizeForLowestEnum.Queued:
                    //lowestValue = testSims.Values.Select(v => new MonteCarloResultSummary(data, v)).OrderBy(t => t.QueuedPositions.Average).FirstOrDefault();
                    lowestIndex = testSims.Values.Select((v,i) => new { index = i, summary = new MonteCarloResultSummary(data, v) }).OrderBy(t => t.summary.QueuedPositions.Average).FirstOrDefault().index;
                    break;
                case OptimizeForLowestEnum.QueuedAndEmpty:
                    //lowestValue = testSims
                    //    .Values
                    //    .Select(v => new MonteCarloResultSummary(data, v))
                    //    .Select(v => new { summary = v, value = v.QueuedPositions.Average + v.EmptyPositions.Average })
                    //    .OrderBy(t => t.value)
                    //    .Select(y => y.summary)
                    //    .First();

                    lowestIndex = testSims
                        .Values
                        .Select(v => new MonteCarloResultSummary(data, v))
                        .Select((v,i) => new { index = i, summary = v, value = v.QueuedPositions.Average + v.EmptyPositions.Average })
                        .OrderBy(t => t.value)
                        .First()
                        .index;
                    break;
                default:
                    //lowestValue = testSims.Values.Select(v => new MonteCarloResultSummary(data, v)).OrderBy(t => t.Intervals.Average).FirstOrDefault();
                    lowestIndex = testSims.Values.Select((v,i) => new { index = i, summary = new MonteCarloResultSummary(data, v) }).OrderBy(t => t.summary.Intervals.Average).FirstOrDefault().index;
                    break;
            }

            ExecuteAddStaffColumnsData chosenColumn = null;
            
            /*
            if (lowestValue != null)
                chosenColumn = testSims.Where(s => s.Value == lowestValue).First().Key;
            */
            if (lowestIndex > -1)
                chosenColumn = testSims.Keys.ElementAt(lowestIndex);
            //

            // write out the result
            if (chosenColumn != null)
            {
                //summary = lowestValue;
                lowestValue = new MonteCarloResultSummary(data, testSims[chosenColumn]);
                summary = lowestValue;

                SetupColumnData setupColumn = data.Setup.Columns.Where(c => c.Id == chosenColumn.Id).First();
                int newWip = setupColumn.WipLimit;

                if (data.Execute.AddStaff.Count > 0)
                    newWip = setupColumn.WipLimit + 1;
                else
                    newWip = setupColumn.WipLimit - 1;


                XElement suggestion = new XElement("wipSuggestion",
                    new XAttribute("index", index),
                    new XAttribute("columnId", chosenColumn.Id),
                    new XAttribute("columnName", setupColumn.Name),
                    new XAttribute("originalWip", setupColumn.WipLimit),
                    new XAttribute("newWip", newWip),

                    new XAttribute("newAverageActiveStaff", lowestValue.ColumnActivePositions != null ? Math.Round(lowestValue.ColumnActivePositions[setupColumn].Average, data.Execute.DecimalRounding) : 0.0),
                    new XAttribute("newMinimumActiveStaff", lowestValue.ColumnActivePositions != null ? Math.Round(lowestValue.ColumnActivePositions[setupColumn].Minimum, data.Execute.DecimalRounding) : 0.0),
                new XAttribute("newMaximumActiveStaff", lowestValue.ColumnActivePositions != null ? Math.Round(lowestValue.ColumnActivePositions[setupColumn].Maximum, data.Execute.DecimalRounding) : 0.0),

                    new XAttribute("intervalImprovement", baseline.Intervals != null ? calculateImprovement(baseline.Intervals.Average, lowestValue.Intervals.Average) : 0.0),
                    new XAttribute("cycleTimeImprovement", baseline.WorkCycleTime != null ? calculateImprovement(baseline.WorkCycleTime.Average, lowestValue.WorkCycleTime.Average) : 0.0),
                    new XAttribute("emptyPositionsImprovement", baseline.EmptyPositions != null ? calculateImprovement(baseline.EmptyPositions.Average, lowestValue.EmptyPositions.Average) : 0.0),
                    new XAttribute("queuedPositionsImprovement", baseline.QueuedPositions != null ? calculateImprovement(baseline.QueuedPositions.Average, lowestValue.QueuedPositions.Average) : 0.0),
                    new XAttribute("queuedAndEmptyPositionsImprovement", baseline.EmptyPositions != null && baseline.QueuedPositions != null ? calculateImprovement(baseline.EmptyPositions.Average + baseline.QueuedPositions.Average, lowestValue.EmptyPositions.Average + lowestValue.QueuedPositions.Average) : 0.0),


                    ExecuteForecastDateSimulation.processResults(data, testSims[chosenColumn] /*.Select(r => r.Intervals)*/),

                    new XElement("original", baseline.AsXML()),
                    new XElement("new", lowestValue.AsXML())
                    );

                ExecuteForecastDateSimulation.processResults(data, testSims[chosenColumn]/*.Select(r => r.Intervals)*/);

                return suggestion;
            }
            else
            {
                // no column found....
                XElement suggestion = new XElement("addStaffSuggestion",
                    new XAttribute("message", Strings.Message1));

                return suggestion;
            }
        }

        internal static double calculateImprovement(dynamic originalValue, dynamic newValue)
        {
            return Math.Round((((originalValue * 1.0) - (newValue * 1.0)) / (originalValue * 1.0)) * 100.0, 2);
        }
    }
}
