using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("deliverable", "A collection of <custom>...</custom> element(s). Deliverables can be simulated as groups and have specific skipping percentages to specify risk likelihood or intangibility.", false, HasMandatoryAttributes = true)]
    public class SetupBacklogDeliverableData : ContractDataBase, IValidate
    {
        public SetupBacklogDeliverableData()
        {
        }

        public SetupBacklogDeliverableData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _name;
        private string _dueDate = "";
        private double _skipPercentage = 0.0;
        private int _order = int.MaxValue;
        private List<SetupBacklogCustomData> _customBacklog = new List<SetupBacklogCustomData>();


        // Elements of Cost of delay values. MVP going to use a single number they can use formulas and parameters to build
        /*

        private double _revenueIncreased = 0.0;
        private TimeUnitEnum _revenueIncreasedUnit = TimeUnitEnum.Month;
        
        private double _revenueProtected = 0.0;
        private TimeUnitEnum _revenueProtectedUnit = TimeUnitEnum.Month;

        private double _costReduced = 0.0;
        private TimeUnitEnum _costReducedUnit = TimeUnitEnum.Month;
        
        private double _costAvoided = 0.0;
        private TimeUnitEnum _costAvoidedUnit = TimeUnitEnum.Month;

        private string _opportunityEarliestDate = "";
        private string _opportunityPeakDate = "";
        private string _opportunityFiftyPercentDate = "";
        private string _opportunityEndOfLifeDate = "";
        */

        private string _preRequisiteDeliverables = "";

        private string _earliestStartDateAsString = "";
        private DateTime _earliestStartDate;
        private bool _earliestStartDateValid = false;


        // public properties
        [SimMLAttribute("name", "A name for this deliverable. This name is used when specifying this deliverable as a reference in other attributes, and can be displayed in the name of each story by specifying the {3} special tag in the <backlog nameFormat=... attribute.", true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("dueDate", "Due date used for ordering start of work (earliest due date to latest) with the SAME priority order value during simulation. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD.", false)]
        public string DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        [SimMLAttribute("skipPercentage", "The percentage of times this deliverable is omitted from the initial backlog. Used to simulate risk factors that may or may nor come true given some probability.", false)]
        public double SkipPercentage
        {
            get { return _skipPercentage; }
            set { _skipPercentage = value; }
        }

        [SimMLAttribute("order", "The sort order deliverables will be prioritized from lowest to highest. The default value of deliverables that omit this attribute will be the highest allowed (and started last).", false)]
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        [SimMLElement("custom", "An individual custom backlog entry.", false, HasMandatoryAttributes = true)]
        public List<SetupBacklogCustomData> CustomBacklog
        {
            get { return _customBacklog; }
        }

        [SimMLAttribute("preRequisiteDeliverables", "A single deliverable name, or set of deliverable names separated by a | that need to be completed before this deliverable can start. Used to model strict dependency order.", false)]
        public string PreRequisiteDeliverables
        {
            get { return _preRequisiteDeliverables; }
            set { _preRequisiteDeliverables = value; }
        }

        [SimMLAttribute("earliestStartDate", "The earliest date this deliverable can be started. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD.", false)]
        public DateTime EarliestStartDate
        {
            get
            {
                if (_earliestStartDateValid)
                    return _earliestStartDate;
                else
                    return DateTime.MinValue;
            }
        }


        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _name = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "name",
                _name,
                true);

            _dueDate = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "dueDate",
                string.Empty,
                false);

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _skipPercentage,
                source,
                errors,
                "skipPercentage",
                _skipPercentage,
                false);

            result = result && ContractCommon.ReadAttributeIntValue(
                out _order,
                source,
                errors,
                "order",
                _order,
                false);

            _preRequisiteDeliverables = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "preRequisiteDeliverables",
                string.Empty,
                false);

            _earliestStartDateAsString = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "earliestStartDate",
                string.Empty,
                false);


            // add the custom backlog data
            foreach (XElement custom in source.Elements("custom"))
            {
                _customBacklog.Add(
                    new SetupBacklogCustomData(custom, errors));
            }

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("deliverable");

            result.Add(new XAttribute("name", _name));
            
            if (!string.IsNullOrEmpty(_dueDate))
                 result.Add(new XAttribute("dueDate", _dueDate));

            result.Add(new XAttribute("skipPercentage", _skipPercentage.ToString()));

            result.Add(new XAttribute("order", _order.ToString()));

            result.Add(new XAttribute("preRequisiteDeliverables", _preRequisiteDeliverables));
            result.Add(new XAttribute("earliestStartDate", _earliestStartDateAsString));

            foreach (var custom in _customBacklog)
                result.Add(custom.AsXML(simType));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (!string.IsNullOrWhiteSpace(_dueDate))
            {
                DateTime? temp = _dueDate.ToSafeDate(data.Execute.DateFormat, null);
                if (temp == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 30, string.Format(Strings.Error30, "setup/backlog/deliverable"), Source);
                }
            }

            // pre req deliverables
            bool deliverablesOK = true;
            if (_preRequisiteDeliverables != "")
               deliverablesOK = _preRequisiteDeliverables
                    .Split(new char[] { '|', ',' })
                    .All(p => data.Setup.Backlog.Deliverables.Any(q => string.Compare(p, q.Name, true) == 0));

            if (!deliverablesOK)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 7, string.Format(Strings.Error61, this.PreRequisiteDeliverables, "setup/backlog/deliverable", this.Name), Source);
            }

            if (!string.IsNullOrEmpty(_earliestStartDateAsString))
            {
                DateTime? tempdate = _earliestStartDateAsString.ToSafeDate(data.Execute.DateFormat, null);
                if (tempdate == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 30, string.Format(Strings.Error30, "backlog/deliverable"), Source);
                }
                else
                {
                    _earliestStartDate = tempdate.Value;
                    _earliestStartDateValid = true;
                }
            }


            foreach (var cb in CustomBacklog)
                  success &= cb.Validate(data, errors);

            return success;
        }
    }


}
