using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class DefectProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private SetupDefectData _defect;
        private TimeInterval _latestTimeInterval;
        private Distribution _occurrenceDistribution;

        private int _cardsPassedSoFar = 0;
        private int _cardCountToResetCounter = -1;
        private int _cardIndexToDefect = -1;

        private string[] _phases;

        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;

        internal DefectProcessor(KanbanSimulation sim, SetupDefectData defect)
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
            sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            sim.RaiseCardMoveEvent += handleCardMoveEvent;
            sim.RaiseCardCompleteEvent -= handleCardCompleteEvent;
            sim.RaiseCardCompleteEvent += handleCardCompleteEvent;
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
                    this._simulator.RaiseCardCompleteEvent -= handleCardCompleteEvent;
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

        private void handleCardMoveEvent(object sender, CardMoveEventArgs e)
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
                if (e.Card.Deliverable != _targetDeliverable)
                    return;

            if (_targetCustomBacklog != null)
                if (e.Card.CustomBacklog != _targetCustomBacklog)
                    return;

            // is this a column i'm watching?
            //todo: include defects and  added scope as well?
            if (_defect.ColumnId == e.ToColumn.Id &&
                e.Card.CardType == Enums.CardTypeEnum.Work)
            {
                // zero indicates do not count. Most often because sensitivy multiplier is 0.0
                if (_cardCountToResetCounter > 0)
                {
                    // card moving into watched column. Increment the count
                    _cardsPassedSoFar++;

                    // if the number of cards passed equals the randomly chosen, add defect now
                    if (_cardsPassedSoFar == _cardIndexToDefect)
                    {
                        if (_defect.IsCardMove)
                            _simulator.SendCardToCompleteEarly(_latestTimeInterval, e.Card, _defect.ColumnId);
                        
                        // create defects
                        for (int i = 0; i < _defect.Count; i++)
                            createDefect(e.Card);
                    }

                    // reset after high-bound scaled to 1 in x cards exceeded
                    if (_cardsPassedSoFar >= _cardCountToResetCounter)
                        reset();
                }
            }
        }
        
        private void handleCardCompleteEvent(object sender, CardCompleteEventArgs e)
        {
            // phase check
            if (_phases.Length > 0 &&
                (
                   _simulator.CurrentPhase == null || // phases are specified, but we arent in a phase
                  (_simulator.CurrentPhase != null && !_phases.Contains(_simulator.CurrentPhase.Name)) // phases specified and we are in a specified phase
                )
               )
                return;

            // completed card watching. -1 is the id to watch (i know this is bad, since -1 = backlog for startsInColumnId
            if (_defect.ColumnId == -1 &&
                e.Card.CardType == Enums.CardTypeEnum.Work
                )
            {
                // card moving into watched column. Increment the count
                _cardsPassedSoFar++;

                // if the number of cards passed equals the randomly chosen, add defect now
                if (_cardsPassedSoFar == _cardIndexToDefect)
                {
                    if (_defect.IsCardMove)
                        _simulator.SendCardToCompleteEarly(_latestTimeInterval, e.Card, _defect.ColumnId);

                    // create defects
                    for (int i = 0; i < _defect.Count; i++)
                        createDefect(e.Card);
                }

                // reset after high-bound scaled to 1 in x cards exceeded
                if (_cardsPassedSoFar >= _cardCountToResetCounter)
                    reset();
            }
        }

        private void createDefect(Card cardCausingDefect)
        {
            int nextIndex = _simulator.AllCardsList.Max(i => i.Index) + 1;

            Card card = new Card
            {
                Index = nextIndex,
                CardType = Enums.CardTypeEnum.Defect,
                Status = Enums.CardStatusEnum.None,
                Name = string.Format(_defect.Name, nextIndex, cardCausingDefect.Index, cardCausingDefect.Name),
                DefectData = _defect,
                Simulator = _simulator,
                CustomBacklog = cardCausingDefect.CustomBacklog,  //TODO:is this a good idea?
                ClassOfServiceName = this._defect.ClassOfService,
                Deliverable = cardCausingDefect.Deliverable // defect in the same deliverable as the triggering card
            };

            _simulator.AddToDefectBacklog(
                _defect.StartsInColumnId,
                card);

            //System.Diagnostics.Debug.WriteLine(string.Format("Defect card added: {0} (index:{1}) in column id {2}", card.Name, card.Index, _defect.ColumnId), "sim");
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            _latestTimeInterval = e.TimeInterval;
        }

        private void reset()
        {
            _cardsPassedSoFar = 0;
            pickNextCandidate();
        }

        private void pickNextCandidate()
        {
            _cardCountToResetCounter = pickRandomOccurrence();
            _cardIndexToDefect = pickRandomCardIndex(_cardCountToResetCounter);
        }
        
        private int pickRandomOccurrence()
        {
            return Common.PickRandomOccurrenceValueInt(_defect.OccurenceType, _defect.Scale, _defect.OccurrenceLowBound, _defect.OccurrenceHighBound, _defect.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
        }

        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }
    }
}
