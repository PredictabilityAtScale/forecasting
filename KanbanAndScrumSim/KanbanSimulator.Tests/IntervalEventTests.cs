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
    public class IntervalEventTests
    {
        [TestMethod]
        public void ReplenishFirstColumnTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ReplenishFirstColumnTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("25", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("25", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void ReplenishFirstColumnBufferTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ReplenishFirstColumnBufferTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("18", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("18", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void ReplenishColumnTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ReplenishColumnTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("48", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("48", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void ReplenishColumnBufferTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ReplenishColumnBufferTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("28", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("28", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }


        [TestMethod]
        public void CompleteLastColumnTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.CompleteLastColumnTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("45", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("45", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void CompleteLastColumnBufferTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.CompleteLastColumnBufferTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("45", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("45", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void CompleteColumnTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.CompleteColumnTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("46", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("46", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void CompleteColumnBufferTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.CompleteColumnBufferTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("46", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("46", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }
    }
}
