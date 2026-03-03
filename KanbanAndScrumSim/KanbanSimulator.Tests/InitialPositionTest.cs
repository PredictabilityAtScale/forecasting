using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class InitialPositionTests
    {
        [TestMethod]
        public void InitialPositionTest()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.InitialPositionTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("10", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("10", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);

        }
    }
}
