using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public class ExecuteSensitivityData : ContractDataBase, IValidate
    {
        public ExecuteSensitivityData()
        {
        }
        
        public ExecuteSensitivityData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _cycles = 1000;
        private double _occurrenceMultiplier = 1.0;
        private double _estimateMultiplier  = 1.0;
        private double _iterationMultiplier = 1.0;

        private SensitivityTypeEnum _sensitivityType = SensitivityTypeEnum.Intervals;
        private SortOrderEnum _sortOrder = SortOrderEnum.Ascending;
        private string _googleImprovementUrlFormat = @"http://chart.apis.google.com/chart?chxt=y&chbh=a&chs=600x400&cht=bvg&chco={1}&chd=t:{2}&chdl={3}&chds={4},{5}&chxr=0,{4},{5}&chg=0,10&chtt={0}+Improvement";

        // public properties
        
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        public double EstimateMultiplier
        {
            get { return _estimateMultiplier; }
            set { _estimateMultiplier = value; }
        }

        public double OccurrenceMultiplier
        {
            get { return _occurrenceMultiplier; }
            set { _occurrenceMultiplier = value; }
        }

        public double IterationMultiplier
        {
            get { return _iterationMultiplier; }
            set { _iterationMultiplier = value; }
        }

        public SensitivityTypeEnum SensitivityType
        {
            get { return _sensitivityType; }
            set { _sensitivityType = value; }
        }

        public SortOrderEnum SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        public string GoogleImprovementUrlFormat
        {
            get { return _googleImprovementUrlFormat; }
            set { _googleImprovementUrlFormat = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            result = result && ContractCommon.ReadAttributeIntValue(
                out _cycles,
                source,
                errors,
                "cycles",
                _cycles
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _estimateMultiplier,
                source,
                errors,
                "estimateMultiplier",
                _estimateMultiplier
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _iterationMultiplier,
                source,
                errors,
                "iterationMultiplier",
                _iterationMultiplier,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _occurrenceMultiplier,
                source,
                errors,
                "occurrenceMultiplier",
                _occurrenceMultiplier
                );

            _sensitivityType = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "sensitivityType",
                "intervals", SensitivityTypeEnum.Intervals,
                "iterations", SensitivityTypeEnum.Iterations,
                "cycleTime", SensitivityTypeEnum.CycleTime,
                "empty", SensitivityTypeEnum.Empty,
                "queued", SensitivityTypeEnum.Queued,
                "pullTransactions", SensitivityTypeEnum.PullTransactions);


            _sortOrder = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "sortOrder",
                "ascending", SortOrderEnum.Ascending,
                "descending", SortOrderEnum.Descending);

            _googleImprovementUrlFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "googleImprovementUrlFormat",
                _googleImprovementUrlFormat,
                false);

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("sensitivity");

            result.Add(new XAttribute("cycles", _cycles.ToString()));
            result.Add(new XAttribute("estimateMultiplier", _estimateMultiplier.ToString()));
            result.Add(new XAttribute("occurrenceMultiplier", _occurrenceMultiplier.ToString()));
            result.Add(new XAttribute("iterationMultiplier", _iterationMultiplier.ToString()));
            result.Add(new XAttribute("sensitivityType", _sensitivityType.ToString()));
            result.Add(new XAttribute("sortOrder", _sortOrder.ToString()));
            result.Add(new XAttribute("googleImprovementUrlFormat", _googleImprovementUrlFormat.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            success &= ContractCommon.CheckValueGreaterThan(errors, Cycles, 0, "cycles", "execute/sensitivity", Source);

            if (data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (this.SensitivityType != SensitivityTypeEnum.Iterations)
                {
                    Helper.AddError(errors, ErrorSeverityEnum.Information, 36, Strings.Error36, Source);
                    this.SensitivityType = SensitivityTypeEnum.Iterations;
                }
            }

            return success;
        }
    }
    

}
