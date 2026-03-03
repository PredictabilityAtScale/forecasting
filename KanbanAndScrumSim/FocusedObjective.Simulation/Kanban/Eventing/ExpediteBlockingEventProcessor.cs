using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class ExpediteCardBlockingEventProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;

        // key is the card, the value is time blocked so far
        private Dictionary<string,double> _cardsBlockedByThisEvent = new Dictionary<string,double>();
        private Dictionary<string, double> _timeCardSpentBlockedByColumn = new Dictionary<string, double>();

        internal ExpediteCardBlockingEventProcessor(KanbanSimulation sim)
        {
            _simulator = sim;
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
                    //this._simulator.RaiseCardCompleteEvent -= handleCardCompleteEvent;
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~ExpediteCardBlockingEventProcessor()
        {
            Dispose (false);
        }

        internal double TimeSpentBlockedBecauseOfViolateWIP(SetupColumnData column, Card card)
        {
            double result = 0.0;

            if (_timeCardSpentBlockedByColumn.ContainsKey(column.Name + "." + card.Name))
                result = _timeCardSpentBlockedByColumn[column.Name + "." + card.Name];

            return result;
        }


        internal void AddCardToBlockList(SetupColumnData column, Card card, double blockTime)
        {

            // use this dictionary entry to keep track of how much longer to block
            // this is a decremented interval count - DO NOT USE for time calcs
            if (_cardsBlockedByThisEvent.ContainsKey(column.Name + "." + card.Name))
                _cardsBlockedByThisEvent[column.Name + "." + card.Name] = _cardsBlockedByThisEvent[column.Name + "." + card.Name] + blockTime;
            else
                _cardsBlockedByThisEvent.Add(column.Name + "." + card.Name, blockTime);

            // keep track of how long total we are blocked by each column for cycle-time calcs
            if (_timeCardSpentBlockedByColumn.ContainsKey(column.Name + "." + card.Name))
                _timeCardSpentBlockedByColumn[column.Name + "." + card.Name] = _timeCardSpentBlockedByColumn[column.Name + "." + card.Name] + blockTime;
            else
                _timeCardSpentBlockedByColumn.Add(column.Name + "." + card.Name, blockTime);

            card.Status = Enums.CardStatusEnum.Blocked;
        }

        internal double BlockTimeForCard(SetupColumnData column, Card card)
        {
            double result = 0.0;

            // this is the original time
            if (_timeCardSpentBlockedByColumn.Keys.Contains(column.Name + "." + card.Name))
                result = _timeCardSpentBlockedByColumn[column.Name + "." + card.Name];

            return result;
        }
      
        internal bool IsCardBlocked(SetupColumnData column, Card card, double timeInterval)
        {
            bool result = false;

            if (_cardsBlockedByThisEvent.Keys.Contains(column.Name + "." + card.Name))
            {
                _cardsBlockedByThisEvent[column.Name + "." + card.Name] -= timeInterval;

                if (_cardsBlockedByThisEvent[column.Name + "." + card.Name] > 0.0)
                {
                    result = true;
                }

                if (_cardsBlockedByThisEvent[column.Name + "." + card.Name] <= 0.0)
                {
                    _cardsBlockedByThisEvent.Remove(column.Name + "." + card.Name);
                    card.Status = Enums.CardStatusEnum.SameStatusThisInterval;
                }
            }

            return result;
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            // we need partial time intervals, so adding time is done in the IsBlocked method...
        }
    }
}
