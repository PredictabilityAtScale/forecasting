using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Extensions;
using Troschuetz.Random;

namespace FocusedObjective.Simulation.Scrum
{
    internal class ScrumSimulation : IDisposable
    {
        // private fields
        private bool disposed = false;
        private SimulationData _simulationData = null;
        private List<Story> _backlogList = null;
        private List<Story> _completedWorkList = null;
        private List<Story> _allStoriesList = null;
        private List<Iteration> _iterations = null;
        private List<Story> _defects = null;
        private SetupPhaseData _currentPhase = null;

        private Distribution _skipDistribution = new ContinuousUniformDistribution(
            new ALFGenerator(), 0.0, 100.0);

        private Dictionary<SetupDistributionData, Distribution> _distributions = new Dictionary<SetupDistributionData, Distribution>();


        // constructors
        internal ScrumSimulation(SimulationData data)
        {
            _simulationData = data;
            data.SetCurrentThreadsCulture();
        }

        // internal properties
        internal SimulationData SimulationData
        {
            get
            {
                return _simulationData;
            }
        }

        internal List<Iteration> Iterations
        {
            get { return _iterations; }
        }

        internal List<Story> AllStories
        {
            get { return _allStoriesList; }
        }

        internal List<Story> Backlog
        {
            get { return _backlogList; }
        }

        internal List<Story> Defects
        {
            get { return _defects; }
        }

        public SetupPhaseData CurrentPhase
        {
            get { return _currentPhase; }
            set { _currentPhase = value; }
        }

        internal SetupClassOfServiceData DefaultClassOfService = new SetupClassOfServiceData
        {
            Name = "Default",
            Default = true
        };

        // eventing

        internal event EventHandler<StoryEventArgs> RaiseStoryStartEvent;
        internal event EventHandler<StoryEventArgs> RaiseStoryCompleteEvent;
        internal event EventHandler<IterationEventArgs> RaiseIterationStartEvent;
        internal event EventHandler<IterationEventArgs> RaiseIterationCompleteEvent;

        protected virtual void OnRaiseStoryStartEvent(StoryEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of a race condition if the last subscriber unsubscribes immediately after the null check and before the event is raised.
            EventHandler<StoryEventArgs> handler = RaiseStoryStartEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnRaiseStoryCompleteEvent(StoryEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of a race condition if the last subscriber unsubscribes immediately after the null check and before the event is raised.
            EventHandler<StoryEventArgs> handler = RaiseStoryCompleteEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnRaiseIterationStartEvent(IterationEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of a race condition if the last subscriber unsubscribes immediately after the null check and before the event is raised.
            EventHandler<IterationEventArgs> handler = RaiseIterationStartEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnRaiseIterationCompleteEvent(IterationEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of a race condition if the last subscriber unsubscribes immediately after the null check and before the event is raised.
            EventHandler<IterationEventArgs> handler = RaiseIterationCompleteEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
                handler(this, e);
        }

        // internal methods
        internal bool RunSimulation()
        {
            // initialize collections
            _iterations = new List<Iteration>();
            _completedWorkList = new List<Story>();
            _allStoriesList = new List<Story>();
            _defects = new List<Story>();

            Distribution storyPointsDistribution = null;
            if (!string.IsNullOrWhiteSpace(_simulationData.Setup.Iteration.StoryPointsPerIterationDistribution))
            {
                     SetupDistributionData distribution = 
                        _simulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name,_simulationData.Setup.Iteration.StoryPointsPerIterationDistribution, true) == 0);

                     if (distribution != null)
                         storyPointsDistribution = FocusedObjective.Distributions.DistributionHelper.CreateDistribution(distribution);
            }

            // build backlog
            buildDistributions();
            _backlogList = buildBacklog();

            // create the phases event handler
            PhaseProcessor phaseHandler = new PhaseProcessor(this);

            // value and date processor
            ValueAndDateProcessor valueAndDateHandler = new ValueAndDateProcessor(this, _simulationData.Setup.ForecastDate);

            // create event handlers
            // create a blocking event handler for each defined blocking event
            List<BlockingEventProcessor> _blockingEventList = new List<BlockingEventProcessor>();
            foreach (var block in _simulationData.Setup.BlockingEvents)
                _blockingEventList.Add(new BlockingEventProcessor(this, block));

            // create the added scope processors
            List<AddedScopeProcessor> _addedAcopeList = new List<AddedScopeProcessor>();
            foreach (var addedScope in _simulationData.Setup.AddedScopes)
                _addedAcopeList.Add(new AddedScopeProcessor(this, addedScope));

            // create the added scope processors
            List<DefectProcessor> _defectList = new List<DefectProcessor>();
            foreach (var defect in _simulationData.Setup.Defects)
                _defectList.Add(new DefectProcessor(this, defect));


            // add iteration 0
            _iterations.Add(new Iteration
            {
                Sequence = 0,
                CountStoriesInBacklog = _backlogList.Count,
                CountStoriesInComplete = 0,
                PreviousIteration = null
            });

            // loop maximum the limit of intervals (iterations in this scrum simulation case)
            for (int iter = 1; iter < _simulationData.Execute.LimitIntervalsTo; iter++)
            {
                // create the  iteration
                Iteration thisIteration = new Iteration
                {
                    Sequence = iter,
                    PreviousIteration = _iterations.LastOrDefault()
                };

                _iterations.Add(thisIteration);
                
                // raise iteration start event
                OnRaiseIterationStartEvent(new IterationEventArgs { Iteration = thisIteration });

                // points allocatable for last and this iteration
                double pointsToAllocateLastIteration = thisIteration.PreviousIteration.PointsAllocatableThisIteration;
                double pointsToAllocateThisIteration = getRandomStoryPoints(_simulationData.Setup.Iteration.StoryPointsPerIterationLowBound, _simulationData.Setup.Iteration.StoryPointsPerIterationHighBound, storyPointsDistribution, _simulationData.Setup.Iteration.SensitivityIterationEstimateMultiplier);
                thisIteration.PointsAllocatableThisIteration = pointsToAllocateThisIteration;

                // complete last iteration stories, carry over those that didn't fit or were blocked
                if (thisIteration.PreviousIteration != null)
                { 
                    // process stories from last iteration, in priority order
                    foreach (var story in thisIteration.PreviousIteration.Stories.OrderBy(s => s, StoryPriorityComparer.Instance))
                    {
                        /* blocked stories - they take up remaining points, but don't get any allocated "work" time until the BlockedPoints expires */
                        if (pointsToAllocateLastIteration - (story.TotalRemainingPoints + story.TotalRemainingBlockedPoints) >= 0)
                        {
                            // case: where enough remaining points in the iteration for the entire remaining and blocked points

                            // reduce last iteration allocation by blocked points
                            pointsToAllocateLastIteration -= story.TotalRemainingBlockedPoints;
                            story.BlockedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, story.TotalRemainingBlockedPoints);

                            // complete this story for last iteration
                            pointsToAllocateLastIteration -= story.TotalRemainingPoints;
                            story.CompletedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, story.TotalRemainingPoints);
                            story.Status = Enums.StoryStatusEnum.Completed;
                            _completedWorkList.Add(story);

                            OnRaiseStoryCompleteEvent(new StoryEventArgs { Story = story });
                        }
                        else
                        {
                            // take blocked points off first
                            if (story.TotalRemainingBlockedPoints > 0)
                            {
                                if (pointsToAllocateLastIteration - story.TotalRemainingBlockedPoints > 0)
                                {
                                    // case: enough room for all blocked points, but only partial story point progress
                                    pointsToAllocateLastIteration -= story.TotalRemainingBlockedPoints;
                                    story.BlockedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, story.TotalRemainingBlockedPoints);

                                }
                                else
                                {
                                    // case: not enough room for ALL of the remaining blocked work. Take what we can
                                    story.BlockedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, pointsToAllocateLastIteration);
                                    pointsToAllocateLastIteration = 0.0;
                                }
                            }

                            // if still points to allocate, decrease work
                            // take partial credit, and carry story over, and zero remaining points 
                            story.CompletedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, pointsToAllocateLastIteration);

                            if (story.TotalRemainingBlockedPoints > 0)
                                story.Status = Enums.StoryStatusEnum.Blocked;
                            else
                                story.Status = Enums.StoryStatusEnum.StillDoing;

                            pointsToAllocateLastIteration = 0.0;

                            // add to this iteration
                            thisIteration.Stories.Add(story);
                            pointsToAllocateThisIteration -= story.TotalRemainingPoints;
                        }
                    }
                }

                // fill remaining space from defects and backlog
                while ( (_backlogList.Any() || _defects.Any())
                        && pointsToAllocateThisIteration > 0)
                {
                    Story defectStory = null;
                    Story backlogStory = null;

                    if (_defects.Count > 0)
                        defectStory = _defects
                                    .OrderBy(d => d, StoryPriorityComparer.Instance)
                                    .FirstOrDefault();

                    backlogStory = nextAllowedBacklogStory(thisIteration); 
                    
                    bool useBacklogCard = 
                    (defectStory == null) || 
                    (backlogStory != null && backlogStory.CompareTo(defectStory) < 0);

                    Story story = null;

                    // defects
                    if (!useBacklogCard)
                        story = defectStory;
                    else
                        story = backlogStory;

                    // if there was a story or defect, but it was skipped - exit the loop
                    if (story == null)
                        break;


                    if ((pointsToAllocateThisIteration - story.TotalRemainingPoints > 0) ||
                        _simulationData.Setup.Iteration.AllowedToOverAllocate)
                    {
                        if (_backlogList.Contains(story))
                            _backlogList.Remove(story);

                        if (_defects.Contains(story))
                            _defects.Remove(story);

                        thisIteration.Stories.Add(story);
                        story.Status = Enums.StoryStatusEnum.Doing;

                        // raise StartStoryEvent event
                        OnRaiseStoryStartEvent(new StoryEventArgs { Story = story });
                        
                        // story blocked time added in event. we need to set the status AFTER StoryStart is raised.
                        if (story.TotalRemainingBlockedPoints > 0)
                            story.Status = Enums.StoryStatusEnum.Blocked;

                        pointsToAllocateThisIteration -=  story.TotalRemainingPoints;
                    }
                    else
                    {
                        break;
                    }
                }

                thisIteration.CountStoriesInBacklog = _backlogList.Count;
                thisIteration.CountStoriesInComplete = _completedWorkList.Count();

                thisIteration.ValueDeliveredSoFar = valueAndDateHandler.ValueDeliveredSoFar;
                thisIteration.CurrentDate = valueAndDateHandler.CurrentDate;
                    
                // update status history
                foreach (var story in _allStoriesList)
                    story.SnapshotStatusHistory(thisIteration.Sequence);

                // raise iteration end event
                OnRaiseIterationCompleteEvent(new IterationEventArgs { Iteration = thisIteration });

                // exit if no more backlog: stories, defects, or added scope
                if (thisIteration.CountStoriesInComplete == _allStoriesList.Count)
                    break;
            }
 
            // dispose event processors - unwite events to avoid memory leak.
            foreach (var item in _addedAcopeList)
                item.Dispose();

            foreach (var item in _blockingEventList)
                item.Dispose();

            foreach (var item in _defectList)
                item.Dispose();

            phaseHandler.Dispose();

            valueAndDateHandler.Dispose();

            // return true if completed in the number of iterations LESS than limitIntervalsTo. That is. Simulation complete successfully...
            return _iterations.Count < _simulationData.Execute.LimitIntervalsTo;
        }

        private void buildDistributions()
        {
            foreach (var dist in _simulationData.Setup.Distributions)
                _distributions.Add(dist, FocusedObjective.Distributions.DistributionHelper.CreateDistribution(dist));

            // add support for simulation model distribution
            foreach (var dist in _distributions.Values)
                ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(dist);
        }

        private double getRandomStoryPoints(double low, double high, Distribution distribution, double multiplier)
        {
            double phaseIterationMultiplier = this.CurrentPhase == null ? 1.0 : this.CurrentPhase.IterationMultiplier;

            if (distribution == null)
            {
                double delta = high - low;
                if (delta > 0)
                    return TrueRandom.NextDouble(low, high) * multiplier * phaseIterationMultiplier;
                else
                    return high * multiplier * phaseIterationMultiplier; // they are both the same!
            }
            else
            {
                return distribution.GetNextDoubleForDistribution() * multiplier * phaseIterationMultiplier;
            }
        }


        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // private and protected methods
        private List<Story> buildBacklog()
        {
            List<Story> result = new List<Story>();

            switch (_simulationData.Setup.Backlog.BacklogType)
            {
                case BacklogTypeEnum.Simple:
                    {
                        // not supported in Scrum simulation
                        throw new NotSupportedException("Simple backlog is not supported for Scrum simulation.");
                    }
                case BacklogTypeEnum.Custom:
                    {
                        if (_simulationData.Execute.Deliverables != "")
                        {
                            string[] deliverablesChosen = _simulationData.Execute.Deliverables.Split(new char[] { '|', ',' });

                            int index = -1;

                            foreach (var deliverableName in deliverablesChosen)
                            {
                                SetupBacklogDeliverableData deliverable = _simulationData.Setup.Backlog.Deliverables.Where(d => string.Compare(d.Name, deliverableName, true) == 0).FirstOrDefault();

                                if (deliverable != null)
                                {
                                    bool skip = false;
                                    if (deliverable.SkipPercentage > 0.0)
                                    {
                                        double random = _skipDistribution.NextDouble();
                                        skip = deliverable.SkipPercentage >= random;
                                    }

                                    if (!skip)
                                    {
                                        foreach (var custom in deliverable.CustomBacklog.Where(c => c.Completed == false))
                                        {
                                            Distribution dist = null;
                                            if (!string.IsNullOrWhiteSpace(custom.EstimateDistribution))
                                                dist = _distributions.First(d => d.Key.Name == custom.EstimateDistribution).Value;

                                            result.AddRange(
                                                (from i in Enumerable.Range(0, custom.Count)
                                                 select new Story
                                                 {
                                                     Index = index++,
                                                     Name = string.Format(_simulationData.Setup.Backlog.NameFormat, index, custom.Name, custom.Order, deliverable != null ? deliverable.Name : "", deliverable != null ? deliverable.Order.ToString() : ""),
                                                     EstimateDistribution = dist,
                                                     Status = Enums.StoryStatusEnum.InBacklog,
                                                     StoryType = Enums.StoryTypeEnum.Work,
                                                     CustomBacklog = custom,
                                                     Deliverable = deliverable,
                                                     Simulator = this
                                                 })
                                                );
                                        }
                                    }
                                }
                            }

                            //TODO: may not want to shuffle all deliverable in time. We might want the items in a deliverable shuffled, but the deliverables delivered sequentially.
                        }
                        else
                        {
                            int index = -1;

                            // do basic custom backlogs first
                            foreach (var custom in _simulationData.Setup.Backlog.CustomBacklog.Where(c => c.Completed == false))
                            {
                                Distribution dist = null;
                                if (!string.IsNullOrWhiteSpace(custom.EstimateDistribution))
                                    dist = _distributions.First(d => d.Key.Name == custom.EstimateDistribution).Value;

                                result.AddRange(
                                    (from i in Enumerable.Range(0, custom.Count)
                                     select new Story
                                     {
                                         Index = index++,
                                         Name = string.Format(_simulationData.Setup.Backlog.NameFormat, index, custom.Name, custom.Order, "", ""),
                                         Status = Enums.StoryStatusEnum.InBacklog,
                                         StoryType = Enums.StoryTypeEnum.Work,
                                         CustomBacklog = custom,
                                         EstimateDistribution = dist,
                                         Deliverable = null,
                                         Simulator = this
                                     })
                                    );
                            }

                            // now do all backlogs ina deliverable
                            // every deliverable as well
                            foreach (var deliverable in _simulationData.Setup.Backlog.Deliverables)
                            {
                                                                bool skip = false;
                                if (deliverable.SkipPercentage > 0.0)
                                {
                                    double random = _skipDistribution.NextDouble();
                                    skip = deliverable.SkipPercentage >= random;
                                }

                                if (!skip)
                                {

                                    foreach (var custom in deliverable.CustomBacklog.Where(c => c.Completed == false))
                                    {
                                        Distribution dist = null;
                                        if (!string.IsNullOrWhiteSpace(custom.EstimateDistribution))
                                            dist = _distributions.First(d => d.Key.Name == custom.EstimateDistribution).Value;

                                        result.AddRange(
                                            (from i in Enumerable.Range(0, custom.Count)
                                             select new Story
                                             {
                                                 Index = index++,
                                                 Name = string.Format(_simulationData.Setup.Backlog.NameFormat, index, custom.Name, custom.Order, deliverable != null ? deliverable.Name : "", deliverable != null ? deliverable.Order.ToString() : ""),
                                                 Status = Enums.StoryStatusEnum.InBacklog,
                                                 StoryType = Enums.StoryTypeEnum.Work,
                                                 CustomBacklog = custom,
                                                 EstimateDistribution = dist,
                                                 Deliverable = deliverable,
                                                 Simulator = this
                                             })
                                            );
                                    }
                                }
                            }

                        }

                        // shuffle the stories if that option is chosen for the backlog (default is true)
                        if (_simulationData.Setup.Backlog.Shuffle)
                            result.Shuffle();


                        result = result
                            .OrderBy(s => s, StoryPriorityComparer.Instance)
                            .ToList();
                        
                        // add the backlog to the all card list as well.
                        _allStoriesList.AddRange(result);
                        break;
                    }
            }

            return result;
        }

        //
        private Story nextAllowedBacklogStory(Iteration iteration)
        {
            Story result = null;

            if (_backlogList.Any())
            {
                if (SimulationData.Setup.ClassOfServices != null &&
                   SimulationData.Setup.ClassOfServices.Any())
                {
                    // get the # stories by COS on the board
                    var storiesByCOS = iteration
                                    .Stories
                                    .GroupBy(s => s.ClassOfService)
                                    .ToDictionary(g => g.Key, g => g.Count()) ;

                    result = _backlogList
                        .Where(s =>
                            // this story doesn't have a class of service
                            s.ClassOfService == null ||
                            // and do not exceed the number allowed on the board 
                            (storiesByCOS.ContainsKey(s.ClassOfService) == false || // 0 count otherwise
                                  s.ClassOfService.MaximumAllowedOnBoard > storiesByCOS[s.ClassOfService]) // non-zero count, make sure it is under the limit
                            )
                        .OrderBy(s => s, StoryPriorityComparer.Instance)
                        .FirstOrDefault();

                    if (result != null &&
                        result.ClassOfService != null &&
                        result.ClassOfService.SkipPercentage > 0.0)
                    {
                        // perhaps we skip...
                        double random = _skipDistribution.NextDouble();

                        if (result.ClassOfService.SkipPercentage >= random)
                        {
                            _backlogList.Remove(result);
                            completeStory(iteration, result);
                            result = nextAllowedBacklogStory(iteration);
                        }
                    }
                }
                else
                {
                    // if no COS, then just take the next most prioritized card 
                    if (result == null)
                    {
                        result = _backlogList
                            .OrderBy(c => c, StoryPriorityComparer.Instance)
                            .First();
                    }
                }
            }

            return result;
        }

        private void completeStory(Iteration iteration, Story story)
        {
            story.Status = Enums.StoryStatusEnum.Completed;
            _backlogList.Remove(story);
            _completedWorkList.Add(story);

            OnRaiseStoryCompleteEvent(new StoryEventArgs { Story = story });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    /* should i do this?
                    RaiseCardCompleteEvent = null;
                    RaiseCardMoveEvent = null;
                    RaiseTimeIntervalTickEvent = null;
                     * */
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                //_intervals = null;

                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~ScrumSimulation()
        {
            Dispose (false);
        }


    }
}

/*
                        // if story was marked for blocking (one or more blocking events)
                        double overRunBlockedTime = 0.0;
                        if (story.BlockedPoints > 0.0)
                        {
                            // increase the blocked time history with all remaining points from the last iteration
                            story.BlockedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, pointsToAllocateLastIteration);

                            if (story.IsBlocked)
                            {
                                // carry over, add to this iteration.
                                thisIteration.Stories.Add(story);
                                story.Status = Enums.StoryStatusEnum.Blocked;
                                pointsToAllocateLastIteration -= 0; // no time allocated for last iteration
                                pointsToAllocateThisIteration -= story.TotalRemainingPoints;
                            }
                            else
                            {
                                // was blocked, but now moving. Account for excess blocked time...
                                overRunBlockedTime = story.BlockedPointsHistory.Sum(v => v.Value) - story.BlockedPoints;


                                //story.CompletedPointsHistory(
                            }
                            
                            
                        }
                        else
                        {

 ----
 * 
 * 
                    // first process the doing and still doing cards (anything other than blocked to be safe). complete where possible
                    foreach (var story in thisIteration.PreviousIteration.Stories.Where(
                        s => s.StatusHistory[thisIteration.PreviousIteration.Sequence] != Enums.StoryStatusEnum.Blocked))
                    {
                        if (pointsToAllocateLastIteration - story.TotalRemainingPoints >= 0)
                        {
                            // complete this story for last iteration
                            pointsToAllocateLastIteration -= story.TotalRemainingPoints;
                            story.CompletedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, story.TotalRemainingPoints);
                            story.Status = Enums.StoryStatusEnum.Completed;
                            _completedWorkList.Add(story);

                            OnRaiseStoryCompleteEvent(new StoryEventArgs { Story = story });
                        }
                        else
                        {
                            // if still points to allocate, decrease work
                            // take partial credit, and carry story over, and zero remaining points 
                            story.CompletedPointsHistory.Add(thisIteration.PreviousIteration.Sequence, pointsToAllocateLastIteration);
                            story.Status = Enums.StoryStatusEnum.StillDoing;
                            pointsToAllocateLastIteration = 0.0;

                            // add to this iteration
                            thisIteration.Stories.Add(story);
                            pointsToAllocateThisIteration -= story.TotalRemainingPoints;
                        }
                    }

                    // now do the blocked cards. decrease points from blocking time BEFORE work time though
                    foreach (var story in thisIteration.PreviousIteration.Stories.Where(
                        s => s.StatusHistory[thisIteration.PreviousIteration.Sequence] == Enums.StoryStatusEnum.Blocked))
                    {
                        
                    }


 
 
 
 */