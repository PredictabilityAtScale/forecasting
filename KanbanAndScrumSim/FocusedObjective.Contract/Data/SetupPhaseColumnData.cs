using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("column", "Column overrides to use when this phase is active.", false, ParentElement = "phase", ParentParentElement="phases")]
    public class SetupPhaseColumnData : ContractDataBase
    {
        public SetupPhaseColumnData()
        {
        }

        public SetupPhaseColumnData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _columnId = -1;
        private int _wipLimit = 0;

        // public properties
        [SimMLAttribute("id", "Column id to override default values when this phase is active (kanban only).", true)]
        public int ColumnId
        {
            get { return _columnId; }
            set { _columnId = value; }
        }

        [SimMLAttribute("wipLimit", "The WIP limit to apply when this phase is active (kanban only). Often used to model team growth and decline throughout different parts of a project.", true)]
        public int WipLimit
        {
            get { return _wipLimit; }
            set { _wipLimit = value; }
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
           
            success = success && ContractCommon.ReadAttributeIntValue(
                out _wipLimit,
                source,
                errors,
                "wipLimit",
                _wipLimit);
            
            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("column");

            result.Add(new XAttribute("id", _columnId.ToString()));
            result.Add(new XAttribute("wipLimit", _wipLimit.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // id must match a column id
            if (data.Setup.Columns.Count(c => c.Id == this.ColumnId) == 0)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 24, string.Format(Strings.Error24, ColumnId), Source);
            }

            // > 0
            success &= ContractCommon.CheckValueGreaterThan(errors, WipLimit, 0, "wipLimit", "setup/phases/phase: " + ColumnId, Source);

            return success;
        }
    }


}
