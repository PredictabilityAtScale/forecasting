using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public class ExecuteAddStaffColumnsData : ContractDataBase, IValidate
    {
        public ExecuteAddStaffColumnsData()
        {
        }

        public ExecuteAddStaffColumnsData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _id = 0;
        private int _minWip = 1;
        private int _maxWip = 50;
        private double _wipToStaffRatio = 2.0;
        private string _staffCategory = "";

        // public properties
        
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int MinWip
        {
            get { return _minWip; }
            set { _minWip = value; }
        }

        public int MaxWip
        {
            get { return _maxWip; }
            set { _maxWip = value; }
        }

        public double WipToStaffRatio
        {
            get { return _wipToStaffRatio; }
            set { _wipToStaffRatio = value; }
        }

        public string StaffCategory
        {
            get { return _staffCategory; }
            set { _staffCategory = value; }
        }
        
        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            result = result && ContractCommon.ReadAttributeIntValue(
                out _id,
                source,
                errors,
                "id",
                _id
                );

            result = result && ContractCommon.ReadAttributeIntValue(
                out _minWip,
                source,
                errors,
                "minWip",
                _minWip,
                false
                );

            result = result && ContractCommon.ReadAttributeIntValue(
                out _maxWip,
                source,
                errors,
                "maxWip",
                _maxWip
                ); 
            
            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _wipToStaffRatio,
                source,
                errors,
                "wipToStaffRatio",
                _wipToStaffRatio,
                false
                );

            _staffCategory = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "staffCategory",
                _staffCategory,
                false);

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("column");

            result.Add(new XAttribute("id", _id.ToString()));
            result.Add(new XAttribute("minWip", _minWip.ToString()));
            result.Add(new XAttribute("maxWip", _maxWip.ToString()));
            result.Add(new XAttribute("wipToStaffRatio", _wipToStaffRatio.ToString()));
            result.Add(new XAttribute("staffCategory", _staffCategory.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // id must match a column id
            if (data.Setup.Columns.Count(c => c.Id == this.Id) == 0)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 46, string.Format(Strings.Error46, Id), Source);
            }

            // must be > 0
            success &= ContractCommon.CheckValueGreaterThan(errors, MinWip, 0, "minWip", "execute/addStaff/column/minWip", Source);
            success &= ContractCommon.CheckValueGreaterThan(errors, MaxWip, 0, "maxWip", "execute/addStaff/column/maxWip", Source);

            // max must be greater than or equal too min bound
            if (MaxWip < MinWip)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 45, string.Format(Strings.Error45, Id), Source);
            }

            return success;
        }
    }


}
