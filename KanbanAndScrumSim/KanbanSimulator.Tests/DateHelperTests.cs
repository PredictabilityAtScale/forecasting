using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FocusedObjective.Common;
using System.Collections.Generic;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class DateHelperTests
    {
        [TestMethod]
        public void GetWorkDaysForIntervalCountTest()
        {
            Assert.AreEqual(1, DateHelpers.GetWorkDaysForIntervalCount(1, 1));
            Assert.AreEqual(1, DateHelpers.GetWorkDaysForIntervalCount(2, 1));
            Assert.AreEqual(10, DateHelpers.GetWorkDaysForIntervalCount(1, 10));
        }

        [TestMethod]
        public void GetWorkDaysForIterationCountTest()
        {
            Assert.AreEqual(1, DateHelpers.GetWorkDaysForIterationCount(1, 1));
            Assert.AreEqual(20, DateHelpers.GetWorkDaysForIterationCount(10, 2));
            Assert.AreEqual(20, DateHelpers.GetWorkDaysForIterationCount(2, 10));
        }

        [TestMethod]
        public void GetDateByWorkDaysTest()
        {
            // return the start date
            Assert.AreEqual(new DateTime(2016, 1, 1), DateHelpers.GetDateByWorkDays(0, "dd-MMM-yyyy", "01-Jan-2016"));
            Assert.AreEqual(new DateTime(2016, 1, 1), DateHelpers.GetDateByWorkDays(1, "dd-MMM-yyyy", "01-Jan-2016", "")); // no workdays

            // 1 jan is a friday on friday...
            Assert.AreEqual(new DateTime(2016, 1, 1), DateHelpers.GetDateByWorkDays(1, "dd-MMM-yyyy", "01-Jan-2016"));

            // 2 jan is sat, 3 jan is sun, 4th next work day
            Assert.AreEqual(new DateTime(2016, 1, 4), DateHelpers.GetDateByWorkDays(2, "dd-MMM-yyyy", "01-Jan-2016"));

            // override default work days
            Assert.AreEqual(new DateTime(2016, 1, 2), DateHelpers.GetDateByWorkDays(2, "dd-MMM-yyyy", "01-Jan-2016", "monday,tuesday,wednesday,thursday,friday,saturday,sunday"));

            // add an exclude date and weekends excluded
            Assert.AreEqual(new DateTime(2016, 1, 5), DateHelpers.GetDateByWorkDays(2, "dd-MMM-yyyy", "01-Jan-2016", "monday,tuesday,wednesday,thursday,friday", new List<DateTime> { new DateTime(2016, 1, 4) }));
        }

        [TestMethod]
        public void GetLatestStartDateTest()
        {
            // return the start date
            Assert.AreEqual(new DateTime(2016, 1, 1), DateHelpers.GetLatestStartDate(0, "dd-MMM-yyyy", "01-Jan-2016"));
            Assert.AreEqual(new DateTime(2016, 1, 1), DateHelpers.GetLatestStartDate(1, "dd-MMM-yyyy", "01-Jan-2016", "")); // no workdays

            // 1 jan is a friday 
            Assert.AreEqual(new DateTime(2015, 12, 31), DateHelpers.GetLatestStartDate(2, "dd-MMM-yyyy", "01-Jan-2016"));

            // monday before the week before.... 
            Assert.AreEqual(new DateTime(2015, 12, 25), DateHelpers.GetLatestStartDate(6, "dd-MMM-yyyy", "01-Jan-2016"));
        }

        [TestMethod]
        public void GetCostOfDelayTest()
        {
            Assert.AreEqual(0.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "01-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Day));

            // negative COD - delivered EARLY
            Assert.AreEqual(-31.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "01-Feb-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Day));
            Assert.AreEqual(-4.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "29-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Week));
            Assert.AreEqual(-1.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "01-Feb-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Month));
            Assert.AreEqual(-1.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "01-Jan-2017", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Year));

            // compound delays - these are approximate for low value items over long durations...always within a day though....
            Assert.AreEqual(-79.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "20-Mar-2016", "dd-MMM-yyyy", 7.0, TimeUnitEnum.Week));
            Assert.AreEqual(-78.387, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "20-Mar-2016", "dd-MMM-yyyy", 30.0, TimeUnitEnum.Month), 0.1);
            Assert.AreEqual(-365.0 - 79, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 1), "20-Mar-2017", "dd-MMM-yyyy", 365.0, TimeUnitEnum.Year), 1.0);

            // positive COD - delivered LATE
            Assert.AreEqual(31.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 2, 1), "01-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Day));
            Assert.AreEqual(4.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 1, 29), "01-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Week));
            Assert.AreEqual(1.0, DateHelpers.GetCostOfDelay(new DateTime(2016, 2, 1), "01-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Month));
            Assert.AreEqual(1.0, DateHelpers.GetCostOfDelay(new DateTime(2017, 1, 1), "01-Jan-2016", "dd-MMM-yyyy", 1.0, TimeUnitEnum.Year));
        }

        [TestMethod]
        public void GetDelayInDaysTest()
        {
            Assert.AreEqual(0, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 1), "01-Jan-2016", "dd-MMM-yyyy"));

            // before target
            Assert.AreEqual(-31, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 1), "01-Feb-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(-28, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 1), "29-Jan-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(-31, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 1), "01-Feb-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(-366, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 1), "01-Jan-2017", "dd-MMM-yyyy"));

            // after target
            Assert.AreEqual(31, DateHelpers.GetDelayInDays(new DateTime(2016, 2, 1), "01-Jan-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(28, DateHelpers.GetDelayInDays(new DateTime(2016, 1, 29), "01-Jan-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(31, DateHelpers.GetDelayInDays(new DateTime(2016, 2, 1), "01-Jan-2016", "dd-MMM-yyyy"));
            Assert.AreEqual(366, DateHelpers.GetDelayInDays(new DateTime(2017, 1, 1), "01-Jan-2016", "dd-MMM-yyyy"));

        }


    }
}
    
