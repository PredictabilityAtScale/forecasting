using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FocusedObjective.Contract;
using FocusedObjective.Simulation.Kanban;
using System.Xml.Linq;
using FocusedObjective.Simulation;

namespace KanbanSimulator.Tests
{
    /// <summary>
    /// Summary description for SensitivitySimulationTests
    /// </summary>
    [TestClass]
    public class SensitivitySimulationTests
    {
        public SensitivitySimulationTests()
        {

        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

private string baseXML = @"<simulation name=""Sensitivity Test"" version=""1.0"" >
	<execute limitIntervalsTo=""1000"" decimalRounding=""3"">
  		<sensitivity cycles=""2""  sensitivityType=""intervals"" occurrenceMultiplier=""2"" estimateMultiplier=""0.5"" sortOrder=""ascending"" /> 
        <visual showVisualizer=""false"" />
 	</execute>
  
 	<setup>
		<backlog type=""simple"" simpleCount=""50"" shuffle=""false""/>
  		<columns>
     			<column id=""1"" estimateLowBound=""10"" estimateHighBound=""10"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""10"" estimateHighBound=""10"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""10"" estimateHighBound=""10"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""10"" estimateHighBound=""10"" wipLimit=""1"">Column4</column>
     			<column id=""5"" estimateLowBound=""10"" estimateHighBound=""10"" wipLimit=""1"">Column5</column>
  		</columns>
  		<blockingEvents>
 			<blockingEvent columnId=""3"" scale=""1"" occurrenceLowBound=""10"" occurrenceHighBound=""10""  estimateLowBound=""10"" estimateHighBound=""10"">Block(10/10)</blockingEvent>
		</blockingEvents>
		<defects>
   			<defect columnId=""4"" startsInColumnId=""-1""  scale=""1"" occurrenceLowBound=""10"" occurrenceHighBound=""10"">Defect(10/10)</defect>
  		</defects>
 		<addedScopes>
 			<addedScope scale=""1"" occurrenceLowBound=""10"" occurrenceHighBound=""10"">AddedScope(10/10)</addedScope>
		</addedScopes>
    </setup>

</simulation>
";

        [TestMethod]
        public void SensitivityBasicTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(9, results.Element("tests").Elements("test").Count());

            int[] expectedIntervalDeltas = new int[] { -40, -40, -30, -30, -25, -5, -5, 0, 0 };
            string[] expectedNames = new string[] { "Defect(10/10)", "AddedScope(10/10)", "Block(10/10)", "Block(10/10)", "Column3", "Column1", "Column2", "Column4", "Column5" }; 
            
            int i = 0;
            foreach (var test in results.Element("tests").Elements("test"))
            {
                Assert.AreEqual(i, int.Parse(test.Attribute("index").Value));
                Assert.AreEqual(expectedIntervalDeltas[i], int.Parse(test.Attribute("intervalDelta").Value));
                Assert.AreEqual(expectedNames[i], test.Attribute("name").Value);
                i++;
            }
        }

        [TestMethod]
        public void SensitivityColumnEstimateTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);


            // no other artefacts this round.
            data.Setup.BlockingEvents.Clear();
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            // manually change each column and remember the total iterations
            int[] manualResults = new int[5];
            int col = 0;
            foreach (var column in data.Setup.Columns)
            {
                double originalLow = column.EstimateLowBound;
                double originalHigh = column.EstimateHighBound;

                column.EstimateLowBound = originalLow * 0.5;
                column.EstimateHighBound = originalHigh * 0.5;

                XElement single = FocusedObjective.Simulation.ExecuteVisualSimulation.AsXML(data);
                manualResults[col] = int.Parse(single.Element("statistics").Element("intervals").Attribute("value").Value);

                column.EstimateLowBound = originalLow;
                column.EstimateHighBound = originalHigh;

                col++;
            }

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(5, results.Element("tests").Elements("test").Count());
            Assert.AreEqual(manualResults[0], int.Parse(results.Element("tests").Elements("test").ElementAt(0).Element("statistics").Element("intervals").Attribute("average").Value));
            Assert.AreEqual(manualResults[1], int.Parse(results.Element("tests").Elements("test").ElementAt(1).Element("statistics").Element("intervals").Attribute("average").Value));
            Assert.AreEqual(manualResults[2], int.Parse(results.Element("tests").Elements("test").ElementAt(2).Element("statistics").Element("intervals").Attribute("average").Value));
            Assert.AreEqual(manualResults[3], int.Parse(results.Element("tests").Elements("test").ElementAt(3).Element("statistics").Element("intervals").Attribute("average").Value));
            Assert.AreEqual(manualResults[4], int.Parse(results.Element("tests").Elements("test").ElementAt(4).Element("statistics").Element("intervals").Attribute("average").Value));
        }

        [TestMethod]
        public void SensitivityDefectOccurrenceTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);

            // manually change defect and remember the total iterations
            SetupDefectData defect = data.Setup.Defects[0];
            double originalLow = defect.OccurrenceLowBound;
            double originalHigh = defect.OccurrenceHighBound;
            defect.OccurrenceLowBound = originalLow * 2;
            defect.OccurrenceHighBound = originalHigh * 2;

            XElement single = FocusedObjective.Simulation.ExecuteVisualSimulation.AsXML(data);
            
            int expectedIntervals = int.Parse(single.Element("statistics").Element("intervals").Attribute("value").Value);
            defect.OccurrenceLowBound = originalLow;
            defect.OccurrenceHighBound = originalHigh;

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(9, results.Element("tests").Elements("test").Count());
            Assert.AreEqual(expectedIntervals, int.Parse(results.Element("tests").Elements("test").ElementAt(0).Element("statistics").Element("intervals").Attribute("average").Value));
        }

        [TestMethod]
        public void SensitivityAddedScopeOccurrenceTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);

            // manually change addedScope and remember the total iterations
            SetupAddedScopeData addedScope = data.Setup.AddedScopes[0];
            double originalLow = addedScope.OccurrenceLowBound;
            double originalHigh = addedScope.OccurrenceHighBound;
            addedScope.OccurrenceLowBound = originalLow * 2;
            addedScope.OccurrenceHighBound = originalHigh * 2;

            XElement single = FocusedObjective.Simulation.ExecuteVisualSimulation.AsXML(data);

            int expectedIntervals = int.Parse(single.Element("statistics").Element("intervals").Attribute("value").Value);
            addedScope.OccurrenceLowBound = originalLow;
            addedScope.OccurrenceHighBound = originalHigh;

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(9, results.Element("tests").Elements("test").Count());
            Assert.AreEqual(expectedIntervals, int.Parse(results.Element("tests").Elements("test").ElementAt(1).Element("statistics").Element("intervals").Attribute("average").Value));
        }

        [TestMethod]
        public void SensitivityBlockOccurrenceTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);

            // manually change addedScope and remember the total iterations
            SetupBlockingEventData block = data.Setup.BlockingEvents[0];
            double originalLow = block.OccurrenceLowBound;
            double originalHigh = block.OccurrenceHighBound;
            block.OccurrenceLowBound = originalLow * 2;
            block.OccurrenceHighBound = originalHigh * 2;

            XElement single = FocusedObjective.Simulation.ExecuteVisualSimulation.AsXML(data);

            int expectedIntervals = int.Parse(single.Element("statistics").Element("intervals").Attribute("value").Value);
            block.OccurrenceLowBound = originalLow;
            block.OccurrenceHighBound = originalHigh;

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(9, results.Element("tests").Elements("test").Count());
            Assert.AreEqual(expectedIntervals, int.Parse(results.Element("tests").Elements("test").ElementAt(2).Element("statistics").Element("intervals").Attribute("average").Value));
        }

        [TestMethod]
        public void SensitivityBlockEstimateTest()
        {
            XDocument doc = XDocument.Parse(baseXML);
            SimulationData data = new SimulationData(doc);

            // manually change addedScope and remember the total iterations
            SetupBlockingEventData block = data.Setup.BlockingEvents[0];
            double originalLow = block.EstimateLowBound;
            double originalHigh = block.EstimateHighBound;
            block.EstimateLowBound = originalLow * 0.5;
            block.EstimateHighBound = originalHigh * 0.5;

            XElement single = FocusedObjective.Simulation.ExecuteVisualSimulation.AsXML(data);

            int expectedIntervals = int.Parse(single.Element("statistics").Element("intervals").Attribute("value").Value);
            block.EstimateLowBound = originalLow;
            block.EstimateHighBound = originalHigh;

            // perform sensitivity and check results
            XElement results = FocusedObjective.Simulation.ExecuteSensitivitySimulation.AsXML(data);

            Assert.AreEqual(9, results.Element("tests").Elements("test").Count());
            Assert.AreEqual(expectedIntervals, int.Parse(results.Element("tests").Elements("test").ElementAt(3).Element("statistics").Element("intervals").Attribute("average").Value));
        }
    }
}
