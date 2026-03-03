using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class ExecuteModelAuditData : ContractDataBase, IValidate
    {
        public ExecuteModelAuditData()
        {
        }

        public ExecuteModelAuditData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults

        // public properties

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("modelAudit");
            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            return success;
        }
    }
    

}
