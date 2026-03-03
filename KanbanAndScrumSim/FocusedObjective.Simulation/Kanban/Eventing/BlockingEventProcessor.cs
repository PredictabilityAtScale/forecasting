using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class BlockingEventProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private SetupBlockingEventData _blockingEvent;
        private Distribution _occurrenceDistribution;
        private Distribution _estimateDistribution;

        private int _cardsPassedSoFar = 0;
        private int _cardCountToResetCounter = -1;
        private int _cardIndexToBlock = -1;
        private double _chosenBlockTime = 0.0;

        // key is the card, the value is time blocked so far
        private Dictionary<Card,double> _cardsBlockedByThisEvent = new Dictionary<Card,double>();

        private string[] _phases;

        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;
        private List<BlockingEventHistory> _history = new List<BlockingEventHistory>();

        internal BlockingEventProcessor(KanbanSimulation sim, SetupBlockingEventData blockingEvent)
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
            else
            {
                switch (_simulator.SimulationData.Execute.DefaultDistribution)
                {
                    case "weibull":
                        DistributionData distribution = DistributionHelper.CreateDefaultWeibull(_blockingEvent.EstimateLowBound, _blockingEvent.EstimateHighBound);
                        _estimateDistribution = DistributionHelper.CreateDistribution(distribution);
                        break;
                    default: break;
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
            sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            sim.RaiseCardMoveEvent += handleCardMoveEvent;
            //sim.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
            //sim.RaiseTimeIntervalTickEvent += handleTimeIntervalTickEvent;
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
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~BlockingEventProcessor()
        {
            Dispose (false);
        }

        internal bool IsCardBlocked(SetupColumnData column, Card card, double timeInterval)
        {
            bool result = false;

            // zero indicates do not count. Most often because sensitivy multiplier is 0.0
            if (_cardCountToResetCounter > 0)
            {
                // are we blocking this card?
                if (_blockingEvent.ColumnId == column.Id &&
                    _cardsBlockedByThisEvent.Keys.Contains(card))
                {
                    _cardsBlockedByThisEvent[card] -= timeInterval;

                    // finished blocking yet?
                    if (_cardsBlockedByThisEvent[card] < 0.0)
                        _cardsBlockedByThisEvent.Remove(card);
                    else
                        result = true;
                }
            }

            return result;
        }

        private void handleCardMoveEvent(object sender, CardMoveEventArgs e)
        {
            // phase check (do we need to do the isCardBlocked call as well? No, we just don't block any more)
            if (_phases.Length > 0 &&
                (
                   _simulator.CurrentPhase == null || // phases are specified, but we arent in a phase
                  (_simulator.CurrentPhase != null && !_phases.Contains(_simulator.CurrentPhase.Name)) // phases specified and we are in a specified phase
                )
               )
                return;

            // if targets are set, then exist if this card isn't a candidate
            if (_targetDeliverable != null)
                if (e.Card.Deliverable != _targetDeliverable)
                    return;

            if (_targetCustomBacklog != null)
                if (e.Card.CustomBacklog != _targetCustomBacklog)
                    return;

           // is this a column i'm watching?
            if (_blockingEvent.ColumnId == e.ToColumn.Id && 
                
                ((e.Card.CardType == Enums.CardTypeEnum.Work && _blockingEvent.BlockWork) ||
                (e.Card.CardType == Enums.CardTypeEnum.Defect && _blockingEvent.BlockDefects) ||
                (e.Card.CardType == Enums.CardTypeEnum.AddedScope && _blockingEvent.BlockAddedScope))
               
                )
            {
                // card moving into watched column. Increment the count
                _cardsPassedSoFar++;

                // if the number of cards passed equals the randomly chosen, block this card when complete
                if (_cardsPassedSoFar == _cardIndexToBlock)
                {
                    _cardsBlockedByThisEvent.Add(e.Card, _chosenBlockTime);
                    _history.Add(new BlockingEventHistory { Card = e.Card, TotalBlockedTime = _chosenBlockTime });
                }

                // reset after high-bound scaled to 1 in x cards exceeded
                if (_cardsPassedSoFar >= _cardCountToResetCounter)
                {
                    _cardsPassedSoFar = 0;
                    pickNextCandidate(); 
                }
               
            }
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            // we need partial time intervals, so adding time is done in the IsBlocked method...
        }

        private void reset()
        {
            _cardsBlockedByThisEvent.Clear();
            _cardsPassedSoFar = 0;
            pickNextCandidate();
        }

        private void pickNextCandidate()
        {
            _cardCountToResetCounter = pickRandomOccurrence();
            _cardIndexToBlock = pickRandomCardIndex(_cardCountToResetCounter);
            _chosenBlockTime = pickRandomBlockTime();
        }
        
        private int pickRandomOccurrence()
        {
            return Common.PickRandomOccurrenceValueInt(_blockingEvent.OccurenceType, _blockingEvent.Scale, _blockingEvent.OccurrenceLowBound, _blockingEvent.OccurrenceHighBound, _blockingEvent.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
        }

        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }

        private double pickRandomBlockTime()
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
