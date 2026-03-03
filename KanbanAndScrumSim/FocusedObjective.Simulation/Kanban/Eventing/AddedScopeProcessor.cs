using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class AddedScopeProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private SetupAddedScopeData _addedScope;
        private Distribution _occurrenceDistribution;

        private int _cardsPassedSoFar = 0;
        private int _cardCountToResetCounter = -1;
        private int _cardIndexToTrigger = -1;

        private SetupBacklogCustomData _customBacklog = null;
        private SetupBacklogDeliverableData _deliverable = null;

        private SetupBacklogCustomData _targetCustomBacklog = null;
        private SetupBacklogDeliverableData _targetDeliverable = null;

        private string[] _phases;

        internal AddedScopeProcessor(KanbanSimulation sim, SetupAddedScopeData addedScope)
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

            if (_addedScope.OccurrenceDistribution != "")
            {
                SetupDistributionData distribution = 
                    _simulator.SimulationData.Setup.Distributions.FirstOrDefault(d => string.Compare(d.Name,_addedScope.OccurrenceDistribution, true) == 0);

                // the distribution settings were tested as part of the SimML validation, so this should NEVER be null
                if (distribution != null)
                    _occurrenceDistribution = DistributionHelper.CreateDistribution(distribution);
            }

            /* set the deliverable and custom backlog new added items inherit if they are specified.
             * customBacklog's can be within deliverables. If deliverable isn't specified look for the first that matches the name (priority to one that has no deliverable).
             */
            if (!string.IsNullOrWhiteSpace(this._addedScope.Deliverable))
                _deliverable = _simulator.SimulationData.Setup.Backlog.Deliverables.FirstOrDefault(c => c.Name == this._addedScope.Deliverable);

            if (!string.IsNullOrWhiteSpace(this._addedScope.CustomBacklog))
            {
                if (_deliverable == null)
                {
                    _customBacklog = _simulator.SimulationData.Setup.Backlog.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.CustomBacklog);
                    if (_customBacklog == null)
                    {
                        foreach (var d in _simulator.SimulationData.Setup.Backlog.Deliverables)
                        {
                            _customBacklog = d.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.CustomBacklog);
                            if (_customBacklog != null)
                                break;
                        }
                    }
                }
                else
                {
                    _customBacklog = _deliverable.CustomBacklog.FirstOrDefault(c => c.Name == this._addedScope.CustomBacklog);
                }
            }

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

            // connect to events required...

            //sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            //sim.RaiseCardMoveEvent += handleCardMoveEvent;
            //sim.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
            //sim.RaiseTimeIntervalTickEvent += handleTimeIntervalTickEvent;
            sim.RaiseCardCompleteEvent -= handleCardCompleteEvent; 
            sim.RaiseCardCompleteEvent += handleCardCompleteEvent;
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
                    //this._simulator.RaiseCardMoveEvent -= handleCardMoveEvent; 
                    this._simulator.RaiseTimeIntervalTickEvent -= handleTimeIntervalTickEvent;
                    this._simulator.RaiseCardCompleteEvent -= handleCardCompleteEvent;
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

            // if targets are set, then exist if this card isn't a candidate
            if (_targetDeliverable != null)
                if (e.Card.Deliverable != _targetDeliverable)
                    return;

            if (_targetCustomBacklog != null)
                if (e.Card.CustomBacklog != _targetCustomBacklog)
                    return;

            //todo: include defects and  added scope as well?
            if (e.Card.CardType == Enums.CardTypeEnum.Work)
            {
                // zero indicates do not count. Most often because sensitivy multiplier is 0.0
                if (_cardCountToResetCounter > 0)
                {
                    // card completed. Increment the count
                    _cardsPassedSoFar++;

                    // if the number of cards passed equals the randomly chosen, block this card when complete
                    if (_cardsPassedSoFar == _cardIndexToTrigger)
                    {
                        for (int count = 0; count < _addedScope.Count; count++)
                        {
                            // add scope
                            int nextIndex = _simulator.AllCardsList.Max(i => i.Index) + 1;

                            Card card = new Card
                                {
                                    CardType = Enums.CardTypeEnum.AddedScope,
                                    Status = Enums.CardStatusEnum.InBacklog,
                                    Index = nextIndex,
                                    Name = string.Format(_addedScope.Name, nextIndex),
                                    Simulator = _simulator,
                                    ClassOfServiceName = this._addedScope.ClassOfService,
                                    Deliverable = _deliverable ??  e.Card.Deliverable,
                                    CustomBacklog = _customBacklog ?? e.Card.CustomBacklog
                                };

                            _simulator.BacklogList.Add(card);
                            _simulator.AllCardsList.Add(card);

                            //System.Diagnostics.Debug.WriteLine(string.Format("Added scope card added: {0}", card.Name), "sim");

                        }

                    }

                    // reset after high-bound scaled to 1 in x cards exceeded
                    if (_cardsPassedSoFar >= _cardCountToResetCounter)
                        reset();
                }
            }
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            // we need partial time intervals, so adding time is done in the IsBlocked method...
        }

        private void reset()
        {
            _cardsPassedSoFar = 0;
            pickNextCandidate();
        }

        private void pickNextCandidate()
        {
            _cardCountToResetCounter = pickRandomOccurrence();
            _cardIndexToTrigger = pickRandomCardIndex(_cardCountToResetCounter);
        }
        
        private int pickRandomOccurrence()
        {
            return Common.PickRandomOccurrenceValueInt(_addedScope.OccurenceType, _addedScope.Scale, _addedScope.OccurrenceLowBound, _addedScope.OccurrenceHighBound, _addedScope.SensitivityOccurrenceMultiplier, _simulator.CurrentPhase, _occurrenceDistribution);
        }

        private int pickRandomCardIndex(int max)
        {
            //todo:pick random index in the range. for now, just return the max.
            return max;
        }
    }
}
