using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using System.Reflection;

namespace FocusedObjective.Simulation.Scrum
{

    internal class SimulationResultSummary
    {
        private ScrumSimulation _simulator;
        private bool _disconnect = true;

        internal SimulationResultSummary(ScrumSimulation simulator, bool disconnect = true)
        {
            _simulator = simulator;
            _disconnect = disconnect;

            // to save ram, disconnect caches values....
            if (disconnect)
                Disconnect();
        }

        internal void Disconnect()
        {
            // read all data once so it is cached
            object o;
            PropertyInfo[] propertyInfos = typeof(SimulationResultSummary).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var p in propertyInfos)
                o = p.GetValue(this, null);

            //  remove the reference to the simulator
            _simulator = null;
        }

        private int _iterations;
        
        /// <summary>
        /// Gets the number of iterations for the simulation run.
        /// </summary>
        internal int Iterations
        {
            get
            {   
                // -2 because: 0 is start and the last one is the end....
                if (_simulator != null)
                    if (_simulator.Iterations.Any())
                        _iterations = _simulator.Iterations.Count() - 2;

                return _iterations;
            }
        }

        private StatisticResults<double> _pointsAllocatedPerIteration;
        internal StatisticResults<double> PointsAllocatedPerIteration
        {
            get
            {
                if (_simulator != null)
                    _pointsAllocatedPerIteration = new StatisticResults<double>(
                        _simulator
                        .Iterations
                        .Where(i => i.Stories.Count > 0)
                        .Select(j => j.Stories.Sum(s => s.GetRemainingPoints(j.Sequence)))
                         , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _pointsAllocatedPerIteration;
            }
        }

        private int _workCount;
        internal int WorkCount
        {
            get
            {
                if (_simulator != null)
                    _workCount = _simulator.AllStories.Count(s => s.StoryType == Enums.StoryTypeEnum.Work);

                return _workCount;
            }
        }

        private int _addedScopeCount;
        internal int AddedScopeCount
        {
            get
            {
                if (_simulator != null)
                    _addedScopeCount = _simulator.AllStories.Count(s => s.StoryType == Enums.StoryTypeEnum.AddedScope);

                return _addedScopeCount;
            }
        }

        private int _defectCount;
        internal int DefectCount
        {
            get
            {
                if (_simulator != null)
                    _defectCount = _simulator.AllStories.Count(s => s.StoryType == Enums.StoryTypeEnum.Defect);

                return _defectCount;
            }
        }


        private StatisticResults<double> _workPointSize;
        internal StatisticResults<double> WorkPointSize
        {
            get
            {
                if (_simulator != null)
                    _workPointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.Work)
                        .Select(s => s.CalculatedStorySize)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _workPointSize;
            }
        }

        private StatisticResults<double> _defectPointSize;
        internal StatisticResults<double> DefectPointSize
        {
            get
            {
                if (_simulator != null)
                    _defectPointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.Defect)
                        .Select(s => s.CalculatedStorySize)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _defectPointSize;
            }
        }

        private StatisticResults<double> _addedScopePointSize;
        internal StatisticResults<double> AddedScopePointSize
        {
            get
            {
                if (_simulator != null)
                    _addedScopePointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.AddedScope)
                        .Select(s => s.CalculatedStorySize)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _addedScopePointSize;
            }
        }

        private StatisticResults<double> _workBlockPointSize;
        internal StatisticResults<double> WorkBlockPointSize
        {
            get
            {
                if (_simulator != null)
                    _workBlockPointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.Work)
                        .Select(s => s.CalculatedBlockedPoints)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _workBlockPointSize;
            }
        }

        private StatisticResults<double> _defectBlockPointSize;
        internal StatisticResults<double> DefectBlockPointSize
        {
            get
            {
                if (_simulator != null)
                    _defectBlockPointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.Defect)
                        .Select(s => s.CalculatedBlockedPoints)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _defectBlockPointSize;
            }
        }

        private StatisticResults<double> _addedScopeBlockPointSize;
        internal StatisticResults<double> AddedScopeBlockPointSize
        {
            get
            {
                if (_simulator != null)
                    _addedScopeBlockPointSize = new StatisticResults<double>(
                        _simulator
                        .AllStories
                        .Where(s => s.StoryType == Enums.StoryTypeEnum.AddedScope)
                        .Select(s => s.CalculatedBlockedPoints)
                        , _simulator.SimulationData.Execute.DecimalRounding, _disconnect, _simulator.SimulationData.Execute.GoogleHistogramUrlFormat);
                return _addedScopeBlockPointSize;
            }
        }

        internal XElement AsXML()
        {
            XElement result = new XElement("statistics");

            result.Add(new XElement("iterations",
                new XAttribute("value", this.Iterations)));

            result.Add(this.PointsAllocatedPerIteration.AsXML("pointsAllocatedPerIteration"));
            
            result.Add(
                new XElement("cards",
                    new XElement("work",
                        new XAttribute("value", this.WorkCount),
                        this.WorkPointSize.AsXML("pointSize"),
                        this.WorkBlockPointSize.AsXML("blockPointSize")),
            
                    new XElement("addedScope",
                        new XAttribute("value", this.AddedScopeCount),
                        this.AddedScopePointSize.AsXML("pointSize"),
                        this.AddedScopeBlockPointSize.AsXML("blockPointSize")),


                    new XElement("defect",
                        new XAttribute("value", this.DefectCount),
                        this.DefectPointSize.AsXML("pointSize"),
                        this.DefectBlockPointSize.AsXML("blockPointSize"))
                        )
                    );
            
            return result;
        }
    }
}
