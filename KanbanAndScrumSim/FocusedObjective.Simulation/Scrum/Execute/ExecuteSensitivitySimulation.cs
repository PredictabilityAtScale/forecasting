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
    internal class ExecuteSensitivitySimulation
    {
        internal static XElement AsXML(SimulationData data, BackgroundWorker worker = null)
        {
            XElement result = new XElement("sensitivity",
                                   new XAttribute("success", "false"));

            Stopwatch timer = new Stopwatch();
            timer.Restart();

            // get baseline
            Scrum.MonteCarloResultSummary baseline = new Scrum.MonteCarloResultSummary(data,
               ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, data.Execute.Sensitivity.Cycles, worker, "Baseline Test. Cycle {0}"));

            List<SensitivityTestData> testData = performSensitivitySimulation(data, data.Execute.Sensitivity.Cycles, worker);

            // process the run summaries into a result...
            result = processResults(data, baseline, testData);
            result.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));



            return result;
        }

        internal static double calculateImprovement(dynamic originalValue, dynamic newValue)
        {
            return Math.Round((((originalValue * 1.0) - (newValue * 1.0)) / (originalValue * 1.0)) * 100.0, 2);
        }

        private static XElement processResults(SimulationData data, Scrum.MonteCarloResultSummary baseline, List<SensitivityTestData> testData)
        {
            XElement result = new XElement("sensitivity");

            result.Add(new XAttribute("estimateMultiplier", data.Execute.Sensitivity.EstimateMultiplier));
            result.Add(new XAttribute("occurrenceMultiplier", data.Execute.Sensitivity.OccurrenceMultiplier));
            result.Add(new XAttribute("iterationMultiplier", data.Execute.Sensitivity.IterationMultiplier));
            result.Add(new XAttribute("sensitivityType", data.Execute.Sensitivity.SensitivityType.ToString()));
            result.Add(new XAttribute("sortOrder", data.Execute.Sensitivity.SortOrder.ToString()));

            // order the results
            // determine which result made the most impact
           List<SensitivityTestData> orderedResults;

          if (data.Execute.Sensitivity.SortOrder == SortOrderEnum.Ascending)
              orderedResults = testData.OrderBy(t => t.Result.Iterations.Average - baseline.Iterations.Average).ToList();
          else
              orderedResults = testData.OrderByDescending(t => t.Result.Iterations.Average - baseline.Iterations.Average).ToList();


            result.Add( new XElement("tests", 
                orderedResults.Select( 
                (r,i) => new XElement(
                    "test", 
                    new XAttribute("index", i),
                    new XAttribute("type", r.Type.ToString()),
                    new XAttribute("name", r.Name),
                    new XAttribute("changeType", r.ChangeType.ToString()),
                    new XAttribute("iterationDelta", Math.Round(r.Result.Iterations.Average - baseline.Iterations.Average, data.Execute.DecimalRounding)),
                    new XAttribute("originalOccurrenceLowBound", r.OriginalOccurrenceLowValue),
                    new XAttribute("newOccurrenceLowBound", r.NewOccurrenceLowValue),
                    new XAttribute("originalOccurrenceHighBound", r.OriginalOccurrenceHighValue),
                    new XAttribute("newOccurrenceHighBound", r.NewOccurrenceHighValue),
                    new XAttribute("originalEstimateLowBound", r.OriginalEstimateLowValue),
                    new XAttribute("newEstimateLowBound", r.NewEstimateLowValue),
                    new XAttribute("originalEstimateHighBound", r.OriginalEstimateHighValue),
                    new XAttribute("newEstimateHighBound", r.NewEstimateHighValue),

                    new XAttribute("originalIterationLowBound", r.OriginalIterationLowValue),
                    new XAttribute("newIterationLowBound", r.NewIterationLowValue),
                    new XAttribute("originalIterationHighBound", r.OriginalIterationHighValue),
                    new XAttribute("newIterationHighBound", r.NewIterationHighValue),

                    r.Result.AsXML())
                    )
                ));

            result.Add(new XElement("baseline", baseline.AsXML()));
            result.Add(new XAttribute("success", "true"));
            return result;
        }

        internal enum SensitivityObjectType
        {
            Defect,
            AddedScope,
            BlockingEvent,
            Column,
            Iteration
        }

        internal enum SensitivityChangeType
        {
            Occurrence,
            Estimate,
            StoryCount,
            StoryMix
        }

        internal struct SensitivityTestData
        {
            internal dynamic Object;
            internal string Name;
            internal double OriginalOccurrenceLowValue;
            internal double NewOccurrenceLowValue;
            internal double OriginalEstimateLowValue;
            internal double NewEstimateLowValue;
            internal double OriginalIterationLowValue;
            internal double NewIterationLowValue;
            internal double OriginalOccurrenceHighValue;
            internal double NewOccurrenceHighValue;
            internal double OriginalEstimateHighValue;
            internal double NewEstimateHighValue;
            internal double OriginalIterationHighValue;
            internal double NewIterationHighValue; 
            internal Scrum.MonteCarloResultSummary Result;
            internal SensitivityChangeType ChangeType;
            internal SensitivityObjectType Type;
        }

        internal static List<SensitivityTestData> performSensitivitySimulation(SimulationData data, int cycles, BackgroundWorker worker = null)
        {
            List<SensitivityTestData> testData = new List<SensitivityTestData>();

            int numberOfTests =
                (data.Setup.Defects != null ? data.Setup.Defects.Count : 0) +
                (data.Setup.AddedScopes != null ? data.Setup.AddedScopes.Count : 0) +
                (data.Setup.BlockingEvents != null ? (data.Setup.BlockingEvents.Count * 2) : 0) +
                1; // iteration multiplier

            int currentTest = 1;
            string progressString = "";

            // remember the original multiplier values
            double original = 1.0;

            // sim defects
            foreach (var defect in data.Setup.Defects)
            {
                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;

                original = defect.SensitivityOccurrenceMultiplier;

                try
                {
                    defect.SensitivityOccurrenceMultiplier *= data.Execute.Sensitivity.OccurrenceMultiplier;

                    Scrum.MonteCarloResultSummary test = new Scrum.MonteCarloResultSummary(data,
                        ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, cycles, worker, progressString));

                    testData.Add(new SensitivityTestData
                    {
                        Object = defect,
                        Name = defect.Name,
                        OriginalOccurrenceLowValue = defect.OccurrenceLowBound,
                        NewOccurrenceLowValue = defect.OccurrenceLowBound * defect.SensitivityOccurrenceMultiplier,
                        OriginalOccurrenceHighValue = defect.OccurrenceHighBound,
                        NewOccurrenceHighValue = defect.OccurrenceHighBound * defect.SensitivityOccurrenceMultiplier,
                        Result = test,
                        ChangeType = SensitivityChangeType.Occurrence,
                        Type = SensitivityObjectType.Defect
                    });
                }
                finally
                {
                    defect.SensitivityOccurrenceMultiplier = original;
                }
            }

            // sim added scope: 
            foreach (var scope in data.Setup.AddedScopes)
            {
                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;

                original = scope.SensitivityOccurrenceMultiplier;

                try
                {
                    scope.SensitivityOccurrenceMultiplier *= data.Execute.Sensitivity.OccurrenceMultiplier;

                    Scrum.MonteCarloResultSummary test = new Scrum.MonteCarloResultSummary(data,
                        ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, cycles, worker, progressString));

                    testData.Add(new SensitivityTestData
                    {
                        Object = scope,
                        Name = scope.Name,
                        OriginalOccurrenceLowValue = scope.OccurrenceLowBound,
                        NewOccurrenceLowValue = scope.OccurrenceLowBound * scope.SensitivityOccurrenceMultiplier,
                        OriginalOccurrenceHighValue = scope.OccurrenceHighBound,
                        NewOccurrenceHighValue = scope.OccurrenceHighBound * scope.SensitivityOccurrenceMultiplier,
                        Result = test,
                        ChangeType = SensitivityChangeType.Occurrence,
                        Type = SensitivityObjectType.AddedScope
                    });
                }
                finally
                {
                    scope.SensitivityOccurrenceMultiplier = original;
                }
            }
            
            // sim blocked: 
            foreach (var block in data.Setup.BlockingEvents)
            {
                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;

                original = block.SensitivityOccurrenceMultiplier;

                try
                {
                    block.SensitivityOccurrenceMultiplier *= data.Execute.Sensitivity.OccurrenceMultiplier;

                    Scrum.MonteCarloResultSummary test = new Scrum.MonteCarloResultSummary(data,
                        ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, cycles, worker, progressString));

                    testData.Add(new SensitivityTestData
                    {
                        Object = block,
                        Name = block.Name,
                        OriginalOccurrenceLowValue = block.OccurrenceLowBound,
                        NewOccurrenceLowValue = block.OccurrenceLowBound * block.SensitivityOccurrenceMultiplier,
                        OriginalOccurrenceHighValue = block.OccurrenceHighBound,
                        NewOccurrenceHighValue = block.OccurrenceHighBound * block.SensitivityOccurrenceMultiplier,
                        Result = test,
                        ChangeType = SensitivityChangeType.Occurrence,
                        Type = SensitivityObjectType.BlockingEvent
                    });
                }
                finally
                {
                    block.SensitivityOccurrenceMultiplier = original;
                }

                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;
                
                original = block.SensitivityEstimateMultiplier;

                try
                {
                    block.SensitivityEstimateMultiplier *= data.Execute.Sensitivity.EstimateMultiplier;

                    Scrum.MonteCarloResultSummary test = new Scrum.MonteCarloResultSummary(data,
                        ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, cycles, worker, progressString));

                    testData.Add(new SensitivityTestData
                    {
                        Object = block,
                        Name = block.Name,
                        OriginalEstimateLowValue = block.EstimateLowBound,
                        NewEstimateLowValue = block.EstimateLowBound * (block.SensitivityEstimateMultiplier),
                        OriginalEstimateHighValue = block.EstimateHighBound,
                        NewEstimateHighValue = block.EstimateHighBound * (block.SensitivityEstimateMultiplier),
                        Result = test,
                        ChangeType = SensitivityChangeType.Estimate,
                        Type = SensitivityObjectType.BlockingEvent
                    });
                }
                finally
                {
                    block.SensitivityEstimateMultiplier = original;
                }
            }

            // iteration estimates
            original = data.Setup.Iteration.SensitivityIterationEstimateMultiplier;
            try
            {
                progressString = "Test " + currentTest + " of " + numberOfTests + " Cycle: {0}";
                currentTest++;

                data.Setup.Iteration.SensitivityIterationEstimateMultiplier *= data.Execute.Sensitivity.IterationMultiplier;

                Scrum.MonteCarloResultSummary test = new Scrum.MonteCarloResultSummary(data,
                    ExecuteMonteCarloSimulation.performMonteCarloSimulation(data, cycles, worker, progressString));

                testData.Add(new SensitivityTestData
                {
                    Object = data.Setup.Iteration,
                    Name = "Iteration",
                    OriginalIterationLowValue = data.Setup.Iteration.StoryPointsPerIterationLowBound,
                    NewIterationLowValue = data.Setup.Iteration.StoryPointsPerIterationLowBound * data.Setup.Iteration.SensitivityIterationEstimateMultiplier,
                    OriginalIterationHighValue = data.Setup.Iteration.StoryPointsPerIterationHighBound,
                    NewIterationHighValue = data.Setup.Iteration.StoryPointsPerIterationHighBound * data.Setup.Iteration.SensitivityIterationEstimateMultiplier,
                    Result = test,
                    ChangeType = SensitivityChangeType.Estimate,
                    Type = SensitivityObjectType.Iteration
                });
            }
            finally
            {
                data.Setup.Iteration.SensitivityIterationEstimateMultiplier = original;
            }            
            

            // # stories
            // # H/M/L

            return testData;

        }
    }
}
