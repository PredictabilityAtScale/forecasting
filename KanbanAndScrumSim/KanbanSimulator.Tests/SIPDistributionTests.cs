using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Kanban;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class SIPDistributionTests
    {
        [TestMethod]
        public void SimpleSIPTest()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.SIP.xml"));
            
            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("64", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("64", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

    }
}
