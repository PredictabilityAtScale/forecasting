using FocusedObjective.Simulation.Scrum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FocusedObjective.Contract;
using System.Linq;
using System.Xml.Linq;

namespace KanbanSimulator.Tests
{
    
    
    /// <summary>
    ///This is a test class for ScrumSimulationTest and is intended
    ///to contain all ScrumSimulationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScrumSimulationTest
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


        string baseXML = @"<simulation name=""Scrum Test"" version=""1.0"" >
 <execute type=""Scrum"" limitIntervalsTo=""500"" decimalRounding=""3"">
  	<visual showVisualizer=""false"" />
 </execute>
 <setup>
 	<iteration storyPointsPerIterationLowBound=""1"" storyPointsPerIterationHighBound=""1""   />
 	<backlog type=""custom"">
 		<custom name=""small"" count=""10"" estimateLowBound=""1"" estimateHighBound=""1"" />
 	</backlog>

  	<blockingEvents>
   		<blockingEvent  occurrenceType=""stories"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1""
    					estimateLowBound=""1"" estimateHighBound=""1"">Block</blockingEvent>
  	</blockingEvents>

  	<defects>
   		<defect  occurrenceType=""stories"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" 
   				  estimateLowBound=""1"" estimateHighBound=""1"" >Bug</defect>
  	</defects>

  	<addedScopes>
            <addedScope  occurrenceType=""stories"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" 
            				  estimateLowBound=""1"" estimateHighBound=""1"">Added Scope</addedScope>
  	</addedScopes>

 </setup>
</simulation>
";

        [TestMethod()]
        public void RunSimulationMostBasic()
        {
            // 10 stories, size of 1 point, 1 point per iteration
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.BlockingEvents.Clear();
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            ScrumSimulation target = new ScrumSimulation(data); 
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 10 iterations + last iteration
            Assert.AreEqual(12, target.Iterations.Count);
            
            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);

            Assert.AreEqual(0, target.Iterations[11].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[11].CountStoriesInComplete);
        }

        [TestMethod()]
        public void RunSimulationCarryOverTimeTest()
        {
            // 10 stories, size of 1.5 point, 1 point per iteration
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.BlockingEvents.Clear();
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            // change the estimate size
            data.Setup.Backlog.CustomBacklog[0].EstimateLowBound = 1.5;
            data.Setup.Backlog.CustomBacklog[0].EstimateHighBound = 1.5;

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 15 iterations + last iteration
            Assert.AreEqual(17, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);

            Assert.AreEqual(0, target.Iterations[16].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[16].CountStoriesInComplete);
        }

        [TestMethod()]
        public void RunSimulationCarryUnderTimeTest()
        {
            // 10 stories, size of 1 point, 1.5 point per iteration
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.BlockingEvents.Clear();
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();
            
            // change the iteration points
            data.Setup.Iteration.StoryPointsPerIterationLowBound = 1.5;
            data.Setup.Iteration.StoryPointsPerIterationHighBound = 1.5;

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 7 iterations + last iteration
            Assert.AreEqual(9, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);

            Assert.AreEqual(0, target.Iterations[8].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[8].CountStoriesInComplete);
        }

        [TestMethod()]
        public void RunSimulationMostBasic_WithBlock()
        {
            // 10 stories, size of 1 point, 1 point per iteration
            // block on every 1 card, for 1 story point
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 20 iterations + last iteration
            Assert.AreEqual(22, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);
            Assert.AreEqual(0, target.Iterations[21].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[21].CountStoriesInComplete);

            // check completed are as expected
            Assert.AreEqual(10, target.AllStories.Sum(s => s.CompletedPointsHistory.Sum(c => c.Value)));
            Assert.AreEqual(10, target.AllStories.Sum(s => s.BlockedPointsHistory.Sum(c => c.Value)));
        }

        [TestMethod()]
        public void RunSimulationCarryOverTimeTest_WithBlock()
        {
            // 10 stories, size of 1.5 point, 1 point per iteration
            // block on every 1 card, for 1 story point

            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            // change the estimate size
            data.Setup.Backlog.CustomBacklog[0].EstimateLowBound = 1.5;
            data.Setup.Backlog.CustomBacklog[0].EstimateHighBound = 1.5;

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 25 iterations + last iteration
            Assert.AreEqual(27, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);
            Assert.AreEqual(0, target.Iterations[26].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[26].CountStoriesInComplete);

            // check completed are as expected
            Assert.AreEqual(15, target.AllStories.Sum(s => s.CompletedPointsHistory.Sum(c => c.Value)));
            Assert.AreEqual(10, target.AllStories.Sum(s => s.BlockedPointsHistory.Sum(c => c.Value)));
        }

        [TestMethod()]
        public void RunSimulationCarryUnderTimeTest_WithBlock()
        {
            // 10 stories, size of 1 point, 1.5 point per iteration
            // block on every 1 card, for 1 story point

            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            // change the iteration time
            data.Setup.Iteration.StoryPointsPerIterationLowBound = 1.5;
            data.Setup.Iteration.StoryPointsPerIterationHighBound = 1.5;

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 14 iterations + last iteration
            Assert.AreEqual(16, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);
            Assert.AreEqual(0, target.Iterations[15].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[15].CountStoriesInComplete);

            // check completed are as expected
            Assert.AreEqual(10, target.AllStories.Sum(s => s.CompletedPointsHistory.Sum(c => c.Value)));
            Assert.AreEqual(10, target.AllStories.Sum(s => s.BlockedPointsHistory.Sum(c => c.Value)));
        }

        [TestMethod()]
        public void RunSimulationMostBasic_WithMultipleBlock()
        {
            // 10 stories, size of 1 point, 1 point per iteration
            // block on every 1 card, for 1 story point
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.Defects.Clear();
            data.Setup.AddedScopes.Clear();

            // add a second blocking event
            data.Setup.BlockingEvents.Add( new SetupBlockingEventData {
                Scale = 1.0,
                OccurrenceLowBound = 1.0,
                OccurrenceHighBound = 1.0,
                EstimateLowBound = 1.0,
                EstimateHighBound = 1.0,
                Name = "Block2" });

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 30 iterations + last iteration
            Assert.AreEqual(32, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);
            Assert.AreEqual(0, target.Iterations[31].CountStoriesInBacklog);
            Assert.AreEqual(10, target.Iterations[31].CountStoriesInComplete);

            // check completed are as expected
            Assert.AreEqual(10, target.AllStories.Sum(s => s.CompletedPointsHistory.Sum(c => c.Value)));
            Assert.AreEqual(20, target.AllStories.Sum(s => s.BlockedPointsHistory.Sum(c => c.Value)));
        }

        [TestMethod()]
        public void RunSimulationMostBasic_DefectTest()
        {
            // 10 stories, size of 1 point, 1 point per iteration
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.BlockingEvents.Clear();
            data.Setup.AddedScopes.Clear();

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 20 iterations + last iteration
            Assert.AreEqual(22, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);

            Assert.AreEqual(0, target.Iterations[21].CountStoriesInBacklog);
            Assert.AreEqual(20, target.Iterations[21].CountStoriesInComplete);
        }

        [TestMethod()]
        public void RunSimulationMostBasic_AddedScopeTest()
        {
            // 10 stories, size of 1 point, 1 point per iteration
            SimulationData data = new SimulationData(XDocument.Parse(baseXML));

            // remove unwanted events for this test
            data.Setup.BlockingEvents.Clear();
            data.Setup.Defects.Clear();

            ScrumSimulation target = new ScrumSimulation(data);
            bool result = target.RunSimulation();
            Assert.AreEqual(true, result);

            // iteration 0 + 20 iterations + last iteration
            Assert.AreEqual(22, target.Iterations.Count);

            Assert.AreEqual(10, target.Iterations[0].CountStoriesInBacklog);
            Assert.AreEqual(0, target.Iterations[0].CountStoriesInComplete);

            Assert.AreEqual(0, target.Iterations[21].CountStoriesInBacklog);
            Assert.AreEqual(20, target.Iterations[21].CountStoriesInComplete);
        }

    }
}
