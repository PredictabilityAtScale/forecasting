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
    /// Summary description for CustomBacklogTests
    /// </summary>
    [TestClass]
    public class CustomBacklogTests
    {
        public CustomBacklogTests()
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

private string baseXML = @"<simulation name=""Custom Backlog Test"" version=""1.0"" >
	<execute limitIntervalsTo=""1000"" decimalRounding=""3"">
  		<visual showVisualizer=""false"" />
 	</execute>
   	<setup>
		<backlog type=""custom"" simpleCount=""10"" shuffle=""false"" >
		   	<custom name=""small""  count=""10"" percentageLowBound=""0""   percentageHighBound=""0"" />
			<custom name=""medium"" count=""10"" percentageLowBound=""50""  percentageHighBound=""50"" />
			<custom name=""large""  count=""10"" percentageLowBound=""100"" percentageHighBound=""100"" />
		</backlog>
  		<columns>
     			<column id=""1"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column4</column>
     			<column id=""5"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column5</column>
  		</columns>
 	</setup>
</simulation>";

        [TestMethod]
        public void CustomBacklogBasicTest()
        {
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(211, result.Intervals);
            Assert.AreEqual(10, result.WorkCycleTime.Histogram[5]);
            Assert.AreEqual(10, result.WorkCycleTime.Histogram[27.5]);
            Assert.AreEqual(10, result.WorkCycleTime.Histogram[50]);
        }

        private string baseColumnXML = @"<simulation name=""Custom Backlog Test"" version=""1.0"" >
	<execute limitIntervalsTo=""1000"" decimalRounding=""3"">
  		<visual showVisualizer=""false"" />
 	</execute>
   	<setup>
		<backlog type=""custom""  shuffle=""false"">
		
			<!-- override the first two columns, the inherit the last three -->
		   	<custom name=""small"" count=""10""  percentageLowBound=""0"" percentageHighBound=""0"" > 
		   		<column id=""1"" estimateLowBound=""1"" estimateHighBound=""1"" />
		   		<column id=""2"" estimateLowBound=""1"" estimateHighBound=""1"" />
		   	</custom>
			
			<custom name=""medium""  count=""11"">
		   		<column id=""1"" estimateLowBound=""5"" estimateHighBound=""5"" />
		   		<column id=""2"" estimateLowBound=""5"" estimateHighBound=""5"" />
		   		<column id=""3"" estimateLowBound=""5"" estimateHighBound=""5"" />
		   		<column id=""4"" estimateLowBound=""5"" estimateHighBound=""5"" />
		   		<column id=""5"" estimateLowBound=""5"" estimateHighBound=""5"" />
   			</custom>
			
			<!-- should inherit from the original columns -->
			<custom name=""large"" count=""12"" percentageLowBound=""100"" percentageHighBound=""100"" /> 

		</backlog>
  		
  		<columns>
     			<column id=""1"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column4</column>
     			<column id=""5"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column5</column>
  		</columns>
  		
 	</setup>
</simulation>
";

        [TestMethod]
        public void CustomBacklogColumnTest()
        {
            SimulationData data = new SimulationData(XDocument.Parse(baseColumnXML));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(226, result.Intervals);
            Assert.AreEqual(10, result.WorkCycleTime.Histogram[5]);
            Assert.AreEqual(11, result.WorkCycleTime.Histogram[25]);
            Assert.AreEqual(12, result.WorkCycleTime.Histogram[50]);
        }

    
    }
}
