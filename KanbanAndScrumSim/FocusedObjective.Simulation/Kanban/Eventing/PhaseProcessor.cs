using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class PhaseProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private TimeInterval _latestTimeInterval;
        private SetupPhaseData _lastPhaseApplied = null;

        private List<Card> _cardsInOriginalBacklog;
        private int _cardsPassedSoFar = 0;

        internal PhaseProcessor(KanbanSimulation sim)
        {
            _simulator = sim;

            // copy the original cards to check later
            _cardsInOriginalBacklog = new List<Card>(_simulator.BacklogList);

            // by default, make all current column WIP's the default
            //foreach (var column in _simulator.SimulationData.Setup.Columns)
            //    column.CurrentWipLimit = column.WipLimit;

            // Set initial phase
            // set the starting phase if there is one...
            SetupPhaseData initialPhase = _simulator
                .SimulationData
                .Setup
                .Phases
                .Where(p => p.Start == 0.0).FirstOrDefault();

            if (initialPhase != null)
            {
                setAllSensitivities(initialPhase);
                _lastPhaseApplied = initialPhase;
                _simulator.CurrentPhase = _lastPhaseApplied;
            }


            // connect to events required...
            sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            sim.RaiseCardMoveEvent += handleCardMoveEvent;
            //sim.RaiseCardCompleteEvent -= handleCardCompleteEvent;
            //sim.RaiseCardCompleteEvent += handleCardCompleteEvent;
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

                    this._simulator.RaiseCardMoveEvent -= handleCardMoveEvent; 
                    this._simulator.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
                    //this._simulator.RaiseCardCompleteEvent -= handleCardCompleteEvent;
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.

                if (_lastPhaseApplied != null)
                {
                    reverseAllSensitivities(_lastPhaseApplied);
                    _lastPhaseApplied = null;
                }

                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~PhaseProcessor()
        {
            Dispose (false);
        }
        
        private void handleCardMoveEvent(object sender, CardMoveEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Card Move: {0} (index: {1}) From: {2} pos {3} To: {4} pos {5}", e.Card.Name, e.Card.Index, e.FromColumn == null ? "" : e.FromColumn.Name, e.FromPosition, e.ToColumn == null ? "" : e.ToColumn.Name, e.ToPosition), "sim");

            // only proceed if there are phases defined
            if (!_simulator.SimulationData.Setup.Phases.Any())
                return;

            // only proceed if percentage tracking
            if (_simulator.SimulationData.Setup.Phases.Unit != PhaseUnitEnum.Percentage)
                return;

            // only count moved from the backlog
            if (e.FromColumn != null)
                return;

            // increment our counter if this card was in the original backlog
            if (_cardsInOriginalBacklog.Contains(e.Card))
                _cardsPassedSoFar++;

            // get the current percentage or original complete
            double pct = 0.0;

            // if we have processed past the original backlog count, assume 100%
            if (_cardsPassedSoFar > _cardsInOriginalBacklog.Count)
                pct = 100.0;

            pct = Math.Min(100.0, 100.0 - ((((double)_cardsInOriginalBacklog.Count - (double)_cardsPassedSoFar) / (double)_cardsInOriginalBacklog.Count) * 100.0));

            // find the phase if there is one (remember if we have applied it)
            var phase = (from p in _simulator.SimulationData.Setup.Phases
                         where pct >= p.Start &&
                               pct <= p.End
                         select p).FirstOrDefault();

            // remove or update new or leaving phases.
            if (phase != null)
            {
                if (phase != _lastPhaseApplied)
                {
                    if (_lastPhaseApplied != null)
                        reverseAllSensitivities(_lastPhaseApplied);
                    
                    setAllSensitivities(phase);
                    _lastPhaseApplied = phase;
                }
            }
            else
            {
                // if there WAS a phase applied, reverse the sensitivity
                if (_lastPhaseApplied != null)
                    reverseAllSensitivities(_lastPhaseApplied);
                
                _lastPhaseApplied = null;
            }

            // update the phase for the time interval
            if (_latestTimeInterval != null)
                _latestTimeInterval.Phase = _lastPhaseApplied;
            
            _simulator.CurrentPhase = _lastPhaseApplied;
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            _latestTimeInterval = e.TimeInterval;

            // update the phase for the time interval - default to the last one there was.
            // this will be overridden if a change is made in the CompleteCard Handler
            _latestTimeInterval.Phase = _lastPhaseApplied;

            // only proceed if there are phases defined
            if (!_simulator.SimulationData.Setup.Phases.Any())
                return;

            if (_simulator.SimulationData.Setup.Phases.Unit != PhaseUnitEnum.Interval)
                return;


            // find the phase if there is one (remember if we have applied it)
            var phase = (from p in _simulator.SimulationData.Setup.Phases
                         where e.TimeInterval.Sequence >= p.Start &&
                               e.TimeInterval.Sequence <= p.End
                         select p).FirstOrDefault();

            // remove or update new or leaving phases.
            if (phase != null)
            {
                if (phase != _lastPhaseApplied)
                {
                    if (_lastPhaseApplied != null)
                        reverseAllSensitivities(_lastPhaseApplied);

                    setAllSensitivities(phase);
                    _lastPhaseApplied = phase;
                }
            }
            else
            {
                // if there WAS a phase applied, reverse the sensitivity
                if (_lastPhaseApplied != null)
                    reverseAllSensitivities(_lastPhaseApplied);

                _lastPhaseApplied = null;
            }

            // update the phase for the time interval
            _latestTimeInterval.Phase = _lastPhaseApplied;
            _simulator.CurrentPhase = _lastPhaseApplied;
        }

        private void reverseAllSensitivities(SetupPhaseData phaseData)
        {
        }

        private void setAllSensitivities(SetupPhaseData phaseData)
        {
        }
    }
}
