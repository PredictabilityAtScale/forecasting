using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using FocusedObjective.Simulation.Extensions;
using System.Runtime.CompilerServices;
using Troschuetz.Random;
using FocusedObjective.Distributions;
using System.Security.Cryptography;
using FocusedObjective.Common;

namespace FocusedObjective.Simulation.Kanban
{

    internal class KanbanSimulation : IDisposable
    {
        internal KanbanSimulation(SimulationData data)
        {
            _simulationData = data;
            data.SetCurrentThreadsCulture();
        }

        #region private declarations-------------------------------------------

        private bool disposed = false;
        private SimulationData _simulationData = null;
        
        private OrderedList<Card> _backlogList;
        
        private List<Card> _completedWorkList;
        private List<Card> _allCardsList;
        private List<ReplenishIntervalProcessor> _replenishIntervalList;
        private List<CompleteIntervalProcessor> _completeIntervalList;
        private Dictionary<int, List<Card>> _defectBacklog = new Dictionary<int, List<Card>>();
        private List<TimeInterval> _intervals;
        
        private Dictionary<SetupColumnData, Distribution> _columnDistributions = new Dictionary<SetupColumnData, Distribution>();
        private Dictionary<SetupBacklogCustomColumnData, Distribution> _backlogColumnDistributions = new Dictionary<SetupBacklogCustomColumnData, Distribution>();
              
        private ExpediteCardBlockingEventProcessor _expediteBlockingEventProcessor;
        private ValueAndDateProcessor _valueAndDateProcessor;
        private Distribution _skipDistribution = new ContinuousUniformDistribution(
            new ALFGenerator(), 0.0, 100.0);

        private Random _positionShuffler = new Random();
        
        #endregion //private declarations

        #region Internal Declarations -----------------------------------------

        internal event EventHandler<CardMoveEventArgs> RaiseCardMoveEvent;
        internal event EventHandler<TimeIntervalTickEventArgs> RaiseTimeIntervalTickEvent;
        internal event EventHandler<CardCompleteEventArgs> RaiseCardCompleteEvent;

        internal Dictionary<SetupColumnData, Distribution> ColumnDistributions { get { return _columnDistributions; } }
        internal Dictionary<SetupBacklogCustomColumnData, Distribution> BacklogColumnDistributions { get { return _backlogColumnDistributions; } }
        
        // trying a global list of distributions...
        internal Dictionary<SetupDistributionData, Distribution> _distributions = new Dictionary<SetupDistributionData, Distribution>();
        internal Dictionary<SetupDistributionData, Distribution> Distributions { get { return _distributions; } }
        internal SetupPhaseData _currentPhase = null;

        internal SetupClassOfServiceData DefaultClassOfService = new SetupClassOfServiceData
        {
            Name = "Default",
            Default = true
        };

        #endregion // internal declarations
 

        public SetupPhaseData CurrentPhase
        {
            get { return _currentPhase; }
            set { _currentPhase = value; }
        }

        public int CurrentColumnWIPLimit(SetupColumnData column)
        {
            int result = column.WipLimit;

            if (CurrentPhase != null)
            {
                var pCol = CurrentPhase.Columns.FirstOrDefault(pc => pc.ColumnId == column.Id);
                if (pCol != null)
                    result = pCol.WipLimit;
            }

            return result;
        }

        protected virtual void OnRaiseCardMoveEvent(CardMoveEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CardMoveEventArgs> handler = RaiseCardMoveEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void OnRaiseTimeIntervalTickEvent(TimeIntervalTickEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<TimeIntervalTickEventArgs> handler = RaiseTimeIntervalTickEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void OnRaiseCardCompleteEvent(CardCompleteEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CardCompleteEventArgs> handler = RaiseCardCompleteEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        internal SimulationData SimulationData
        {
            get
            {
                return _simulationData;
            }
        }

        internal OrderedList<Card> BacklogList
        {
            get { return _backlogList; }
        }

        internal List<Card> CompletedWorkList
        {
            get { return _completedWorkList; }
        }

        internal List<Card> AllCardsList
        {
            get { return _allCardsList; }
        }

        internal List<TimeInterval> ResultTimeIntervals
        {
            get { return _intervals; }
        }


        internal bool RunSimulation()
        {
            const double intervalTime = 1.0;
            _completedWorkList = new List<Card>();
            _allCardsList = new List<Card>();
            _intervals = new List<TimeInterval>();

            // calculate and cache the column max wips including phases. <= 0 would mean infinite
            foreach (var column in _simulationData.Setup.Columns)
                column.HighestWipLimit = column.FindMaximumColumnWip(_simulationData.Setup.Phases);


            _backlogList = buildBacklog();

            // initialize any custom distributions for estimates or occurrences
            buildDistributions();

            // cant do this - overwrites sensitivity tests...
            //resetAllSensitivities();

            // create the phases event handler
            PhaseProcessor phaseHandler = new PhaseProcessor(this);

            if (SimulationData.Setup.ForecastDate != null &&
              
                  ( string.IsNullOrWhiteSpace(SimulationData.Execute.ReturnResults) ||
                    SimulationData.Execute.ReturnResults.Contains("ValueAndDate") ||
                    SimulationData.Execute.ReturnResults.Contains("TotalCost") ||
                    SimulationData.Setup.Backlog.Deliverables.Any(del => del.EarliestStartDate != DateTime.MinValue)
                  )
                )
            {
                _valueAndDateProcessor = new ValueAndDateProcessor(
                    this,
                    SimulationData.Setup.ForecastDate);
            }

            // create the expedite blocking "event"
            _expediteBlockingEventProcessor = new ExpediteCardBlockingEventProcessor(this);

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

            // create the replenish interval colmumn processors
            _replenishIntervalList = new List<ReplenishIntervalProcessor>();
            foreach (var replenish in _simulationData.Setup.Columns.Where(c => c.ReplenishInterval > 0))
                _replenishIntervalList.Add(new ReplenishIntervalProcessor(this, replenish, replenish.ReplenishInterval));

            // create the cmplete interval colmumn processors
            _completeIntervalList = new List<CompleteIntervalProcessor>();
            foreach (var replenish in _simulationData.Setup.Columns.Where(c => c.CompleteInterval > 0))
                _completeIntervalList.Add(new CompleteIntervalProcessor(this, replenish, replenish.CompleteInterval));

            try
            {
                // interval 0 is starting point. Add empty time interval.
                TimeInterval firstInterval = new TimeInterval();
                firstInterval.Simulator = this;
                firstInterval.Sequence = 0;
                firstInterval.ElapsedTime = 0.0;
                firstInterval.PreviousTimeInterval = null;
                firstInterval.CountCardsInBacklog = _backlogList.Count;
                firstInterval.UpdateWipLimitsForThisInterval(_simulationData.Setup.Columns);

                Dictionary<SetupColumnData, int> pos = new Dictionary<SetupColumnData, int>();

                // move cards to initial positions
                foreach (var item in _allCardsList)
                {
                    if (item.CustomBacklog != null && item.CustomBacklog.InitialColumn != -1)
                    {
                        // get column
                        var col = _simulationData.Setup.Columns.Where(c => c.Id == item.CustomBacklog.InitialColumn).FirstOrDefault();

                        if (pos.ContainsKey(col))
                            pos[col] = pos[col] + 1;
                        else
                            pos.Add(col, 1);

                        // move card to column
                        if (moveCard(firstInterval, null, -1, col, pos[col], item))
                            _backlogList.Remove(item);
                    }
                }
                        
                _intervals.Add(firstInterval);

                for (int iter = 1; iter < _simulationData.Execute.LimitIntervalsTo; iter++)
                {
                    TimeInterval thisInterval = stepOneInterval(iter, intervalTime, _blockingEventList, _replenishIntervalList);

                    double completPct =
                        ((double)thisInterval.CountCompletedCards / (double)this.AllCardsList.Count) * 100.0;

                    double currentActivePositionsPct = 100.0 -
                        ((((double)thisInterval.TotalWipLimitBoardPositions - (double)thisInterval.CountTotalCardsOnBoard())) / (double)thisInterval.TotalWipLimitBoardPositions) * 100.0;

                    // complete % THEN active positions
                    if (completPct >= _simulationData.Execute.CompletePercentage)
                        if (currentActivePositionsPct <= _simulationData.Execute.ActivePositionsCompletePercentage)
                            break;

                    // optimization - burn all data on the previous, previous time interval
                    if (_simulationData.Execute.ReturnResults == "Intervals")
                    {
                        if (thisInterval.PreviousTimeInterval != null)
                            thisInterval.PreviousTimeInterval.BurnAllData();

                    }
                }
            }
            finally
            {
                // dispose event processors - unwite events to avoid memory leak.
                foreach (var item in _addedAcopeList)
                    item.Dispose();

                foreach (var item in _blockingEventList)
                    item.Dispose();

                foreach (var item in _defectList)
                    item.Dispose();

                foreach (var item in _replenishIntervalList)
                    item.Dispose();

                foreach (var item in _completeIntervalList)
                    item.Dispose();

                phaseHandler.Dispose();

                if (_valueAndDateProcessor != null)
                    _valueAndDateProcessor.Dispose();

                _expediteBlockingEventProcessor.Dispose();

            }

            return _intervals.Count < _simulationData.Execute.LimitIntervalsTo;
        }

        private TimeInterval stepOneInterval(int intervalSequence, double intervalTime, List<BlockingEventProcessor> _blockingEventList, List<ReplenishIntervalProcessor> _replenishIntervalList)
        {

#if DEBUGGING
            // sometimes a card goes missing and simulation never ends. This trap shows that occurrence.
            // The trigger i saw was when a defect started in a column after the one where it is found.
            if (_backlogList.Count + _defectBacklog.Values.Sum(v => v.Count) + 
                _completedWorkList.Count + 
                (_intervals.LastOrDefault() == null ? 0 : _intervals.LastOrDefault().CardPositions.Values.Sum(var => var.Count)) 
                
                != AllCardsList.Count)
            {
                if (_intervals.LastOrDefault() != null)
                {
                    var _missing1 = _allCardsList.Except(_backlogList).Except(_completedWorkList).ToList().ToList();

                    foreach (var cpl in _intervals.LastOrDefault().CardPositions.Values)
                        foreach (var c in cpl)
                            _missing1.Remove(c.Card);

                    foreach (var def in _defectBacklog.Values)
                        foreach (var d in def)
                            _missing1.Remove(d);

                    int i = _missing1.Count;
                }
            }
#endif
            // setup this time interval
            TimeInterval thisInterval = new TimeInterval();
            thisInterval.Simulator = this;
            thisInterval.Sequence = intervalSequence;
            thisInterval.ElapsedTime = (intervalSequence - 1) * intervalTime;
            thisInterval.PreviousTimeInterval = _intervals.LastOrDefault();
            _intervals.Add(thisInterval);

            OnRaiseTimeIntervalTickEvent(new TimeIntervalTickEventArgs
            {
                TimeInterval = thisInterval,
                IntervalTime = intervalTime
            });

            // start from the last column
            foreach (var column in _simulationData.Setup.Columns.OrderByDescending(s => s.Id))
            {
                incrementTimeSoFarForColumn(thisInterval, column, intervalTime);

                // keep a list of any WIP violators added this round.
                List<Card> wipViolatorsAddedThisInterval = new List<Card>();

                // we need this when moving, but also when blocking due to violators
                SetupColumnData nextColumn = getNextColumnForColumnAndCard(column);
                SetupColumnData priorColumn = getNextColumnForColumnAndCard(column, -1);

                // get the set of position in "some" order to process. This will be the pull order....
                var positionsToProcess = positionOrderToProcessList(thisInterval, column);

                foreach (int position in positionsToProcess.ToList())
                {
                    // if not the very first time interval
                    if (thisInterval.PreviousTimeInterval != null)
                    {
                        // grab the card in this position during the previous interval
                        Card card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, position);
                        if (card != null)
                        {
                            nextColumn = getNextColumnForColumnAndCard(column, +1, card);

                            double soFar = card.GetTimeSoFarInColumn(column);

                            if (soFar >= card.CalculatedRandomWorkTimeForColumn(column)
                                             + _expediteBlockingEventProcessor.BlockTimeForCard(column, card))
                            {
                                // card has spent enough time in this column for work ... longer for blocking?

                                bool blockCard = false;
                                foreach (var b in _blockingEventList)
                                {
                                    // try them in order, block on first found
                                    blockCard = b.IsCardBlocked(column, card, intervalTime);

                                    //todo:carry forward time to blocked if completed less than interval. 

                                    if (blockCard)
                                        break;
                                } 

                                if (blockCard)
                                {
                                    thisInterval.AddCardInPositionForColumn(column, position, thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, position));
                                    card.Status = Enums.CardStatusEnum.Blocked;
                                }
                                else
                                {
                                    // check to see if there is a completeInterval inhibiting "completion" or "move"
                                    if (isColumnAllowedToCompleteWork(column) &&
                                        isStrictFIFOAllowsComplete(thisInterval, column, card))
                                    {
                                        // look for the next column. if its null, that means wwe are at the end.

                                        // if the last status, complete the card
                                        if (nextColumn == null ||
                                            column == _simulationData.Setup.Columns.OrderBy(c => c.Id).Last())
                                        {
                                            completeCard(thisInterval, _completedWorkList, column, position);
                                            card.Status = Enums.CardStatusEnum.Completed;
                                        }
                                        else
                                        {
                                            // TODO:if the next column is skipped, and then complete....complete the card

                                            // if this COS can violate WIP, 
                                            // move to the next column and block the lowest priority card to compensate for this lost time
                                            if (card.ClassOfService.ViolateWIP)
                                            {
                                                moveCard(thisInterval, column, position, nextColumn, -1);
                                                wipViolatorsAddedThisInterval.Add(card);
                                            }
                                            else
                                            {
                                                // move to the right if possible
                                                int avail = nextAvailablePositionInColumn(
                                                    thisInterval,
                                                    nextColumn);

                                                //TODO:Removed substantial code for defects of higher COS priority....None of it worked...
                                                // Need to make test, COS priority of work vs defect test case working sometime

                                                if (avail > -1)
                                                {
                                                   moveCard(thisInterval, column, position, nextColumn, avail);
                                                }
                                                else
                                                {
                                                    // has to stay where it is. mark as queued and bump the card along in time intervals
                                                    thisInterval.AddCardInPositionForColumn(column, position, thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, position));
                                                    card.Status = Enums.CardStatusEnum.CompletedButWaitingForFreePosition;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // card is blocked from completion by completeInterval. Keep in current spot
                                        thisInterval.AddCardInPositionForColumn(column, position, thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, position));
                                        card.Status = Enums.CardStatusEnum.CompletedButWaitingForFreePosition;
                                    }
                                }
                            }
                            else
                            {
                                // not finished yet, bump along from the previous time interval
                                thisInterval.AddCardInPositionForColumn(column, position, thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, position));

                                if (_expediteBlockingEventProcessor.IsCardBlocked(column, card, intervalTime))
                                    card.Status = Enums.CardStatusEnum.Blocked;
                                else
                                    card.Status = Enums.CardStatusEnum.SameStatusThisInterval;

                            } 
                            
                        } 
                        else
                        {
                            // card was null... do nothing yet...
                            // reset the next column
                            nextColumn = getNextColumnForColumnAndCard(column);
                        } 
                    } 

                    // fill position with defect or move card from backlog if position is empty
                    // and this is a position that falls within the current WIP limits
                    if (  position > -1 && //this might have been a violator before...
                          isColumnAllowedToStartNewWork(column) &&
                          (position < CurrentColumnWIPLimit(column) || CurrentColumnWIPLimit(column) <= -1))
                        fillPosition(thisInterval, column, position);
                }

                // if any violate WIPers, compensate by blocking another card?
                foreach (var wipViolatingCard in wipViolatorsAddedThisInterval)
                    blockLowestPriorityCardInColumn(intervalTime, thisInterval, nextColumn, wipViolatingCard);

            } // positions


            // TODO:Now - do we have any ViolateWIP cards as the next off the block?
            SetupColumnData firstColumn = _simulationData.Setup.Columns.OrderBy(c => c.Id).First();


            // TODO:WIP violators and infinite WIP

            if (isColumnAllowedToStartNewWork(firstColumn))
            {

                // get the next card of the top of the backlog INCLUDING WIP violators
                Card potentialWIPViolatorCard = nextAllowedBacklogCard(thisInterval, true);

                while (potentialWIPViolatorCard != null &&

                      ((potentialWIPViolatorCard.ClassOfService != null && potentialWIPViolatorCard.ClassOfService.ViolateWIP) ||
                      (firstColumn.HighestWipLimit <= 0)) // infinite WIP column, just take all cards in order
                      
                      )
                {
                    //at the top of the backlog is a WIP violator, add it to the board
                    _backlogList.Remove(potentialWIPViolatorCard);
                    bool positionFilled = moveCard(thisInterval, null, 0, firstColumn, -1, potentialWIPViolatorCard);

                    //block and compensate
                    if (potentialWIPViolatorCard.ClassOfService != null && potentialWIPViolatorCard.ClassOfService.ViolateWIP)
                        blockLowestPriorityCardInColumn(intervalTime, thisInterval, firstColumn, potentialWIPViolatorCard);

                    potentialWIPViolatorCard = nextAllowedBacklogCard(thisInterval, true);
                }
            }

            // TODO:swarming and splitting...
            foreach (var column in _simulationData.Setup.Columns.OrderByDescending(s => s.Id))
            {
                // repeat: if there are usused wip positions or more cards to split

                // starting with the highest priority cards in this column

                   // if the cards backlog type allows swarming/splitting

                   // or, if the cards class of serive allows swarming/splitting

                   // or, if the column allows swarming/splitting

                   // split the card into two...
            }

            thisInterval.CountCardsInBacklog = _backlogList.Count;
            thisInterval.CountCompletedCards = _completedWorkList.Count;

            if (_valueAndDateProcessor != null)
            {
                thisInterval.ValueDeliveredSoFar = _valueAndDateProcessor.ValueDeliveredSoFar;
                thisInterval.CurrentDate = _valueAndDateProcessor.CurrentDate;
            }

            thisInterval.UpdateWipLimitsForThisInterval(_simulationData.Setup.Columns);

            // update status history
            foreach (var card in _allCardsList)
                card.SnapshotStatusHistory(thisInterval.Sequence);

            return thisInterval;
        }

        private bool isStrictFIFOAllowsComplete(TimeInterval thisInterval, SetupColumnData column, Card card)
        {
            // strict fifo order. 
            // even if a card is complete, it will queue waiting for the first in card

            bool result = true;

            if (   SimulationData.Execute.ShufflePositions == ShufflePositionsEnum.FIFOStrict // shuffle positions is obsolete
                || SimulationData.Execute.PullOrder == PullOrderEnum.FIFOStrict)
            {

                // last columns don't count. complete the card
                if (column == _simulationData.Setup.Columns.OrderBy(c => c.Id).Last())
                    return result;

                
                if (thisInterval.PreviousTimeInterval != null)
                {
                    if (thisInterval.PreviousTimeInterval.CardPositions[column] != null)
                    {
                        if (thisInterval.PreviousTimeInterval.CardPositions[column]
                            .Where(cp => cp.Card != null)
                            .OrderBy(cp => cp.Card.PullOrder)
                            .First().Card != card)
                            result = false;
                    }

                }

            }

            return result;

        }

        private IEnumerable<int> positionOrderToProcessList(TimeInterval thisInterval, SetupColumnData column)
        {
            // we need an ordered list of positions. 
            // start with the allowed WIP limits (lots may be empty), and then add infinite wip columns and expedites.
            var positionsToProcess = Enumerable.Range(
                0,
                column.HighestWipLimit);

            if (thisInterval.PreviousTimeInterval.CardPositions.ContainsKey(column))
            {

                positionsToProcess = (
                    positionsToProcess.Concat(
                        thisInterval.PreviousTimeInterval.CardPositions[column].Select(cp => cp.Position)))
                 .Distinct();
            }

            // shufflePositions is the obsolete flag. if its not the default, do some work

            if (SimulationData.Execute.ShufflePositions == ShufflePositionsEnum.afterOrdering)
            {

                switch (SimulationData.Execute.PullOrder)
                {
                    case PullOrderEnum.random:
                        positionsToProcess = from i in positionsToProcess
                                             let random = _positionShuffler.Next()
                                             orderby random
                                             select i;

                        break;

                    case PullOrderEnum.indexSequence:
                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let index = card == null ? int.MaxValue : card.Index
                                              orderby index, i
                                              select i);
                        break;

                    case PullOrderEnum.FIFO:
                    case PullOrderEnum.FIFOStrict:
                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let pullOrder = card == null ? int.MaxValue : card.PullOrder
                                              orderby pullOrder, i
                                              select i);

                        // FIFO Strict need post-processing to queue non FIFO complete cards. For now, just process in FIFO order.
                        break;

                    default: // randomAfterOrdering
                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let backlogPriority = card == null || card.CustomBacklog == null ? int.MaxValue : card.CustomBacklog.Order
                                              let cosPriority = card == null || card.ClassOfService == null ? int.MaxValue : card.ClassOfService.Order
                                              let backlogDueDate = card == null || card.CustomBacklog == null ? DateTime.MaxValue : card.CustomBacklog.SafeDueDate
                                              let random = _positionShuffler.Next()
                                              let index = card == null ? int.MaxValue : card.Index
                                              orderby backlogPriority,
                                                        cosPriority,
                                                            backlogDueDate,
                                                                random,
                                                                  index
                                              select i);
                        break;
                }

            }
            else
            {
                // use the obsolete shuffle positions flags.

                switch (SimulationData.Execute.ShufflePositions)
                {
                    case ShufflePositionsEnum.@false:

                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let index = card == null ? int.MaxValue : card.Index
                                              orderby index, i
                                              select i);

                        break;

                    case ShufflePositionsEnum.@true:

                        positionsToProcess = from i in positionsToProcess
                                             let random = _positionShuffler.Next()
                                             orderby random
                                             select i;

                        break;

                    case ShufflePositionsEnum.FIFO:
                    case ShufflePositionsEnum.FIFOStrict:

                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let pullOrder = card == null ? int.MaxValue : card.PullOrder
                                              orderby pullOrder, i
                                              select i);

                        // FIFO Strict need post-processing to queue non FIFO complete cards. For now, just process in FIFO order.

                        break;


                    default: //  after ordering

                        positionsToProcess = (from i in positionsToProcess
                                              let card = thisInterval.PreviousTimeInterval.GetCardInPositionForColumn(column, i)
                                              let backlogPriority = card == null || card.CustomBacklog == null ? int.MaxValue : card.CustomBacklog.Order
                                              let cosPriority = card == null || card.ClassOfService == null ? int.MaxValue : card.ClassOfService.Order
                                              let backlogDueDate = card == null || card.CustomBacklog == null ? DateTime.MaxValue : card.CustomBacklog.SafeDueDate
                                              let random = _positionShuffler.Next()
                                              let index = card == null ? int.MaxValue : card.Index
                                              orderby backlogPriority,
                                                        cosPriority,
                                                            backlogDueDate,
                                                                random,
                                                                  index
                                              select i);

                        break;
                }
            }
            
            return positionsToProcess;
        }

        private void blockLowestPriorityCardInColumn(double intervalTime, TimeInterval thisInterval, SetupColumnData nextColumn, Card wipViolatingCard)
        {
            // if at or above wip limit then block, else...
            //int wipLimit = CurrentColumnWIPLimit(nextColumn);

            //if (wipLimit > 0 && // not infinite column
            //    thisInterval.CountCardsNotViolatingWIPForColumn(nextColumn) >= wipLimit)
            //{
                Card lowestPriorityActiveCardInNextColumn = findLowestPriorityActiveCardInNextColumnToBlock(thisInterval, nextColumn, wipViolatingCard);

                if (lowestPriorityActiveCardInNextColumn != null)
                {
                    double time = wipViolatingCard.CalculatedRandomWorkTimeForColumn(nextColumn);
                    _expediteBlockingEventProcessor.AddCardToBlockList(nextColumn, lowestPriorityActiveCardInNextColumn, time);
                }
                else
                {

                    //???
                }
            //}
        }

        private Card findLowestPriorityActiveCardInNextColumnToBlock(TimeInterval thisInterval, SetupColumnData nextColumn, Card excludeCard)
        {
            int wip = CurrentColumnWIPLimit(nextColumn);

            // don't block infinite columns
            if (wip <= 0)
                return null;

            // only block if at or above WIP limit
            int active = thisInterval.CardPositions[nextColumn]
                        .Where(pos =>
                            pos.Card != null && // that position isn't a null card
                            (pos.Card.Status == Enums.CardStatusEnum.NewStatusThisInterval || // and i'm active
                             pos.Card.Status == Enums.CardStatusEnum.SameStatusThisInterval) 
                             // && pos.HasViolatedWIP == false should count expedites as part of the WIP as well....
                             )
                        .Count();

            if (active <= wip)
                return null;

            // if the next column contains no active positions, there is nothing to block
            if (thisInterval.CardPositions.ContainsKey(nextColumn))
            {
                // get the lowest priority card that is being actively worked on thats not a WIP violator itself (if it exists)
                return thisInterval.CardPositions[nextColumn]
                    .Where(pos =>
                        pos.Card != null && // that position isn't a null card
                        pos.Card != excludeCard && // not me
                        //pos.Card.CompareTo(excludeCard) > 0 && // and this card a lower priority than me
                       (pos.Card.Status == Enums.CardStatusEnum.NewStatusThisInterval || // and i'm active
                        pos.Card.Status == Enums.CardStatusEnum.SameStatusThisInterval) &&
                        !pos.HasViolatedWIP) // And isn't a violator itself
                    .OrderByDescending(pos => pos.Card, CardPriorityComparer.Instance)
                    .Select(cp => cp.Card)
                    .FirstOrDefault();
            }

            return null;
        }

        private void buildDistributions()
        {
            // lets try building the distributions once and using those.
            foreach (var distData in _simulationData.Setup.Distributions)
            {
                _distributions.Add(
                    distData,
                    DistributionHelper.CreateDistribution(distData));
            }

            // if these are models, sim and build the samples
            foreach (var dist in _distributions.Values)
                ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(dist);

            foreach (var column in _simulationData.Setup.Columns)
            {
                if (!string.IsNullOrEmpty(column.EstimateDistribution))
                {
                    SetupDistributionData distribution =
                       _simulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, column.EstimateDistribution, true) == 0);

                    if (distribution != null)
                        _columnDistributions.Add(column, _distributions[distribution]);
                }
                else
                {
                    switch (_simulationData.Execute.DefaultDistribution)
                    {
                        case "weibull" :
                            DistributionData distribution = DistributionHelper.CreateDefaultWeibull(column.EstimateLowBound, column.EstimateHighBound);
                            _columnDistributions.Add(column, DistributionHelper.CreateDistribution(distribution));
                            break;
                        default: break;
                    }
                }
            }

            // build for base level
            foreach (var custom in _simulationData.Setup.Backlog.CustomBacklog)
                buildDistributionForCustomColumnOverride(custom);

            // build for all overrides within deliverables
            foreach (var deliverable in _simulationData.Setup.Backlog.Deliverables)
                foreach (var custom in deliverable.CustomBacklog)
                    buildDistributionForCustomColumnOverride(custom);

            //build for cos
            foreach (var cos in _simulationData.Setup.ClassOfServices)
                buildDistributionForCOSColumnOverride(cos);



        }

        private void buildDistributionForCustomColumnOverride(SetupBacklogCustomData custom)
        {
            foreach (var colOverride in custom.Columns)
            {
                addColumnOverrideDistribution(colOverride);
            }
        }

        private void buildDistributionForCOSColumnOverride(SetupClassOfServiceData cos)
        {
            foreach (var colOverride in cos.Columns)
            {
                addColumnOverrideDistribution(colOverride);
            }
        }

        private void addColumnOverrideDistribution(SetupBacklogCustomColumnData colOverride)
        {
            if (!string.IsNullOrEmpty(colOverride.EstimateDistribution))
            {
                SetupDistributionData distribution =
                   _simulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name, colOverride.EstimateDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null && !_backlogColumnDistributions.ContainsKey(colOverride))
                    _backlogColumnDistributions.Add(colOverride, this.Distributions[distribution]);
            }
            else
            {
                switch (_simulationData.Execute.DefaultDistribution)
                {
                    case "weibull":
                        DistributionData distribution = DistributionHelper.CreateDefaultWeibull(colOverride.EstimateLowBound, colOverride.EstimateHighBound);
                        if (!_backlogColumnDistributions.ContainsKey(colOverride))
                            _backlogColumnDistributions.Add(colOverride, DistributionHelper.CreateDistribution(distribution));
                        break;
                    default: break;
                }
            }
        }
        
        private bool fillPosition(TimeInterval thisInterval, SetupColumnData column, int position, List<Card> excludeList = null)
        {
            bool positionFilled = true;

            if (isPositionEmpty(thisInterval, column, position))
            {
                bool isFirstColumn = column == _simulationData.Setup.Columns.OrderBy(c => c.Id).First();
                
                int defectColumnIndex = column.Id;

                // the first column is the same as the backlog...
                if (isFirstColumn)
                {
                    bool defectInColumn = (_defectBacklog.ContainsKey(defectColumnIndex) &&
                                           _defectBacklog[defectColumnIndex].Any());

                    bool defectInBacklog = (_defectBacklog.ContainsKey(-1) &&
                                            _defectBacklog[-1].Any()); 

                    if (!defectInColumn && defectInBacklog)
                        defectColumnIndex = -1;
                }

                // get the next card in the backlog
                Card backlogCard = null;
                if (isFirstColumn)
                    backlogCard = nextAllowedBacklogCard(thisInterval, false, excludeList);

                // get the next defect card is there is any with the lowest COS order
                Card defectCard = null;
                if (_defectBacklog.ContainsKey(defectColumnIndex) &&
                    _defectBacklog[defectColumnIndex].Any())
                {
                    defectCard = _defectBacklog[defectColumnIndex]
                                 .OrderBy(d => d, CardPriorityComparer.Instance)
                                 .FirstOrDefault();
                }

                // rules:
                // card with the lowest order goes first, defects if there is a tie
                bool useBacklogCard = 
                    (defectCard == null) || 
                    (backlogCard != null && backlogCard.CompareTo(defectCard) < 0); 

                // defects
                if (!useBacklogCard)
                {
                    // remove from dictionary
                    _defectBacklog[defectColumnIndex].Remove(defectCard);
                    positionFilled = moveCard(thisInterval, null, 0, column, position, defectCard);
                }
                else
                {
                    // backlog to first column
                    if (isFirstColumn)
                    {
                        if (backlogCard != null)
                        {
                            var tempCol = getNextColumnForColumnAndCard(null, +1, backlogCard);

                            if (tempCol != column)
                            {
                                // skip if distribution gives zero time....
                                var avail = nextAvailablePositionInColumn(
                                    thisInterval,
                                    tempCol);

                                if (avail > -1)
                                {
                                    _backlogList.Remove(backlogCard);
                                    positionFilled = moveCard(thisInterval, null, 0, tempCol, avail, backlogCard);
                                }
                                else
                                {
                                    // need to stop this going recursive...
                                    if (excludeList == null)
                                        excludeList = new List<Card> { backlogCard };
                                    else
                                        excludeList.Add(backlogCard);

                                    return fillPosition(thisInterval, column, position, excludeList);
                                }
                            }
                            else
                            {
                                // move card into position
                                _backlogList.Remove(backlogCard);
                                positionFilled = moveCard(thisInterval, null, 0, column, position, backlogCard);
                            }
                        }
                    }
                    else
                    {
                        // we don't do this here; yet!
                    }
                }
            }

            if ( !positionFilled 
                && !_completeIntervalList.Any(c => c.Column == column) ) // don't force fill if there is a completion interval active
                return fillPosition(thisInterval, column, position);

            return positionFilled;
        }

        private Card nextAllowedBacklogCard(TimeInterval timeInterval, bool allowWIPViolators = false, List<Card> excludeList = null)
        {
            Card result = null;

            if (excludeList == null)
                excludeList = new List<Card>();

            if (_backlogList.Except(excludeList).Any())
            {
                if (SimulationData.Setup.ClassOfServices != null &&
                   SimulationData.Setup.ClassOfServices.Any())
                {
                    // get the # cards by COS on the board
                    var cardsByCOS = timeInterval
                                    .CardPositions
                                    .Values
                                    .SelectMany(cp => cp)
                                    .Where(cp => cp.Card != null && cp.Card.ClassOfService != null)
                                    .GroupBy(cp => cp.Card.ClassOfService)
                                    .ToDictionary(g => g.Key, g => g.Count()) ;

                    result = _backlogList
                        .Except(excludeList)
                        .Where(c =>

                            // this card doesn't have a class of service
                            c.ClassOfService == null ||

                            // now, avoid violateWIP's, 
                                // and do not exceed the number allowed on the board 
                            (
                              (allowWIPViolators || c.ClassOfService.ViolateWIP == false) &&

                              (cardsByCOS.ContainsKey(c.ClassOfService) == false || // 0 count otherwise
                                  c.ClassOfService.MaximumAllowedOnBoard > cardsByCOS[c.ClassOfService]) // non-zero count, make sure it is under the limit
                            )
                        )
                        //.OrderBy(c => c, CardPriorityComparer.Instance) using ordered list now
                        .FirstOrDefault();

                    if (result != null && 
                        result.ClassOfService != null && 
                        result.ClassOfService.SkipPercentage > 0.0)
                    {
                        // perhaps we skip...
                        // double random = TrueRandom.NextDouble(0.0, 100.0);
                        double random = _skipDistribution.NextDouble();

                        if (result.ClassOfService.SkipPercentage >= random)
                        {
                            _backlogList.Remove(result);
                            completeCard(timeInterval, _completedWorkList, null, -1, result);
                            result = nextAllowedBacklogCard(timeInterval, allowWIPViolators);
                        }
                    }
                }
                else
                {
                    // if no COS, then just take the next most prioritized card 
                    if (result == null)
                        result = _backlogList
                            .Except(excludeList)
                            //.OrderBy(c => c, CardPriorityComparer.Instance) using ordered list now
                            .First();
                }

                // perhaps the deliverable can't start yet
                if (result != null &&
                    result.Deliverable != null &&
                    _valueAndDateProcessor != null &&
                    _valueAndDateProcessor.CurrentDate < result.Deliverable.EarliestStartDate ) // earliest start date defaults to DateTime.Min if not specified
                {
                    // can't start this card yet, beafore earliest start date
                    excludeList.Add(result);
                    result = nextAllowedBacklogCard(timeInterval, allowWIPViolators, excludeList);
                }


                // if there are pre-requisites. Perhaps we cant start yet.
                if (result != null &&
                    result.Deliverable != null &&
                    result.Deliverable.PreRequisiteDeliverables != "")
                {

                    // check to see all pre-reqs complete 
                    bool allPreReqsComplete = true;
                    foreach (var del in result.Deliverable.PreRequisiteDeliverables.Split(new char[] { '|', ',' }))
                    {
                        allPreReqsComplete = allPreReqsComplete &&
                            isDeliverableIsComplete(_simulationData.Setup.Backlog.Deliverables.First(d => string.Compare(d.Name, del, true) == 0));

                        if (!allPreReqsComplete)
                            break;

                    }

                    // if pre-reqs not complete, skip this card
                    if (!allPreReqsComplete)
                    {
                        // can't start this card yet, pre-reqs not finished
                        excludeList.Add(result);
                        result = nextAllowedBacklogCard(timeInterval, allowWIPViolators, excludeList);
                    }

                } // pre-reqs in deliverable

            } // nothing in backlog

            return result;
        }

        private Dictionary<SetupBacklogDeliverableData, bool> _deliverableCompleteList = new Dictionary<SetupBacklogDeliverableData, bool>();

        private bool isDeliverableIsComplete(SetupBacklogDeliverableData deliverable)
        {
             return _deliverableCompleteList.ContainsKey(deliverable)
                        && _deliverableCompleteList[deliverable];
        }

        private OrderedList<Card> buildBacklog()
        {
            List<Card> result = null;

            switch (_simulationData.Setup.Backlog.BacklogType)
            {
                case BacklogTypeEnum.Simple :
                    {  
                        
                        result = 
                            (from i in Enumerable.Range(0, _simulationData.Setup.Backlog.SimpleCount)
                             select new Card
                             {
                                 Name = string.Format(_simulationData.Setup.Backlog.NameFormat, i+1),
                                 Index = i,
                                 Status = Enums.CardStatusEnum.InBacklog,
                                 CardType = Enums.CardTypeEnum.Work,
                                 Simulator = this
                             })
                            .ToList();

                        _allCardsList.AddRange(result);

                        break;
                    }
                case BacklogTypeEnum.Custom :
                    {
                        result = new List<Card>();

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
                                            result.AddRange(
                                                (from i in Enumerable.Range(0, custom.Count)
                                                 select new Card
                                                 {
                                                     Index = index++,
                                                     Name = string.Format(_simulationData.Setup.Backlog.    NameFormat, index, string.Format(custom.Name, index, "", custom.Order, deliverable.Name, deliverable.Order), custom.Order, deliverable.Name, deliverable.Order),
                                                     Status = Enums.CardStatusEnum.InBacklog,
                                                     CardType = Enums.CardTypeEnum.Work,
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
                            
                            // backlog if there is no deliverable element parent
                            foreach (var custom in _simulationData.Setup.Backlog.CustomBacklog.Where(c => c.Completed == false))
                            {
                                result.AddRange(
                                    (from i in Enumerable.Range(0, custom.Count)
                                     select new Card
                                     {
                                         Index = index++,
                                         Name = string.Format(_simulationData.Setup.Backlog.NameFormat, index, string.Format(custom.Name, index, "", custom.Order, "",""), custom.Order, "", ""),
                                         Status = Enums.CardStatusEnum.InBacklog,
                                         CardType = Enums.CardTypeEnum.Work,
                                         CustomBacklog = custom,
                                         Deliverable = null,
                                         Simulator = this
                                     })
                                    );
                            }

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
                                        result.AddRange(
                                            (from i in Enumerable.Range(0, custom.Count)
                                             select new Card
                                             {
                                                 Index = index++,
                                                 Name = string.Format(_simulationData.Setup.Backlog.NameFormat, index, string.Format(custom.Name, index, "", custom.Order, deliverable.Name, deliverable.Order), custom.Order, deliverable.Name, deliverable.Order),
                                                 Status = Enums.CardStatusEnum.InBacklog,
                                                 CardType = Enums.CardTypeEnum.Work,
                                                 CustomBacklog = custom,
                                                 Deliverable = deliverable,
                                                 Simulator = this
                                             })
                                            );
                                    }
                                }
                            }

                        }

                        // shuffle the cards if that option is chosen for the backlog (default is true)
                        if (_simulationData.Setup.Backlog.Shuffle)
                            result.ShuffleFast();

                        // using ordered list now...do we need to do this?
                        result = result
                            .OrderBy(c => c, CardPriorityComparer.Instance)
                            .ToList();
                        

                        // add the backlog to the all card list as well.
                        _allCardsList.AddRange(result);
                        break;
                    }
            }



            return new OrderedList<Card>(result, CardPriorityComparer.Instance);
        }

        public static string GetCumulativeFlowData(List<SetupColumnData> columns, List<TimeInterval> intervals)
        {
            // cumulative flow
            StringBuilder builder = new StringBuilder();

            string header = ",Backlog";

            foreach (var column in columns)
                header += "," + column.Name;

            header += ",Complete";

            foreach (var column in columns)
                header += "," + column.Name + " Blocked";

            builder.AppendLine(header);

            foreach (TimeInterval interval in intervals)
            {
                string line = interval.Sequence.ToString();

                line += "," + interval.CountCardsInBacklog;

                foreach (SetupColumnData column in columns)
                    line += "," + interval.CountCardsInPositionForColumn(column).ToString();

                line += "," + interval.CountCompletedCards;

                foreach (SetupColumnData column in columns)
                    line += "," + interval.CountCardsForColumn(column, Enums.CardStatusEnum.Blocked).ToString();

                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        private bool isPositionEmpty(TimeInterval interval, SetupColumnData column, int position)
        {
            return interval.GetCardInPositionForColumn(column, position) == null;
        }

        private void incrementTimeSoFarForColumn(TimeInterval interval, SetupColumnData column, double time)
        {
            if (interval.Sequence > 0) // we just skip the very first interval
            {
                if (interval.PreviousTimeInterval.CardPositions.ContainsKey(column))
                {
                    foreach (var cp in interval.PreviousTimeInterval.CardPositions[column])
                    {
                        if (cp.Card != null)
                            cp.Card.UpdateTimeSoFarInColumn(column, time);
                    }
                }
            }
        }

        private void completeCard(TimeInterval interval, List<Card> completedCards, SetupColumnData fromColumn, int fromWIP, Card card = null)
        {
            if (card == null)
            {
                card = interval.PreviousTimeInterval.GetCardInPositionForColumn(fromColumn, fromWIP);

                completedCards.Add(card);

                // empty the old position
                interval.RemoveCardInPositionForColumn(fromColumn, fromWIP);

                // if this card completed earlier than the interval, deduct the extra time from the last column
                // don't move time if this card was queued...
                if (card != null && interval.PreviousTimeInterval != null &&
                    card.StatusHistoryForInterval(interval.PreviousTimeInterval.Sequence) != Enums.CardStatusEnum.CompletedButWaitingForFreePosition &&
                    card.StatusHistoryForInterval(interval.PreviousTimeInterval.Sequence) != Enums.CardStatusEnum.Blocked)
                {
                    double time = card.GetTimeSoFarInColumn(fromColumn) -
                        (card.CalculatedRandomWorkTimeForColumn(fromColumn) +
                            _expediteBlockingEventProcessor.TimeSpentBlockedBecauseOfViolateWIP(fromColumn, card));

                    if (time > 0)
                    {
                        card.UpdateTimeSoFarInColumn(fromColumn, time * -1.0);
                    }
                }
            }
            else
            {
                completedCards.Add(card);

                // empty the old position
                if (fromColumn != null)
                    interval.RemoveCardInPositionForColumn(fromColumn, fromWIP);
            }

            //TODO: add complete flag to deliverable contract and track there so we can see this deliverable is complete from other sources
            // does this complete a deliverable
            if (card.Deliverable != null)
            {
                bool completeDeliverable =
                    _completedWorkList.Where(c => c.Deliverable == card.Deliverable).OrderBy(cd => cd.Index)
                    .SequenceEqual(
                    _allCardsList.Where(c => c.Deliverable == card.Deliverable).OrderBy(cd => cd.Index));

                if (completeDeliverable && !_deliverableCompleteList.ContainsKey(card.Deliverable))
                    _deliverableCompleteList.Add(card.Deliverable, true);
            }

            // send CardComplete event
            OnRaiseCardCompleteEvent(new CardCompleteEventArgs
            {
                Card = card,
                FromColumn = fromColumn,
                FromPosition = fromWIP
            });
        }


        private bool isColumnAllowedToStartNewWork(SetupColumnData column)
        {
            bool result = true;

            // the to column must be able to accept cards based on replenish intervals
            var replenishEvent = _replenishIntervalList.FirstOrDefault(r => r.Column == column);

            result =
                ((replenishEvent == null) ||
                (replenishEvent != null && replenishEvent.TriggerThisInterval == true));

            return result;
        }

        private bool isColumnAllowedToCompleteWork(SetupColumnData column)
        {
            bool result = true;

            // the to column must be able to complete cards based on complete intervals
            var completeEvent = _completeIntervalList.FirstOrDefault(r => r.Column == column);

            result =
                ((completeEvent == null) ||
                (completeEvent != null && completeEvent.TriggerThisInterval == true));

            return result;
        }

        private int nextAvailablePositionInColumn(TimeInterval interval, SetupColumnData column)
        {
            // first check this column is allowed to start new work based on replenishInterval's
            if (!isColumnAllowedToStartNewWork(column))
                return -1;

            int count = 0;
            int firstEmptyPosition = -1;

            if (column.HighestWipLimit <= 0)
            {
                // infinite column

                // get the higest position used + 1
                if (interval.CardPositions.ContainsKey(column) && interval.CardPositions[column].Any())
                    return interval.CardPositions[column].Max(c => c.Position)+1;
                else 
                    return 0;
            }
            else
            {
                // traditional column with WIP

                // with phase wip overrides, the algorithm is:
                //   - count all currently used positions
                //   - if this is < the current wip limit,
                //      - return the position of the first un-used position


                
                for (int i = 0; i < column.HighestWipLimit; i++)
                {
                    if (interval.GetCardInPositionForColumn(column, i) == null)
                    {
                        if (firstEmptyPosition == -1)
                            firstEmptyPosition = i;
                    }
                    else
                    {
                        count++;
                    }
                }

                if (count < CurrentColumnWIPLimit(column))
                {
                    return firstEmptyPosition;
                }
                else
                {
                    return -1;
                }
            }
        }

        private SetupColumnData getNextColumnForColumnAndCard(SetupColumnData column, int direction = +1, Card card = null)
        {
            SetupColumnData nextColumn;

            if (column == null)
            {
                nextColumn =
                 _simulationData
                 .Setup
                 .Columns
                 .OrderBy(c => c.Sequence)
                 .FirstOrDefault();
            }
            else
            {
                // we need this when moving, but also when blocking due to violators
                nextColumn =
                     _simulationData
                     .Setup
                     .Columns
                     .Where(c => c.Sequence == column.Sequence + direction)
                     .FirstOrDefault();
            }

            if (nextColumn != null)
            {

                // 1. defect column - skipPercentage

                //TODO: Implement defect skipping

                // 2. custom backlog column
                if (card != null &&
                    card.CustomBacklog != null &&
                    card.CustomBacklog.Columns != null &&
                    card.CustomBacklog.Columns.Count > 0 &&
                    card.CustomBacklog.Columns.Any(cb => cb.ColumnId == nextColumn.Id))
                {
                    // column has been overridden at a custom backlog level
                    var toColumnCustom = card.CustomBacklog.Columns.First(cb => cb.ColumnId == nextColumn.Id);

                    // there is a column override. skip or try the next level
                    double rand = TrueRandom.NextDouble(0.0, 100.0); // should apply default dist?
                    if (rand <= toColumnCustom.SkipPercentage ||
                        card.CalculatedRandomWorkTimeForColumn(nextColumn) == 0.0)
                        return getNextColumnForColumnAndCard(nextColumn, direction, card);
                    else
                        return nextColumn;
                }

                // 2.5 COS NOT TESTED!!!!
                if (card != null &&
                    card.ClassOfService != null &&
                    card.ClassOfService.Columns != null &&
                    card.ClassOfService.Columns.Count > 0 &&
                    card.ClassOfService.Columns.Any(cb => cb.ColumnId == nextColumn.Id))
                {
                    // column has been overridden at a COS backlog level
                    var toColumnCustom = card.ClassOfService.Columns.First(cb => cb.ColumnId == nextColumn.Id);

                    // there is a column override. skip or try the next level
                    double rand = TrueRandom.NextDouble(0.0, 100.0); // should apply default dist?
                    if (rand <= toColumnCustom.SkipPercentage ||
                        card.CalculatedRandomWorkTimeForColumn(nextColumn) == 0.0)
                        return getNextColumnForColumnAndCard(nextColumn, direction, card);
                    else
                        return nextColumn;
                }

                // 3. phase column

                // 4. basic column
                if (nextColumn.SkipPercentage > 0.0)
                {
                    double rand = TrueRandom.NextDouble(0.0, 100.0); // should apply default dist?
                    if (rand  <= nextColumn.SkipPercentage)
                        return getNextColumnForColumnAndCard(nextColumn, direction, card);
                }
            }

            return nextColumn;
        }

        long cardMoveSequence = 0;

        private bool moveCard(TimeInterval interval, SetupColumnData fromColumn, int fromWIP, SetupColumnData toColumn, int toWIP, Card newCard = null)
        {
            bool result = true;

            // traditional move
            
            if (toWIP > -1 && !isPositionEmpty(interval, toColumn, toWIP))
                throw new Exception(string.Format("Destination position {0} item {1} not empty.", toColumn.Name, toWIP));

            Card card = null;

            // move card
            if (fromColumn == null && newCard != null)
            {
                // moving a card from the backlog
                interval.AddCardInPositionForColumn(toColumn, toWIP, newCard);
                newCard.Status = Enums.CardStatusEnum.NewStatusThisInterval;
                card = newCard;

                card.PullOrder = cardMoveSequence++;

                // send CardMove event
                OnRaiseCardMoveEvent(new CardMoveEventArgs {
                    Card = card,
                    FromColumn = null,
                    FromPosition = -1,
                    ToColumn = toColumn,
                    ToPosition = toWIP });
            }
            else
            {
                // moving a card from another board position
                card = interval.PreviousTimeInterval.GetCardInPositionForColumn(fromColumn, fromWIP); 

                // if its in the same column, don't update the pull order, otherwise assign a new pull order
                if (fromColumn != toColumn)
                    card.PullOrder = cardMoveSequence++;
                
                interval.AddCardInPositionForColumn(toColumn, toWIP, card);
                card.Status = Enums.CardStatusEnum.NewStatusThisInterval;
                
                // send CardMove event
                OnRaiseCardMoveEvent(new CardMoveEventArgs
                {
                    Card = card,
                    FromColumn = fromColumn,
                    FromPosition = fromWIP,
                    ToColumn = toColumn,
                    ToPosition = toWIP
                });

                // if from position has extra time, account for it in the to work so far. 
                // don't move time if this card was queued...
                if (card.StatusHistoryForInterval(interval.PreviousTimeInterval.Sequence) != Enums.CardStatusEnum.CompletedButWaitingForFreePosition &&
                    card.StatusHistoryForInterval(interval.PreviousTimeInterval.Sequence) != Enums.CardStatusEnum.Blocked &&
                    fromColumn.IsBuffer == false)
                {
                    double time = card.GetTimeSoFarInColumn(fromColumn) -
                        (card.CalculatedRandomWorkTimeForColumn(fromColumn) +
                            _expediteBlockingEventProcessor.TimeSpentBlockedBecauseOfViolateWIP(fromColumn, card));

                    if (time > 0)
                    {
                        card.UpdateTimeSoFarInColumn(toColumn, time);
                        card.UpdateTimeSoFarInColumn(fromColumn, time * -1.0);
                    }
                }
            }

            if (toColumn.IsBuffer)
            {
                // this is a buffer column. Set status to complete, and try and move to next column if available (this should already have been processed)
                card.Status = Enums.CardStatusEnum.CompletedButWaitingForFreePosition;

                if (!_completeIntervalList.Any(c => c.Column == toColumn))
                {
                    SetupColumnData nextColumn =
                        _simulationData
                        .Setup
                        .Columns
                        .Where(c => c.Sequence == toColumn.Sequence + 1).FirstOrDefault();

                    if (nextColumn == null) // last column
                    {
                        // complete the card here
                        completeCard(interval, _completedWorkList, toColumn, toWIP, card);
                        card.Status = Enums.CardStatusEnum.Completed;


                        result = false;
                    }
                    else
                    {
                        // move to the right if possible
                        int avail = nextAvailablePositionInColumn(
                            interval,
                            nextColumn);

                        if (avail > -1 ||
                           (card.ClassOfService != null && card.ClassOfService.ViolateWIP) ) // empty position to the right available
                        {
                            // it didn't stay in this column, remove it from the list.
                            interval.RemoveCardInPositionForColumn(toColumn, toWIP);

                            if (fromColumn == null && newCard != null) // from the backlog
                                moveCard(interval, fromColumn, fromWIP, nextColumn, avail, newCard);
                            else
                                moveCard(interval, fromColumn, fromWIP, nextColumn, avail);

                            result = false;
                        }
                    }
                }
            }

            //TODO: Account for partial time in blocking, for example, if the card completed blocking
            // earlier than the next interval, should we bubble the difference through?

            return result;
        }

        internal void AddToDefectBacklog(int columnId, Card card)
        {
            List<Card> list;

            if (_defectBacklog.ContainsKey(columnId))
            {
                list = _defectBacklog[columnId];
            }
            else
            {
                list = new List<Card>();
                _defectBacklog.Add(columnId, list);
            }

            list.Add(card);

            if (!AllCardsList.Exists(c => c == card))
                AllCardsList.Add(card);
        }

        internal void SendCardToCompleteEarly(TimeInterval latestInterval, Card card, int columnId)
        {
            // find this card column and position in last interval
            SetupColumnData col = _simulationData.Setup.Columns.Where(c => c.Id == columnId).FirstOrDefault();
            int pos = -1;
            
            if (col != null)
                pos = latestInterval.GetPositionForCardInColumn(col, card);
            
            completeCard(latestInterval, this.CompletedWorkList, col, pos);
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
                    /* should i do this?
                    RaiseCardCompleteEvent = null;
                    RaiseCardMoveEvent = null;
                    RaiseTimeIntervalTickEvent = null;
                     * */

                    if (_valueAndDateProcessor != null)
                        _valueAndDateProcessor.Dispose();

                    if (_expediteBlockingEventProcessor != null)
                        _expediteBlockingEventProcessor.Dispose();

                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _intervals = null;

                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~KanbanSimulation()
        {
            Dispose (false);
        }

    }
}

