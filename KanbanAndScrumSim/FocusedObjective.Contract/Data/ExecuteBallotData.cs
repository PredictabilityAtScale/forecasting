using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class ExecuteBallotData : ContractDataBase, IValidate
    {
        public ExecuteBallotData()
        {
        }

        public ExecuteBallotData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _data;
        private BallotTypeEnum _ballotType = BallotTypeEnum.Schulze;

        // public properties
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public BallotTypeEnum BallotType
        {
            get { return _ballotType; }
            set { _ballotType = value; }
        }
        
        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            _data = source.Value;

            _ballotType = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "type",
                "borda", BallotTypeEnum.Borda,
                "schulze", BallotTypeEnum.Schulze);
            
            return true;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("ballot");

            result.Value = _data;
            result.Add(new XAttribute("type", _ballotType.ToString()));
                            
            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            return success;
        }
    }


}
