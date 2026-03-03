using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using System.Text.RegularExpressions;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("?parameter", "Parameter definition. Specify parameters to use throughout the model by name @[name] and to expose for interactive experimentation.", false, HasMandatoryAttributes = true, ParentElement = "simulation")]
    public class VariableData : ContractDataBase, IValidate
    {

        //  <?parameter name="Designers"  value="1" type="number" lowest="1" highest="10" step="1" ?>

        public VariableData()
        {
        }

        public VariableData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _value = "";
        private string _name = "";

        // public properties
       
        [SimMLAttribute("name", "Unique name to identify this parameter. This parameter can be referenced by prefixing its name with a '@' character in any place within this model. Must not contain characters @, <, >, \\, or /.", true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("value", "The value assigned when the parameters is specified by @[name] elsewhere in the model. Must follow the same rules as any value entered in those fields where it is used.", true)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        // fake parameter for code completion attributes.
        [SimMLAttribute("type", "Includes this parameter for interactive experiments. Can be 'number' or 'date'", false, ValidValues="number|date")]
        [SimMLAttribute("lowest", "Lowest allowed numeric value when exposed in the interactive experiments window. Only used for number types.", false)]
        [SimMLAttribute("highest", "Highest allowed numeric value when exposed in the interactive experiments window. Only used for number types.", false)]
        [SimMLAttribute("step", "Increment step size for numeric value when exposed in the interactive experiments window. Only used for number types.", false)]
        public string CodeCompletion { get; set; }
       
        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _value = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "value",
                _value
                );

            _name = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "name");

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("phase");

            result.Add(new XAttribute("value", _value.ToString()));
            result.Add(new XAttribute("name", _name));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            string validVariableNameRegex = @"[a-z,A-Z,_]+";
            Regex myRegex = new Regex(validVariableNameRegex, RegexOptions.None);
            if (myRegex.Matches(_name).Count == 0)
            {
                // name was invalid
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 57, string.Format(Strings.Error57, Name), Source);   
            }

            return success;
        }
    }
    

}
