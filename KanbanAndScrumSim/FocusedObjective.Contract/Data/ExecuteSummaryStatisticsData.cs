using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class ExecuteSummaryStatisticsData : ContractDataBase, IValidate
    {
        public ExecuteSummaryStatisticsData()
        {
        }

        public ExecuteSummaryStatisticsData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _data = "";
        private string _separatorCharacter = ",";
        SetupDistributionData _distributionData = null;
        private bool _returnData = false;

        // public properties
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string SeparatorCharacter
        {
            get { return _separatorCharacter; }
            set { _separatorCharacter = value; }
        }

        public SetupDistributionData Distribution
        {
            get { return _distributionData; }
            set { _distributionData = value; }
        }

        public bool ReturnData
        {
            get { return _returnData; }
            set { _returnData = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _separatorCharacter = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "separatorCharacter",
                _separatorCharacter,
                false
                );

            _distributionData = ContractCommon.ReadElement(
                source,
                typeof(SetupDistributionData),
                "distribution",
                errors,
                false);

            _returnData = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "returnData",
                "false", false,
                "no", false,
                "true", true,
                "yes", true
                );

            _data = source.Value;

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("summaryStatistics");

            if (_distributionData != null)
                result.Add(_distributionData.AsXML(simType));

            if (!string.IsNullOrEmpty(_data))
                result.Add(_data);
            
            if (_returnData)
                result.Add(new XAttribute("returnData", _returnData.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            return success;
        }
    }
    

}
