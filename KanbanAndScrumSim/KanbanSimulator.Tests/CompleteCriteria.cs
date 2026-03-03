using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Kanban;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class UnitTest2
    {


        [TestMethod]
        public void BacklogRemainingCompletePercentageTest()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.BasicKanbanThreeColumnsModel.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Execute.CompletePercentage = 50.0;
            target.SimulationData.Execute.ActivePositionsCompletePercentage = 100.0;

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(28, result.Intervals);
        }

        [TestMethod]
        public void ActivePositionsCompletePercentageTest()
        {
            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.CompleteByActivePercentageTest.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            Assert.IsTrue(target.RunSimulation());
            SimulationResultSummary result = new SimulationResultSummary(target);

            Assert.AreEqual(30, result.Intervals);
        }
    }
}
