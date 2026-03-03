using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using System.Reflection;
using FocusedObjective.Contract;

namespace FocusedObjective.Simulation.Scrum
{

    internal class MonteCarloResultSummary
    {
        private SimulationData _data;
        private List<SimulationResultSummary> _simResults;
        private bool _disconnect = true;

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

        internal MonteCarloResultSummary(SimulationData data, List<SimulationResultSummary> simResults, bool disconnect = true)
        {
            _simResults = simResults;
            _data = data;
            _disconnect = disconnect;

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

        private StatisticResults<int> _iterations;
        
        /// <summary>
        /// Gets the number of time-intervals for each the simulation run.
        /// </summary>
        internal StatisticResults<int> Iterations
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _iterations = new StatisticResults<int>(
                            _simResults.Select(r => r.Iterations), 
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _iterations;
            }
        }

        private StatisticResults<double> _pointsAllocatedPeriteration;
        internal StatisticResults<double> PointsAllocatedPerIteration
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _pointsAllocatedPeriteration = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.PointsAllocatedPerIteration)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _pointsAllocatedPeriteration;
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

        private StatisticResults<double> _workPointSize;
        internal StatisticResults<double> WorkPointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _workPointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.WorkPointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _workPointSize;
            }
        }

        private StatisticResults<double> _defectPointSize;
        internal StatisticResults<double> DefectPointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _defectPointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.DefectPointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _defectPointSize;
            }
        }

        private StatisticResults<double> _addedScopePointSize;
        internal StatisticResults<double> AddedScopePointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _addedScopePointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.AddedScopePointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _addedScopePointSize;
            }
        }

        private StatisticResults<double> _workBlockedPointSize;
        internal StatisticResults<double> WorkBlockedPointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _workBlockedPointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.WorkBlockPointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _workBlockedPointSize;
            }
        }

        private StatisticResults<double> _defectBlockPointSize;
        internal StatisticResults<double> DefectBlockPointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _defectBlockPointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.DefectBlockPointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _defectBlockPointSize;
            }
        }

        private StatisticResults<double> _addedScopeBlockPointSize;
        internal StatisticResults<double> AddedScopeBlockPointSize
        {
            get
            {
                if (_simResults != null)
                    if (_simResults.Any())
                        _addedScopeBlockPointSize = new StatisticResults<double>(
                            _simResults.Select(r => currentDoubleSelector(r.AddedScopeBlockPointSize)),
                            _data.Execute.DecimalRounding, _disconnect, _data.Execute.GoogleHistogramUrlFormat);

                return _addedScopeBlockPointSize;
            }
        }

        internal XElement AsXML()
        {
            XElement result = new XElement("statistics");

            if (this.Iterations != null)
                result.Add(this.Iterations.AsXML("iterations"));

            if (this.PointsAllocatedPerIteration != null)
                result.Add(this.PointsAllocatedPerIteration.AsXML("pointsAllocatedPerIteration"));



            XElement cards = new XElement("cards");
            result.Add(cards);

            XElement work = new XElement("work");
            XElement addedScope = new XElement("addedScope");
            XElement defect = new XElement("defect");

            cards.Add(work);
            
            if (this.WorkCount != null)
                work.Add(this.WorkCount.AsXML("count"));

            if ( this.WorkPointSize != null)
                work.Add(this.WorkPointSize.AsXML("pointSize"));

            if (this.WorkBlockedPointSize != null)
                 work.Add(this.WorkBlockedPointSize.AsXML("blockPointSize"));

            cards.Add(addedScope);

            if (this.AddedScopeCount != null)
                addedScope.Add(this.AddedScopeCount.AsXML("count"));

            if (this.AddedScopePointSize != null)
                addedScope.Add(this.AddedScopePointSize.AsXML("pointSize"));

            if (this.AddedScopeBlockPointSize != null)
                addedScope.Add(this.AddedScopeBlockPointSize.AsXML("blockPointSize"));

            cards.Add(defect);
                    
            if (this.DefectCount != null)
                defect.Add(this.DefectCount.AsXML("count"));

            if ( this.DefectPointSize != null)
                defect.Add(this.DefectPointSize.AsXML("pointSize"));

            if ( this.DefectBlockPointSize != null)
                defect.Add(this.DefectBlockPointSize.AsXML("blockPointSize"));

            return result;
        }
    }
}
