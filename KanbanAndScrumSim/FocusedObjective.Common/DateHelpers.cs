using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Common;

namespace FocusedObjective.Common
{
    public static class DateHelpers
    {
        public static int GetWorkDaysForIntervalCount(int intervalsToOneDay, int intervals)
        {
            return (int)Math.Ceiling((double)intervals / (double)intervalsToOneDay);
        }

        public static int GetWorkDaysForIterationCount(int workDaysPerIteration, int iterations)
        {
            return workDaysPerIteration * iterations;
        }

        public static DateTime GetDateByWorkDays(int days, string dateFormat, string startDateString, string workDaysString = "monday,tuesday,wednesday,thursday,friday" , List<DateTime> excludesList = null)
        {
            // this has been validated before we execute in the ExecuteForecastDateData class in Contracts.
            DateTime startDate = startDateString.ToSafeDate(dateFormat, null).Value;

            DateTime result = new DateTime(startDate.Year, startDate.Month, startDate.Day);

            bool forward = days >= 0;
            days = Math.Abs(days);

            if (days > 0 && !string.IsNullOrWhiteSpace(workDaysString))
            {
                string[] workdays =
                    workDaysString.Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower()).ToArray();

                // now find the calendar date
                while (days > 0)
                {
                    if (workdays.Contains(result.DayOfWeek.ToString().ToLower())
                        && (excludesList == null || excludesList.Count(d => d.Date == result.Date) == 0))
                    {
                        days--;
                    }

                    if (days > 0)
                    {
                        if (forward)
                            result = result.AddDays(1);
                        else
                            result = result.AddDays(-1);
                    }
                }
            }

            return result;
        }

        public static DateTime GetLatestStartDate(int delayInDays, string dateFormat, string startDateString, string workDaysString = "monday,tuesday,wednesday,thursday,friday", List<DateTime> excludesList = null)
        {
            return GetDateByWorkDays((delayInDays * -1), dateFormat, startDateString, workDaysString, excludesList);
        }

        public static double GetCostByWorkDays(int workDays, double costPerDay)
        {
            double result = 0.0;

            if (workDays > 0)
                result = (workDays * 1.0) * costPerDay;

            return result;
        }

        public static int GetDelayInDays(DateTime date, string targetDateTime, string dateFormat)
        {
            int result = 0;

            DateTime? target = targetDateTime.ToSafeDate(dateFormat, null);
            if (target != null)
                result = (int)Math.Ceiling(date.Subtract(target.Value).TotalDays);

            return result;
        }



        public static double GetCostOfDelay(DateTime date, string targetDateTime, string dateFormat, double revenue, TimeUnitEnum revenueUnit)
        {
            double result = 0.0;

            DateTime? target = targetDateTime.ToSafeDate(dateFormat, null);
            if (target != null)
            {
                DateTimeSpan diff = DateTimeSpan.CompareDates(date, target.Value);
                double polarity = date < target.Value ? -1.0 : 1.0;

                switch (revenueUnit)
                {
                    case TimeUnitEnum.Day:
                        result = (DateHelpers.GetDelayInDays(date, targetDateTime, dateFormat) * 1.0) * revenue;
                        break;

                    case TimeUnitEnum.Week:
                        result = (DateHelpers.GetDelayInDays(date, targetDateTime, dateFormat) * 1.0) * (revenue / 7.0);
                        break;

                    case TimeUnitEnum.Month:
                        result = (diff.Months * 1.0) * revenue;
                        result += (diff.Days * 1.0) * (revenue / (DateTime.DaysInMonth(date.Year, date.Month)));
                        result = result * polarity;
                        break;

                    case TimeUnitEnum.Year:
                        result = (diff.Years * 1.0) * revenue;
                        result += (diff.Months * 1.0) * (revenue / 12);
                        result += (diff.Days * 1.0) * (revenue / (DateTime.IsLeapYear(date.Year) ? 366.0 : 365.0));
                        result = result * polarity;
                        break;
                    default:
                        break;
                }

                
            }

            return result;
        }

    }
}
