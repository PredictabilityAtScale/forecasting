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
    [SimMLElement("exclude", "Specific non-work dates to exclude when forecasting.", false, HasMandatoryAttributes = true, ParentElement = "excludes")]
    public class ForecastDateExcludeData : ContractDataBase, IValidate
    {
        public ForecastDateExcludeData()
        {
        }
        
        public ForecastDateExcludeData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _dateAsString = "";
        private string _name = "";
        private DateTime _date;
        private bool _dateValid = false;

        // public properties
        
        public string DateString
        {
            get { return _dateAsString; }
            set { _dateAsString = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("date", "The calendar date to exclude. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD", true)]
        public DateTime Date
        {
            get
            {
                if (_dateValid)
                    return _date;
                else
                    return DateTime.MinValue;
            }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _dateAsString = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "date",
                string.Empty);

            _name = source.Value;

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("exclude",
                _name);

            result.Add(new XAttribute("date", _dateAsString));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            
            DateTime? temp = _dateAsString.ToSafeDate(data.Execute.DateFormat, null);
            if (temp == null)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 30, string.Format(Strings.Error30, "forecastDate/exclude"), Source);
            }
            else
            {
                _date = temp.Value;
                _dateValid = true;
            }

            return success;
        }

    }


}
