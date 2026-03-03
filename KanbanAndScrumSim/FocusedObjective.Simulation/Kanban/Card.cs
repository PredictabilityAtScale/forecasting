using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using FocusedObjective.Contract;
using Troschuetz.Random;

namespace FocusedObjective.Simulation.Kanban
{
    internal class CardPriorityComparer : IComparer<Card>
    {
        public static readonly IComparer<Card> 
            Instance = new CardPriorityComparer(); 

        public int Compare(Card first, Card other)
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

            int delPriority = ourDeliverablePriority.CompareTo(otherDeliverablePriority);
            if (delPriority != 0) return delPriority;

            // then check for a difference in backlog order priority
            int ourBacklogPriority = first.CustomBacklog != null ? first.CustomBacklog.Order : int.MaxValue;
            int otherBacklogPriority = (other != null && other.CustomBacklog != null) ? other.CustomBacklog.Order : int.MaxValue;

            int backlogPriority = ourBacklogPriority.CompareTo(otherBacklogPriority);
            if (backlogPriority != 0) return backlogPriority;

            // if the backlog order is the same, lets check the COS priority order
            int ourCosPriority = first.ClassOfService != null ? first.ClassOfService.Order : int.MaxValue;
            int otherCosPriority = (other != null && other.ClassOfService != null) ? other.ClassOfService.Order : int.MaxValue;

            int cosPriority = ourCosPriority.CompareTo(otherCosPriority);
            if (cosPriority != 0) return cosPriority;

            // if the COS priority order is the same, lets go by due date
            DateTime ourDueDate = first.CustomBacklog != null ? first.CustomBacklog.SafeDueDate : DateTime.MaxValue;
            DateTime otherDueDate = (other != null && other.CustomBacklog != null) ? other.CustomBacklog.SafeDueDate : DateTime.MaxValue;

            int datePriority = ourDueDate.CompareTo(otherDueDate);
            if (datePriority != 0) 
                return datePriority;

            // we are the same priority, order by sort order
            return first.SortOrder.CompareTo(other.SortOrder);
        }
    }

    internal class Card : IComparable, IComparable<Card>
    {

        private Dictionary<SetupColumnData, double> _calculatedWorkTimeForColumn = new Dictionary<SetupColumnData, double>();
        private Enums.CardTypeEnum _cardType = Enums.CardTypeEnum.Work;
        private Dictionary<SetupColumnData, double> _timeSoFarForColumn = new Dictionary<SetupColumnData, double>();
        private Dictionary<int, Enums.CardStatusEnum> _statusHistory = new Dictionary<int, Enums.CardStatusEnum>();

        internal string Name { get; set; }
        internal int Index { get; set; }
        internal SetupDefectData DefectData { get; set; }
        internal Enums.CardStatusEnum Status { get; set; }
        internal SetupBacklogDeliverableData Deliverable { get; set; }

        internal KanbanSimulation Simulator { get; set; }
        
        // the backlog custom data that created this card
        internal SetupBacklogCustomData CustomBacklog { get; set; }

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
        
        internal Enums.CardTypeEnum CardType
        {
            get
            {
                return _cardType;
            }
            set
            {
                _cardType = value;
            }
        }

        internal string ClassOfServiceName { get; set; }

        private SetupClassOfServiceData _cachedClassOfService = null;
  

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

        internal Dictionary<int, Enums.CardStatusEnum> StatusHistory
        {
            get
            {
                return _statusHistory;
            }

        }

        internal double CalculatedRandomWorkTimeForColumn(SetupColumnData column)
        {
            double result = 0.0;

            if (_calculatedWorkTimeForColumn.ContainsKey(column))
            {
                result = _calculatedWorkTimeForColumn[column];
            }
            else
            {
                switch (this.CardType)
                {
                    case Enums.CardTypeEnum.Work:
                        {
                            result = getRandomTime(column, getLowBoundForColumn(column), getHighBoundForColumn(column), column.SensitivityEstimateMultiplier);
                            break;

                        }
                    case Enums.CardTypeEnum.AddedScope:
                        {
                            result = getRandomTime(column, getLowBoundForColumn(column), getHighBoundForColumn(column), 1.0);
                            break;
                        }
                    case Enums.CardTypeEnum.Defect:
                        {
                            result = getRandomTime(column, getLowBoundForColumn(column), getHighBoundForColumn(column), 1.0);
                            break;
                        }
                }

                _calculatedWorkTimeForColumn.Add(column, result);
            }

            return result;
        }

        private double getRandomTime(SetupColumnData column, double low, double high, double multiplier)
        {
            //TODO:Default distribution for defect column estimates for defects!
            
            // column distribution
            Distribution columnDist = null;
            Distribution backlogColumnDist = null;
            Distribution defectColumnDistribution = null;
            Distribution cosDistribution = null;

            double phaseMultiplier = Simulator.CurrentPhase == null ? 1.0 : Simulator.CurrentPhase.EstimateMultiplier;

            if (column != null && Simulator.ColumnDistributions.ContainsKey(column))
                columnDist = Simulator.ColumnDistributions[column];

            // if there is a custom backlog, check for a distribution
            if (column != null && this.CustomBacklog != null)
            {
                // column overrides take precendent, 
                SetupBacklogCustomColumnData colOverride =
                    this.CustomBacklog.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();

                if (colOverride != null)
                {
                    if (Simulator.BacklogColumnDistributions.ContainsKey(colOverride))
                        backlogColumnDist = Simulator.BacklogColumnDistributions[colOverride];
                    else
                        columnDist = null; // blank out the distribution. an override estimateLowBound and estimateHighBound is used...
                }
            }

            // if there is a defect, check for a distribution
            if (column != null && this.DefectData != null)
            {
                // column overrides take precendent, 
                SetupDefectColumnData defColOverride =
                    this.DefectData.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();

                if (defColOverride != null)
                {
                    if (!string.IsNullOrWhiteSpace(defColOverride.EstimateDistribution))
                    {
                        // lookup the dist by name
                        var setupDist = Simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => d.Name == defColOverride.EstimateDistribution);
                        if (setupDist != null)
                        {
                            if (Simulator.Distributions.ContainsKey(setupDist))
                            {
                                defectColumnDistribution = Simulator.Distributions[setupDist];
                            }
                        }
                    }
                    else
                    {
                        // override the lows and the highs
                        low = defColOverride.EstimateLowBound;
                        high = defColOverride.EstimateHighBound;

                        //TODO:BUG: what if there is  a default distribution based on weibull????
                    }
                }
            }

            // if there is a COS, check for a distribution
            if (column != null && this.ClassOfService != null)
            {
                // COS overrides take precendent, 
                SetupBacklogCustomColumnData colOverride =
                    this.ClassOfService.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();

                if (colOverride != null)
                {
                    if (Simulator.BacklogColumnDistributions.ContainsKey(colOverride))
                        cosDistribution = Simulator.BacklogColumnDistributions[colOverride];
                    else
                        columnDist = null; // blank out the distribution. an override estimateLowBound and estimateHighBound is used...
                }
            }


            if (columnDist == null && backlogColumnDist == null && defectColumnDistribution == null && cosDistribution == null)
            {
                double delta = high - low;
                if (delta > 0)
                    return TrueRandom.NextDouble(low, high) * multiplier * phaseMultiplier;
                else
                    return high * multiplier * phaseMultiplier; // they are both the same!
            }
            else
            {
                if (defectColumnDistribution != null)
                    return defectColumnDistribution.GetNextDoubleForDistribution() * multiplier * phaseMultiplier;
                else
                    if (backlogColumnDist != null)
                        return backlogColumnDist.GetNextDoubleForDistribution() * multiplier * phaseMultiplier;
                    else
                        if (cosDistribution != null)
                            return cosDistribution.GetNextDoubleForDistribution() * multiplier * phaseMultiplier;
                        else
                            return columnDist.GetNextDoubleForDistribution() * multiplier * phaseMultiplier;
            }
        }

        internal double GetTimeSoFarInColumn(SetupColumnData column)
        {
            if (_timeSoFarForColumn.ContainsKey(column))
                return _timeSoFarForColumn[column];
            else
                return 0;
        }

        internal void UpdateTimeSoFarInColumn(SetupColumnData column, double time)
        {
            if (_timeSoFarForColumn.ContainsKey(column))
                _timeSoFarForColumn[column] = (_timeSoFarForColumn[column] * 1.0) + (time * 1.0);
            else
                _timeSoFarForColumn.Add(column, (time * 1.0));
        }

        internal void SnapshotStatusHistory(int interval)
        {
            _statusHistory.Add(interval, this.Status);
        }

        internal Enums.CardStatusEnum StatusHistoryForInterval(int interval)
        {
            if (_statusHistory.ContainsKey(interval))
                return _statusHistory[interval];
            else
                return Enums.CardStatusEnum.None;
        }

        internal double CycleTime
        {
            get
            {
                if (Status != Enums.CardStatusEnum.Completed)
                    return 0;

                double result = 0;

                foreach (var column in _timeSoFarForColumn.Keys)
                    result += _timeSoFarForColumn[column];

                return result;
           }
        }

        private double getLowBoundForColumn(SetupColumnData column)
        {
            double result = column.EstimateLowBound;

            SetupBacklogCustomColumnData cosOverride = null;

            // if there is a cos use it
            if (this.ClassOfService != null)
            {
                // column overrides take precendent, otherwise apply percentage adjustment
                cosOverride =
                    this.ClassOfService.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();
            }

            if (cosOverride != null)
            {
                    result = cosOverride.EstimateLowBound;
            }
            else
            {
                // if there is a custom backlog use it
                if (this.CustomBacklog != null)
                {
                    // column overrides take precendent, otherwise apply percentage adjustment
                    SetupBacklogCustomColumnData colOverride =
                        this.CustomBacklog.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();

                    if (colOverride != null)
                    {
                        result = colOverride.EstimateLowBound;
                    }
                    else
                    {
                        double lowPct = Math.Max(0, this.CustomBacklog.PercentageLowBound);
                        // find what 1% of the original column range equals
                        double onePercent = (column.EstimateHighBound - column.EstimateLowBound) / 100;
                        result = column.EstimateLowBound + (lowPct * onePercent);
                    }

                }
            }

            return result;
        }

        private double getHighBoundForColumn(SetupColumnData column)
        {
            double result = column.EstimateHighBound;

            SetupBacklogCustomColumnData cosOverride = null;

            // if there is a cos use it
            if (this.ClassOfService != null)
            {
                // column overrides take precendent, otherwise apply percentage adjustment
                cosOverride =
                    this.ClassOfService.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();
            }

            if (cosOverride != null)
            {
                result = cosOverride.EstimateHighBound;
            }
            else
            {

                // if there is a custom backlog use it
                if (this.CustomBacklog != null)
                {
                    // column overrides take precendent, otherwise apply percentage adjustment
                    SetupBacklogCustomColumnData colOverride =
                        this.CustomBacklog.Columns.Where(c => c.ColumnId == column.Id).FirstOrDefault();

                    if (colOverride != null)
                    {
                        result = colOverride.EstimateHighBound;
                    }
                    else
                    {
                        // should we clip at 100%?
                        double highPct = Math.Max(0, this.CustomBacklog.PercentageHighBound);
                        // find what 1% of the original column range equals
                        double onePercent = (column.EstimateHighBound - column.EstimateLowBound) / 100;
                        result = column.EstimateLowBound + (highPct * onePercent);
                    }
                }
            }

            return result;
        }

        public int CompareTo(object obj)
        {
            Card other = obj as Card;
            
            if (other == null)
                return -1;

            return
                CardPriorityComparer.Instance.Compare(this, other);
        }

        public int CompareTo(Card other)
        {
            return
                CardPriorityComparer.Instance.Compare(this, other);
        }

        public long PullOrder { get; set; }
    }
}
