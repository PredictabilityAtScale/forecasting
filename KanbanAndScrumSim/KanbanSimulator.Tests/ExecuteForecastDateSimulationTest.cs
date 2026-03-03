using FocusedObjective.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FocusedObjective.Contract;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Linq;

namespace KanbanSimulator.Tests
{
    
    
    /// <summary>
    ///This is a test class for ExecuteForecastDateSimulationTest and is intended
    ///to contain all ExecuteForecastDateSimulationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ExecuteForecastDateSimulationTest
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
        ///A test for ByDeliverables_AsXML
        ///</summary>
        [TestMethod()]
        public void ByDeliverables_AsXMLTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.Permutations_deliverables.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            var perms = sim.Result.Element("forecastDatePermutations").Elements("permutation");

            Assert.AreEqual(3, perms.Count());
            Assert.AreEqual("20", perms.ElementAt(0).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("20", perms.ElementAt(1).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("40", perms.ElementAt(2).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
        }

        /// <summary>
        ///A test for BySequentialBacklog_AsXML
        ///</summary>
        [TestMethod()]
        public void BySequentialBacklog_AsXMLTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.Permutations_sequentialBacklog.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            var perms = sim.Result.Element("forecastDatePermutations").Elements("permutation");

            Assert.AreEqual(4, perms.Count());
            Assert.AreEqual("10", perms.ElementAt(0).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("20", perms.ElementAt(1).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("30", perms.ElementAt(2).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("40", perms.ElementAt(3).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
        }

        /// <summary>
        ///A test for BySequentialDeliverables_AsXML
        ///</summary>
        [TestMethod()]
        public void BySequentialDeliverables_AsXMLTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.Permutations_sequentialDeliverables.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            var perms = sim.Result.Element("forecastDatePermutations").Elements("permutation");

            Assert.AreEqual(2, perms.Count());
            Assert.AreEqual("20", perms.ElementAt(0).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
            Assert.AreEqual("40", perms.ElementAt(1).Element("forecastDate").Element("dates").Element("date").Attribute("workDays").Value);
        }
    }
}
