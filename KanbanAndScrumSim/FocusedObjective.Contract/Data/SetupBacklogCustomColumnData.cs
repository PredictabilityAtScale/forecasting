using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("column", "Defines a column override for these class of service (Kanban models only).", false, HasMandatoryAttributes = true, HasAnyAttributes = true, ParentElement = "classOfService", ParentParentElement = "classOfServices")]
    [SimMLElement("column", "Defines a column override for these custom backlog items (Kanban models only).", false, HasMandatoryAttributes = true, HasAnyAttributes = true, ParentElement = "custom", ParentParentElement = "deliverable")]
    public class SetupBacklogCustomColumnData : ContractDataBase, IValidate
    {
        public SetupBacklogCustomColumnData()
        {
        }

        public SetupBacklogCustomColumnData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _columnId = -1;
        private double _estimateLowBound = 0;
        private double _estimateHighBound = 0;
        private string _estimateDistribution = "";
        private double _skipPercentage = 0.0;

        // public properties
        [SimMLAttribute("id", "Column id to override default values for custom backlog items of this type (kanban only).", true)]
        public int ColumnId
        {
            get { return _columnId; }
            set { _columnId = value; }
        }

        [SimMLAttribute("estimateLowBound", "Lowest allowed value of cycle-time value for this column for these custom backlog types (kanban only). ", false)]
        public double EstimateLowBound
        {
            get { return _estimateLowBound; }
            set { _estimateLowBound = value; }
        }

        [SimMLAttribute("estimateHighBound", "Highest allowed value of cycle-time value for this column for these custom backlog types (kanban only). ", false)]
        public double EstimateHighBound
        {
            get { return _estimateHighBound; }
            set { _estimateHighBound = value; }
        }

        [SimMLAttribute("estimateDistribution", "Distribution used to generate cycle-time estimates for this column for these custom backlog types (kanban only). Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string EstimateDistribution
        {
            get { return _estimateDistribution; }
            set { _estimateDistribution = value; }
        }

        [SimMLAttribute("skipPercentage", "How often items of this custom backlog type skip this column in percentage value (0 to 100). Used to model some items doing extra work, or skipping work. Default is 0, which means no items skip this column. (Kanban only)", false)]
        public double SkipPercentage
        {
            get { return _skipPercentage; }
            set { _skipPercentage = value; }
        }
        
        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeIntValue(
                out _columnId,
                source,
                errors,
                "id",
                _columnId);


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
                string.IsNullOrWhiteSpace(_estimateDistribution));
            
            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _estimateHighBound,
                source,
                errors,
                "estimateHighBound",
                _estimateHighBound,
                string.IsNullOrWhiteSpace(_estimateDistribution));

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

            result.Add(new XAttribute("id", _columnId.ToString()));
            result.Add(new XAttribute("estimateLowBound", _estimateLowBound.ToString()));
            result.Add(new XAttribute("estimateHighBound", _estimateHighBound.ToString()));
            result.Add(new XAttribute("estimateDistribution", _estimateDistribution.ToString()));
            result.Add(new XAttribute("skipPercentage", _skipPercentage.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // column id must match a defined column
            if (data.Setup.Columns.Count(c => c.Id == this.ColumnId) == 0)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 29, string.Format(Strings.Error29, ColumnId), Source);
            }

            if (this.EstimateDistribution == "" && this.SkipPercentage != 100.0)
            {
                // must be > 0
                success &= ContractCommon.CheckValueGreaterThan(errors, EstimateHighBound, 0, "estimateHighBound", "setup/backlog/column: " + this.ColumnId, Source);
                success &= ContractCommon.CheckValueGreaterThan(errors, EstimateLowBound, 0, "estimateLowBound", "setup/backlog/column: " + this.ColumnId, Source);

                // highbound must be greater than or equal too low bound
                if (EstimateHighBound < EstimateLowBound)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 28, string.Format(Strings.Error28, ColumnId), Source);
                }
            }
            else
            {
                if (this.EstimateDistribution != "")
                {
                    SetupDistributionData dist =
                        data.Setup.Distributions.Where(d => string.Compare(d.Name, EstimateDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, EstimateDistribution, "setup/backlog/custom/column", this.ColumnId), Source);
                    }
                }
            }

            return success;
        }
    }


}
