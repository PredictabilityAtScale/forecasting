using KanbanSimulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using FocusedObjective.Simulation.Extensions;

namespace KanbanSimulator.Tests
{
    
    
    /// <summary>
    ///This is a test class for StatisticExtensionsTest and is intended
    ///to contain all StatisticExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StatisticExtensionsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void HistogramSegmentation()
        {
            var testData = Enumerable.Range(0, 100);

            var histogram = testData.Histogram(20);

            Assert.AreEqual(5, histogram.ElementAt(0).Value);
            Assert.AreEqual(4.95, histogram.ElementAt(0).Key);

            Assert.AreEqual(5, histogram.ElementAt(19).Value);
            Assert.AreEqual(99, histogram.ElementAt(19).Key);
        }

        [TestMethod()]
        public void HistogramSegmentation2()
        {
            // checking the key is up to and including
            // bit of a brainteaser (for me at least...)

            var testData = Enumerable.Range(1, 3);

            var histogram = testData.Histogram(3);

            Assert.AreEqual(1, histogram.ElementAt(0).Value);
            Assert.AreEqual(1.0, histogram.ElementAt(0).Key);

            Assert.AreEqual(1, histogram.ElementAt(1).Value);
            Assert.AreEqual(2.0, histogram.ElementAt(1).Key);

            Assert.AreEqual(1, histogram.ElementAt(2).Value);
            Assert.AreEqual(3.0, histogram.ElementAt(2).Key);
        }

        [TestMethod()]
        public void MedianTest()
        {
            var testData = new double[] { 1, 2, 2, 3, 4, 7, 9 };
            Assert.AreEqual(3.0, testData.Median());
        }

        [TestMethod()]
        public void MeanTest()
        {
            var testData = new double[] { 1, 2, 2, 3, 4, 7, 9 };
            Assert.AreEqual(4.0, testData.Average());
        }

        [TestMethod()]
        public void ModeTest()
        {
            var testData = new double[] { 1, 2, 2, 3, 4, 7, 9 };
            Assert.AreEqual(2.0, testData.Mode().First());
        }

        [TestMethod()]
        public void Test_Quartile()
        {
            double[] x = { 1, 2, 4, 7, 8, 9, 10, 12 };

            Assert.AreEqual(3.5, x.Percentile(25), 0.00001);
            Assert.AreEqual(7.5, x.Percentile(50), 0.00001);
            Assert.AreEqual(9.25, x.Percentile(75), 0.00001);
            Assert.AreEqual(5.75, x.Percentile(75) - x.Percentile(25), 0.00001);
        }

        [TestMethod()]
        public void Test_Percentile()
        {
            double[] x = { 1, 3, 2, 4 };

            Assert.AreEqual(1.9, x.Percentile(30), 0.00001);
        }

        [TestMethod()]
        public void Test_Percentile2()
        {
            double[] x = { 8,1,23,4};

            Assert.AreEqual(3.7, x.Percentile(30), 0.00001);
        }


        [TestMethod()]
        public void Test_SummaryPercentiles()
        {
            double[] x = { 1, 2, 4, 7, 8, 9, 10, 12 };

            double[] results = x.SummaryPercentiles();

            Assert.AreEqual(1.0, results[0], 0.00001); //min
            Assert.AreEqual(1.35, results[1], 0.00001);  //5
            Assert.AreEqual(3.5, results[2], 0.00001);  //25
            Assert.AreEqual(7.5, results[3], 0.00001); //50
            Assert.AreEqual(9.25, results[4], 0.00001); //75
            Assert.AreEqual(11.3, results[5], 0.00001); //95
            Assert.AreEqual(12.0, results[6], 0.00001); //max

            Assert.AreEqual(x.Median(), results[3]); 

        }

        [TestMethod()]
        public void Test_SummaryStatistics()
        {
            var testData = new double[] { 1, 2, 2, 3, 4, 7, 9 };
            double[] results = testData.SummaryStatistics();            

            Assert.AreEqual(4.0, testData.Average());

            Assert.AreEqual(7.0, results[0], 0.00001);  //count
            Assert.AreEqual(28, results[1], 0.00001);   //sum5
            Assert.AreEqual(4.0, results[2], 0.00001);  //average
            Assert.AreEqual(testData.SampleStandardDeviation(), Math.Sqrt(results[3]), 0.00001); //samples sd
            Assert.AreEqual(testData.PopulationStandardDeviation(), Math.Sqrt(results[4]), 0.00001); //population sd
        }

    }
}
