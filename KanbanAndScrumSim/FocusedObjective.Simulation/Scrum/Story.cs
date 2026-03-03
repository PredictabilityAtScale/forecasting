using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;

namespace FocusedObjective.Simulation.Scrum
{

    internal class StoryPriorityComparer : IComparer<Story>
    {
        public static readonly IComparer<Story>
            Instance = new StoryPriorityComparer();

        public int Compare(Story first, Story other)
        {
            // if we are null, the other card must be higher priority
            if (first == null)
                return 1;

            // if the other is null, we must be higher priority
            if (other == null)
                return -1;


            // first deliverable order
            int ourDeliverablePriority = first.Deliverable != null ? first.Deliverable.Order : int.MaxValue;
            int otherDeliverablePriority = (other != null && other.Deliverable != null) ? other.Deliverable.Order : int.MaxValue;

            if (ourDeliverablePriority != otherDeliverablePriority)
                return ourDeliverablePriority.CompareTo(otherDeliverablePriority);

            // per: if just one backlog in a deliverable or not, exit quickly.
            if (first.Simulator.SimulationData.Setup.Backlog.CustomBacklog.Count() == 1)
                return 0;

            if (first.Simulator.SimulationData.Setup.Backlog.Deliverables.Count() == 1
                && first.Simulator.SimulationData.Setup.Backlog.Deliverables[0].CustomBacklog.Count() == 1)
                return 0;

            // then check for a difference in backlog order priority
            int ourBacklogPriority = first.CustomBacklog != null ? first.CustomBacklog.Order : int.MaxValue;
            int otherBacklogPriority = (other != null && other.CustomBacklog != null) ? other.CustomBacklog.Order : int.MaxValue;

            if (ourBacklogPriority != otherBacklogPriority)
                return ourBacklogPriority.CompareTo(otherBacklogPriority);

            // if the backlog order is the same, lets check the COS priority order
            int ourCosPriority = first.ClassOfService != null ? first.ClassOfService.Order : int.MaxValue;
            int otherCosPriority = (other != null && other.ClassOfService != null) ? other.ClassOfService.Order : int.MaxValue;

            if (ourCosPriority != otherCosPriority)
                return ourCosPriority.CompareTo(otherCosPriority);

            // if the COS priority order is the same, lets go by due date
            DateTime? ourDueDate = first.CustomBacklog != null ? first.CustomBacklog.SafeDueDate : DateTime.MaxValue;
            DateTime? otherDueDate = (other != null && other.CustomBacklog != null) ? other.CustomBacklog.SafeDueDate : DateTime.MaxValue;

            if (ourDueDate != otherDueDate)
                return ourDueDate.Value.CompareTo(otherDueDate);

            // we are the same priority, order by sort order
            return first.SortOrder.CompareTo(other.SortOrder);
        }
    }


    internal class Story : IComparable, IComparable<Story>
    {

        private double _calculatedStorySize = 0.0;
        private Dictionary<int, Enums.StoryStatusEnum> _statusHistory = new Dictionary<int, Enums.StoryStatusEnum>();
        private Dictionary<int, double> _completedPointsHistory = new Dictionary<int, double>();
        private Dictionary<int, double> _blockedPointsHistory = new Dictionary<int, double>();

        internal string Name { get; set; }
        internal int Index { get; set; }
        internal Enums.StoryStatusEnum Status { get; set; }
        internal Enums.StoryTypeEnum StoryType { get; set; }
        internal Contract.SetupBacklogCustomData CustomBacklog { get; set; }
        internal FocusedObjective.Contract.SetupDefectData DefectData { get; set; }
        internal FocusedObjective.Contract.SetupAddedScopeData AddedScopeData { get; set; }
        internal Distribution EstimateDistribution { get; set; }
        internal double CalculatedBlockedPoints { get; set; }

        internal Scrum.ScrumSimulation Simulator { get; set; }

        internal string ClassOfServiceName { get; set; }

        private SetupClassOfServiceData _cachedClassOfService = null;




        private double? _sortOrder = null;

        internal double SortOrder
        {
            get
            {
                if (!_sortOrder.HasValue)
                    if (Simulator.SimulationData.Setup.Backlog.Shuffle)
                        _sortOrder = TrueRandom.NextDouble(0, 1);
                    else
                        _sortOrder = Index;

                return _sortOrder.Value;
            }
        }

        internal double GetRemainingPoints (int iteration)
        { 
            return CalculatedStorySize - 
                _completedPointsHistory.Where(i => i.Key < iteration).Sum(v => v.Value);
        }

        internal double GetRemainingBlockedPoints(int iteration)
        {
            return CalculatedBlockedPoints -
                _blockedPointsHistory.Where(i => i.Key < iteration).Sum(v => v.Value);
        }
        
        internal double CalculatedStorySize
        {
            get
            {
                double result = _calculatedStorySize;

                if (_calculatedStorySize == 0.0)
                {
                    switch (this.StoryType)
                    {
                        case Enums.StoryTypeEnum.Work:
                            {
                                result = getRandomSize(getLowBound(), getHighBound(), EstimateDistribution, this.CustomBacklog == null ? 1.0 : 1.0 /*CustomBacklog.SensitivityEstimateMultiplier*/);
                                break;

                            }
                        case Enums.StoryTypeEnum.AddedScope:
                            {
                                result = getRandomSize(getLowBound(), getHighBound(), EstimateDistribution, 1.0);
                                break;
                            }
                        case Enums.StoryTypeEnum.Defect:
                            {
                                result = getRandomSize(getLowBound(), getHighBound(), EstimateDistribution, 1.0);
                                break;
                            }
                    }

                    _calculatedStorySize = result;
                }

                return _calculatedStorySize;
            }
        }

        internal double TotalCompletedPoints
        {
            get { return _completedPointsHistory.Sum(v => v.Value); }
        }

        internal double TotalRemainingPoints
        {
            get { return CalculatedStorySize - _completedPointsHistory.Sum(v => v.Value); }
        }

        internal double TotalRemainingBlockedPoints
        {
            get { return CalculatedBlockedPoints - _blockedPointsHistory.Sum(v => v.Value); }
        }


        internal bool IsBlocked
        {
            get { return _blockedPointsHistory.Sum(b => b.Value) < CalculatedBlockedPoints; }
        }
        
        internal Dictionary<int, double> CompletedPointsHistory
        {
            get { return _completedPointsHistory; }
        }

        internal Dictionary<int, double> BlockedPointsHistory
        {
            get { return _blockedPointsHistory; }
        }

        internal Dictionary<int, Enums.StoryStatusEnum> StatusHistory
        {
            get { return _statusHistory; }
        }

        internal void SnapshotStatusHistory(int interval)
        {
            _statusHistory.Add(interval, this.Status);
        }

        internal Enums.StoryStatusEnum StatusHistoryForInterval(int interval)
        {
            if (_statusHistory.ContainsKey(interval))
                return _statusHistory[interval];
            else
                return Enums.StoryStatusEnum.None;
        }

        internal SetupClassOfServiceData ClassOfService
        {
            get
            {
                // performance optimization - cache after calculated
                if (_cachedClassOfService != null)
                    return _cachedClassOfService;

                if (!string.IsNullOrWhiteSpace(ClassOfServiceName))
                {
                    // explicitly set by name
                    _cachedClassOfService = Simulator.SimulationData.Setup.ClassOfServices.FirstOrDefault(c => string.Compare(c.Name, ClassOfServiceName, true) == 0);
                    return _cachedClassOfService;
                }
                else
                {
                    if (CustomBacklog != null && !string.IsNullOrEmpty(CustomBacklog.ClassOfService))
                    {
                        // OR is it defined as part of the backlog
                        _cachedClassOfService = Simulator.SimulationData.Setup.ClassOfServices.FirstOrDefault(c => string.Compare(c.Name, CustomBacklog.ClassOfService, true) == 0);
                        return _cachedClassOfService;
                    }
                    else
                    {
                        // return the default. This was checked in the validate of setupclassofservicedata, but if NO cos exist, create one and return it.
                        _cachedClassOfService = COSMarkedAsDefault ?? Simulator.DefaultClassOfService;
                        return _cachedClassOfService;
                    }
                }
            }
        }

        private SetupClassOfServiceData _cosMarkedAsDefault = null;
        private bool hasEvaluatedCOSMarkedAsDefault = false;


        private SetupClassOfServiceData COSMarkedAsDefault
        {
            get
            {
                if (!hasEvaluatedCOSMarkedAsDefault)
                {
                    _cosMarkedAsDefault = Simulator.SimulationData.Setup.ClassOfServices.FirstOrDefault(c => c.Default == true);
                    hasEvaluatedCOSMarkedAsDefault = true;
                }

                return _cosMarkedAsDefault;
            }
        }

        private double getRandomSize(double low, double high, Distribution distribution, double multiplier)
        {
            double phaseMultiplier = Simulator.CurrentPhase == null ? 1.0 : Simulator.CurrentPhase.EstimateMultiplier;

            if (distribution == null)
            {
                double delta = high - low;
                if (delta > 0)
                    return TrueRandom.NextDouble(low, high) * multiplier * phaseMultiplier;
                else
                    return high * multiplier * phaseMultiplier; // they are both the same!
            }
            else
            {
                return distribution.GetNextDoubleForDistribution() * multiplier * phaseMultiplier;
            }
        }

        private double getLowBound()
        {
            double result = 0.0;

            if (this.DefectData != null)
                result = this.DefectData.EstimateLowBound;
            else if (this.AddedScopeData != null)
                result = this.AddedScopeData.EstimateLowBound;
            else if (this.CustomBacklog != null)
                result = this.CustomBacklog.EstimateLowBound;

            return result;
        }

        private double getHighBound()
        {
            double result = 0.0;

            if (this.DefectData != null)
                result = this.DefectData.EstimateHighBound;
            else if (this.AddedScopeData != null)
                result = this.AddedScopeData.EstimateHighBound;
            else if (this.CustomBacklog != null)
                result = this.CustomBacklog.EstimateHighBound;

            return result;
        }

        internal SetupBacklogDeliverableData Deliverable { get; set; }

        public int CompareTo(object obj)
        {
            Story other = obj as Story;

            if (other == null)
                return -1;

            return
                StoryPriorityComparer.Instance.Compare(this, other);
        }

        public int CompareTo(Story other)
        {
            return
                StoryPriorityComparer.Instance.Compare(this, other);
        }

    }
}
