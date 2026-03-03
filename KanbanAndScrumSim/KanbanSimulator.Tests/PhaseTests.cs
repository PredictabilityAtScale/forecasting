using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Reflection;
using System.IO;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class PhaseTests
    {
        [TestMethod]
        public void SimpleKanbanPhaseTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BasicKanbanPhaseTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("149", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("7", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("149", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("7", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

        [TestMethod]
        public void ComplexKanbanPhaseTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ComplexKanbanPhaseTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("435", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("100", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("count").Attribute("average").Value);
            Assert.AreEqual("75", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("addedScope").Element("count").Attribute("average").Value);
            Assert.AreEqual("75", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
            Assert.AreEqual("0.377", sim.Result.Element("monteCarlo").Element("statistics").Element("blockedPositions").Attribute("average").Value);

            Assert.AreEqual("435", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("100", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Attribute("value").Value);
            Assert.AreEqual("75", sim.Result.Element("visual").Element("statistics").Element("cards").Element("addedScope").Attribute("value").Value);
            Assert.AreEqual("75", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);
            Assert.AreEqual("0.377", sim.Result.Element("visual").Element("statistics").Element("blockedPositions").Attribute("average").Value);
        }

        [TestMethod]
        public void KanbanPhaseEventTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PhasesEventTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("200", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("100", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("count").Attribute("average").Value);
            Assert.AreEqual("25", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("addedScope").Element("count").Attribute("average").Value);
            Assert.AreEqual("20", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
            Assert.AreEqual("0.265", sim.Result.Element("monteCarlo").Element("statistics").Element("blockedPositions").Attribute("average").Value);

            Assert.AreEqual("200", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("100", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Attribute("value").Value);
            Assert.AreEqual("25", sim.Result.Element("visual").Element("statistics").Element("cards").Element("addedScope").Attribute("value").Value);
            Assert.AreEqual("20", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);
            Assert.AreEqual("0.265", sim.Result.Element("visual").Element("statistics").Element("blockedPositions").Attribute("average").Value);
        }

        [TestMethod]
        public void SimpleScrumPhaseTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ScrumPhaseTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("22", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
            Assert.AreEqual("4.545", sim.Result.Element("monteCarlo").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("minimum").Value);
            Assert.AreEqual("4.545", sim.Result.Element("monteCarlo").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("average").Value);
            Assert.AreEqual("4.545", sim.Result.Element("monteCarlo").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("maximum").Value);

            Assert.AreEqual("22", sim.Result.Element("visual").Element("statistics").Element("iterations").Attribute("value").Value);
            Assert.AreEqual("4", sim.Result.Element("visual").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("minimum").Value);
            Assert.AreEqual("4.545", sim.Result.Element("visual").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("average").Value);
            Assert.AreEqual("5", sim.Result.Element("visual").Element("statistics").Element("pointsAllocatedPerIteration").Attribute("maximum").Value);
        }

        [TestMethod]
        public void ScrumPhaseEventTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ScrumPhasesEventTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("209", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
            Assert.AreEqual("100", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("count").Attribute("average").Value);
            Assert.AreEqual("20", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("addedScope").Element("count").Attribute("average").Value);
            Assert.AreEqual("39", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
            Assert.AreEqual("0.5", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("blockPointSize").Attribute("average").Value);

            Assert.AreEqual("209", sim.Result.Element("visual").Element("statistics").Element("iterations").Attribute("value").Value);
            Assert.AreEqual("100", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Attribute("value").Value);
            Assert.AreEqual("20", sim.Result.Element("visual").Element("statistics").Element("cards").Element("addedScope").Attribute("value").Value);
            Assert.AreEqual("39", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);
            Assert.AreEqual("0.5", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("blockPointSize").Attribute("average").Value);
        }

        [TestMethod]
        public void KanbanIntervalPhaseTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PhasesByInterval.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("32", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("32", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void ScrumIterationPhaseTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ScrumPhaseIterationTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("7", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
            Assert.AreEqual("7", sim.Result.Element("visual").Element("statistics").Element("iterations").Attribute("value").Value);
        }


        [TestMethod]
        public void KanbanIntervalPhaseCostPerDayNoOverrideTest()
        {
            // no phase override test.
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PhasesByInterval.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            // 32 days @ $2500 per day
            Assert.AreEqual("80000", sim.Result.Element("visual").Element("statistics").Element("totalCost").Attribute("value").Value);
        }

        [TestMethod]
        public void KanbanIntervalPhaseCostPerDayWithPhaseOverrideTest()
        {
            // no phase override test.
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PhasesByIntervalWithCostPerDay.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            // 26 days @ $2500 per day + 6 days @ $1000
            Assert.AreEqual("71000", sim.Result.Element("visual").Element("statistics").Element("totalCost").Attribute("value").Value);
        }
    }
}
