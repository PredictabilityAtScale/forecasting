using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Scrum
{

    internal class AddedScopeProcessor : IDisposable
    {
        private bool disposed = false;

        private ScrumSimulation _simulator;
        private SetupAddedScopeData _addedScope;
        private Distribution _occurrenceDistribution;
        private Distribution _estimateDistribution;

        private int _storiesPassedSoFar = 0;
        private int _storyCountToResetCounter = -1;
        private int _storyIndexToTrigger = -1;

        private double _storySizePassedSoFar = 0;
        private double _storySizeToResetCounter = -1;

        private string[] _phases;

        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;
        
        internal AddedScopeProcessor(ScrumSimulation sim, SetupAddedScopeData addedScope)
        {
            _simulator = sim;
            _addedScope = addedScope;

            // get an array of phases
            _phases = _addedScope
                .Phases
                .Split(new char[] { '|', ',' })
                .Where(s => !string.IsNullOrEmpty(s.Trim()))
                .Select(s => s.Trim())
                .ToArray();

            /* set the deliverable and custom backlog targets (cards only counted if they match this deliverable and/or customBacklog name) if they are specified.
            * customBacklog's can be within deliverables. If deliverable isn't specified look for the first that matches the name (priority to one that has no deliverable).
            */
            if (!string.IsNullOrWhiteSpace(this._addedScope.TargetDeliverable))
                _targetDeliverable = _simulator.SimulationData.Setup.Backlog.Deliverables.FirstOrDefault(c => c.Name == this._addedScope.TargetDeliverable);


            if (!string.IsNullOrWhiteSpace(this._addedScope.TargetCustomBacklog))
            {
                if (_targetDeliverable == null)
                {
                    _targetCustomBacklog = _simulator.SimulationData.Setup.Backlog.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.TargetCustomBacklog);
                    if (_targetCustomBacklog == null)
                    {
                        foreach (var d in _simulator.SimulationData.Setup.Backlog.Deliverables)
                        {
                            _targetCustomBacklog = d.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.TargetCustomBacklog);
                            if (_targetCustomBacklog != null)
                                break;
                        }
                    }
                }
                else
                {
                    _targetCustomBacklog = _targetDeliverable.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.TargetCustomBacklog);
                }
            }

            reset();

            if (_addedScope.OccurrenceDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _addedScope.OccurrenceDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                    _occurrenceDistribution = DistributionHelper.CreateDistribution(distribution);
            }

            if (_addedScope.EstimateDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _addedScope.EstimateDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                {
                    _estimateDistribution = DistributionHelper.CreateDistribution(distribution);
                    ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(_estimateDistribution);
                }
            }
            
            // connect to events required...

            //sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            //sim.RaiseCardMoveEvent += handleCardMoveEvent;
            //sim.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
            //sim.RaiseTimeIntervalTickEvent += handleTimeIntervalTickEvent;
            sim.RaiseStoryCompleteEvent -= handleStoryCompleteEvent; 
            sim.RaiseStoryCompleteEvent += handleStoryCompleteEvent;
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
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~AddedScopeProcessor()
        {
            Dispose (false);
        }

        private void handleStoryCompleteEvent(object sender, StoryEventArgs e)
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

            //todo: include defects and  added scope as well?
            if (e.Story.StoryType == Enums.StoryTypeEnum.Work)
            {
                // zero indicates do not count. Most often because sensitivy multiplier is 0.0
                if (_storyCountToResetCounter > 0 || _storySizeToResetCounter > 0)
                {
                    // stories completed. Increment the count
                    _storiesPassedSoFar++;
                    _storySizePassedSoFar += e.Story.TotalCompletedPoints;

                    // two types of counting, size and story card count
                    if (_addedScope.OccurenceType == OccurrenceTypeEnum.Size)
                    {
                        if (_storySizePassedSoFar >= _storySizeToResetCounter)
                        {
                            addStoriesToBacklog(e.Story);

                            // carry over any extra
                            _storySizePassedSoFar = _storySizePassedSoFar - _storySizeToResetCounter;
                            
                            // pick next candidate
                            reset();
                        }
                    }
                    else
                    {
                        // count or percentage occurence type

                        // if the number of cards passed equals the randomly chosen, add scope when this card when complete
                        if (_storiesPassedSoFar == _storyIndexToTrigger)
                            addStoriesToBacklog(e.Story);

                        // reset after high-bound scaled to 1 in x cards exceeded
                        if (_storiesPassedSoFar >= _storyCountToResetCounter)
                            reset();
                    }
                }
            }
        }

        private void addStoriesToBacklog(Story triggeringStory)
        {
            for (int count = 0; count < _addedScope.Count; count++)
            {
                // add scope
                int nextIndex = _simulator.AllStories.Max(i => i.Index) + 1;

                Story story = new Story
                {
                    StoryType = Enums.StoryTypeEnum.AddedScope,
                    Status = Enums.StoryStatusEnum.InBacklog,
                    Index = nextIndex,
                    Name = string.Format(_addedScope.Name, nextIndex),
                    AddedScopeData = _addedScope,
                    EstimateDistribution = _estimateDistribution,
                    Simulator = _simulator,
                    ClassOfServiceName = this._addedScope.ClassOfService,
                    CustomBacklog = triggeringStory.CustomBacklog, // put in the same backlog as the triggering story (to inherit order)
                    Deliverable = triggeringStory.Deliverable // put in the same deliverable as the triggering story (to inherit order)
                };

                _simulator.Backlog.Add(story);
                _simulator.AllStories.Add(story);
            }
        }

        private void reset()
        {
            _storiesPassedSoFar = 0;
            
            //_storySizePassedSoFar = 0; - can't do this here because we want to carry over excess points.

            pickNextCandidate();
        }

        private void pickNextCandidate()
        {
            if (_addedScope.OccurenceType == OccurrenceTypeEnum.Size)
            {
                _storySizeToResetCounter = Common.PickRandomOccurrenceValueDouble(_addedScope.OccurenceType, _addedScope.Scale, _addedScope.OccurrenceLowBound, _addedScope.OccurrenceHighBound, _addedScope.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
            }
            else
            {
                _storyCountToResetCounter = Common.PickRandomOccurrenceValueInt(_addedScope.OccurenceType, _addedScope.Scale, _addedScope.OccurrenceLowBound, _addedScope.OccurrenceHighBound, _addedScope.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
                _storyIndexToTrigger = pickRandomCardIndex(_storyCountToResetCounter);
            }
        }

        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }
    }
}
