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
    [SimMLElement("custom", "An individual custom backlog entry.", false, HasMandatoryAttributes = true)]
    public class SetupBacklogCustomData : ContractDataBase, IValidate
    {
        public SetupBacklogCustomData()
        {
        }

        public SetupBacklogCustomData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _name = "";
        private int _count = 1;
        private int _order = int.MaxValue;
        private string _classOfService = string.Empty;
        
        // for Kanban
        private double _percentageLowBound = 0;
        private double _percentageHighBound = 100;
        
        public string _dueDate = "";
        private DateTime _safeDueDate = DateTime.MaxValue;

        // for scrum
        private double _estimateLowBound = 0;
        private double _estimateHighBound = 0;
        private string _estimateDistribution = "";

        private List<SetupBacklogCustomColumnData> _columns = new List<SetupBacklogCustomColumnData>();

        private double _valueLowBound = 0;
        private double _valueHighBound = 0;

        private bool _completed = false;
        private int _initialColumn = -1;
        
        // public properties
        [SimMLAttribute("count", "The number of items to be added to the backlog with this custom entry.", false)]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        [SimMLAttribute("completed", "If set to true, this custom entry is considered completed at the start of simulation. Used to pre-populate progress when modelling partially completed projects. Default is false.", false, ValidValues="false|true")]
        public bool Completed
        {
            get { return _completed; }
            set { _completed = value; }
        }

        [SimMLAttribute("name", "A name for this custom backlog entry. This name is used to identify this custom entry in other attributes.", false)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("estimateLowBound", "The lowest story point estimate for backlog items of this type for Scrum models only (Kanban models use custom column estimates.)", false)]
        public double EstimateLowBound
        {
            get { return _estimateLowBound; }
            set { _estimateLowBound = value; }
        }

        [SimMLAttribute("estimateHighBound", "The highest story point estimate for backlog items of this type for Scrum models only (Kanban models use custom column estimates.)", false)]
        public double EstimateHighBound
        {
            get { return _estimateHighBound; }
            set { _estimateHighBound = value; }
        }

        [SimMLAttribute("estimateDistribtion", "The distribution used to generate story point estimates for backlog items of this type for Scrum models only (Kanban models use custom column estimates.)", false)]
        public string EstimateDistribution
        {
            get { return _estimateDistribution; }
            set { _estimateDistribution = value; }
        }

        [SimMLAttribute("classOrfService", "The class of service of this backlog item (Kanban only). Enter the name of the class of service defined for any defined <classOfService>[name]</classOfservice> element.)", false)]
        public string ClassOfService
        {
            get { return _classOfService; }
            set { _classOfService = value; }
        }

        [SimMLAttribute("dueDate", "Due date used for ordering start of work (earliest due date to latest) with the SAME priority order value during simulation. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD.", false)]
        public string DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        public DateTime SafeDueDate
        {
            get { return _safeDueDate; }
        }

        [SimMLAttribute("percentageLowBound", "Lowest percentage of a columns default cycle time range for these backlog items (Kanban only). Used as an easy way to weight work from low effort to high effort by biasing the total column cycle time range to an area based on percentages. 0 = the lowest column cycle time value to 100 = the highest cycle time value.", false)]
        public double PercentageLowBound
        {
            get { return _percentageLowBound; }
            set { _percentageLowBound = value; }
        }

        [SimMLAttribute("percentageHighBound", "Highest percentage of a columns default cycle time range for these backlog items (Kanban only). Used as an easy way to weight work from low effort to high effort by biasing the total column cycle time range to an area based on percentages. 0 = the lowest column cycle time value to 100 = the highest cycle time value.", false)]
        public double PercentageHighBound
        {
            get { return _percentageHighBound; }
            set { _percentageHighBound = value; }
        }

        [SimMLAttribute("order", "Sort order custom backlog entries will be prioritized from lowest to highest. The default value of items that omit this attribute will be the highest allowed (and started last).", false)]
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        [SimMLAttribute("valueLowBound", "Lowest value allowed when a random value amount is generated during simulation for these backlog items. Used to simulate how much value being delivered by completed work.", false)]
        public double ValueLowBound
        {
            get { return _valueLowBound; }
            set { _valueLowBound = value; }
        }

        [SimMLAttribute("valueHighBound", "Highest value allowed when a random value amount is generated during simulation for these backlog items. Used to simulate how much value being delivered by completed work over time.", false)]
        public double ValueHighBound
        {
            get { return _valueHighBound; }
            set { _valueHighBound = value; }
        }

       public List<SetupBacklogCustomColumnData> Columns
        {
            get { return _columns; }
        }

        [SimMLAttribute("initialColumn", "Initial work item start column for simulation (Kanban only). Specify the column by its id value from the <column id=\"..\" attribute.", false)]
        public int InitialColumn
        {
            get { return _initialColumn; }
            set { _initialColumn = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeIntValue(
                out _count,
                source,
                errors,
                "count",
                _count,
                false);

            _name = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "name",
                _name,
                false);

            _estimateDistribution = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "estimateDistribution",
                _estimateDistribution,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _estimateLowBound,
                source,
                errors,
                "estimateLowBound",
                _estimateLowBound,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _estimateHighBound,
                source,
                errors,
                "estimateHighBound",
                _estimateHighBound,
                false);

            
            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _percentageLowBound,
                source,
                errors,
                "percentageLowBound",
                _percentageLowBound,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _percentageHighBound,
                source,
                errors,
                "percentageHighBound",
                _percentageHighBound,
                false);

            success = success && ContractCommon.ReadAttributeIntValue(
                out _order,
                source,
                errors,
                "order",
                _order,
                false);

            _classOfService = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "classOfService",
                _classOfService,
                false);

            _dueDate = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "dueDate",
                string.Empty,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _valueLowBound,
                source,
                errors,
                "valueLowBound",
                _valueLowBound,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _valueHighBound,
                source,
                errors,
                "valueHighBound",
                _valueHighBound,
                false);

            _completed = ContractCommon.ReadMandatoryAttributeListValue(
                 source,
                 errors,
                 "completed",
                 "false", false,
                 "no", false,
                 "true", true,
                 "yes", true);

            success = success && ContractCommon.ReadAttributeIntValue(
                out _initialColumn,
                source,
                errors,
                "initialColumn",
                _initialColumn,
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
            XElement result = new XElement("custom");

            result.Add(new XAttribute("count", _count.ToString()));
            result.Add(new XAttribute("name",  _name.ToString()));
            result.Add(new XAttribute("order", _order.ToString()));

            if (simType == SimulationTypeEnum.Kanban)
            {
                result.Add(new XAttribute("initialColumn", _initialColumn.ToString()));

                if (_percentageLowBound != 0.0 || _percentageHighBound != 100.0)
                {
                    result.Add(new XAttribute("percentageLowBound", _percentageLowBound.ToString())); ;
                    result.Add(new XAttribute("percentageHighBound", _percentageHighBound.ToString())); ;
                }


                foreach (var column in _columns)
                    result.Add(column.AsXML(simType));

            }
            else
            {
                result.Add(new XAttribute("estimateLowBound", _estimateLowBound.ToString())); ;
                result.Add(new XAttribute("estimateHighBound", _estimateHighBound.ToString())); ;
                result.Add(new XAttribute("estimateDistribution", _estimateDistribution.ToString()));
            }


            if (!string.IsNullOrEmpty(_dueDate))
                result.Add(new XAttribute("dueDate", _dueDate));

            if (!string.IsNullOrEmpty(_classOfService))
                result.Add(new XAttribute("classOfService", _classOfService));

            result.Add(new XAttribute("valueLowBound", _valueLowBound.ToString())); ;
            result.Add(new XAttribute("valueHighBound", _valueHighBound.ToString())); ;

            if (Completed)
                result.Add(new XAttribute("completed", Completed.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // must be > -1
            success &= ContractCommon.CheckValueGreaterThan(errors, Count, 0, "count", "setup/backlog/custom: " + Name, Source);

            if (!string.IsNullOrWhiteSpace(_dueDate))
            {
                DateTime temp = _dueDate.ToSafeDate(data.Execute.DateFormat, DateTime.MaxValue);
                if (temp == DateTime.MaxValue)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 52, string.Format(Strings.Error52, "setup/backlog/custom", Name), Source);
                }

                _safeDueDate = temp;


            }

            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                success &= ContractCommon.CheckValueGreaterThan(errors, PercentageHighBound, -1, "percentageHighBound", "setup/backlog/custom: " + Name, Source);
                success &= ContractCommon.CheckValueGreaterThan(errors, PercentageLowBound, -1, "percentageLowBound", "setup/backlog/custom: " + Name, Source);

                if (PercentageHighBound < PercentageLowBound)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 27, string.Format(Strings.Error27, Name), Source);
                }

                /*
                if (SkipPercentage != 0.0 && SkipPercentage != 100.0)
                {
                    success &= ContractCommon.CheckValueGreaterThan(errors, SkipPercentage, 0, "skipPercentage", "setup/backlog/custom: " + Name, Source);
                    success &= ContractCommon.CheckValueLessThan(errors, SkipPercentage, 100, "skipPercentage", "setup/backlog/custom: " + Name, Source);
                }
                 */
            }

            if (data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (this.EstimateDistribution == "")
                {

                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateHighBound, 0, "estimateHighBound", "setup/backlog/custom: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateLowBound, 0, "estimateLowBound", "setup/backlog/custom: " + Name, Source);

                    if (EstimateHighBound < EstimateLowBound)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 26, string.Format(Strings.Error26, Name), Source);
                    }
                }
                else
                {
                    SetupDistributionData dist =
                        data.Setup.Distributions.Where(d => string.Compare(d.Name, EstimateDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, EstimateDistribution, "setup/backlog/custom", this._name), Source);
                    }

                }
            }

            foreach (var col in Columns)
                success &= col.Validate(data, errors);

            //initial column id must be valid
            if (this.InitialColumn != -1 && data.Setup.Columns.Count(c => c.Id == this.InitialColumn) == 0)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 60, string.Format(Strings.Error60, this.InitialColumn), Source);
            }


            // check a class of service exists of the given name
            if (!string.IsNullOrEmpty(_classOfService))
            {
                bool found = data.Setup.ClassOfServices.Any(c => string.Compare(c.Name, _classOfService, true) == 0);
                if (!found)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 54, string.Format(Strings.Error54, _classOfService, "setup/backlog/custom", Name), Source);
                }
            }

            return success;
        }
    }


}
