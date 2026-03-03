using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;
using FocusedObjective.Common;

namespace FocusedObjective.Simulation.Kanban
{

    internal class ValueAndDateProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private int _intervalsPastSoFar = 0;
        private int _completedCardsSoFar = 0;
        private double _valueDeliveredSoFar = 0.0;
        private ForecastDateData _startDate = null;
        
        private DayOfWeek[] _workdays = 
            new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        private Dictionary<int, DateTime> dateCache = new Dictionary<int, DateTime>();

        internal ValueAndDateProcessor(KanbanSimulation sim, ForecastDateData startDate)
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
            //sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            //sim.RaiseCardMoveEvent += handleCardMoveEvent;
            sim.RaiseCardCompleteEvent -= handleCardCompleteEvent;
            sim.RaiseCardCompleteEvent += handleCardCompleteEvent;
            sim.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
            sim.RaiseTimeIntervalTickEvent += handleTimeIntervalTickEvent;
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
                    //this._simulator.RaiseCardMoveEvent -= handleCardMoveEvent; 
                    this._simulator.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
                    this._simulator.RaiseCardCompleteEvent -= handleCardCompleteEvent;
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

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("New Interval Event: {0}", e.TimeInterval.Sequence), "sim");

            _intervalsPastSoFar++;

            if (e.TimeInterval.Phase != null && e.TimeInterval.Phase.CostPerDay > 0.0)
            {
                // a phase cost per day exists, use it
                if (e.TimeInterval.PreviousTimeInterval != null)
                {
                    e.TimeInterval.CostPerDaySoFar = e.TimeInterval.PreviousTimeInterval.CostPerDaySoFar + (e.TimeInterval.Phase.CostPerDay / e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.IntervalsToOneDay) ;
                }
                else
                {
                    e.TimeInterval.CostPerDaySoFar = (e.TimeInterval.Phase.CostPerDay / e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.IntervalsToOneDay);
                }
            }
            else
            {
                if (e.TimeInterval.PreviousTimeInterval != null)
                {
                    e.TimeInterval.CostPerDaySoFar = e.TimeInterval.PreviousTimeInterval.CostPerDaySoFar + (e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.CostPerDay / e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.IntervalsToOneDay);
                }
                else
                {
                    e.TimeInterval.CostPerDaySoFar = (e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.CostPerDay / e.TimeInterval.Simulator.SimulationData.Setup.ForecastDate.IntervalsToOneDay);
                }
            }
        }

        internal DateTime? CurrentDate
        {
            get
            {
                if (_startDate != null)
                    return getDateFromIntervals(_simulator.SimulationData, _intervalsPastSoFar-1);
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

        private void handleCardCompleteEvent(object sender, CardCompleteEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Card Complete Event: {0} (column:{1} pos:{2})", e.Card.Name, e.FromColumn.Name, e.FromPosition), "sim");

            _completedCardsSoFar++;

            if (e.Card != null &&
                e.Card.CustomBacklog != null &&
                  (e.Card.CustomBacklog.ValueLowBound != 0.0 && 
                   e.Card.CustomBacklog.ValueHighBound != 0.0)
                )
            {
                _valueDeliveredSoFar += 
                    TrueRandom.NextDouble(
                        e.Card.CustomBacklog.ValueLowBound, 
                        e.Card.CustomBacklog.ValueHighBound);
            }
        }



        private bool isWorkDay(DayOfWeek day)
        {
            if (_workdays == null || _workdays.Length == 0)
                return false;

            return _workdays.Contains(day);
        }

        private DateTime getDateFromIntervals(SimulationData data, int intervals)
        {
            // performance improvement - if we have calculated this before, just return it
            if (dateCache.ContainsKey(intervals))
                return dateCache[intervals];

            // this has been validated before we execute in the ExecuteForecastDateData class in Contracts.
            DateTime startDate; 
            int intervalCount = intervals;

            // performance - start from the latest date cached
            if (dateCache.Any())
            {
                var lastCachedDate =
                     dateCache
                     .OrderBy(i => i.Key)
                     .Last();

                startDate = lastCachedDate.Value;
                intervalCount = intervals - lastCachedDate.Key;
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

            if (data.Setup.ForecastDate.IntervalsToOneDay > 0)
            {
                int days = getDays(data.Setup.ForecastDate.IntervalsToOneDay, intervalCount);

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
            if( result.Date != startDate.Date)
                dateCache.Add(intervals, result);

            return result;
        }

        private int getDays(int intervalsToOneDay, int intervals)
        {
            return (int)Math.Floor((double)intervals / (double)intervalsToOneDay);
        }

    }
}
