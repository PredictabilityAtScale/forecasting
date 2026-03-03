using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class EmbedModelTests
    {
        [TestMethod]
        [Ignore] // TODO: Fix this test to include the file it's trying to parse
        public void EmbedModelTest()
        {

            // tests embedding the same model twice, one with two parameters set...
              // will fail if: parameter setting broken
              // loading files is broken

            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.ModelEmbedTest.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());
            Assert.AreEqual("34", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
        }
    }
}
