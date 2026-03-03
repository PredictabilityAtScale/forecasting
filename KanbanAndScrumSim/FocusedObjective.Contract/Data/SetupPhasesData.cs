using FocusedObjective.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    [SimMLElement("phases", "Contains the phase definitions. Phases allow projects to be segments into multiple time periods that can override default model behavior.", false, HasMandatoryAttributes = true)]
    public class SetupPhasesData : List<SetupPhaseData>, /*ContractDataBase,*/ IValidate
    {
        public SetupPhasesData()
        {
        }

        public SetupPhasesData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private PhaseUnitEnum _unit = PhaseUnitEnum.Percentage;
        
        // public properties
        public XElement Source
        {
            get;
            set;
        }

        [SimMLAttribute("unit", "The units the phase start and end values are specified. Percentage (the default), interval or iteration (scrum only) are valid values.", false, ValidValues = "percentage|interval|iteration")]
        public PhaseUnitEnum Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        public List<SetupPhaseData> Phases
        {
            get { return this; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            _unit = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "unit",
                "percentage", PhaseUnitEnum.Percentage,
                "percentages",PhaseUnitEnum.Percentage,
                "interval",   PhaseUnitEnum.Interval,
                "intervals",  PhaseUnitEnum.Interval,
                "iteration",  PhaseUnitEnum.Iteration,
                "iterations", PhaseUnitEnum.Iteration);

            // add the phases data
            foreach (XElement phase in source.Elements("phase"))
            {
                this.Add(
                    new SetupPhaseData(phase, errors));
            }

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("phases");

            result.Add(new XAttribute("unit", _unit.ToString().ToLower()));
    
            foreach (var phase in this)
                result.Add(phase.AsXML(simType));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            return success;
        }
    }
}
