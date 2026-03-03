using FocusedObjective.Distributions.Markov;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KanbanSimulator.Tests
{

    /// <summary>
    ///This is a test class for MarkovChainTest and is intended
    ///to contain all MarkovChainTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MarkovChainTest
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


        /// <summary>
        ///A test for GenerateOccurrenceRatesBasedOnSample
        ///</summary>
        [TestMethod()]
        public void GenerateOccurrenceRatesBasedOnSampleTest()
        {
            List<int> samples = new List<int> { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
 
            int windowSize = 5; 
            int maximumGeneratedSamples = 1000;
            IEnumerable<int> actual;
            actual = MarkovChain.GenerateOccurrenceRatesBasedOnSample(samples, windowSize, maximumGeneratedSamples);

            List<int> expected = new List<int> { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            
            Assert.IsTrue(expected.SequenceEqual(actual.Take(16)));
            
        }

        [TestMethod()]
        public void GenerateOccurrenceRatesBasedOnSample6040Test()
        {
            List<int> samples = new List<int> { 1,1,1,2,1,1,1,2,1,1,1,2,1,1,1,2,1,2,1,2 };

            int windowSize = 2;
            int maximumGeneratedSamples = 1000;
            IEnumerable<int> actual;
            actual = MarkovChain.GenerateOccurrenceRatesBasedOnSample(samples, windowSize, maximumGeneratedSamples);

            int sample1 = actual.Count(i => i == 1);
            int sample2 = actual.Count(i => i == 2);

            Assert.IsTrue(sample1 > 600 && sample1 < 800, "wrong number of ones");
            Assert.IsTrue(sample2 > 200 && sample2 < 500, "wrong number of twos");


        }
    }
}
