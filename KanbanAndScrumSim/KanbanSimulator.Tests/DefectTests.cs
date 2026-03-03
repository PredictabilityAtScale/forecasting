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
    public class DefectTests
    {
        [TestMethod]
        public void DefectKanbanDistribution()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectKanbanBase.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("155", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("50", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);

            Assert.AreEqual("155", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("50", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
        }

        [TestMethod]
        public void DefectKanbanColumnAndDistribution()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectKanbanColumn.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("36", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("10", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);

            Assert.AreEqual("36", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("10", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
        }

        [Ignore]
        [TestMethod]
        public void DefectKanbanColumnLowerCOSOrderTest()
        {
            //TODO:Code fails at the moment. Defects lower COS order than cards, but defects start first
            // they should start after all cards completed. If this is the case, intervals should be lower because work cards in first two columns not blocked.
            // problem in implementation. Fill, fills positions in a column with defects before cards from an earlier column get a chance to be completed and fill positions.

            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectKanbanColumnLowerCOSOrderTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("36", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
            Assert.AreEqual("10", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);

            Assert.AreEqual("36", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("10", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
        }

        [TestMethod]
        public void DefectScrumDistribution()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectScrumBase.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("150", sim.Result.Element("visual").Element("statistics").Element("iterations").Attribute("value").Value);
            Assert.AreEqual("50", sim.Result.Element("visual").Element("statistics").Element("cards").Element("defect").Attribute("value").Value);

            Assert.AreEqual("150", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
            Assert.AreEqual("50", sim.Result.Element("monteCarlo").Element("statistics").Element("cards").Element("defect").Element("count").Attribute("average").Value);
        }

        [TestMethod]
        public void DefectTargeting()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectTargets.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("27", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
        }

        [TestMethod]
        public void DefectTargetingNoDeliverable()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectTargetsNoDeliverable.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            // should still find the custom backlog by name

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("27", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
        }

        [TestMethod]
        public void DefectScrumTargeting()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.DefectScrumTarget.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("42", sim.Result.Element("monteCarlo").Element("statistics").Element("iterations").Attribute("average").Value);
        }
    }
}
