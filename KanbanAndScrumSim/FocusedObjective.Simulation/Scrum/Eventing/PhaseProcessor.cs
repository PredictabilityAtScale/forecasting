using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Scrum
{

    internal class PhaseProcessor : IDisposable
    {
        private bool disposed = false;

        private ScrumSimulation _simulator;
        private Iteration _latestIteration;
        private SetupPhaseData _lastPhaseApplied = null;

        private int _originalStoryCount = 0;
        private int _storiesCompletedSoFar = 0;

        private List<Story> _storiesInOriginalBacklog;

        internal PhaseProcessor(ScrumSimulation sim)
        {
            _simulator = sim;
            
            _originalStoryCount = _simulator.Backlog.Count();

            // copy the original cards to check later
            _storiesInOriginalBacklog = new List<Story>(_simulator.Backlog);

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
            sim.RaiseStoryStartEvent -= handleStoryStartEvent;
            sim.RaiseStoryStartEvent += handleStoryStartEvent;

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
                    if (_lastPhaseApplied != null)
                        reverseAllSensitivities(_lastPhaseApplied);

                    this._simulator.RaiseStoryStartEvent -= handleStoryStartEvent;
                    this._simulator.RaiseIterationStartEvent -= handleIterationStartEvent;

                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~PhaseProcessor()
        {
            Dispose (false);
        }

        private void handleStoryStartEvent(object sender, StoryEventArgs e)
        {
            // only proceed if there are phases defined
            if (!_simulator.SimulationData.Setup.Phases.Any())
                return;

            // % complete is the percent of original backlog stories completed
            if (_storiesInOriginalBacklog.Contains(e.Story))
                _storiesCompletedSoFar++;
        }

        private void calculateAndApplyPhase()
        {
            SetupPhaseData phase = null;

            if (_simulator.SimulationData.Setup.Phases.Unit == PhaseUnitEnum.Percentage)
            {
                // get the current percentage or original complete
                double pct = 0.0;

                // if we have processed past the original backlog count, assume 100%
                if (_storiesCompletedSoFar > _originalStoryCount)
                    pct = 100.0;

                pct = Math.Min(100.0, 100.0 - ((((double)_originalStoryCount - (double)_storiesCompletedSoFar) / (double)_originalStoryCount) * 100.0));

                // find the phase if there is one (remember if we have applied it)
                phase = (from p in _simulator.SimulationData.Setup.Phases
                         where pct >= p.Start &&
                               pct <= p.End
                         select p)
                         .FirstOrDefault();
            }
            else
            {
                // iterations or intervals

                // find the phase if there is one (remember if we have applied it)
                phase = (from p in _simulator.SimulationData.Setup.Phases
                         where _latestIteration.Sequence >= p.Start &&
                               _latestIteration.Sequence <= p.End
                         select p)
                         .FirstOrDefault();
            }

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
            _latestIteration.Phase = _lastPhaseApplied;
            _simulator.CurrentPhase = _lastPhaseApplied;

        }

        private void handleIterationStartEvent(object sender, IterationEventArgs e)
        {
            // only proceed if there are phases defined
            if (!_simulator.SimulationData.Setup.Phases.Any())
                return;

            _latestIteration = e.Iteration;



            calculateAndApplyPhase();

        }

        private void reverseAllSensitivities(SetupPhaseData phaseData)
        {
        }

        private void setAllSensitivities(SetupPhaseData phaseData)
        {
        }
    }
}
