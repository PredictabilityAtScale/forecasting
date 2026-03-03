using FocusedObjective.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    [SimMLElement("backlog", "Contains the initial work to be simulated as deliverables or individual items.", true, HasMandatoryAttributes = false)]
    public class SetupBacklogData : ContractDataBase, IValidate
    {
        public SetupBacklogData()
        {
        }

        public SetupBacklogData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private BacklogTypeEnum _type = BacklogTypeEnum.Simple;
        private int _simpleCount = 100;
        private string _nameFormat = @"Story {0}";
        private bool _shuffle = true;
        private List<SetupBacklogCustomData> _customBacklog = new List<SetupBacklogCustomData>();
        private List<SetupBacklogDeliverableData> _deliverables = new List<SetupBacklogDeliverableData>();

        // public properties
        [SimMLAttribute("type", "There are two ways of specifying a backlog of work, custom or simple. \"simple\" allows an initial count of items to be placed in the backlog. \"custom\" allows multiple deliverables and custom entries to be defined. \"custom\" is the the most common type of backlog type.", false, ValidValues = "custom|simple")]
        public BacklogTypeEnum BacklogType
        {
            get { return _type; }
            set { _type = value; }
        }

        [SimMLAttribute("simpleCount", "Specifies the initial backlog size when the type=\"simple\". Used for initial testing of models and examples.", false)]
        public int SimpleCount
        {
            get { return _simpleCount; }
            set { _simpleCount = value; }
        }

        [SimMLAttribute("nameFormat", "Defines how the work item name appears visually during simulation. Use any text and the special placeholders {0} = running card index, {1} = the card name, {2} = the order of the custom backlog entry, {3} = the deliverable name, {4} = the deliverable order. Default is \"Story {0}\"", false)]
        public string NameFormat
        {
            get { return _nameFormat; }
            set { _nameFormat = value; }
        }

        [SimMLAttribute("shuffle", "Determines if the backlog is initially randomized (shuffled). If true, the backlog will be sorted in a random order for all entries with the SAME order attribute value.", false, ValidValues = "true|false")]
        public bool Shuffle
        {
            get { return _shuffle; }
            set { _shuffle = value; }
        }

        [SimMLElement("custom", "An individual custom backlog entry. These can be individual like this, or contained within a <deliverable>...</deliverable> element as groups.", false, HasMandatoryAttributes = true)]
        public List<SetupBacklogCustomData> CustomBacklog
        {
            get { return _customBacklog; }
        }

        [SimMLElement("deliverable", "A collection of <custom>...</custom> element(s). Deliverables can be simulated as groups and have specific skipping percentages to specify risk likelihood or intangibility.", false, HasMandatoryAttributes = true)]
        public List<SetupBacklogDeliverableData> Deliverables
        {
            get { return _deliverables; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            _type = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "type",
                "CUSTOM", BacklogTypeEnum.Custom,
                "SIMPLE", BacklogTypeEnum.Simple
                );

            success = success && ContractCommon.ReadAttributeIntValue(
                out _simpleCount,
                source,
                errors,
                "simpleCount",
                _simpleCount,
                false);

            _nameFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "nameFormat",
                _nameFormat,
                false);

            _shuffle = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "shuffle",
                "true", true,
                "false", false,
                "yes", true,
                "no", false);

            // add the custom backlog data
            foreach (XElement custom in source.Elements("custom"))
            {
                _customBacklog.Add(
                    new SetupBacklogCustomData(custom, errors));
            }

            // add the deliverable data
            foreach (XElement del in source.Elements("deliverable"))
            {
                _deliverables.Add(
                    new SetupBacklogDeliverableData(del, errors));
            }

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("backlog");

            result.Add(new XAttribute("type", _type.ToString().ToLower()));
    
            if (_type == BacklogTypeEnum.Simple)
                result.Add(new XAttribute("simpleCount", _simpleCount.ToString()));

            result.Add(new XAttribute("nameFormat", _nameFormat.ToString()));

            if (_shuffle)
                result.Add(new XAttribute("shuffle", "true"));
            else
                result.Add(new XAttribute("shuffle", "false"));

            foreach (var custom in _customBacklog)
                result.Add(custom.AsXML(simType));

            foreach (var deliverable in _deliverables)
                result.Add(deliverable.AsXML(simType));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (BacklogType == BacklogTypeEnum.Simple)
            {
                success &= ContractCommon.CheckValueGreaterThan(errors, SimpleCount, 0, "simpleCount", "setup/backlog", Source);
            }

            if (BacklogType == BacklogTypeEnum.Custom)
            {
                foreach (var cb in CustomBacklog)
                    success &= cb.Validate(data, errors);

                foreach (var d in Deliverables)
                    success &= d.Validate(data, errors);
            }
            
            return success;
        }
    }


}
