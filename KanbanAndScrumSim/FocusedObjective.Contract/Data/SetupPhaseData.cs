using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("phase", "Phases allow projects to be segments into multiple time periods that can override default model behavior.", false, HasMandatoryAttributes = true, ParentElement="phases")]
    public class SetupPhaseData : ContractDataBase, IValidate
    {
        public SetupPhaseData()
        {
        }
        
        public SetupPhaseData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private double _startPercentage = 0.0;
        private double _endPercentage = 100.0;
        private double _occurrenceMultiplier = 1.0;
        private double _estimateMultiplier  = 1.0;
        private double _iterationMultiplier = 1.0;
        private double _costPerDay = 0.0;
        private string _name = "";
        private List<SetupPhaseColumnData> _columns = new List<SetupPhaseColumnData>();
        
        private PhaseUnitEnum _phaseUnit = PhaseUnitEnum.Percentage;
        private double _start = 0.0;
        private double _end = 0.0;

        // public properties
        public double StartPercentage
        {
            get { return _startPercentage; }
            set { _startPercentage = value; }
        }

        public double EndPercentage
        {
            get { return _endPercentage; }
            set { _endPercentage = value; }
        }

        public PhaseUnitEnum PhaseUnit
        {
            get { return _phaseUnit; }
            set { _phaseUnit = value; }
        }

        [SimMLAttribute("start", "Lowest trigger value to activate this phase in the units specified in the <phases unit=\"..\" /> value. ", true)]
        public double Start
        {
            get { return _start; }
            set { _start = value; }
        }

        [SimMLAttribute("end", "Highest trigger value to de-activate this phase in the units specified in the <phases unit=\"..\" /> value. ", true)]
        public double End
        {
            get { return _end; }
            set { _end = value; }
        }

        [SimMLAttribute("estimateMultiplier", "When this phase is active, all cycle time estimates will be multiplied by this amount. 1.0 is the default.", false)]
        public double EstimateMultiplier
        {
            get { return _estimateMultiplier; }
            set { _estimateMultiplier = value; }
        }

        [SimMLAttribute("occurrenceMultiplier", "When this phase is active, all occurrence rates will be increased by this amount (>1 more often, <1 less often). 1.0 is the default.", false)]
        public double OccurrenceMultiplier
        {
            get { return _occurrenceMultiplier; }
            set { _occurrenceMultiplier = value; }
        }

        [SimMLAttribute("iterationMultiplier", "When this phase is active, iteration estimate targets will be multiplied by this amount (scrum only). 1.0 is the default.", false)]
        public double IterationMultiplier
        {
            get { return _iterationMultiplier; }
            set { _iterationMultiplier = value; }
        }

        [SimMLAttribute("costPerDay", "When this phase is active, The cost per day specified is used instead of the globl cost per day in the ForecastDate data.", false)]
        public double CostPerDay
        {
            get { return _costPerDay; }
            set { _costPerDay = value; }
        }
            
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<SetupPhaseColumnData> Columns
        {
            get { return _columns; }
        }
        
        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _startPercentage,
                source,
                errors,
                "startPercentage",
                _startPercentage,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _endPercentage,
                source,
                errors,
                "endPercentage",
                _endPercentage,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _start,
                source,
                errors,
                "start",
                _start,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _end,
                source,
                errors,
                "end",
                _end,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _estimateMultiplier,
                source,
                errors,
                "estimateMultiplier",
                _estimateMultiplier,
                false
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
                _occurrenceMultiplier,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _costPerDay,
                source,
                errors,
                "costPerDay",
                _costPerDay,
                false
                );

            _name = source.Value.Trim();


            foreach (XElement col in source.Elements("column"))
            {
                _columns.Add(
                    new SetupPhaseColumnData(col, errors));
            }

            // StartPercentage and EndPercentage are obsolete, this copies the values across.
            if (_start == 0.0 && _end == 0.0)
            {
                _start = _startPercentage;
                _end = _endPercentage;
            }

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("phase");

            // obsolete: result.Add(new XAttribute("startPercentage", _startPercentage.ToString()));
            // obsolete: result.Add(new XAttribute("endPercentage", _endPercentage.ToString()));
            result.Add(new XAttribute("estimateMultiplier", _estimateMultiplier.ToString()));
            result.Add(new XAttribute("start", _start.ToString()));
            result.Add(new XAttribute("end", _end.ToString()));
            result.Add(new XAttribute("occurrenceMultiplier", _occurrenceMultiplier.ToString()));
            result.Add(new XAttribute("iterationMultiplier", _iterationMultiplier.ToString()));
            result.Add(new XAttribute("costPerDay", _costPerDay.ToString()));

            result.Add(_name);

            if (simType == SimulationTypeEnum.Kanban)
            {
                //XElement columns = new XElement("columns");

                // add the column data
                foreach (var col in _columns)
                    result.Add(col.AsXML(simType));

                //result.Add(columns);
            }


            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            

            // percentage ranges
            success &= ContractCommon.CheckValueGreaterThan(errors, StartPercentage, -1, "startPercentage", "setup/phases/phase: " + Name, Source);
            success &= ContractCommon.CheckValueGreaterThan(errors, EndPercentage, -1, "endPercentage", "setup/phases/phase: " + Name, Source);

            success &= ContractCommon.CheckValueLessThan(errors, StartPercentage, 101, "startPercentage", "setup/phases/phase: " + Name, Source);
            success &= ContractCommon.CheckValueLessThan(errors, EndPercentage, 101, "endPercentage", "setup/phases/phase: " + Name, Source);

            if (EndPercentage < StartPercentage)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 48, string.Format(Strings.Error48, Name), Source);
            }

            if (End < Start)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 58, string.Format(Strings.Error58, Name), Source);
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                Helper.AddError(errors, ErrorSeverityEnum.Warning, 50, string.Format(Strings.Error50, "setup/phases/phase"), Source);
            }

            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                // do the columns
                foreach (var col in Columns)
                    success &= col.Validate(data, errors);
            }

            return success;
        }
    }
    

}
