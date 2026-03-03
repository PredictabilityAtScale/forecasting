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
    public class DeliverableTests
    {
        [TestMethod]
        public void DeliverablePreReqsTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.prereqtest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("32", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("3", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("32", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("3", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

        [TestMethod]
        public void DeliverableEarliestStartDateTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.earlieststarttest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("103", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("3", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);

            Assert.AreEqual("103", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("3", sim.Result.Element("visual").Element("statistics").Element("cards").Element("work").Element("cycleTime").Attribute("average").Value);
        }

    }
}
