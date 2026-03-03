using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class ReplenishIntervalProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private int _intervalsPastSoFar = 0;
        private SetupColumnData _column = null;
        private int _triggerValue = -1;
        private bool _triggerThisInterval = false;

        internal ReplenishIntervalProcessor(KanbanSimulation sim, SetupColumnData column, int triggerValue)
        {
            _simulator = sim;
            _column = column;
            _triggerValue = triggerValue;

            // connect to events required...
            //sim.RaiseCardMoveEvent -= handleCardMoveEvent; 
            //sim.RaiseCardMoveEvent += handleCardMoveEvent;
            //sim.RaiseCardCompleteEvent -= handleCardCompleteEvent;
            //sim.RaiseCardCompleteEvent += handleCardCompleteEvent;
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
        ~ReplenishIntervalProcessor()
        {
            Dispose (false);
        }

        internal bool TriggerThisInterval
        {
            get { return _triggerThisInterval; }
        }

        internal SetupColumnData Column
        {
            get { return _column; }
        }

        private void handleTimeIntervalTickEvent(object sender, TimeIntervalTickEventArgs e)
        {
            _triggerThisInterval = false;
            _intervalsPastSoFar++;

            // initially take work rather than wait for trigger...
            //NOTE: FUTURE MAYBE: If this is an infinite WIP column and a buffer, don't initially take work
            bool triggerFirstInterval = e.TimeInterval.Sequence == 1;
                 // && (Column.WipLimit > 0);

            if (_intervalsPastSoFar >= _triggerValue
                || triggerFirstInterval) 
            {
                _triggerThisInterval = true;
                _intervalsPastSoFar = 0;
            }
        }
    }
}
