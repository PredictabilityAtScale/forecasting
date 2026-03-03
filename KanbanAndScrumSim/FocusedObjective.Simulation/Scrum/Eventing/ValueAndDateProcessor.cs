using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;
using FocusedObjective.Common;

namespace FocusedObjective.Simulation.Scrum
{

    internal class ValueAndDateProcessor : IDisposable
    {
        private bool disposed = false;

        private ScrumSimulation _simulator;
        private int _iterationsPastSoFar = 0;
        private int _completedStoriesSoFar = 0;
        private double _valueDeliveredSoFar = 0.0;
        private ForecastDateData _startDate = null;
        
        private DayOfWeek[] _workdays = 
            new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        private Dictionary<int, DateTime> dateCache = new Dictionary<int, DateTime>();

        internal ValueAndDateProcessor(ScrumSimulation sim, ForecastDateData startDate)
        {
            _simulator = sim;
            _startDate = startDate;

            if (sim.SimulationData.Setup.ForecastDate != null)
            {
                _workdays = (from day in sim.SimulationData.Setup.ForecastDate.WorkDays.Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                             select (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true))
                             .ToArray();
            }

            // connect to events required...
            sim.RaiseStoryCompleteEvent -= handleStoryCompleteEvent;
            sim.RaiseStoryCompleteEvent += handleStoryCompleteEvent;
            sim.RaiseIterationStartEvent -= handleIterationStartEvent;
            sim.RaiseIterationStartEvent += handleIterationStartEvent;
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this._simulator.RaiseStoryCompleteEvent -= handleStoryCompleteEvent;
                    this._simulator.RaiseIterationStartEvent -= handleIterationStartEvent;
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~ValueAndDateProcessor()
        {
            Dispose (false);
        }

        private void handleIterationStartEvent(object sender, IterationEventArgs e)
        {
            _iterationsPastSoFar++;
        }

        internal DateTime? CurrentDate
        {
            get
            {
                if (_startDate != null)
                    return getDateFromIterations(_simulator.SimulationData, _iterationsPastSoFar-1);
                else
                    return null;
            }
        }

        internal double ValueDeliveredSoFar
        {
            get
            {
                return _valueDeliveredSoFar;
            }
        }

        private void handleStoryCompleteEvent(object sender, StoryEventArgs e)
        {
            _completedStoriesSoFar++;

            if (e.Story != null &&
                e.Story.CustomBacklog != null &&
                  (e.Story.CustomBacklog.ValueLowBound != 0.0 && 
                   e.Story.CustomBacklog.ValueHighBound != 0.0)
                )
            {
                _valueDeliveredSoFar += 
                    TrueRandom.NextDouble(
                        e.Story.CustomBacklog.ValueLowBound, 
                        e.Story.CustomBacklog.ValueHighBound);
            }
        }



        private bool isWorkDay(DayOfWeek day)
        {
            if (_workdays == null || _workdays.Length == 0)
                return false;

            return _workdays.Contains(day);
        }

        private DateTime getDateFromIterations(SimulationData data, int iterations)
        {
            // performance improvement - if we have calculated this before, just return it
            if (dateCache.ContainsKey(iterations))
                return dateCache[iterations];

            // this has been validated before we execute in the ExecuteForecastDateData class in Contracts.
            DateTime startDate; 
            int iterationCount = iterations;

            // performance - start from the latest date cached
            if (dateCache.Any())
            {
                var lastCachedDate =
                     dateCache
                     .OrderBy(i => i.Key)
                     .Last();

                startDate = lastCachedDate.Value;
                iterationCount = iterations - lastCachedDate.Key;
            }
            else
            {
                startDate = data.Setup.ForecastDate.StartDate.ToSafeDate(data.Execute.DateFormat, null).Value;
            }

            while ((!isWorkDay(startDate.DayOfWeek)
                    || data.Setup.ForecastDate.Excludes.Count(d => d.Date == startDate.Date) != 0))
            {
                startDate = startDate.AddDays(1);
            }

            DateTime result = new DateTime(startDate.Year, startDate.Month, startDate.Day);

            if (data.Setup.ForecastDate.WorkDaysPerIteration > 0)
            {
                int days = getDays(data.Setup.ForecastDate.WorkDaysPerIteration, iterationCount);

                // now find the calendar date
                while (days > 0)
                {
                    result = result.AddDays(1);

                    if (isWorkDay(result.DayOfWeek)
                        && data.Setup.ForecastDate.Excludes.Count(d => d.Date == result.Date) == 0)
                        days--;
                }
            }

            // remember this as a performance improvement
            dateCache.Add(iterations, result);

            return result;
        }

        private static double getCostFromIterations(SimulationData data, int iterations)
        {
            if (data.Setup.ForecastDate.WorkDaysPerIteration > 0)
            {
                int days = getDays(data.Setup.ForecastDate.WorkDaysPerIteration, iterations);

                return (days * 1.0) * data.Setup.ForecastDate.CostPerDay;
            }
            else
            {
                return 0.0;
            }
        }

        private static int getDays(int workDaysPerIteration, int iterations)
        {
            return workDaysPerIteration * iterations;
        }
    }
}
