using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using System.Reflection;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Enums;

namespace FocusedObjective.Simulation.Kanban
{

    internal class MonteCarloResultSummary
    {
        private SimulationData _data;
        private List<SimulationResultSummary> _simResults;
        private bool _disconnect = true;
        private bool _rawResults = false;

        private static Func<StatisticResults<int>, double> selectorAverageInt = r => r.Average;
        private static Func<StatisticResults<double>, double> selectorAverageDouble = r => r.Average;
        private static Func<StatisticResults<int>, double> selectorMedianInt = r => r.Median;
        private static Func<StatisticResults<double>, double> selectorMedianDouble = r => r.Median;
        private static Func<StatisticResults<int>, double> selectorFifthInt = r => r.FifthPercentile;
        private static Func<StatisticResults<double>, double> selectorFifthDouble = r => r.FifthPercentile;
        private static Func<StatisticResults<int>, double> selectorTwentyFifthInt = r => r.TwentyFifthPercentile;
        private static Func<StatisticResults<double>, double> selectorTwentyFifthDouble = r => r.TwentyFifthPercentile;
        private static Func<StatisticResults<int>, double> selectorSeventyFifthInt = r => r.SeventyFifthPercentile;
        private static Func<StatisticResults<double>, double> selectorSeventyFifthDouble = r => r.SeventyFifthPercentile;
        private static Func<StatisticResults<int>, double> selectorNinetyFifthInt = r => r.NinetyFifthPercentile;
        private static Func<StatisticResults<double>, double> selectorNinetyFifthDouble = r => r.NinetyFifthPercentile;
        private static Func<StatisticResults<int>, double> selectorMaximumInt = r => r.Maximum;
        private static Func<StatisticResults<double>, double> selectorMaximumDouble = r => r.Maximum;

        private Func<StatisticResults<int>, double> currentIntSelector = selectorAverageInt;
        private Func<StatisticResults<double>, double> currentDoubleSelector = selectorAverageDouble;

        internal MonteCarloResultSummary(SimulationData data, List<SimulationResultSummary> simResults, bool disconnect = true, bool rawResults = false)
        {
            _simResults = simResults;
            _data = data;
            _disconnect = disconnect;
            _rawResults = rawResults;

            switch (data.Execute.AggregationValue)
            {

                case AggregationValueEnum.Median:
                    currentIntSelector = selectorMedianInt;
                    currentDoubleSelector = selectorMedianDouble;
                    break;

                case AggregationValueEnum.Fifth:
                    currentIntSelector = selectorFifthInt;
                    currentDoubleSelector = selectorFifthDouble;
                    break;

                case AggregationValueEnum.TwentyFifth:
                    currentIntSelector = selectorTwentyFifthInt;
                    currentDoubleSelector = selectorTwentyFifthDouble;
                    break;

                case AggregationValueEnum.SeventyFifth:
                    currentIntSelector = selectorSeventyFifthInt;
                    currentDoubleSelector = selectorSeventyFifthDouble;
                    break;
                
                case AggregationValueEnum.NinetyFifth:
                    currentIntSelector = selectorNinetyFifthInt;
                    currentDoubleSelector = selectorNinetyFifthDouble;
                    break;

                case AggregationValueEnum.Maximum:
                    currentIntSelector = selectorMaximumInt;
                    currentDoubleSelector = selectorMaximumDouble;
                    break;
                
                default:
                    currentIntSelector = selectorAverageInt;
                    currentDoubleSelector = selectorAverageDouble;
                    break;
            }


            // to save ram, disconnect caches values....
            if (disconnect)
                Disconnect(data.Execute.ReturnResults);
        }

        internal void Disconnect(string returnResults)
        {
            string[] cache = new string[] { };

            if (!string.IsNullOrWhiteSpace(returnResults))
                cache = returnResults.Split(new char[] { ',', ' ', '|' });

            // read all data once so it is cached
            object o;
            PropertyInfo[] propertyInfos = typeof(MonteCarloResultSummary).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var p in propertyInfos)
            {
                if (cache.Length == 0 ||
                    cache.Contains(p.Name))
                {
                    o = p.GetValue(this, null);
                }
            }

            //  remove the reference to the simulator
            _simResults = null;
        }

        private StatisticResults<int> _intervals;
        
        /// <summary>
        /// Gets the number of time-intervals for each the simulation run.
        /// </summary>
        internal StatisticResults<int> Intervals
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _intervals = new StatisticResults<int>(
                            _simResults.Select(r => r.Intervals), 
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _intervals;
            }
        }

        private StatisticResults<double> _emptyPositions;
        internal StatisticResults<double> EmptyPositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _emptyPositions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.EmptyPositions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _emptyPositions;
            }
        }

        private StatisticResults<double> _queuedPositions;
        internal StatisticResults<double> QueuedPositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _queuedPositions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.QueuedPositions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat); 
                
                return _queuedPositions;
            }
        }

        private StatisticResults<double> _blockedPositions;
        internal StatisticResults<double> BlockedPositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _blockedPositions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.BlockedPositions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat); 

                return _blockedPositions;
            }
        }

        private StatisticResults<int> _workCount;
        internal StatisticResults<int> WorkCount
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _workCount = new StatisticResults<int>(
                            _simResults.Select(r => r.WorkCount),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);
                
                return _workCount;
            }
        }

        private StatisticResults<int> _addedScopeCount;
        internal StatisticResults<int> AddedScopeCount
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _addedScopeCount = new StatisticResults<int>(
                            _simResults.Select(r => r.AddedScopeCount),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _addedScopeCount;
            }
        }

        private StatisticResults<int> _defectCount;
        internal StatisticResults<int> DefectCount
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _defectCount = new StatisticResults<int>(
                            _simResults.Select(r => r.DefectCount),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _defectCount;
            }
        }

        private StatisticResults<double> _workCycleTime;
        internal StatisticResults<double> WorkCycleTime
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _workCycleTime = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.WorkCycleTime)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _workCycleTime;
            }
        }

        private StatisticResults<double> _addedScopeCycleTime;
        internal StatisticResults<double> AddedScopeCycleTime
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _addedScopeCycleTime = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.AddedScopeCycleTime)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat); 
                
                return _addedScopeCycleTime;
            }
        }

        private StatisticResults<double> _defectCycleTime;
        internal StatisticResults<double> DefectCycleTime
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _defectCycleTime = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.DefectCycleTime)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat); 
                
                return _defectCycleTime;
            }

        }

        private StatisticResults<double> _activePositions;
        internal StatisticResults<double> ActivePositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _activePositions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.ActivePositions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat); 

                return _activePositions;
            }
        }

        private StatisticResults<double> _pullTransactions;
        internal StatisticResults<double> PullTransactions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _pullTransactions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.PullTransactions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _pullTransactions;
            }
        }

        private StatisticResults<double> _inActivePositions;
        internal StatisticResults<double> InActivePositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _inActivePositions = new StatisticResults<double>(
                            _simResults.Select(r => currentIntSelector(r.InActivePositions)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _inActivePositions;
            }
        }


        private Dictionary<FocusedObjective.Contract.SetupClassOfServiceData, StatisticResults<double>> _classOfServiceCycleTimes;
        internal Dictionary<FocusedObjective.Contract.SetupClassOfServiceData, StatisticResults<double>> ClassOfServiceCycleTimes
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                    {
                        _classOfServiceCycleTimes = new Dictionary<SetupClassOfServiceData,StatisticResults<double>>();

                        foreach (var cos in _simResults.First().ClassOfServiceCycleTimes.Keys)
                        {
                            _classOfServiceCycleTimes.Add(cos,

                                new StatisticResults<double>(

                                from r in _simResults
                                where r.ClassOfServiceCycleTimes.ContainsKey(cos)
                                let g = r.ClassOfServiceCycleTimes[cos]
                                select currentDoubleSelector(g),

                                _data.Execute.DecimalRounding,
                                _disconnect,
                                _data.Execute.GoogleHistogramUrlFormat));
                        }
                    }       

                return _classOfServiceCycleTimes;
            }

        }

        private Dictionary<FocusedObjective.Contract.SetupColumnData, StatisticResults<double>> _columnActivePositions;
        internal Dictionary<FocusedObjective.Contract.SetupColumnData, StatisticResults<double>> ColumnActivePositions
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                    {
                        _columnActivePositions = new Dictionary<SetupColumnData, StatisticResults<double>>();

                        foreach (var col in _simResults.First().ColumnActivePositions.Keys)
                        {
                            _columnActivePositions.Add(col,

                                new StatisticResults<double>(

                                from r in _simResults
                                where r.ColumnActivePositions.ContainsKey(col)
                                let g = r.ColumnActivePositions[col]
                                select currentIntSelector(g),

                                _data.Execute.DecimalRounding,
                                _disconnect,
                                _data.Execute.GoogleHistogramUrlFormat));
                        }
                    }

                return _columnActivePositions;
            }

        }

        private XElement _rawXMLResults = null;

        internal XElement RawXMLResults
        {
            get
            {
                if (_rawResults)
                    if (_simResults != null)
                        if (_simResults.Any())
                        {
                            _rawXMLResults = new XElement("rawResults");

                            foreach (var item in _simResults)
                                _rawXMLResults.Add(item.AsXML());
                        }

                return _rawXMLResults;
            }
        }

        internal XElement AsXML()
        {
            if (_rawResults)
            {
                return RawXMLResults;
            }
            else
            {
                XElement result = new XElement("statistics");

                if (this.Intervals != null)
                    result.Add(this.Intervals.AsXML("intervals"));

                XElement cards = new XElement("cards");
                result.Add(cards);

                XElement work = new XElement("work");

                if (WorkCount != null)
                    work.Add(this.WorkCount.AsXML("count"));

                if (WorkCycleTime != null)
                    work.Add(this.WorkCycleTime.AsXML("cycleTime"));

                if (WorkCount != null || WorkCycleTime != null)
                    cards.Add(work);

                XElement addedScope = new XElement("addedScope");

                if (AddedScopeCount != null)
                    addedScope.Add(this.AddedScopeCount.AsXML("count"));

                if (this.AddedScopeCycleTime != null)
                    addedScope.Add(this.AddedScopeCycleTime.AsXML("cycleTime"));

                if (AddedScopeCount != null || AddedScopeCycleTime != null)
                    cards.Add(addedScope);

                XElement defect = new XElement("defect");

                if (DefectCount != null)
                    defect.Add(this.DefectCount.AsXML("count"));

                if (DefectCycleTime != null)
                    defect.Add(this.DefectCycleTime.AsXML("cycleTime"));

                if (DefectCount != null || DefectCycleTime != null)
                    cards.Add(defect);

                if (this.ActivePositions != null)
                    result.Add(this.ActivePositions.AsXML("activePositions"));

                if (this.InActivePositions != null)
                    result.Add(this.InActivePositions.AsXML("inActivePositions"));

                if (this.PullTransactions != null)
                    result.Add(this.PullTransactions.AsXML("pullTransactions"));

                if (this.EmptyPositions != null)
                    result.Add(this.EmptyPositions.AsXML("emptyPositions"));

                if (this.QueuedPositions != null)
                    result.Add(this.QueuedPositions.AsXML("queuedPositions"));

                if (this.BlockedPositions != null)
                    result.Add(this.BlockedPositions.AsXML("blockedPositions"));


                if (this.ClassOfServiceCycleTimes != null && this.ClassOfServiceCycleTimes.Any())
                {
                    XElement cosElement = new XElement("classOfServices");

                    foreach (var cos in _classOfServiceCycleTimes.Keys)
                    {
                        cosElement.Add(new XElement("classOfService",
                            new XAttribute("name", cos.Name),
                            new XAttribute("count", _classOfServiceCycleTimes[cos].Count),
                            _classOfServiceCycleTimes[cos].AsXML("cycleTime")));
                    }

                    result.Add(cosElement);
                }

                if (this._columnActivePositions != null && this._columnActivePositions.Any())
                {

                    XElement colElement = new XElement("columns");

                    foreach (var col in _columnActivePositions.Keys)
                    {
                        colElement.Add(new XElement("column",
                            new XAttribute("name", col.Name),
                            new XAttribute("count", _columnActivePositions[col].Count),
                            _columnActivePositions[col].AsXML("activePositions")));
                    }

                    result.Add(colElement);
                }

                return result;
            }
        }
    }
}
