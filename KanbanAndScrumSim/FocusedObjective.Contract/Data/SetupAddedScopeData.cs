using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("addedScope", "Added scope entries represent work added to the backlog once a project has started.", false, HasMandatoryAttributes = true, ParentElement="addedScopes")]
    public class SetupAddedScopeData : SensitivityBase
    {
        public SetupAddedScopeData()
        {
        }
        
        public SetupAddedScopeData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private double _scale = 1.0;
        private double _occurrenceLowBound = 5.0;
        private double _occurrenceHighBound = 10.0;
        private string _occurrenceDistribution = "";
        private OccurrenceTypeEnum _occurrenceType = OccurrenceTypeEnum.Count;
        private string _name = "";
        private int _count = 1;
        private string _phases = "";
        private string _classOfService = string.Empty;
        private string _customBacklog = string.Empty;
        private string _deliverable = string.Empty;
        private string _targetCustomBacklog = string.Empty;
        private string _targetDeliverable = string.Empty;
        // scrum only
        private double _estimateLowBound = 0;
        private double _estimateHighBound = 0;
        private string _estimateDistribution = "";

        // public properties
        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        [SimMLAttribute("occurrenceType", "Measurement unit for occurrence rates (as specified in low, high or distribution occurrence rates). Valid values are count, cards, stories, size (Scrum only), points (Scrum only), percentage. Default is count for Kanban, points for Scrum.", true, ValidValues = "count|cards|stories|size|points|percentage")]
        public OccurrenceTypeEnum OccurenceType
        {
            get { return _occurrenceType; }
            set { _occurrenceType = value; }
        }
        
        [SimMLAttribute("occurrenceLowBound", "Lowest value of the occurrence rate in units of measure specified in the occurrenceType attribute.", true)]
        public double OccurrenceLowBound
        {
            get { return _occurrenceLowBound; }
            set { _occurrenceLowBound = value; }
        }

        [SimMLAttribute("occurrenceHighBound", "Highest value of the occurrence rate in units of measure specified in the occurrenceType attribute.", true)]
        public double OccurrenceHighBound
        {
            get { return _occurrenceHighBound; }
            set { _occurrenceHighBound = value; }
        }

        [SimMLAttribute("occurrenceDistribution", "Distribution by name useed to generate the occurrence rate values (instead of using low and high values). Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string OccurrenceDistribution
        {
            get { return _occurrenceDistribution; }
            set { _occurrenceDistribution = value; }
        }

        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("estimateLowBound", "Lowest possible story point estimate for these added scope items (scrum only).", false)]
        public double EstimateLowBound
        {
            get { return _estimateLowBound; }
            set { _estimateLowBound = value; }
        }

        [SimMLAttribute("estimateHighBound", "Highest possible story point estimate for these added scope items (scrum only).", false)]
        public double EstimateHighBound
        {
            get { return _estimateHighBound; }
            set { _estimateHighBound = value; }
        }

        [SimMLAttribute("estimateDistribution", "Distribution used to generate story point estimates for these added scope items (scrum only). Must be a valid distribution defined in the <distributions>...</distributions> section.", false)]
        public string EstimateDistribution
        {
            get { return _estimateDistribution; }
            set { _estimateDistribution = value; }
        }

        [SimMLAttribute("count", "Number of items added to the backlog each time this event is triggered. Default is 1.", false)]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        [SimMLAttribute("phases", "Phase by name (or phases separated by | symbols) where this event is active. Leave blank (the default) for all phases. Must be a valid phase defined in the <phases>...</phases> section.", false)]
        public string Phases
        {
            get { return _phases; }
            set { _phases = value; }
        }

        [SimMLAttribute("classOfService", "Class of service assigned to new items created by this event. Leave blank (the default) for the same class of service as the triggering card. Must be a valid class of service defined in the <classOfServices>...</classOfServices> section.", false)]
        public string ClassOfService
        {
            get { return _classOfService; }
            set { _classOfService = value; }
        }

        [SimMLAttribute("customBacklog", "Custom backlog by name assigned to new items created by this event. Leave blank (the default) for the same backlog as the triggering card. Must be a valid custom backlog defined a the <custom>...</custom> section.", false)]
        public string CustomBacklog
        {
            get { return _customBacklog; }
            set { _customBacklog = value; }
        }

        [SimMLAttribute("deliverable", "Deliverable by name assigned to new items created by this event. Leave blank (the default) for the same deliverable as the triggering card. Must be a valid custom backlog defined in a <deliverable>...</deliverable> section.", false)]
        public string Deliverable
        {
            get { return _deliverable; }
            set { _deliverable = value; }
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
            
            _name = source.Value.Trim();

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
                out _count,
                source,
                errors,
                "count",
                _count,
                false);

            _phases = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "phases",
                _phases,
                false);

            _classOfService = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "classOfService",
                _classOfService,
                false);

            _customBacklog = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "customBacklog",
                _customBacklog,
                false);

            _deliverable = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "deliverable",
                _deliverable,
                false);

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
            XElement result = new XElement("addedScope");

            //SKIP: result.Add(new XAttribute("scale", _scale.ToString())); now obsolete
            result.Add(new XAttribute("count", _count.ToString())); 
            result.Add(new XAttribute("occurrenceLowBound", _occurrenceLowBound.ToString()));
            result.Add(new XAttribute("occurrenceHighBound",_occurrenceHighBound.ToString()));
            result.Add(new XAttribute("occurrenceDistribution", _occurrenceDistribution)); 
            result.Add(new XAttribute("occurrenceType", _occurrenceType.ToString().ToLower()));
            result.Add(_name);
            result.Add(new XAttribute("phases", _phases)); 
            
            if (simType == SimulationTypeEnum.Scrum)
            {
                result.Add(new XAttribute("estimateLowBound", _estimateLowBound.ToString()));
                result.Add(new XAttribute("estimateHighBound", _estimateHighBound.ToString()));
                result.Add(new XAttribute("estimateDistribution", _estimateDistribution));
            }

            if (!string.IsNullOrEmpty(_classOfService))
                result.Add(new XAttribute("classOfService", _classOfService));

            if (!string.IsNullOrEmpty(_customBacklog))
                result.Add(new XAttribute("customBacklog", _customBacklog));

            if (!string.IsNullOrEmpty(_deliverable))
                result.Add(new XAttribute("deliverable", _deliverable)); 
            
            if (!string.IsNullOrEmpty(_targetCustomBacklog))
                result.Add(new XAttribute("targetCustomBacklog", _targetCustomBacklog));

            if (!string.IsNullOrEmpty(_targetDeliverable))
                result.Add(new XAttribute("targetDeliverable", _targetDeliverable)); 
            
            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            
            success &= ContractCommon.CheckValueGreaterThan(errors, Count, 0, "count", "setup/addedScopes/addedScope: " + Name, Source);

            if (OccurrenceDistribution == "")
            {
                // must be > 0
                success &= ContractCommon.CheckValueGreaterThan(errors, Scale, 0, "scale", "setup/addedScopes/addedScope: " + Name, Source);


                if (OccurenceType == OccurrenceTypeEnum.Percentage)
                {
                    success &= ContractCommon.CheckValueGreaterThanOrEqualTo(errors, OccurrenceHighBound, 0, "occurrenceHighBound", "setup/addedScopes/addedScope: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThanOrEqualTo(errors, OccurrenceLowBound, 0, "occurrenceLowBound", "setup/addedScopes/addedScope: " + Name, Source);
                }
                else
                {
                    success &= ContractCommon.CheckValueGreaterThan(errors, OccurrenceHighBound, 0, "occurrenceHighBound", "setup/addedScopes/addedScope: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThan(errors, OccurrenceLowBound, 0, "occurrenceLowBound", "setup/addedScopes/addedScope: " + Name, Source);
                }

                if (OccurrenceHighBound < OccurrenceLowBound)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 18, string.Format(Strings.Error18, Name), Source);
                }
            }
            else
            {
                SetupDistributionData dist = 
                    data.Setup.Distributions.Where(d => string.Compare(d.Name, OccurrenceDistribution, true) == 0).FirstOrDefault();

                if (dist == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, OccurrenceDistribution, "setup/addedScopes/addedScope", this.Name), Source);
                }

                // distribution validated in setup/distribution
            }

            if (data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (EstimateDistribution == "")
                {
                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateHighBound, 0, "estimateHighBound", "setup/defects/defect: " + Name, Source);
                    success &= ContractCommon.CheckValueGreaterThan(errors, EstimateLowBound, 0, "estimateLowBound", "setup/defects/defect: " + Name, Source);

                    // highbound must be greater than or equal too low bound
                    if (EstimateHighBound < EstimateLowBound)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 34, string.Format(Strings.Error34, Name), Source);
                    }
                }
                else
                {
                    SetupDistributionData dist = data.Setup.Distributions.Where(d => string.Compare(d.Name, EstimateDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, EstimateDistribution, "setup/addedScopes/addedScope", this.Name), Source);
                    }
                }
            }

            bool phasesOK = true;
            if (_phases != "")
                phasesOK = _phases
                    .Split(new char[] { '|', ',' })
                    .All(p => data.Setup.Phases.Any(q => string.Compare(p, q.Name, true) == 0));

            if (!phasesOK)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 7, string.Format(Strings.Error7, this.Phases, "setup/addedScopes/addedScope", this.Name), Source);
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

            // check a custom backlog exists of the given name
            if (!string.IsNullOrEmpty(_customBacklog))
            {
                bool found = data.Setup.Backlog.CustomBacklog.Any(c => string.Compare(c.Name, _customBacklog, true) == 0);
                if (!found)
                {
                    // need to look inside the deliverables as well
                    foreach (var d in data.Setup.Backlog.Deliverables)
                    {
                       found = d.CustomBacklog.Any(c => string.Compare(c.Name, _customBacklog, true) == 0);
                       if (found)
                           break;
                    }

                    if (!found)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 62, string.Format(Strings.Error62, _customBacklog, "setup/backlog/custom", Name), Source);
                    }
                }
            }

            // check a deliverable exists of the given name
            if (!string.IsNullOrEmpty(_deliverable))
            {
                bool found = data.Setup.Backlog.Deliverables.Any(c => string.Compare(c.Name, _deliverable, true) == 0);
                if (!found)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 63, string.Format(Strings.Error63, _deliverable, "setup/backlog", Name), Source);
                }
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
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 62, string.Format(Strings.Error62, _targetCustomBacklog, "setup/backlog/custom", Name), Source);
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
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 63, string.Format(Strings.Error63, _targetDeliverable, "setup/backlog", Name), Source);
                }
            } 
            
            
            return success;
        }
    }
}
