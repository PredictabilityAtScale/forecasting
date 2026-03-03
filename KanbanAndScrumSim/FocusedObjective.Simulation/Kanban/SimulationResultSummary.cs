using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using System.Reflection;

namespace FocusedObjective.Simulation.Kanban
{


    internal  class SimulationResultSummary
    {
        private KanbanSimulation _simulator;

        private bool _disconnect = true;
        private bool _calculatedTimentervalData = false;

        // to get a real average of empty positions, we want to skip any time interval 
        // before one card makes complete and after no backlog. 
        private Func<TimeInterval, bool> nonStartOrEnd = t => t.CountCardsInBacklog > 0 && t.CountCompletedCards > 0;

        internal  SimulationResultSummary(KanbanSimulation simulator, bool disconnect = true)
        {
            _simulator = simulator;
            
            _disconnect = disconnect;

            // to save ram, disconnect caches values....
            if (disconnect)
                Disconnect(
                    _simulator.SimulationData.Execute.ReturnResults);
        }

        internal SimulationResultSummary(string results)
        {
            // set the values from XML in cloud run...
            FromXML(XElement.Parse(results));
        }

        internal  void Disconnect(string returnResults)
        {

            string[] cache = new string[] { };

            if (!string.IsNullOrWhiteSpace(returnResults))
                cache = returnResults.Split(new char[] { ',', ' ', '|' });

            // this DOUBLED the time taken :( 
            //if (cache.Length == 0)
            //    calculateTimeIntervalData();

            // read all data once so it is cached
            object o;
            PropertyInfo[] propertyInfos = typeof(SimulationResultSummary).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var p in propertyInfos)
            {
                if (cache.Length == 0 ||
                    cache.Contains(p.Name))
                {
                    o = p.GetValue(this, null);
                }
            }

            //  remove the reference to the simulator
            _simulator = null;
        }

        private int _intervals;

        /// <summary>
        /// Gets the number of time-intervals for the simulation run.
        /// </summary>
        internal  int Intervals
        {
            get
            {
                if (_simulator != null)
                    if (_simulator.ResultTimeIntervals.Any())
                        _intervals = _simulator.ResultTimeIntervals.Count() - 1;

                return _intervals;
            }
        }

        // can now be less than 0
        private StatisticResults<int> _emptyPositions;
        internal  StatisticResults<int> EmptyPositions
        {
            get
            {
                if (_simulator != null && !_calculatedTimentervalData)
                    _emptyPositions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(c => c.TotalWipLimitBoardPositions - c.CountTotalCardsOnBoard())
                         , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _emptyPositions;
            }
        }

        private StatisticResults<int> _queuedPositions;
        internal  StatisticResults<int> QueuedPositions
        {
            get
            {
                if (_simulator != null && !_calculatedTimentervalData)
                    _queuedPositions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(c => c.CountTotalCardsOnBoard(Enums.CardStatusEnum.CompletedButWaitingForFreePosition))
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _queuedPositions;
            }
        }

        private StatisticResults<int> _blockedPositions;
        internal  StatisticResults<int> BlockedPositions
        {
            get
            {
                if (_simulator != null && !_calculatedTimentervalData)
                    _blockedPositions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(c => c.CountTotalCardsOnBoard(Enums.CardStatusEnum.Blocked))
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);

                return _blockedPositions;
            }
        }

        private StatisticResults<int> _activePositions;
        internal StatisticResults<int> ActivePositions
        {
            get
            {
                // only cards being worked at the moment active WIP
                if (_simulator != null && !_calculatedTimentervalData)
                    _activePositions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(i => i.CountCardsOnBoard(
                            c => c != null && 
                                 (c.StatusHistoryForInterval(i.Sequence) == Enums.CardStatusEnum.NewStatusThisInterval || 
                                  c.StatusHistoryForInterval(i.Sequence) == Enums.CardStatusEnum.SameStatusThisInterval)))
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);

                return _activePositions;
            }
        }

        private StatisticResults<int> _inActivePositions;
        internal StatisticResults<int> InActivePositions
        {
            get
            {
                // only cards being worked at the moment active WIP
                if (_simulator != null && !_calculatedTimentervalData)
                    _inActivePositions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(i => i.CountCardsOnBoard(
                            c => c != null &&
                                 (c.StatusHistoryForInterval(i.Sequence) != Enums.CardStatusEnum.NewStatusThisInterval ||
                                  c.StatusHistoryForInterval(i.Sequence) != Enums.CardStatusEnum.SameStatusThisInterval)))
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);

                return _inActivePositions;
            }
        }

        private StatisticResults<int> _pullTransactions;
        internal StatisticResults<int> PullTransactions
        {
            get
            {
                // only cards being worked at the moment active WIP
                if (_simulator != null && !_calculatedTimentervalData)
                    _pullTransactions = new StatisticResults<int>(
                        _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(i => i.CountCardsOnBoard(
                            c => c != null &&
                                 (c.StatusHistoryForInterval(i.Sequence) == Enums.CardStatusEnum.NewStatusThisInterval)))
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);

                return _pullTransactions;
            }
        }

        private int _workCount;
        internal  int WorkCount
        {
            get
            {
                if (_simulator != null)
                    _workCount = _simulator.AllCardsList.Count(c => c.CardType == Enums.CardTypeEnum.Work);

                return _workCount;
            }
        }

        private int _addedScopeCount;
        internal  int AddedScopeCount
        {
            get
            {
                if (_simulator != null)
                    _addedScopeCount = _simulator.AllCardsList.Count(c => c.CardType == Enums.CardTypeEnum.AddedScope);

                return _addedScopeCount;
            }
        }

        private int _defectCount;
        internal  int DefectCount
        {
            get
            {
                if (_simulator != null)
                    _defectCount = _simulator.AllCardsList.Count(c => c.CardType == Enums.CardTypeEnum.Defect);

                return _defectCount;
            }
        }

        private StatisticResults<double> _workCycleTime;
        internal  StatisticResults<double> WorkCycleTime
        {
            get
            {
                if (_simulator != null)
                    _workCycleTime = new StatisticResults<double>(
                        _simulator
                        .AllCardsList
                        .Where(c => c.CardType == Enums.CardTypeEnum.Work)
                        .Select(c => c.CycleTime)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _workCycleTime;
            }
        }

        private StatisticResults<double> _addedScopeCycleTime;
        internal  StatisticResults<double> AddedScopeCycleTime
        {
            get
            {
                if (_simulator != null)
                    _addedScopeCycleTime = new StatisticResults<double>(
                        _simulator
                        .AllCardsList
                        .Where(c => c.CardType == Enums.CardTypeEnum.AddedScope)
                        .Select(c => c.CycleTime)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _addedScopeCycleTime;
            }
        }

        private StatisticResults<double> _defectCycleTime;
        internal  StatisticResults<double> DefectCycleTime
        {
            get
            {
                if (_simulator != null)
                    _defectCycleTime = new StatisticResults<double>(
                        _simulator
                        .AllCardsList
                        .Where(c => c.CardType == Enums.CardTypeEnum.Defect)
                        .Select(c => c.CycleTime)
                         , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _defectCycleTime;
            }
        }

        private Dictionary<FocusedObjective.Contract.SetupClassOfServiceData, StatisticResults<double>> _classOfServiceCycleTimes;

        internal Dictionary<FocusedObjective.Contract.SetupClassOfServiceData, StatisticResults<double>> ClassOfServiceCycleTimes
        {
            get
            {
                if (_simulator != null)
                {
                    _classOfServiceCycleTimes = _simulator
                        .AllCardsList
                        .GroupBy(c => c.ClassOfService, c => c.CycleTime)
                        .ToDictionary(g => g.Key, g => new StatisticResults<double>(
                                g,
                                _simulator.SimulationData.Execute.DecimalRounding,
                                _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat));

                   }
                return _classOfServiceCycleTimes;
            }
        }


        private Dictionary<FocusedObjective.Contract.SetupColumnData, StatisticResults<int>> _columnActivePositions;

        internal Dictionary<FocusedObjective.Contract.SetupColumnData, StatisticResults<int>> ColumnActivePositions
        {
            get
            {
                if (_simulator != null)
                {
                    _columnActivePositions = new Dictionary<Contract.SetupColumnData, StatisticResults<int>>();

                    foreach (var col in _simulator.SimulationData.Setup.Columns)
	                {
                        var r = 
                            _simulator
                                .ResultTimeIntervals
                                .Select(ti => 
                                    ti.CountCardsForColumn(col, Enums.CardStatusEnum.NewStatusThisInterval) +
                                    ti.CountCardsForColumn(col, Enums.CardStatusEnum.SameStatusThisInterval));
 

                        _columnActivePositions.Add(col, 
                            new StatisticResults<int>(
                                r,
                                _simulator.SimulationData.Execute.DecimalRounding,
                                _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat));
                    }

                }

                return _columnActivePositions;
            }
        }

        
        private Dictionary<int, int> _intervalCompletedCards;

        internal Dictionary<int, int> IntervalCompletedCards
        {
            get
            {
                if (_simulator != null)
                {
                    _intervalCompletedCards = new Dictionary<int, int>();

                    var r =
                           _simulator
                               .ResultTimeIntervals
                               .Where(ti => ti != null)
                               .Select(ti => 
                                   new {count = ti.CountCompletedCards, 
                                        seq = ti.Sequence }
                                       );

                    foreach (var item in r)
                        _intervalCompletedCards.Add(item.seq, item.count);
                }

                return _intervalCompletedCards;
            }
        }

        private double _totalCost;
        internal double TotalCost
        {
            get
            {
                if (_simulator != null)
                    _totalCost = 
                        _simulator
                        .ResultTimeIntervals
                        .Last().CostPerDaySoFar;

                return _totalCost;
            }
        }


        internal XElement AsXML()
        {

            XElement result = new XElement("statistics");

            result.Add(new XElement("intervals",
                new XAttribute("value", this.Intervals)));

            result.Add(new XElement("totalCost",
                new XAttribute("value", this.TotalCost)));

            XElement cards = new XElement("cards");
            result.Add(cards);

            XElement work = new XElement("work",
                new XAttribute("value", this.WorkCount));

            if (this.WorkCycleTime != null)
                work.Add(this.WorkCycleTime.AsXML("cycleTime"));

            cards.Add(work);

            XElement addedScope = new XElement("addedScope",
                        new XAttribute("value", this.AddedScopeCount));

            if (this.AddedScopeCycleTime != null)
                addedScope.Add(this.AddedScopeCycleTime.AsXML("cycleTime"));

            cards.Add(addedScope);

            XElement defects = new XElement("defect",
                        new XAttribute("value", this.DefectCount));

            if (this.DefectCycleTime != null)
                defects.Add(this.DefectCycleTime.AsXML("cycleTime"));

            cards.Add(defects);

            if (this.EmptyPositions != null)
                result.Add(this.EmptyPositions.AsXML("emptyPositions"));

            if (this.QueuedPositions != null)
                result.Add(this.QueuedPositions.AsXML("queuedPositions"));

            if (this.BlockedPositions != null)
                result.Add(this.BlockedPositions.AsXML("blockedPositions"));

            if (this.ActivePositions != null)
                result.Add(this.ActivePositions.AsXML("activePositions"));

            if (this.InActivePositions != null)
                result.Add(this.InActivePositions.AsXML("inActivePositions"));

            if (this.PullTransactions != null)
                result.Add(this.PullTransactions.AsXML("pullTransactions"));

            if (_classOfServiceCycleTimes != null && _classOfServiceCycleTimes.Count > 0)
            {

                XElement cosElement = new XElement("classOfServices");

                foreach (var cos in _classOfServiceCycleTimes.Keys)
                {
                    cosElement.Add(
                        new XElement("classOfService",
                        new XAttribute("name", cos.Name),
                        new XAttribute("count", _classOfServiceCycleTimes[cos].Count),
                        _classOfServiceCycleTimes[cos].AsXML("cycleTime")));
                }

                result.Add(cosElement);
            }

            if (_columnActivePositions != null && _columnActivePositions.Count > 0)
            {
                XElement colElement = new XElement("columns");

                foreach (var col in _columnActivePositions.Keys)
                {
                    colElement.Add(
                        new XElement("column",
                        new XAttribute("name", col.Name),
                        new XAttribute("count", _columnActivePositions[col].Count),
                        _columnActivePositions[col].AsXML("activePositions")));
                }

                result.Add(colElement);
            }

            if (IntervalCompletedCards != null && IntervalCompletedCards.Any())
            {
                XElement completedCount = new XElement("completedProgress");

                foreach (var item in IntervalCompletedCards)
                {
                    completedCount.Add(
                        new XElement("interval",
                            new XAttribute("sequence", item.Key),
                            new XAttribute("count", item.Value)));
                }

                result.Add(completedCount);
            }

            return result;
        }


        internal bool FromXML(XElement source)
        {

            XElement root = source.Element("statistics");

            if (root == null)
                return false;

            _intervals = int.Parse(root.Element("intervals").Attribute("value").Value);
            _totalCost = double.Parse(root.Element("totalCost").Attribute("value").Value);


            XElement cards = root.Element("cards");
            XElement work = cards.Element("work");

            _workCount = int.Parse(work.Attribute("value").Value);
            _workCycleTime = new StatisticResults<double>(work.Element("cycleTime").Element("sip"));

            XElement addedScope = cards.Element("addedScope");
            if (addedScope != null)
            {
                _addedScopeCount = int.Parse(addedScope.Attribute("value").Value);
                _addedScopeCycleTime = new StatisticResults<double>(addedScope.Element("cycleTime").Element("sip"));
            }

            XElement defects = cards.Element("defects");
            if (defects != null)
            {
                _defectCount = int.Parse(defects.Attribute("value").Value);
                _defectCycleTime = new StatisticResults<double>(defects.Element("cycleTime").Element("sip"));
            }

            XElement emptyPositions = root.Element("emptyPositions");
            if (emptyPositions != null)
                _emptyPositions = new StatisticResults<int>(emptyPositions.Element("sip"));

            XElement queuedPositions = root.Element("queuedPositions");
            if (queuedPositions != null)
                _queuedPositions = new StatisticResults<int>(queuedPositions.Element("sip"));

            XElement blockedPositions = root.Element("blockedPositions");
            if (blockedPositions != null)
                _blockedPositions = new StatisticResults<int>(blockedPositions.Element("sip"));

            XElement activePositions = root.Element("activePositions");
            if (activePositions != null)
                _activePositions = new StatisticResults<int>(activePositions.Element("sip"));

            XElement inActivePositions = root.Element("inActivePositions");
            if (inActivePositions != null)
                _inActivePositions = new StatisticResults<int>(inActivePositions.Element("sip"));

            /*
            if (_classOfServiceCycleTimes != null && _classOfServiceCycleTimes.Count > 0)
            {

                XElement cosElement = new XElement("classOfServices");

                foreach (var cos in _classOfServiceCycleTimes.Keys)
                {
                    cosElement.Add(
                        new XElement("classOfService",
                        new XAttribute("name", cos.Name),
                        new XAttribute("count", _classOfServiceCycleTimes[cos].Count),
                        _classOfServiceCycleTimes[cos].AsXML("cycleTime")));
                }

                result.Add(cosElement);
            }

            if (_columnActivePositions != null && _columnActivePositions.Count > 0)
            {
                XElement colElement = new XElement("columns");

                foreach (var col in _columnActivePositions.Keys)
                {
                    colElement.Add(
                        new XElement("column",
                        new XAttribute("name", col.Name),
                        new XAttribute("count", _columnActivePositions[col].Count),
                        _columnActivePositions[col].AsXML("activePositions")));
                }

                result.Add(colElement);
            }

             if (IntervalCompletedCards != null && IntervalCompletedCards.Any())
            {
                XElement completedCount = new XElement("completedProgress");

                foreach (var item in IntervalCompletedCards)
                {
                    completedCount.Add(
                        new XElement("interval",
                            new XAttribute("sequence", item.Key),
                            new XAttribute("count", item.Value)));
                }

                result.Add(completedCount);
            }

             * */
            return true;
        }

        /*
        private void calculateTimeIntervalData()
        {
            var q = _simulator
                        .ResultTimeIntervals
                        .Where(nonStartOrEnd)
                        .Select(c => new
                        {
                            EmptyPositions = c.TotalWipLimitBoardPositions - c.CountTotalCardsOnBoard(),
                            QueuedPositions = c.CountTotalCardsOnBoard(Enums.CardStatusEnum.CompletedButWaitingForFreePosition),
                            BlockedPositions = c.CountTotalCardsOnBoard(Enums.CardStatusEnum.Blocked),
                            ActivePositions = c.CountCardsOnBoard(
                                    i => i != null &&
                                    (i.StatusHistoryForInterval(c.Sequence) == Enums.CardStatusEnum.NewStatusThisInterval ||
                                    i.StatusHistoryForInterval(c.Sequence) == Enums.CardStatusEnum.SameStatusThisInterval)),
                            InActivePositions = c.CountCardsOnBoard(
                                    i => i != null &&
                                    (i.StatusHistoryForInterval(c.Sequence) != Enums.CardStatusEnum.NewStatusThisInterval ||
                                    i.StatusHistoryForInterval(c.Sequence) != Enums.CardStatusEnum.SameStatusThisInterval)),

                            PullTransactions = c.CountCardsOnBoard(
                                    i => i != null &&
                                    (i.StatusHistoryForInterval(c.Sequence) == Enums.CardStatusEnum.NewStatusThisInterval))
                        });

            _emptyPositions = new StatisticResults<int>(q.Select(v => v.EmptyPositions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
            _queuedPositions = new StatisticResults<int>(q.Select(v => v.QueuedPositions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
            _blockedPositions = new StatisticResults<int>(q.Select(v => v.BlockedPositions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
            _activePositions = new StatisticResults<int>(q.Select(v => v.ActivePositions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
            _inActivePositions = new StatisticResults<int>(q.Select(v => v.InActivePositions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
            _pullTransactions = new StatisticResults<int>(q.Select(v => v.PullTransactions), _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);

            _calculatedTimentervalData = true;
        
        }

    */

    }
}

