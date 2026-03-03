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
    public class ColumnDistributionTests
    {
        [TestMethod]
        public void SimpleColumnDistributionTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ColumnDistributionTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("206", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("6.5", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("206", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("6.5", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

        [TestMethod]
        public void SimpleColumnOverrideDistributionTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ColumnOverrideDistributionTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("206", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("6.53", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("206", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("6.53", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

        [TestMethod]
        public void ComplexColumnOverrideWithinDeliverableTest()
        {
            /* tests :
             * column overrides within a deliverable (multiple)
             * both distribution and range based
             */

            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ColumnOverridesWithinDeliverables.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("64", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("14.5", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("64", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("14.5", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

        [TestMethod]
        public void ComplexColumnOverrideNoDeliverableTest()
        {
            /* tests :
             * column overrides NOT in deliverable - at the backlog level
             * both distribution and range based
             */

            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ColumnOverridesNoDeliverables.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("64", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("14.5", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("64", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("14.5", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }
    
    }
}
