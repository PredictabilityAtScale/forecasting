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
    [SimMLElement("actual", "Actual count of completed work to compare model tracking accuracy. Enter the count of items completed for specified dates.", false, HasMandatoryAttributes = true, ParentElement="actuals")]
    public class ForecastDateActualData : ContractDataBase, IValidate
    {
        public ForecastDateActualData()
        {
        }
        
        public ForecastDateActualData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _dateAsString = "";
        private string _annotation = "";
        private DateTime _date;
        private bool _dateValid = false;
        private double _count = 0.0;


        // public properties
        
        public string DateString
        {
            get { return _dateAsString; }
            set { _dateAsString = value; }
        }


        [SimMLAttribute("date", "The calendar date of this actual measurement. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD", true)]
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

        [SimMLAttribute("count", "Number of completed items as of the specified date.", true)]
        public double Count
        {
            get { return _count; }
            set { _count = value; }
        }

        
        [SimMLAttribute("annotation", "Text annotation of for this actual entry to display on charts.", false)]
        public string Annotation
        {
            get { return _annotation; }
            set { _annotation = value; }
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

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _count,
                source,
                errors,
                "count",
                _count,
                false
                ); 
            
            _annotation = source.Value;

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("actual",
                _annotation);

            result.Add(new XAttribute("date", _dateAsString));
            result.Add(new XAttribute("count", _count));

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
