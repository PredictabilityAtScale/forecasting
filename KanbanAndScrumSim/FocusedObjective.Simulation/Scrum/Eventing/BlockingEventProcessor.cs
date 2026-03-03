using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Scrum
{

    internal class BlockingEventProcessor : IDisposable
    {
        private bool disposed = false;

        private ScrumSimulation _simulator;
        private SetupBlockingEventData _blockingEvent;
        private Distribution _occurrenceDistribution;
        private Distribution _estimateDistribution;

        private int _storiesPassedSoFar = 0;
        private int _storyCountToResetCounter = -1;
        private int _storyIndexToBlock = -1;
        private double _chosenBlockPoints = 0.0;
        private Story _storyBlockedByThisEvent = null;
        private double _pointsBlockedSoFar = 0.0;

        private double _storySizePassedSoFar = 0;
        private double _storySizeToResetCounter = -1;

        private string[] _phases;
        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;

        private List<BlockingEventHistory> _history = new List<BlockingEventHistory>();

        internal BlockingEventProcessor(ScrumSimulation sim, SetupBlockingEventData blockingEvent)
        {
            _simulator = sim;
            _blockingEvent = blockingEvent;

            // get an array of phases
            _phases = _blockingEvent
                .Phases
                .Split(new char[] { '|', ',' })
                .Where(s => !string.IsNullOrEmpty(s.Trim()))
                .Select(s => s.Trim())
                .ToArray();

            if (_blockingEvent.OccurrenceDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _blockingEvent.OccurrenceDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                    _occurrenceDistribution = DistributionHelper.CreateDistribution(distribution);
            }

            if (_blockingEvent.EstimateDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _blockingEvent.EstimateDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                {
                    _estimateDistribution = DistributionHelper.CreateDistribution(distribution);
                    ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(_estimateDistribution);
                }
            }

            /* set the deliverable and custom backlog targets (cards only counted if they match this deliverable and/or customBacklog name) if they are specified.
            * customBacklog's can be within deliverables. If deliverable isn't specified look for the first that matches the name (priority to one that has no deliverable).
            */
            if (!string.IsNullOrWhiteSpace(this._blockingEvent.TargetDeliverable))
                _targetDeliverable = _simulator.SimulationData.Setup.Backlog.Deliverables.FirstOrDefault(c => c.Name == this._blockingEvent.TargetDeliverable);


            if (!string.IsNullOrWhiteSpace(this._blockingEvent.TargetCustomBacklog))
            {
                if (_targetDeliverable == null)
                {
                    _targetCustomBacklog = _simulator.SimulationData.Setup.Backlog.CustomBacklog.FirstOrDefault(c => c.Name == this._blockingEvent.TargetCustomBacklog);
                    if (_targetCustomBacklog == null)
                    {
                        foreach (var d in _simulator.SimulationData.Setup.Backlog.Deliverables)
                        {
                            _targetCustomBacklog = d.CustomBacklog.FirstOrDefault(c => c.Name == this._blockingEvent.TargetCustomBacklog);
                            if (_targetCustomBacklog != null)
                                break;
                        }
                    }
                }
                else
                {
                    _targetCustomBacklog = _targetDeliverable.CustomBacklog.FirstOrDefault(c => c.Name == this._blockingEvent.TargetCustomBacklog);
                }
            }

            reset();

            // connect to events required...
            sim.RaiseStoryStartEvent -= handleStoryStartEvent;
            sim.RaiseStoryStartEvent += handleStoryStartEvent;
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
                    this._simulator.RaiseStoryStartEvent -= handleStoryStartEvent; 
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~BlockingEventProcessor()
        {
            Dispose (false);
        }


        internal double BlockPoints
        {
            get
            {
                return _chosenBlockPoints;
            }
        }

        internal bool IsStoryBlocked(Story story, double pointsToBlock)
        {
            bool result = false;

            // zero indicates do not count. Most often because sensitivy multiplier is 0.0
            if (_storyCountToResetCounter > 0 || _storySizeToResetCounter > 0)
            {
                // are we blocking this card?
                if (_storyBlockedByThisEvent == story)
                {
                    _pointsBlockedSoFar += pointsToBlock;

                    // finished blocking yet?
                    if (_pointsBlockedSoFar <= _chosenBlockPoints)
                    {
                        result = true;
                    }
                    else
                    {
                        _pointsBlockedSoFar = 0.0;
                        pickNextCandidate();
                    }
                }
            }

            return result;
        }

        internal double ExcessBlockedPoints
        {
            get { return _pointsBlockedSoFar - _chosenBlockPoints; }
        }

        private void handleStoryStartEvent(object sender, StoryEventArgs e)
        {
            // phase check
            if (_phases.Length > 0 &&
                (
                   _simulator.CurrentPhase == null || // phases are specified, but we arent in a phase
                  (_simulator.CurrentPhase != null && !_phases.Contains(_simulator.CurrentPhase.Name)) // phases specified and we are in a specified phase
                )
               )
                return;

            // if targets are set, then exist if this card isn't a candidate
            if (_targetDeliverable != null)
                if (e.Story.Deliverable != _targetDeliverable)
                    return;

            if (_targetCustomBacklog != null)
                if (e.Story.CustomBacklog != _targetCustomBacklog)
                    return;

            _storiesPassedSoFar++;
            _storySizePassedSoFar += e.Story.CalculatedStorySize;

            // two types of counting, size and story card count
            if (_blockingEvent.OccurenceType == OccurrenceTypeEnum.Size)
            {
                if (_storySizePassedSoFar >= _storySizeToResetCounter)
                {
                    _storyBlockedByThisEvent = e.Story;
                    // += because one of the other blocked events might also have blocked this story
                    e.Story.CalculatedBlockedPoints += _chosenBlockPoints;
                    _history.Add(new BlockingEventHistory { Story = e.Story, TotalBlockedPoints = _chosenBlockPoints });

                    // carry over any extra
                    _storySizePassedSoFar = _storySizePassedSoFar - _storySizeToResetCounter;
                   
                }
            }
            else
            {
                // if the number of cards passed equals the randomly chosen, block this card when complete
                if (_storiesPassedSoFar == _storyIndexToBlock)
                {
                    _storyBlockedByThisEvent = e.Story;
                    // += because one of the other blocked events might also have blocked this story
                    e.Story.CalculatedBlockedPoints += _chosenBlockPoints;
                    _history.Add(new BlockingEventHistory { Story = e.Story, TotalBlockedPoints = _chosenBlockPoints });
                }

                // reset after high-bound scaled to 1 in x cards exceeded
                if (_storiesPassedSoFar >= _storyCountToResetCounter)
                    _storiesPassedSoFar = 0;
            }
        }

        private void reset()
        {
            _storyBlockedByThisEvent = null;
            _storiesPassedSoFar = 0;

            //_storySizePassedSoFar = 0; - can't do this here because we want to carry over excess points.

            pickNextCandidate();
        }

        private void pickNextCandidate()
        {
            if (_blockingEvent.OccurenceType == OccurrenceTypeEnum.Size)
            {
                _storySizeToResetCounter = Common.PickRandomOccurrenceValueDouble(_blockingEvent.OccurenceType, _blockingEvent.Scale, _blockingEvent.OccurrenceLowBound, _blockingEvent.OccurrenceHighBound, _blockingEvent.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase);
            }
            else
            {
                _storyCountToResetCounter = Common.PickRandomOccurrenceValueInt(_blockingEvent.OccurenceType, _blockingEvent.Scale, _blockingEvent.OccurrenceLowBound, _blockingEvent.OccurrenceHighBound, _blockingEvent.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
                _storyIndexToBlock = pickRandomCardIndex(_storyCountToResetCounter);
            }
            
            _chosenBlockPoints = pickRandomBlockPoints();
        }
        
        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }

        private double pickRandomBlockPoints()
        {
            return Common.PickRandomEstimateValueDouble(
                1.0,
                _blockingEvent.EstimateLowBound,
                _blockingEvent.EstimateHighBound,
                _blockingEvent.SensitivityEstimateMultiplier,
                _simulator.CurrentPhase,
                _estimateDistribution);
        }
    }
}
