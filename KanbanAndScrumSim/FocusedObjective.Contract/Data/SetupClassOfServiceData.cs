using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using System.Globalization;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("classOfService", ".", false, ParentElement = "classOfServices")]
    public class SetupClassOfServiceData : SensitivityBase
    {
        public SetupClassOfServiceData()
        {
        }

        public SetupClassOfServiceData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _name = "";
        private bool _default = false;
        private int _order = 1;
        private bool _violateWip = false;
        private double _skipPercentage = 0.0;

        private int _maximumAllowedOnBoard = int.MaxValue;
        private List<SetupBacklogCustomColumnData> _columns = new List<SetupBacklogCustomColumnData>();


        // public properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("order", "Work with the lowest order value are started first. This value will assign this order to work of this class of service, unless its overridden at the custom backlog entry level.", false)]
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        [SimMLAttribute("default", "Indicates this class of service is the default for work items if they don't specify one.", false, ValidValues = "false|true")]
        public bool Default
        {
            get { return _default; }
            set { _default = value; }
        }

        [SimMLAttribute("violateWip", "Indicated work items of this type are allowed to violate column WIP limit values. When \"true\" work items of this class of service will be started in a column even if it is full. Other work in that column is automatically blocked to compensate.", false, ValidValues="false|true")]
        public bool ViolateWIP
        {
            get { return _violateWip; }
            set { _violateWip = value; }
        }

        [SimMLAttribute("skipPercentage", "How often items of this class of service are skipped in percentage value (0 to 100). Used to model some work types of not blocking completion progress.", false)]
        public double SkipPercentage
        {
            get { return _skipPercentage; }
            set { _skipPercentage = value; }
        }

        [SimMLAttribute("maximumAllowedOnBoard", "Highest number of items assigned this class of service can be active at one time. Used to restrict high priority work from saturating availability of workers.", false)]
        public int MaximumAllowedOnBoard
        {
            get { return _maximumAllowedOnBoard; }
            set { _maximumAllowedOnBoard = value; }
        }

        public List<SetupBacklogCustomColumnData> Columns
        {
            get { return _columns; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            _name = source.Value.Trim();

            success = success && ContractCommon.ReadAttributeIntValue(
                out _order,
                source,
                errors,
                "order",
                _order,
                false);

            _default = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "default",
                "false", false,
                "no", false,
                "true", true,
                "yes", true);

            _violateWip = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "violateWIP",
                "false", false,
                "no", false,
                "true", true,
                "yes", true);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _skipPercentage,
                source,
                errors,
                "skipPercentage",
                _skipPercentage,
                false);

            success = success && ContractCommon.ReadAttributeIntValue(
                out _maximumAllowedOnBoard,
                source,
                errors,
                "maximumAllowedOnBoard",
                _maximumAllowedOnBoard,
                false);

            // add the column data
            foreach (XElement col in source.Elements("column"))
            {
                _columns.Add(
                    new SetupBacklogCustomColumnData(col, errors));
            }

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("classOfService");
            
            result.Add(_name);
            result.Add(new XAttribute("violateWIP", _violateWip.ToString()));
            result.Add(new XAttribute("order", _order.ToString()));
            result.Add(new XAttribute("skipPercentage", _skipPercentage.ToString()));

            if (_default)
                result.Add(new XAttribute("default", _default.ToString()));

            if (_maximumAllowedOnBoard != int.MaxValue)
                result.Add(new XAttribute("maximumAllowedOnBoard", _maximumAllowedOnBoard.ToString()));

            if (simType == SimulationTypeEnum.Kanban)
            {
                foreach (var column in _columns)
                    result.Add(column.AsXML(simType));
            }

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (SkipPercentage != 0.0 && SkipPercentage != 100.0)
            {
                success &= ContractCommon.CheckValueGreaterThan(errors, SkipPercentage, 0, "skipPercentage", "setup/classOfServices/classOfService: " + Name, Source);
                success &= ContractCommon.CheckValueLessThan(errors, SkipPercentage, 100, "skipPercentage", "setup/classOfServices/classOfService: " + Name, Source);
            }

            foreach (var col in Columns)
                success &= col.Validate(data, errors);

            return success;
        }
    }
}
