using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FocusedObjective.Simulation;
using System.Linq;
using System.Collections.Generic;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class StatisticResultsTest
    {
        [TestMethod]
        public void StatisticsResults_IntegerTest()
        {
            StatisticResults<int> r = new StatisticResults<int>(Enumerable.Range(1, 10));

            Assert.AreEqual(10, r.Count);
            Assert.IsNotNull(r.AsXML("statistics"));
            Assert.AreEqual(5.5, r.Average);
            Assert.AreEqual(5.5, r.Median);

            // most basic test....

        }

        [TestMethod]
        public void StatisticsResults_DoubleTest()
        {
            StatisticResults<double> r = new StatisticResults<double>(new List<double> { 1.1, 2.2, 3.3, 4.4, 5.5 });

            Assert.AreEqual(5, r.Count);
            Assert.IsNotNull(r.AsXML("statistics"));
            Assert.AreEqual(3.3, r.Average);
            Assert.AreEqual(3.3, r.Median);

            // most basic test....

        }

        [TestMethod]
        public void StatisticsResultsSip_IntegerTest()
        {
            StatisticResults<int> test = new StatisticResults<int>(Enumerable.Range(1, 10));
            StatisticResults<int> r = new StatisticResults<int>(test.Sip);

            Assert.AreEqual(10, r.Count);
            Assert.IsNotNull(r.AsXML("statistics"));
            Assert.AreEqual(5.5, r.Average);
            Assert.AreEqual(5.5, r.Median);

            // most basic test....

        }

        [TestMethod]
        public void StatisticsResultsSip_DoubleTest()
        {
            StatisticResults<double> test = new StatisticResults<double>(new List<double> { 1.1, 2.2, 3.3, 4.4, 5.5 });
            StatisticResults<double> r = new StatisticResults<double>(test.Sip);

            Assert.AreEqual(5, r.Count);
            Assert.IsNotNull(r.AsXML("statistics"));
            Assert.AreEqual(3.3, r.Average);
            Assert.AreEqual(3.3, r.Median);

            // most basic test....

        }

    }
}
