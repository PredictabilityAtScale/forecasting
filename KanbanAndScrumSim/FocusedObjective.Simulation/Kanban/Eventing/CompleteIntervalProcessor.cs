using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation.Kanban
{

    internal class CompleteIntervalProcessor : IDisposable
    {
        private bool disposed = false;

        private KanbanSimulation _simulator;
        private int _intervalsPastSoFar = 0;
        private SetupColumnData _column = null;
        private int _triggerValue = -1;
        private bool _triggerThisInterval = false;

        internal CompleteIntervalProcessor(KanbanSimulation sim, SetupColumnData column, int triggerValue)
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
        ~CompleteIntervalProcessor()
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

            if (_intervalsPastSoFar >= _triggerValue)
            {
                _triggerThisInterval = true;
                _intervalsPastSoFar = 0;
            }
        }
    }
}
