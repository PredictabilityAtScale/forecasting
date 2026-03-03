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
    public class BlockingEventTests
    {
        [TestMethod]
        public void BlockingEventKanbanDistribution()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingEventKanbanBase.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("205", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("0.5", sim.Result.Element("monteCarlo").Element("statistics").Element("blockedPositions").Attribute("average").Value);

            Assert.AreEqual("205", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("0.5", sim.Result.Element("visual").Element("statistics").Element("blockedPositions").Attribute("average").Value);
        }

        [TestMethod]
        public void BlockingEventScrumDistribution()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingEventScrumBase.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("110", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
            Assert.AreEqual("1", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("blockPointSize").Attribute("average").Value);

            Assert.AreEqual("110", sim.Result.Element("visual").Element("statistics").Element("iterations").Attribute("value").Value);
            Assert.AreEqual("0|2", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("blockPointSize").Attribute("mode").Value);
        
        }

        [TestMethod]
        public void BlockingEventKanbanMultipleColumnDefect()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingEventKanbanMultipleColumnDefect.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("13", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("1.333", sim.Result.Element("monteCarlo").Element("statistics").Element("blockedPositions").Attribute("average").Value);

            Assert.AreEqual("13", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("1.333", sim.Result.Element("visual").Element("statistics").Element("blockedPositions").Attribute("average").Value);
        }

        [TestMethod]
        public void BlockingEventTargeting()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingTargets.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("33", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
        }

        [TestMethod]
        public void BlockingEventTargetingNoDeliverable()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingTargetsNoDeliverable.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            // should still find the custom backlog by name

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("33", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
        }

        [TestMethod]
        public void BlockingEventScrumTargeting()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.BlockingScrumTarget.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("42", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
        }

    }
}
