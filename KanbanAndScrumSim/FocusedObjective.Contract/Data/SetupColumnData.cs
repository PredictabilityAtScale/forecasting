using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("column", "Column entries represent the steps work items progress through to be completed (Kanban only).", true, HasMandatoryAttributes = true, ParentElement = "columns")]
    public class SetupColumnData : SensitivityBase, IValidate
    {
        public SetupColumnData()
        {
        }
        
        public SetupColumnData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _id = -1;
        private double _estimateLowBound = 0;
        private double _estimateHighBound = 0;
        private string _estimateDistribution = "";
        private int _wipLimit = 0;
        private string _name = "";
        private bool _isBuffer = false;
        private int _sequence = -1;
        private int _replenishInterval = -1;
        private int _completeInterval = -1;

        private int _highestWip = 0;
        private int _displayWidth = 1;

        private double _skipPercentage = 0.0;

        // public properties
        [SimMLAttribute("id", "Column id number used to uniquely identify this column in other sections (kanban only).", true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [SimMLAttribute("displayWidth", "Width of this column in the visual simulation of the board (kanban only). Defaults to 1, a single card width.", false)]
        public int DisplayWidth
        {
            get { return _displayWidth; }
            set { _displayWidth = value; }
        }

        [SimMLAttribute("estimateLowBound", "Lowest allowed value of cycle-time value for this column (kanban only). Can be overriden by specifying specific cycle-time override in a custom backlog, defect, phase or class of service.", false)]
        public double EstimateLowBound
        {
            get { return _estimateLowBound; }
            set { _estimateLowBound = value; }
        }

        [SimMLAttribute("estimateHighBound", "Highest allowed value of cycle-time value for this column (kanban only). Can be overriden by specifying specific cycle-time override in a custom backlog, defect, phase or class of service.", false)]
        public double EstimateHighBound
        {
            get { return _estimateHighBound; }
            set { _estimateHighBound = value; }
        }

        [SimMLAttribute("estimateDistribution", "Distribution used to generate cycle-time estimates for this column (kanban only). Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string EstimateDistribution
        {
            get { return _estimateDistribution; }
            set { _estimateDistribution = value; }
        }

        [SimMLAttribute("wipLimit", "Maximum number of items allowed in this column at one time (kanban only). Can be overriden by a column override in a phase or class of service.", true)]
        public int WipLimit
        {
            get { return _wipLimit; }
            set { _wipLimit = value; }
        }


        [SimMLAttribute("skipPercentage", "How often items skip this column in percentage value (0 to 100). Used to model some items doing extra work, or skipping work. Default is 0, which means no items skip this column. (Kanban only)", false)]
        public double SkipPercentage
        {
            get { return _skipPercentage; }
            set { _skipPercentage = value; }
        }

        // not part of XML contract
        public int HighestWipLimit
        {
            get
            {
                return _highestWip;
            }
            set
            {
                _highestWip = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("buffer", "Determines if this column is a buffer or queue column. If set to true, work entering this column is IMMEDIATELY flagged as complete. If set to false, work entering this column will begin comleting based on the relavent cycle time estimate values. Default is false", false, ValidValues="true|false")]
        public bool IsBuffer
        {
            get { return _isBuffer; }
            set { _isBuffer = value; }
        }

        public int Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }

        [SimMLAttribute("replenishInterval", "Number of simulation intervals required before new work is allowed to enter this column. For example, if empty positions in a column are replenished every 5 days, set this value to 5. Default is 1 (every interval)", false)]
        public int ReplenishInterval
        {
            get { return _replenishInterval; }
            set { _replenishInterval = value; }
        }

        [SimMLAttribute("completeInterval", "Number of simulation intervals required before completed work is allowed to exit this column. Default is 1 (every interval)", false)]
        public int CompleteInterval
        {
            get { return _completeInterval; }
            set { _completeInterval = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeIntValue(
                out _id,
                source,
                errors,
                "id",
                _id);

            success = success && ContractCommon.ReadAttributeIntValue(
                out _displayWidth,
                source,
                errors,
                "displayWidth",
                _displayWidth,
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

            success = success && ContractCommon.ReadAttributeIntValue(
                out _wipLimit,
                source,
                errors,
                "wipLimit",
                _wipLimit);

           _isBuffer = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "buffer",
                "false", false,
                "no" , false,
                "true", true,
                "yes", true);

           success = success && ContractCommon.ReadAttributeIntValue(
               out _replenishInterval,
               source,
               errors,
               "replenishInterval",
               _replenishInterval,
               false);

           success = success && ContractCommon.ReadAttributeIntValue(
               out _completeInterval,
               source,
               errors,
               "completeInterval",
               _completeInterval,
               false);
           
            _name = source.Value;

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _skipPercentage,
                source,
                errors,
                "skipPercentage",
                _skipPercentage,
                false);

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("column");

            result.Add(_name);
            result.Add(new XAttribute("id", _id.ToString())); 
            result.Add(new XAttribute("wipLimit", _wipLimit.ToString()));
            result.Add(new XAttribute("skipPercentage", _skipPercentage.ToString()));

            if (this._isBuffer)
            {
                result.Add(new XAttribute("buffer", "true"));
            }
            else
            {
                result.Add(new XAttribute("estimateLowBound", _estimateLowBound.ToString()));
                result.Add(new XAttribute("estimateHighBound", _estimateHighBound.ToString()));
                result.Add(new XAttribute("estimateDistribution", _estimateDistribution.ToString()));
            }

            if (_replenishInterval > 0)
                result.Add(new XAttribute("replenishInterval", _replenishInterval.ToString()));

            if (_completeInterval > 0)
                result.Add(new XAttribute("completeInterval", _completeInterval.ToString()));

            if (_displayWidth > 1)
                result.Add(new XAttribute("displayWidth", _displayWidth.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // must be > 0 (<= 0 now means infinite column
            //success = ContractCommon.CheckValueGreaterThan(errors, WipLimit, 0, "wipLimit", "setup/columns/column: " + Name, Source) && success;

            if (_replenishInterval != -1)
                success = ContractCommon.CheckValueGreaterThan(errors, ReplenishInterval, 0, "replenishInterval", "setup/columns/column: " + Name, Source) && success;

            if (_completeInterval != -1)
                success = ContractCommon.CheckValueGreaterThan(errors, CompleteInterval, 0, "completeInterval", "setup/columns/column: " + Name, Source) && success;

            if (!IsBuffer)
            {
                if (_estimateDistribution == "")
                {
                    success = ContractCommon.CheckValueGreaterThan(errors, EstimateHighBound, 0, "estimateHighBound", "setup/columns/column: " + Name, Source) && success;
                    success = ContractCommon.CheckValueGreaterThan(errors, EstimateLowBound, 0, "estimateLowBound", "setup/columns/column: " + Name, Source) && success;

                    // highbound must be greater than or equal too low bound
                    if (EstimateHighBound < EstimateLowBound)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 44, string.Format(Strings.Error44, Name, Id), Source);
                    }
                }
                else
                {
                    SetupDistributionData dist =
                        data.Setup.Distributions.Where(d => string.Compare(d.Name, EstimateDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, EstimateDistribution, "setup/columns/column", this.Name), Source);
                    }
                }
            }

            return success;
        }
    }
}
