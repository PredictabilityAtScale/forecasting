using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    /// <summary>
    /// Class to split SimML models into partitions for cloud computing. 
    /// Splits on commands. And optionally cycles, but i have no idea how we would ever join them!!!
    /// </summary>
    public class PartitionModel
    {
        SimulationData _originalModel;
        int _maxCycles = 100;
        bool _limitToMax = true;

        List<SimulationData> _partitions = new List<SimulationData>();

        public PartitionModel(SimulationData originalModel, int maxCycles = 100, bool limitToMax = true)
        {
            _originalModel = originalModel;
            _maxCycles = maxCycles;
            _limitToMax = limitToMax;

            doPartition();
        }

        public List<SimulationData> Partitions
        {
            get { return _partitions;  }
        }

        internal void doPartition()
        {
            // split models into different commands, and then groups no bigger than maxCycles

            // forecastDate
            if (_originalModel.Execute.ForecastDate != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.Visual = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.SummaryStatistics = null;

                int originalCycles = _originalModel.Execute.ForecastDate.Cycles;

                if (originalCycles > _maxCycles)
                    newModel.Execute.ForecastDate.Cycles = _maxCycles;

                createAndAddPartitionsToListForCommand("forecastDate", newModel, originalCycles);
            }

            // monteCarlo
            if (_originalModel.Execute.MonteCarlo != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Visual = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.SummaryStatistics = null;

                int originalCycles = _originalModel.Execute.MonteCarlo.Cycles;

                if (originalCycles > _maxCycles)
                    newModel.Execute.MonteCarlo.Cycles = _maxCycles;

                createAndAddPartitionsToListForCommand("monteCarlo", newModel, originalCycles);
            }

            // addStaff
            if (_originalModel.Execute.AddStaff != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Visual = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.SummaryStatistics = null;

                int originalCycles = _originalModel.Execute.AddStaff.Cycles;

                if (originalCycles > _maxCycles)
                    newModel.Execute.AddStaff.Cycles = _maxCycles;

                createAndAddPartitionsToListForCommand("addStaff", newModel, originalCycles);
            }

            // sensitivity
            if (_originalModel.Execute.Sensitivity != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Visual = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.SummaryStatistics = null;

                int originalCycles = _originalModel.Execute.Sensitivity.Cycles;

                if (originalCycles > _maxCycles)
                    newModel.Execute.Sensitivity.Cycles = _maxCycles;

                createAndAddPartitionsToListForCommand("sensitivity", newModel, originalCycles);
            }

            // visual - no paritioning
            if (_originalModel.Execute.Visual != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.SummaryStatistics = null;

                createAndAddPartitionsToListForCommand("visual", newModel, _maxCycles);
            }

            // summaryStatistics - no paritioning
            if (_originalModel.Execute.SummaryStatistics != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.Ballot = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.Visual = null;

                createAndAddPartitionsToListForCommand("summaryStatistics", newModel, _maxCycles);
            }

            // ballot - no paritioning
            if (_originalModel.Execute.Ballot != null)
            {
                SimulationData newModel = new SimulationData(
                    new XDocument(_originalModel.AsXML(_originalModel.Execute.SimulationType)), _originalModel.CalculationEngine);

                // null out all other commands
                newModel.Execute.ForecastDate = null;
                newModel.Execute.Sensitivity = null;
                newModel.Execute.MonteCarlo = null;
                newModel.Execute.SummaryStatistics = null;
                newModel.Execute.AddStaff = null;
                newModel.Execute.Visual = null;

                createAndAddPartitionsToListForCommand("ballot", newModel, _maxCycles);
            }

        }

        private int createAndAddPartitionsToListForCommand(string command, SimulationData newModel, int originalCycles)
        {
            int numPartitions = (int)Math.Ceiling((originalCycles * 1.0) / (_maxCycles * 1.0));

            if (_limitToMax == false && numPartitions > 1)
            {
                for (int i = 1; i < numPartitions + 1; i++)
                {
                    SimulationData p = new SimulationData(
                        new XDocument(newModel.AsXML(_originalModel.Execute.SimulationType)), newModel.CalculationEngine);

                    p.PartitionCommand = command;
                    p.PartitionNumber = i;
                    p.NumberOfPartitions = numPartitions;

                    _partitions.Add(p);
                }
            }
            else
            {
                _partitions.Add(newModel);
            }

            return numPartitions;
        }




    }
}
