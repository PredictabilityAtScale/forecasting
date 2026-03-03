using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;

namespace FocusedObjective.Simulation.Kanban
{

    internal class CardPosition
    {
        internal Card Card;
        internal int Position;
        internal bool HasViolatedWIP;
        internal bool AllowedToViolateWIP
        {
            get
            {
                if (this.Card != null)
                    return this.Card.ClassOfService.ViolateWIP;
                else
                    return false;
            }
        }
    }

    internal class TimeInterval
    {
        private Dictionary<SetupColumnData, List<CardPosition>> _positions = new Dictionary<SetupColumnData, List<CardPosition>>();
        private Dictionary<SetupColumnData, int> _wipLimitsDuringThisIteration = new Dictionary<SetupColumnData, int>();
        private int _violateWipLastPositionIndex = -1;

        internal int Sequence { get; set; }
        internal double ElapsedTime { get; set; }
        internal TimeInterval PreviousTimeInterval { get; set; }
        internal SetupPhaseData Phase { get; set; }

        internal int CountCardsInBacklog { get; set; }
        internal int CountCompletedCards { get; set; }

        public double ValueDeliveredSoFar { get; set; }
        public double CostPerDaySoFar { get; set; }

        public DateTime? CurrentDate { get; set; }

        internal KanbanSimulation Simulator { get; set; }

        internal Dictionary<SetupColumnData, List<CardPosition>> CardPositions
        {
            get { return _positions; }
        }

        internal int TotalWipLimitBoardPositions
        {
            get
            {
                int result = 0;

                foreach (int wip in _wipLimitsDuringThisIteration.Values)
                    result += wip;

                return result;
            }
        }

        internal int EmptyPositionsForColumn(SetupColumnData column)
        {
            // return the number of empty wip positions in a column for this interval.
            // even though during phase transitions there might be more cards
            // in a column than Wip positions, just return 0.
            return Math.Max(
                0,
                _wipLimitsDuringThisIteration[column] - CountCardsInPositionForColumn(column)
                );
        }

        internal void UpdateWipLimitsForThisInterval(List<SetupColumnData> columns)
        {
            foreach (var col in columns)
                _wipLimitsDuringThisIteration.Add(col, Simulator.CurrentColumnWIPLimit(col));
        }

        internal void AddCardInPositionForColumn(SetupColumnData column, int position, Card card)
        {
            List<CardPosition> list = new List<CardPosition>();

            if (_positions.ContainsKey(column))
                list = _positions[column];
            else
                _positions.Add(column, list);

            if (position > -1)
            {
                CardPosition cp = list.FirstOrDefault(pos => pos.Position == position);
                if (cp == null)
                    list.Add(new CardPosition { Card = card, Position = position });
                else // replace the existing card. Should this ever happen?
                    cp.Card = card;
            }
            else
            {
                // this could be a WIP violator, or just a column with infinite WIP
                if (column.WipLimit <= 0)
                    list.Add(new CardPosition { Card = card, Position = _violateWipLastPositionIndex, HasViolatedWIP = false });
                else
                    list.Add(new CardPosition { Card = card, Position = _violateWipLastPositionIndex, HasViolatedWIP = true });

                // everything has to have a unique position id, so keep going backwards
                _violateWipLastPositionIndex = _violateWipLastPositionIndex - 1;
            }

        }


        internal Card GetCardInPositionForColumn(SetupColumnData column, int position)
        {
            // this method is the most called method. Keep fast!
            if (_positions.ContainsKey(column))
            {
                List<CardPosition> positionList = _positions[column];

                for (int i = 0; i < positionList.Count; i++)
                {
                    if (positionList[i].Card != null &&
                        positionList[i].Position == position)
                        return positionList[i].Card;
                }
            }

            return null;
        }

        internal int GetPositionForCardInColumn(SetupColumnData column, Card card)
        {
            if (_positions.ContainsKey(column))
            {
                var cards = _positions[column];

                var pos = cards.FirstOrDefault(cp => cp.Card == card);

                if (pos != null)
                    return pos.Position;
            }

            return -1;
        }

        internal void RemoveCardInPositionForColumn(SetupColumnData column, int position)
        {
            if (_positions.ContainsKey(column))
            {
                var list = _positions[column];

                var cardpos = list.FirstOrDefault(cp => cp.Card != null && cp.Position == position);

                if (cardpos != null)
                    list.Remove(cardpos);
            }
        }

        internal int CountCardsInPositionForColumn(SetupColumnData column)
        {
            if (_positions.ContainsKey(column))
                return _positions[column].Where(c => c != null && c.Card != null).Count();
            else
                return 0;
        }

        internal int CountTotalCardsOnBoard()
        {
            int result = 0;

            foreach (var key in _positions.Keys)
                result += CountCardsInPositionForColumn(key);

            return result;
        }

        internal int CountTotalCardsOnBoard(Enums.CardStatusEnum cardStatus)
        {
            int result = 0;

            foreach (var key in _positions.Keys)
                result +=
                    _positions[key].Where(c => c != null && c.Card != null && c.Card.StatusHistoryForInterval(this.Sequence) == cardStatus).Count();

            return result;
        }

        internal int CountCardsOnBoard(Func<Card, bool> filter)
        {
            int result = 0;

            foreach (var key in _positions.Keys)
                result +=
                    _positions[key].Where(c => c != null && c.Card != null && filter(c.Card)).Count();

            return result;
        }

        internal int CountCardsForColumn(SetupColumnData column, Enums.CardStatusEnum cardStatus)
        {
            if (_positions.ContainsKey(column))
                return _positions[column].Where(c => c != null && c.Card != null && c.Card.StatusHistoryForInterval(this.Sequence) == cardStatus).Count();
            else
                return 0;
        }

        internal void BurnAllData()
        {
            // this method removed all un-necessary data as an optimization for simulation that only require interval data
            _positions.Clear();
            _wipLimitsDuringThisIteration.Clear();
            PreviousTimeInterval = null;
            this.Phase = null;
            this.CurrentDate = null;
            this.Simulator = null;
        }
    }
}
