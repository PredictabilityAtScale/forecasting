using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("blockingEvent", "Blocking events represent delays and impediments to work items being completed.", false, HasMandatoryAttributes = true, ParentElement="blockingEvents")]
    public class SetupBlockingEventData : SensitivityBase, IValidate
    {
        public SetupBlockingEventData()
        {
        }
        
        public SetupBlockingEventData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private double _scale = 1.0;
        private int _columnId = -1;
        private double _occurrenceLowBound = 5.0;
        private double _occurrenceHighBound = 10.0;
        private string _occurrenceDistribution = "";
        private OccurrenceTypeEnum _occurrenceType = OccurrenceTypeEnum.Count;
        private double _estimateLowBound = 0;
        private double _estimateHighBound = 0;
        private string _estimateDistribution = "";
        private string _name = "";
        private string _phases = ""; 
        private string _targetCustomBacklog = string.Empty;
        private string _targetDeliverable = string.Empty;

        private bool _blockWork = true;
        private bool _blockDefects = false;
        private bool _blockAddedScope = false;

        // public properties

        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        [SimMLAttribute("columnId", "Column id number where items are counted towards occurrence rate and blocked when this event is triggered (kanban only).", false)]
        public int ColumnId
        {
            get { return _columnId; }
            set { _columnId = value; }
        }

        [SimMLAttribute("occurrenceLowBound", "Lowest value of the occurrence rate in units of measure specified in the occurrenceType attribute.", true)]
        public double OccurrenceLowBound
        {
            get { return _occurrenceLowBound; }
            set { _occurrenceLowBound = value; }
        }


        [SimMLAttribute("occurrenceType", "Measurement unit for occurrence rates (as specified in low, high or distribution occurrence rates). Valid values are count, cards, stories, size (Scrum only), points (Scrum only), percentage. Default is count for Kanban, points for Scrum.", true, ValidValues = "count|cards|stories|size|points|percentage")]
        public OccurrenceTypeEnum OccurenceType
        {
            get { return _occurrenceType; }
            set { _occurrenceType = value; }
        }

        [SimMLAttribute("occurrenceHighBound", "Highest value of the occurrence rate in units of measure specified in the occurrenceType attribute.", true)]
        public double OccurrenceHighBound
        {
            get { return _occurrenceHighBound; }
            set { _occurrenceHighBound = value; }
        }

        [SimMLAttribute("occurrenceDistribution", "Distribution by name used to generate the occurrence rate values (instead of using low and high values). Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string OccurrenceDistribution
        {
            get { return _occurrenceDistribution; }
            set { _occurrenceDistribution = value; }
        }


        [SimMLAttribute("estimateLowBound", "Lowest possible estimate for blocking units (time for kanban, points for scrum) when this event is triggered for an item.", true)]
        public double EstimateLowBound
        {
            get { return _estimateLowBound; }
            set { _estimateLowBound = value; }
        }

        [SimMLAttribute("estimateHighBound", "Highest possible estimate for blocking units (time for kanban, points for scrum) when this event is triggered for an item.", true)]
        public double EstimateHighBound
        {
            get { return _estimateHighBound; }
            set { _estimateHighBound = value; }
        }

        [SimMLAttribute("estimateDistribution", "Distribution used to generate blocking units (time for kanban, points for scrum) when this event is triggered for an item. Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string EstimateDistribution
        {
            get { return _estimateDistribution; }
            set { _estimateDistribution = value; }
        }
        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("phases", "Phase by name (or phases separated by | symbols) where this event is active. Leave blank (the default) for all phases. Must be a valid phase defined in the <phases>...</phases> section.", false)]
        public string Phases
        {
            get { return _phases; }
            set { _phases = value; }
        }

        [SimMLAttribute("blockWork", "Indicates whether work items can be blocked by this event. Set to \"false\" to inhibit blocking of work items. Default is true.", false, ValidValues="true|false")]
        public bool BlockWork
        {
            get {return _blockWork; }
            set { _blockWork = value; }
        }

        [SimMLAttribute("blockDefects", "Indicates whether defect items can be blocked by this event. Set to \"false\" to inhibit blocking of defect items. Default is true.", false, ValidValues = "true|false")]
        public bool BlockDefects
        {
            get { return _blockDefects; }
            set { _blockDefects = value; }
        }

        [SimMLAttribute("blockAddedScope", "Indicates whether added-scope items can be blocked by this event. Set to \"false\" to inhibit blocking of added-scope items. Default is true.", false, ValidValues = "true|false")]
        public bool BlockAddedScope
        {
            get { return _blockAddedScope; }
            set { _blockAddedScope = value; }
        }

        [SimMLAttribute("targetCustomBacklog", "Only items of this custom backlog type are counted as trigger cards. Leave blank (the default) for all items of any custom backlog.", false)]
        public string TargetCustomBacklog
        {
            get { return _targetCustomBacklog; }
            set { _targetCustomBacklog = value; }
        }

        [SimMLAttribute("targetDeliverable", "Only items of this deliverable type are counted as trigger cards. Leave blank (the default) for all items of any deliverable.", false)]
        public string TargetDeliverable
        {
            get { return _targetDeliverable; }
            set { _targetDeliverable = value; }
        }   

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _scale,
                source,
                errors,
                "scale",
                _scale,
                false);

            success = success && ContractCommon.ReadAttributeIntValue(
                out _columnId,
                source,
                errors,
                "columnId",
                _columnId,
                false);

            _occurrenceDistribution = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "occurrenceDistribution",
                _occurrenceDistribution,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _occurrenceLowBound,
                source,
                errors,
                "occurrenceLowBound",
                _occurrenceLowBound,
                _occurrenceDistribution == "");

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _occurrenceHighBound,
                source,
                errors,
                "occurrenceHighBound",
                _occurrenceHighBound,
                _occurrenceDistribution == "");

            _occurrenceType = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "occurrenceType",
                "count", OccurrenceTypeEnum.Count,
                "cards", OccurrenceTypeEnum.Count,
                "stories", OccurrenceTypeEnum.Count,
                "size", OccurrenceTypeEnum.Size,
                "points", OccurrenceTypeEnum.Size,
                "percentage", OccurrenceTypeEnum.Percentage);

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
                _estimateDistribution == "");
            
            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _estimateHighBound,
                source,
                errors,
                "estimateHighBound",
                _estimateHighBound,
                _estimateDistribution == "");

            _name = source.Value.Trim() ;

            _phases = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "phases",
                _phases,
                false);

            _blockWork = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "blockWork",
                "true", true,
                "yes", true,
                "false", false,
                "no", false);

            _blockDefects = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "blockDefects",
                "true", true,
                "yes", true,
                "false", false,
                "no", false);

            _blockAddedScope = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "blockAddedScope",
                "true", true,
                "yes", true,
                "false", false,
                "no", false);

            _targetCustomBacklog = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "targetCustomBacklog",
                _targetCustomBacklog,
                false);

            _targetDeliverable = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "targetDeliverable",
                _targetDeliverable,
                false); 

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("blockingEvent");

            //SKIP: result.Add(new XAttribute("scale", _scale.ToString())); now obsolete
            result.Add(new XAttribute("columnId",_columnId.ToString()));
            result.Add(new XAttribute("occurrenceLowBound", _occurrenceLowBound.ToString()));
            result.Add(new XAttribute("occurrenceHighBound", _occurrenceHighBound.ToString()));
            result.Add(new XAttribute("occurrenceDistribution", _occurrenceDistribution.ToString())); 
            result.Add(new XAttribute("occurrenceType", _occurrenceType.ToString().ToLower()));
            result.Add(new XAttribute("estimateLowBound", _estimateLowBound.ToString()));
            result.Add(new XAttribute("estimateHighBound",_estimateHighBound.ToString()));
            result.Add(new XAttribute("estimateDistribution", _estimateDistribution.ToString()));
            result.Add(_name);
            result.Add(new XAttribute("phases", _phases));

            result.Add(new XAttribute("blockWork", _blockWork.ToString()));
            result.Add(new XAttribute("blockDefects", _blockDefects.ToString()));
            result.Add(new XAttribute("blockAddedScope", _blockAddedScope.ToString()));


            if (!string.IsNullOrEmpty(_targetCustomBacklog))
                result.Add(new XAttribute("targetCustomBacklog", _targetCustomBacklog));

            if (!string.IsNullOrEmpty(_targetDeliverable))
                result.Add(new XAttribute("targetDeliverable", _targetDeliverable)); 

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                // column id must match a defined column
                if (data.Setup.Columns.Count(c => c.Id == this.ColumnId) == 0)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 21, string.Format(Strings.Error21, Name), Source);
                }
            }

            if (_occurrenceDistribution == "")
            {
                // must be > 0
                success &= ContractCommon.CheckValueGreaterThan(errors, Scale, 0, "scale", "setup/blockingEvents/blockingEvent: " + Name, Source);
                success &= ContractCommon.CheckValueGreaterThan(errors, OccurrenceHighBound, 0, "occurrenceHighBound", "setup/blockingEvents/blockingEvent: " + Name, Source);
                success &= ContractCommon.CheckValueGreaterThan(errors, OccurrenceLowBound, 0, "occurrenceLowBound", "setup/blockingEvents/blockingEvent: " + Name, Source);

                // highbound must be greater than or equal too low bound
                if (OccurrenceHighBound < OccurrenceLowBound)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 20, string.Format(Strings.Error20, Name), Source);
                }
            }
            else
            {
                SetupDistributionData dist =
                    data.Setup.Distributions.Where(d => string.Compare(d.Name, OccurrenceDistribution, true) == 0).FirstOrDefault();

                if (dist == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, OccurrenceDistribution, "setup/blockingEvents/blockingEvent", this.Name), Source);
                }

                // distribution validated in setup/distribution
            }

            if (_estimateDistribution == "")
            {
                if (OccurenceType == OccurrenceTypeEnum.Percentage)
                {
                    success &= ContractCommon.CheckValueGreaterThanOrEqualTo(errors, EstimateHighBound, 0, "estimateHighBound", "setup/blockingEvents/blockingEvent: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThanOrEqualTo(errors, EstimateLowBound, 0, "estimateLowBound", "setup/blockingEvents/blockingEvent: " + Name, Source);
                }
                else
                {
                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateHighBound, 0, "estimateHighBound", "setup/blockingEvents/blockingEvent: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateLowBound, 0, "estimateLowBound", "setup/blockingEvents/blockingEvent: " + Name, Source);
                }

                if (EstimateHighBound < EstimateLowBound)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 19, string.Format(Strings.Error19, Name), Source);
                }
            }
            else
            {
                SetupDistributionData dist =
                    data.Setup.Distributions.Where(d => string.Compare(d.Name, EstimateDistribution, true) == 0).FirstOrDefault();

                if (dist == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47,EstimateDistribution, "setup/blockingEvents/blockingEvent", this.Name), Source);
                }

                // distribution validated in setup/distribution
            }

            bool phasesOK = true;
            if (_phases != "")
                phasesOK = _phases
                    .Split(new char[] { '|', ',' })
                    .All(p => data.Setup.Phases.Any(q => string.Compare(p, q.Name, true) == 0));

            if (!phasesOK)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 7, string.Format(Strings.Error7, this.Phases, "setup/blockingEvents/blockingEvent", this.Name), Source);
            }

            // check a target custom backlog exists of the given name
            if (!string.IsNullOrEmpty(_targetCustomBacklog))
            {
                bool found = data.Setup.Backlog.CustomBacklog.Any(c => string.Compare(c.Name, _targetCustomBacklog, true) == 0);
                if (!found)
                {
                    // need to look inside the deliverables as well
                    foreach (var d in data.Setup.Backlog.Deliverables)
                    {
                        found = d.CustomBacklog.Any(c => string.Compare(c.Name, _targetCustomBacklog, true) == 0);
                        if (found)
                            break;
                    }

                    if (!found)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 66, string.Format(Strings.Error66, _targetCustomBacklog, "setup/backlog/custom", Name), Source);
                    }
                }
            }

            // check a target deliverable exists of the given name
            if (!string.IsNullOrEmpty(_targetDeliverable))
            {
                bool found = data.Setup.Backlog.Deliverables.Any(c => string.Compare(c.Name, _targetDeliverable, true) == 0);
                if (!found)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 67, string.Format(Strings.Error67, _targetDeliverable, "setup/backlog", Name), Source);
                }
            } 

            return success;
        }

    }


}
