using FocusedObjective.Simulation.Kanban;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using FocusedObjective.Contract;
using System.IO;
using System.Reflection;

namespace KanbanSimulator.Tests
{
    /// <summary>
    ///This is a test class for KanbanSimulationTest and is intended
    ///to contain all KanbanSimulationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KanbanSimulationTest
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

        private XDocument basicSimDocument()
        {
            string xml = @"
                <simulation name=""Simple test"">
                  <execute>
                    <visual showVisualizer=""false"" />
                  </execute> 
                  <setup>
                    <backlog type=""simple"" simpleCount=""100"" shuffle=""false""/>
                    <columns>
                      <column id=""0"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 1</column>
                      <column id=""1"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 2</column>
                      <column id=""2"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 3</column>
                      <column id=""3"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 4</column>
                      <column id=""4"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 5</column>
                    </columns>
                  </setup>
                </simulation>";

            return XDocument.Parse(xml);
        }

        [TestMethod()]
        public void RunSimulation_MostBasicTest()
        {
            SimulationData data = new SimulationData(basicSimDocument());
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(0, result.EmptyPositions.Average);
            Assert.AreEqual(0, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, result.QueuedPositions.Average);
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(105, result.Intervals);

            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(5, item.CycleTime);
        }

        [TestMethod()]
        public void RunSimulation_BasicTimeCarryForwardTest()
        {
            string xml = @"
                <simulation name=""Simple test"">
                  <execute>
                    <visual showVisualizer=""false"" />
                  </execute> 
                  <setup>
                    <backlog type=""simple"" simpleCount=""100"" />
                    <columns>
                      <column id=""0"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 1</column>
                      <column id=""1"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 2</column>
                      <column id=""2"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 3</column>
                      <column id=""3"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 4</column>
                      <column id=""4"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 5</column>
                    </columns>
                  </setup>
                </simulation>";

            SimulationData data = new SimulationData(System.Xml.Linq.XDocument.Parse(xml));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(1, result.EmptyPositions.Average);
            Assert.AreEqual(1, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, result.QueuedPositions.Average);
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(207, result.Intervals);


            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(7.5, item.CycleTime);

        }

        [TestMethod()]
        public void RunSimulation_BasicWIPLimitTest()
        {
            string xml = @"
                <simulation name=""Simple test"">
                  <execute>
                    <visual showVisualizer=""false"" />
                  </execute> 
                  <setup>
                    <backlog type=""simple"" simpleCount=""100"" />
                    <columns>
                      <column id=""0"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""2"">Column 1</column>
                      <column id=""1"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""2"">Column 2</column>
                      <column id=""2"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""2"">Column 3</column>
                      <column id=""3"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""2"">Column 4</column>
                      <column id=""4"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""2"">Column 5</column>
                    </columns>
                  </setup>
                </simulation>";

            SimulationData data = new SimulationData(System.Xml.Linq.XDocument.Parse(xml));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(0, result.EmptyPositions.Average);
            Assert.AreEqual(0, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, result.QueuedPositions.Average);
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(55, result.Intervals);

            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(5, item.CycleTime);
        }

        [TestMethod()]
        public void RunSimulation_BasicEmptyTest()
        {
            string xml = @"
                <simulation name=""Simple test"">
                  <execute>
                    <visual showVisualizer=""false"" />
                  </execute> 
                  <setup>
                    <backlog type=""simple"" simpleCount=""100"" />
                    <columns>
                      <column id=""0"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 1</column>
                      <column id=""1"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 2</column>
                      <column id=""2"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 3</column>
                      <column id=""3"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 4</column>
                      <column id=""4"" estimateLowBound=""1.5"" estimateHighBound=""1.5"" wipLimit=""1"">Column 5</column>
                    </columns>
                  </setup>
                </simulation>";

            SimulationData data = new SimulationData(System.Xml.Linq.XDocument.Parse(xml));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);
            
            // a single empty position across all "mid" intervals
            Assert.AreEqual(1, result.EmptyPositions.Average);
            Assert.AreEqual(1, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, result.QueuedPositions.Average);
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(207, result.Intervals);

            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(7.5, item.CycleTime);
        }

        [TestMethod()]
        public void RunSimulation_BasicQueuedTest()
        {
            string xml = @"
                <simulation name=""Simple test"">
                  <execute>
                    <visual showVisualizer=""false"" />
                  </execute> 
                  <setup>
                    <backlog type=""simple"" simpleCount=""100"" />
                    <columns>
                      <column id=""0"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 1</column>
                      <column id=""1"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 2</column>
                      <column id=""2"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 3</column>
                      <column id=""3"" estimateLowBound=""1"" estimateHighBound=""1"" wipLimit=""1"">Column 4</column>
                      <column id=""4"" estimateLowBound=""2"" estimateHighBound=""2"" wipLimit=""1"">Column 5</column>
                    </columns>
                  </setup>
                </simulation>";

            SimulationData data = new SimulationData(System.Xml.Linq.XDocument.Parse(xml));
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.AreEqual(0, result.EmptyPositions.Average);
            Assert.AreEqual(0, result.EmptyPositions.Maximum);
            Assert.AreEqual(2, result.QueuedPositions.Average);
            Assert.AreEqual(4, result.QueuedPositions.Maximum);
            Assert.AreEqual(205, result.Intervals);

            Assert.AreEqual(6, target.CompletedWorkList.Min(i => i.CycleTime));
            Assert.AreEqual(10, target.CompletedWorkList.Max(i => i.CycleTime));
        }

        [TestMethod()]
        public void RunSimulation_BasicBlockingTest()
        {
            string xml = @"
                    <blockingEvents>
                        <blockingEvent columnId=""0"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 1</blockingEvent>
                        <blockingEvent columnId=""1"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 2</blockingEvent>
                        <blockingEvent columnId=""2"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 3</blockingEvent>
                        <blockingEvent columnId=""3"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 4</blockingEvent>
                        <blockingEvent columnId=""4"" scale=""1"" occurrenceLowBound=""1"" occurrenceHighBound=""1"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 5</blockingEvent>
                    </blockingEvents>";

            XDocument doc = basicSimDocument();
            doc.Element("simulation").Element("setup").Add(XElement.Parse(xml));

            SimulationData data = new SimulationData(doc);
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            // all cards should have been blocked at some stage
            Assert.AreEqual(100,
                target.CompletedWorkList.Count(c => c.StatusHistory.Values.Count(v => v == FocusedObjective.Simulation.Enums.CardStatusEnum.Blocked) > 0));

            Assert.AreEqual(0, Math.Round(result.EmptyPositions.Average, 3));
            Assert.AreEqual(0, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, Math.Round(result.QueuedPositions.Average, 3));
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(5, result.BlockedPositions.Maximum);
            Assert.AreEqual(209, result.Intervals);

            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(10, item.CycleTime);
        }

        [TestMethod()]
        public void RunSimulation_BasicBlockingTestOneInFive()
        {
            string xml = @"
                    <blockingEvents>
                        <blockingEvent columnId=""2"" scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""5"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 3</blockingEvent>
                    </blockingEvents>";

            XDocument doc = basicSimDocument();
            doc.Element("simulation").Element("setup").Add(XElement.Parse(xml));

            SimulationData data = new SimulationData(doc);
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);
            
            // one in five should mean 20 cards were blocked
            Assert.AreEqual(20,
                target.CompletedWorkList.Count(c => c.StatusHistory.Values.Count(v => v == FocusedObjective.Simulation.Enums.CardStatusEnum.Blocked) > 0));

            // some other stats to check
            Assert.AreEqual(1, result.BlockedPositions.Maximum);
            Assert.AreEqual(125, result.Intervals);
            Assert.AreEqual(42, target.CompletedWorkList.Where(c => c.CycleTime == 5).Count());
            Assert.AreEqual(58, target.CompletedWorkList.Where(c => c.CycleTime == 6).Count());
        }

        [TestMethod()]
        public void RunSimulation_ScaleBlockingTest()
        {
            string xml = @"
                    <blockingEvents>
                        <blockingEvent columnId=""2"" scale=""100"" occurrenceLowBound=""5"" occurrenceHighBound=""5"" estimateLowBound=""1"" estimateHighBound=""1"">Block column 3</blockingEvent>
                    </blockingEvents>";

            XDocument doc = basicSimDocument();
            doc.Element("simulation").Element("setup").Add(XElement.Parse(xml));

            SimulationData data = new SimulationData(doc);
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            // 5 in every 100 = one in twenty should mean 5 cards were blocked
            Assert.AreEqual(5,
                target.CompletedWorkList.Count(c => c.StatusHistory.Values.Count(v => v == FocusedObjective.Simulation.Enums.CardStatusEnum.Blocked) > 0));

            // some other stats to check
            Assert.AreEqual(1, result.BlockedPositions.Maximum);
            Assert.AreEqual(110, result.Intervals);
            Assert.AreEqual(87, target.CompletedWorkList.Where(c => c.CycleTime == 5).Count());
            Assert.AreEqual(13, target.CompletedWorkList.Where(c => c.CycleTime == 6).Count());
        }

        //todo: blocking event same column
        //todo: blocking event different lower and upper bound random test
        //todo: blocking event full attribute read from xml test

        [TestMethod()]
        public void RunSimulation_BasicAddedTest()
        {
            string xml = @"
                    <addedScopes>
                        <addedScope scale=""1"" occurrenceLowBound=""2"" occurrenceHighBound=""2"">Added {0}</addedScope>
                    </addedScopes>";

            XDocument doc = basicSimDocument();
            doc.Element("simulation").Element("setup").Add(XElement.Parse(xml));

            SimulationData data = new SimulationData(doc);
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            // 50 cards should be added
            Assert.AreEqual(50,
                target.CompletedWorkList.Count(c => c.CardType == FocusedObjective.Simulation.Enums.CardTypeEnum.AddedScope)
                );

            Assert.AreEqual(0, result.EmptyPositions.Average);
            Assert.AreEqual(0, result.EmptyPositions.Maximum);
            Assert.AreEqual(0, result.QueuedPositions.Average);
            Assert.AreEqual(0, result.QueuedPositions.Maximum);
            Assert.AreEqual(155, result.Intervals);

            Assert.AreEqual(150, target.CompletedWorkList.Count);

            foreach (var item in target.CompletedWorkList)
                Assert.AreEqual(5, item.CycleTime);
        }

        //todo: Added scope full xml test

        [TestMethod()]
        public void RunSimulation_BasicDefectTest()
        {
            string xml = @"
                    <defects>
                        <defect columnId=""2"" startsInColumnId=""1"" scale=""100"" occurrenceLowBound=""50"" occurrenceHighBound=""50"">Bug: id = {0}</defect>
                    </defects>";

            XDocument doc = basicSimDocument();
            doc.Element("simulation").Element("setup").Add(XElement.Parse(xml));

            SimulationData data = new SimulationData(doc);
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            // 50 defect cards should be added
            Assert.AreEqual(50,
                target.CompletedWorkList.Count(c => c.CardType == FocusedObjective.Simulation.Enums.CardTypeEnum.Defect)
                );

            Assert.AreEqual(155, result.Intervals);
            Assert.AreEqual(150, target.CompletedWorkList.Count);
            Assert.AreEqual(50, target.CompletedWorkList.Where(c => c.CycleTime == 4).Count());
            Assert.AreEqual(51, target.CompletedWorkList.Where(c => c.CycleTime == 5).Count());
            Assert.AreEqual(49, target.CompletedWorkList.Where(c => c.CycleTime == 6).Count());

            foreach (var item in target.CompletedWorkList.Where(c => c.CardType == FocusedObjective.Simulation.Enums.CardTypeEnum.Defect))
                Assert.AreEqual(4, item.CycleTime);
        }


        [TestMethod()]
        public void PullOrderTestFIFO()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.pullOrderTest.xml"));

            SimulationData data = new SimulationData(XDocument.Parse(reader.ReadToEnd().ToString()));
            KanbanSimulation target = new KanbanSimulation(data);

            // FIFO
            target.SimulationData.Execute.PullOrder = PullOrderEnum.FIFO;
            target.RunSimulation();

            Assert.AreEqual("G1 - 10", target.CompletedWorkList[0].Name);
            Assert.AreEqual("G1 - 11", target.CompletedWorkList[1].Name);
            Assert.AreEqual("G1 - 12", target.CompletedWorkList[2].Name);
            Assert.AreEqual("G1 - 13", target.CompletedWorkList[3].Name);
            Assert.AreEqual("G1 - 14", target.CompletedWorkList[4].Name);
            Assert.AreEqual("G2 - 15", target.CompletedWorkList[5].Name);
            Assert.AreEqual("G2 - 16", target.CompletedWorkList[6].Name);
            Assert.AreEqual("G2 - 17", target.CompletedWorkList[7].Name);
            Assert.AreEqual("G2 - 18", target.CompletedWorkList[8].Name);
            Assert.AreEqual("G2 - 19", target.CompletedWorkList[9].Name);
            Assert.AreEqual("G3 - 5", target.CompletedWorkList[10].Name);
            Assert.AreEqual("G3 - 6", target.CompletedWorkList[11].Name);
            Assert.AreEqual("G3 - 7", target.CompletedWorkList[12].Name);
            Assert.AreEqual("G3 - 8", target.CompletedWorkList[13].Name);
            Assert.AreEqual("G3 - 9", target.CompletedWorkList[14].Name);
            Assert.AreEqual("G4 - 0", target.CompletedWorkList[15].Name);
            Assert.AreEqual("G4 - 1", target.CompletedWorkList[16].Name);
            Assert.AreEqual("G4 - 2", target.CompletedWorkList[17].Name);
            Assert.AreEqual("G4 - 3", target.CompletedWorkList[18].Name);
            Assert.AreEqual("G4 - 4", target.CompletedWorkList[19].Name);

        }

        [TestMethod()]
        public void PullOrderTestFIFOStrict()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.pullOrderTest.xml"));

            SimulationData data = new SimulationData(XDocument.Parse(reader.ReadToEnd().ToString()));
            KanbanSimulation target = new KanbanSimulation(data);

            // FIFOStrict
            target.SimulationData.Execute.PullOrder = PullOrderEnum.FIFOStrict;
            target.RunSimulation();

            Assert.AreEqual("G1 - 10", target.CompletedWorkList[0].Name);
            Assert.AreEqual("G1 - 11", target.CompletedWorkList[1].Name);
            Assert.AreEqual("G1 - 12", target.CompletedWorkList[2].Name);
            Assert.AreEqual("G1 - 13", target.CompletedWorkList[3].Name);
            Assert.AreEqual("G1 - 14", target.CompletedWorkList[4].Name);
            Assert.AreEqual("G2 - 15", target.CompletedWorkList[5].Name);
            Assert.AreEqual("G2 - 16", target.CompletedWorkList[6].Name);
            Assert.AreEqual("G2 - 17", target.CompletedWorkList[7].Name);
            Assert.AreEqual("G2 - 18", target.CompletedWorkList[8].Name);
            Assert.AreEqual("G2 - 19", target.CompletedWorkList[9].Name);
            Assert.AreEqual("G3 - 5", target.CompletedWorkList[10].Name);
            Assert.AreEqual("G3 - 6", target.CompletedWorkList[11].Name);
            Assert.AreEqual("G3 - 7", target.CompletedWorkList[12].Name);
            Assert.AreEqual("G3 - 8", target.CompletedWorkList[13].Name);
            Assert.AreEqual("G3 - 9", target.CompletedWorkList[14].Name);
            Assert.AreEqual("G4 - 0", target.CompletedWorkList[15].Name);
            Assert.AreEqual("G4 - 1", target.CompletedWorkList[16].Name);
            Assert.AreEqual("G4 - 2", target.CompletedWorkList[17].Name);
            Assert.AreEqual("G4 - 3", target.CompletedWorkList[18].Name);
            Assert.AreEqual("G4 - 4", target.CompletedWorkList[19].Name);
        }

        [TestMethod()]
        public void PullOrderTestIndex()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.pullOrderTest.xml"));

            SimulationData data = new SimulationData(XDocument.Parse(reader.ReadToEnd().ToString()));
            KanbanSimulation target = new KanbanSimulation(data);

            // index
            target.SimulationData.Execute.PullOrder = PullOrderEnum.indexSequence;
            target.RunSimulation();

            // just need to check the last one...should always be last card in group 2
            Assert.AreEqual("G2 - 19", target.CompletedWorkList[19].Name);
        }


        [TestMethod()]
        public void PullOrderTestRandom()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.pullOrderTest.xml"));

            string model = reader.ReadToEnd().ToString();

            SimulationData data1 = new SimulationData(XDocument.Parse(model));
            SimulationData data2 = new SimulationData(XDocument.Parse(model));
            SimulationData data3 = new SimulationData(XDocument.Parse(model));
            KanbanSimulation target1 = new KanbanSimulation(data1);
            KanbanSimulation target2 = new KanbanSimulation(data2);
            KanbanSimulation target3 = new KanbanSimulation(data3);

            // random
            target1.SimulationData.Execute.PullOrder = PullOrderEnum.random;
            target1.RunSimulation();

            target2.SimulationData.Execute.PullOrder = PullOrderEnum.random;
            target2.RunSimulation();

            target3.SimulationData.Execute.PullOrder = PullOrderEnum.random;
            target3.RunSimulation();

            // just need to check the last one...
            //TODO:Can't get random results so close....
            //Assert.IsTrue(
            //    target1.CompletedWorkList[19].Name != target2.CompletedWorkList[19].Name || 
            //    target1.CompletedWorkList[19].Name != target3.CompletedWorkList[19].Name || 
            //    target2.CompletedWorkList[19].Name != target3.CompletedWorkList[19].Name);
        }

        [TestMethod()]
        public void PullOrderTestRandomAfterOrdering()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.pullOrderTest.xml"));

            string model = reader.ReadToEnd().ToString();

            SimulationData data1 = new SimulationData(XDocument.Parse(model));
            KanbanSimulation target1 = new KanbanSimulation(data1);

            // randomAfterOrdering
            target1.SimulationData.Execute.PullOrder = PullOrderEnum.randomAfterOrdering;
            target1.RunSimulation();

            Assert.IsTrue(target1.CompletedWorkList[0].Name.StartsWith("G1"));
            Assert.IsTrue(target1.CompletedWorkList[1].Name.StartsWith("G1"));
            Assert.IsTrue(target1.CompletedWorkList[2].Name.StartsWith("G1"));
            Assert.IsTrue(target1.CompletedWorkList[3].Name.StartsWith("G1"));
            Assert.IsTrue(target1.CompletedWorkList[4].Name.StartsWith("G1"));
            Assert.IsTrue(target1.CompletedWorkList[5].Name.StartsWith("G2"));
            Assert.IsTrue(target1.CompletedWorkList[6].Name.StartsWith("G2"));
            Assert.IsTrue(target1.CompletedWorkList[7].Name.StartsWith("G2"));
            Assert.IsTrue(target1.CompletedWorkList[8].Name.StartsWith("G2"));
            Assert.IsTrue(target1.CompletedWorkList[9].Name.StartsWith("G2"));
            Assert.IsTrue(target1.CompletedWorkList[10].Name.StartsWith("G3"));
            Assert.IsTrue(target1.CompletedWorkList[11].Name.StartsWith("G3"));
            Assert.IsTrue(target1.CompletedWorkList[12].Name.StartsWith("G3"));
            Assert.IsTrue(target1.CompletedWorkList[13].Name.StartsWith("G3"));
            Assert.IsTrue(target1.CompletedWorkList[14].Name.StartsWith("G3"));
            Assert.IsTrue(target1.CompletedWorkList[15].Name.StartsWith("G4"));
            Assert.IsTrue(target1.CompletedWorkList[16].Name.StartsWith("G4"));
            Assert.IsTrue(target1.CompletedWorkList[17].Name.StartsWith("G4"));
            Assert.IsTrue(target1.CompletedWorkList[18].Name.StartsWith("G4"));
            Assert.IsTrue(target1.CompletedWorkList[19].Name.StartsWith("G4"));
        }

        [TestMethod()]
        public void CumulativeFlowDataTest()
        {
            SimulationData data = new SimulationData(basicSimDocument());
            KanbanSimulation target = new KanbanSimulation(data);
            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            string s = KanbanSimulation.GetCumulativeFlowData(target.SimulationData.Setup.Columns, target.ResultTimeIntervals);

            Assert.IsFalse(string.IsNullOrWhiteSpace(s));
        }


        //todo: Defects sent into backlog
        //todo: Defects on card completion
        //todo: Defects with random occurrence range (different lower and upper range test)
        //todo: Defect full xml read test


    }
}
