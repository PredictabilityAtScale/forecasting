using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Kanban;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class InfiniteWIPTests
    {
        [TestMethod]
        public void InfiniteWIPLastColumn()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));
            
            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("32", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("32", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void InfiniteWIPLastColumnCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[2].CompleteInterval = 10;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(40, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPLastColumnBufferCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[2].CompleteInterval = 10;
            target.SimulationData.Setup.Columns[2].IsBuffer = true;

            // why do i need to do this in the test but not in the UI?
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 0.0;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 0.0;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(30, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPLastColumnReplenishInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[2].ReplenishInterval = 10;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);
            
            Assert.AreEqual(256, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPLastColumnReplenishIntervalBuffer()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[2].ReplenishInterval = 10;
            target.SimulationData.Setup.Columns[2].IsBuffer = true;
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 0.0;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 0.0;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(251, result.Intervals);
        }


        [TestMethod]
        public void InfiniteWIPMidColumn()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(128, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPMidColumnBuffer()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0; 
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[1].IsBuffer = true;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(127, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPMidColumnCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0;
            target.SimulationData.Setup.Columns[1].CompleteInterval = 10;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(255, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPMidColumnBufferCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[1].IsBuffer = true;
            target.SimulationData.Setup.Columns[1].CompleteInterval = 10;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(255, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPMidColumnReplenishInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0;
            target.SimulationData.Setup.Columns[1].ReplenishInterval = 5;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(132, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPMidColumnBufferReplenishInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[1].WipLimit = 0;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[1].IsBuffer = true;
            target.SimulationData.Setup.Columns[1].ReplenishInterval = 5;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(131, result.Intervals);
        }



        [TestMethod]
        public void InfiniteWIPFirstColumn()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(128, result.Intervals);
        }

        // buffer as first column ending with one more card in complete than started!
        [TestMethod]
        [Ignore]
        public void InfiniteWIPFirstColumnBuffer()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[0].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[0].IsBuffer = true;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(127, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPFirstColumnCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[0].CompleteInterval = 10;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(256, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPFirstColumnBufferCompleteInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[0].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[0].IsBuffer = true;
            target.SimulationData.Setup.Columns[0].CompleteInterval = 10;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(256, result.Intervals);
        }

        [TestMethod]
        public void InfiniteWIPFirstColumnReplenishInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[0].ReplenishInterval = 5;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(128, result.Intervals);
        }

        // buffer as first column ending with one more card in complete than started!
        [TestMethod]
        [Ignore]
        public void InfiniteWIPFirstColumnBufferReplenishInterval()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InfiniteWIPColumnTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 0;
            target.SimulationData.Setup.Columns[0].EstimateLowBound = 0;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 0;
            target.SimulationData.Setup.Columns[0].IsBuffer = true;
            target.SimulationData.Setup.Columns[0].ReplenishInterval = 5;

            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(127, result.Intervals);
        }

    }
}
