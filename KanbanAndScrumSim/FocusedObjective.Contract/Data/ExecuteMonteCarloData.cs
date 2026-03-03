using FocusedObjective.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    [SimMLElement("monteCarlo", "Perform monte-carlo simulation.", false, HasMandatoryAttributes = true)]
    public class ExecuteMonteCarloData : ContractDataBase, IValidate
    {
        public ExecuteMonteCarloData()
        {
        }
        
        public ExecuteMonteCarloData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _cycles = 1000;
        private bool _rawResults = false;


        // public properties

        [SimMLAttribute("cycles", "Number of monte carlo simulation cycles to execute. Smaller is faster; higher is more detailed.", true)]
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        [SimMLAttribute("rawResults", "'true' returns the data for each individual result, 'false' returns aggregate information (default).", false, ValidValues="false|true")]
        public bool RawResults
        {
            get { return _rawResults; }
            set { _rawResults = value; }
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

            _rawResults = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "rawResults",
                "false", false,
                "no", false,
                "true", true,
                "yes", true);

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("monteCarlo");

            result.Add(new XAttribute("cycles", _cycles.ToString()));
            result.Add(new XAttribute("rawResults", _rawResults.ToString()));
         
            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            success &= ContractCommon.CheckValueGreaterThan(errors, Cycles, 0, "cycles", "execute/monteCarlo", Source);
            return success;
        }

    }


}
