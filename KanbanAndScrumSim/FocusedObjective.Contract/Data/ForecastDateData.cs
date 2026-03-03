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
    [SimMLElement("forecastDate", "Contains details about forecasting. Not required for visual simulation, but needed whenever a forecast of a date is performed.", false)]
    public class ForecastDateData : ContractDataBase, IValidate
    {
        public ForecastDateData()
        {
        }
        
        public ForecastDateData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private string _startDate = DateTime.Now.ToString();
        private int _intervalsToOneDay = 1;
        private string _workDays = "monday,tuesday,wednesday,thursday,friday";
        private double _costPerDay = 0.0;
        private int _workDaysPerIteration = 10;


        List<ForecastDateExcludeData> _excludes = new List<ForecastDateExcludeData>();
        List<ForecastDateActualData> _actuals = new List<ForecastDateActualData>();

        private string _targetDate;
        private double _revenue = 0.0;
        private TimeUnitEnum _revenueUnit = TimeUnitEnum.Month;
        private double _targetLikelihood = 85.0;

        //List cost - name category amount unit (blank/hour/day/week/month/quarter/year)


        // public properties
        [SimMLAttribute("startDate", "The first day the project will start. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD", true)]
        public string StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        [SimMLAttribute("intervalsToOneDay", "The number of simulation steps for one calendar day. All estimates defined in other attributes will be in this unit. Default is 1 meaning each simulation step is one full day. Use 8 (for example) to simulate hours.", false)]
        public int IntervalsToOneDay
        {
            get { return _intervalsToOneDay; }
            set { _intervalsToOneDay = value; }
        }

        [SimMLAttribute("workDays", "Comma seperated list of the days of the week that work will occur. Used to skip weekends as working days when calculating a forecast. Default is monday,tuesday,wednesday,thursday,friday.", false)]
        public string WorkDays
        {
            get { return _workDays; }
            set { _workDays = value; }
        }

        [SimMLAttribute("costPerDay", "The amount per working day used when computing total cost. This can be any unit (dollars or Euro for example). Set the formatting used in the <execute currencyFormat=... attribute. Default is 0.", false)]
        public double CostPerDay
        {
            get { return _costPerDay; }
            set { _costPerDay = value; }
        }

        [SimMLAttribute("workDaysPerIteration", "The number of work days per iteration (Scrum only). Default is 10.", false)]
        public int WorkDaysPerIteration
        {
            get { return _workDaysPerIteration; }
            set { _workDaysPerIteration = value; }
        }

        [SimMLAttribute("targetDate", "The target completion date used to calculate the cost of delay if specified. Enter the date in the format specified in the <execute dateformat=... attribute. Default format is YYYYMMDD", false)]
        public string TargetDate
        {
            get { return _targetDate; }
            set { _targetDate = value; }
        }

        [SimMLAttribute("targetLikelihood", "The desired level of probability to represent the date used as a forecast. Monte Carlo simulation returns many dates, this value specifies how much certainty is desired when returning a single date value as a result. Default is 85%.", false)]
        public double TargetLikelihood
        {
            get { return _targetLikelihood; }
            set { _targetLikelihood = value; }
        }

        [SimMLAttribute("revenue", "The expected revenue for a period expected once this project hits the target date. This amount is specified per revenueUnit (default one month). Set the formatting used for currency in the <execute currencyFormat=... attribute. Default is month.", false)]
        public double Revenue
        {
            get { return _revenue; }
            set { _revenue = value; }
        }

        [SimMLAttribute("revenueUnit", "The time period the revenue attribute is specified for (default is month). Valid values are month,day,week or year. Default is month.", false, ValidValues="month|day|week|year")]
        public TimeUnitEnum RevenueUnit
        {
            get { return _revenueUnit; }
            set { _revenueUnit = value; }
        }


        [SimMLElement("excludes", "Contains any excluded dates (public holidays for example) entries. These dates will be skipped as work days when calculating a forecasted date.", false, HasMandatoryAttributes = false, HasAnyAttributes=false)]
        public List<ForecastDateExcludeData> Excludes
        {
            get { return _excludes; }
        }

        public List<DateTime> ExcludedDates
        {
            get { return _excludes.Select(e => e.Date).ToList();  }
        }

        [SimMLElement("actuals", "Contains any actual date and progress entries. These values get included in the progress chart to compare if actuals are tracking the forecast outcomes.", false, HasMandatoryAttributes = false, HasAnyAttributes = false)]
        public List<ForecastDateActualData> Actuals
        {
            get { return _actuals; }
        }

        // methods
        protected bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _startDate = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "startDate",
                string.Empty);

            _targetDate = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "targetDate",
                string.Empty,
                false);

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _targetLikelihood,
                source,
                errors,
                "targetLikelihood",
                _targetLikelihood,
                false);

            result = result && ContractCommon.ReadAttributeIntValue(
                    out _intervalsToOneDay,
                    source,
                    errors,
                    "intervalsToOneDay",
                    _intervalsToOneDay,
                    false
                    );

            _workDays = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "workDays",
                _workDays,
                false);

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _costPerDay,
                source,
                errors,
                "costPerDay",
                _costPerDay,
                false);

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _revenue,
                source,
                errors,
                "revenue",
                _revenue,
                false);

            _revenueUnit = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "revenueUnit",
                "month", TimeUnitEnum.Month,
                "months", TimeUnitEnum.Month, 
                "day", TimeUnitEnum.Day,
                "days", TimeUnitEnum.Day,
                "week", TimeUnitEnum.Week,
                "weeks", TimeUnitEnum.Week,
                "year", TimeUnitEnum.Year,
                "years", TimeUnitEnum.Year);

            result = result && ContractCommon.ReadAttributeIntValue(
                out _workDaysPerIteration,
                source,
                errors,
                "workDaysPerIteration",
                _workDaysPerIteration,
                false);

            // add the exclude elements (both in and not in an includes section
            XElement excludes = source.Element("excludes");
            if (excludes != null)
            {
                foreach (XElement col in excludes.Elements("exclude"))
                {
                    _excludes.Add(
                        new ForecastDateExcludeData(col, errors));
                }
            }

            // excludes may not be in an excludes element, they might be individual
            foreach (XElement col in source.Elements("exclude"))
            {
                _excludes.Add(
                    new ForecastDateExcludeData(col, errors));
            }

            // add the actual elements (both in and not in an actuals section
            XElement actuals = source.Element("actuals");
            if (actuals != null)
            {
                foreach (XElement col in actuals.Elements("actual"))
                {
                    _actuals.Add(
                        new ForecastDateActualData(col, errors));
                }
            }

            // actual's may not be in an actuals element, they might be individual
            foreach (XElement col in source.Elements("actual"))
            {
                _actuals.Add(
                    new ForecastDateActualData(col, errors));
            }

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("forecastDate");

            result.Add(new XAttribute("startDate", _startDate.ToString()));
            
            if (simType == SimulationTypeEnum.Kanban)
                result.Add(new XAttribute("intervalsToOneDay", _intervalsToOneDay.ToString()));
            else
                result.Add(new XAttribute("workDaysPerIteration", _workDaysPerIteration.ToString()));

            result.Add(new XAttribute("workDays", _workDays.ToString()));
            result.Add(new XAttribute("costPerDay", _costPerDay.ToString()));
            
            if (!string.IsNullOrWhiteSpace(_targetDate))
                result.Add(new XAttribute("targetDate", _targetDate.ToString()));

            result.Add(new XAttribute("targetLikelihood", _targetLikelihood.ToString()));

            result.Add(new XAttribute("revenue", _revenue.ToString()));
            result.Add(new XAttribute("revenueUnit", _revenueUnit.ToString()));

            if (_excludes.Any())
            {
                XElement excludes = new XElement("excludes");

                foreach (var exclude in _excludes)
                    excludes.Add(exclude.AsXML(simType));
                
                result.Add(excludes);
            }

            if (_actuals.Any())
            {
                XElement actuals = new XElement("actuals");

                foreach (var actual in _actuals)
                    actuals.Add(actual.AsXML(simType));

                result.Add(actuals);
            }

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            success &= ContractCommon.CheckValueGreaterThan(errors, WorkDaysPerIteration, 0, "workDaysPerIteration", "execute/forecastDate", Source) && success;

            success &= ContractCommon.CheckValueGreaterThan(errors, TargetLikelihood, 0, "targetLikelihood", "execute/forecastDate", Source) && success;
            success &= ContractCommon.CheckValueLessThan(errors, TargetLikelihood, 100, "targetLikelihood", "execute/forecastDate", Source) && success;


            if (string.IsNullOrEmpty(_workDays.Trim()))
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 31, Strings.Error31, Source);
            }

            string[] allowedDays = new string[] { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
            string[] days = _workDays.Split(new char[] {',', '|'});
            if (!days.All(d => allowedDays.Contains(d.Trim().ToLower())))
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 32, Strings.Error32, Source);
            }


            if (_startDate.ToSafeDate(data.Execute.DateFormat, null) == null)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 30, string.Format(Strings.Error30, "forecastDate (startDate)"), Source);
            }

            if (!string.IsNullOrWhiteSpace(_targetDate) && _targetDate.ToSafeDate(data.Execute.DateFormat, null) == null)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 30, string.Format(Strings.Error30, "forecastDate (targetDate)"), Source);
            }

            foreach (var exclude in _excludes)
                success &= exclude.Validate(data, errors);

            foreach (var actual in _actuals)
                success &= actual.Validate(data, errors);

            return success;
        }

    }


}
