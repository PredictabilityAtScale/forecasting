using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public class LicenseData : ContractDataBase, IValidate
    {
        public LicenseData()
        {
        }

        public LicenseData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _licenseTerms = "";
        private string _signature = "";
        
        // public properties
        public string LicenseTerms
        {
            get { return _licenseTerms; }
            set { _licenseTerms = value; }
        }

        public string Signature
        {
            get { return _signature; }
            set { _signature = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            XElement terms = source.Element("licenseTerms");

            if (terms != null)
                _licenseTerms = terms.Value;

            XElement sig = source.Element("signature");

            if (sig != null)
                _signature = sig.Value;
           
            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("license");

            result.Add(new XElement("licenseTerms", _licenseTerms));
            result.Add(new XElement("signature", _signature));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            return success;
        }
    }


}
