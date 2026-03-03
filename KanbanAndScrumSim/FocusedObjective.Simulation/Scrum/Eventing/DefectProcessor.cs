using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Scrum
{

    internal class DefectProcessor : IDisposable
    {
        private bool disposed = false;

        private ScrumSimulation _simulator;
        private SetupDefectData _defect;
        private Distribution _occurrenceDistribution;
        private Distribution _estimateDistribution;

        private int _storiesPassedSoFar = 0;
        private int _storyCountToResetCounter = -1;
        private int _storyIndexToDefect = -1;
        
        private double _storySizePassedSoFar = 0;
        private double _storySizeToResetCounter = -1;

        private string[] _phases;

        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;

        internal DefectProcessor(ScrumSimulation sim, SetupDefectData defect)
        {
            _simulator = sim;
            _defect = defect;

            // get an array of phases
            _phases = _defect
                .Phases
                .Split(new char[] { '|', ',' })
                .Where(s => !string.IsNullOrEmpty(s.Trim()))
                .Select(s => s.Trim())
                .ToArray();

            if (_defect.OccurrenceDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _defect.OccurrenceDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                    _occurrenceDistribution = DistributionHelper.CreateDistribution(distribution);
            }

            if (_defect.EstimateDistribution != "")
            {
                SetupDistributionData distribution =
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, _defect.EstimateDistribution, true) == 0);

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
            if (!string.IsNullOrWhiteSpace(this._defect.TargetDeliverable))
                _targetDeliverable = _simulator.SimulationData.Setup.Backlog.Deliverables.FirstOrDefault(c => c.Name == this._defect.TargetDeliverable);


            if (!string.IsNullOrWhiteSpace(this._defect.TargetCustomBacklog))
            {
                if (_targetDeliverable == null)
                {
                    _targetCustomBacklog = _simulator.SimulationData.Setup.Backlog.CustomBacklog.FirstOrDefault(c => c.Name == this._defect.TargetCustomBacklog);
                    if (_targetCustomBacklog == null)
                    {
                        foreach (var d in _simulator.SimulationData.Setup.Backlog.Deliverables)
                        {
                            _targetCustomBacklog = d.CustomBacklog.FirstOrDefault(c => c.Name == this._defect.TargetCustomBacklog);
                            if (_targetCustomBacklog != null)
                                break;
                        }
                    }
                }
                else
                {
                    _targetCustomBacklog = _targetDeliverable.CustomBacklog.FirstOrDefault(c => c.Name == this._defect.TargetCustomBacklog);
                }
            }

            reset();

            // connect to events required...
            // for now, just process in the on-complete..
            //sim.RaiseStoryStartEvent -= handleStoryStartEvent;
            //sim.RaiseStoryStartEvent += handleStoryStartEvent;
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
        ~DefectProcessor()
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

            if (e.Story.StoryType == Enums.StoryTypeEnum.Work)
            {
                // zero indicates do not count. Most often because sensitivy multiplier is 0.0
                if (_storyCountToResetCounter > 0 || _storySizeToResetCounter > 0)
                {
                    _storiesPassedSoFar++;
                    _storySizePassedSoFar += e.Story.TotalCompletedPoints;

                    // two types of counting, size and story card count
                    if (_defect.OccurenceType == OccurrenceTypeEnum.Size)
                    {
                        if (_storySizePassedSoFar >= _storySizeToResetCounter)
                        {
                            createDefects(e.Story);

                            // carry over any extra
                            _storySizePassedSoFar = _storySizePassedSoFar - _storySizeToResetCounter;

                            reset();
                        }
                    }
                    else
                    {
                        // if the number of stories passed equals the randomly chosen, add defect now
                        if (_storiesPassedSoFar == _storyIndexToDefect)
                            createDefects(e.Story);

                        // reset after high-bound scaled to 1 in x cards exceeded
                        if (_storiesPassedSoFar >= _storyCountToResetCounter)
                            reset();
                    }
                }
            }
        }

        private void createDefects(Story storyCausingDefect)
        {
            for (int count = 0; count < _defect.Count; count++)
            {
                int nextIndex = _simulator.AllStories.Max(i => i.Index) + 1;

                Story story = new Story
                {
                    Index = nextIndex,
                    StoryType = Enums.StoryTypeEnum.Defect,
                    Status = Enums.StoryStatusEnum.InBacklog,
                    Name = string.Format(_defect.Name, nextIndex, storyCausingDefect.Index, storyCausingDefect.Name),
                    DefectData = _defect,
                    EstimateDistribution = _estimateDistribution,
                    Simulator = _simulator,
                    ClassOfServiceName = this._defect.ClassOfService,
                    Deliverable = storyCausingDefect.Deliverable, // to inherit ordering rules
                    CustomBacklog = storyCausingDefect.CustomBacklog  // to inherit ordering rules
                };

                _simulator.AllStories.Add(story);
                _simulator.Defects.Add(story);
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
            if (_defect.OccurenceType == OccurrenceTypeEnum.Size)
            {
                _storySizeToResetCounter = Common.PickRandomOccurrenceValueDouble(_defect.OccurenceType, _defect.Scale, _defect.OccurrenceLowBound, _defect.OccurrenceHighBound, _defect.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
            }
            else
            {
                _storyCountToResetCounter = Common.PickRandomOccurrenceValueInt(_defect.OccurenceType, _defect.Scale, _defect.OccurrenceLowBound, _defect.OccurrenceHighBound, _defect.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
                _storyIndexToDefect = pickRandomCardIndex(_storyCountToResetCounter);
            }
        }

        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }
    }
}
