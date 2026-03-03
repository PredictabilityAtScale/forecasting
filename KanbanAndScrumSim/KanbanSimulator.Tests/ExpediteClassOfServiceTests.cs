using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using FocusedObjective.Contract;
using FocusedObjective.Simulation.Kanban;

namespace KanbanSimulator.Tests
{

    /*
- only block if at wip limit
- stay blocked while this condition exists.


tests -

1. cycle times of 1 with blocking at wip limit done
2. cycle times at 3 with blocking at wip limit done
3. 1 not at wip limit done
4. 2 not at wip limit done
     * 
5. infinit wip columns are always not at wiplimit
6. no more expedites than maximumallowedonboard allows
7. cards blocked by multiple expedites
      
*/



    [TestClass]
    public class OverrideWipExpediteTests
    {
        [TestMethod]
        public void SimpleExpediteTest()
        {
            // 3 columns,
            // cycle times of 1,
            // wip limits at 1.
            // 1 expedite, 1 normal card.
            // = 5 intervals = initial + 4 

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            FocusedObjective.Simulation.Simulator sim = new FocusedObjective.Simulation.Simulator(reader.ReadToEnd());

            Assert.IsTrue(sim.Execute());

            Assert.AreEqual("5", sim.Result.Element("monteCarlo").Element("statistics").Element("intervals").Attribute("average").Value);
            Assert.AreEqual("5", sim.Result.Element("visual").Element("statistics").Element("intervals").Attribute("value").Value);
        }

        [TestMethod]
        public void ExpediteCycleTimeThreeTest()
        {
            // 12 days 13 intervals
            // (3 + 3) + 3 + 3 days = 12 + 1 interval

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 3;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(13, result.Intervals);
        }

        [TestMethod]
        public void ExpediteCycleTimeOneLessThanWIPTest()
        {
            //// 1 + 1 + 1 days = 3 + 1 interval

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].WipLimit = 2;
            target.SimulationData.Setup.Columns[1].WipLimit = 2;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(4, result.Intervals);
        }

        [TestMethod]
        public void ExpediteCycleTimeThreeLessThanWIPTest()
        {
            //// 3 + 3 + 3 days = 9 + 1 interval

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.Columns[0].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[0].WipLimit = 2;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[1].WipLimit = 2;
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(10, result.Intervals);
        }

        [TestMethod]
        public void ExpediteCycleTimeThreeAtWIPWithMultipleStandardTest()
        {
            //// (3 + 3) + 3 + 3 days = 12 + 1 interval

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            // 2 cards for second custom
            target.SimulationData.Setup.Backlog.CustomBacklog[1].Count = 2;

            target.SimulationData.Setup.Columns[0].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[0].WipLimit = 2;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[1].WipLimit = 2;
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[2].WipLimit = 2;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(13, result.Intervals);
        }

        [TestMethod]
        public void ExpediteCycleTimeThreeMultipleExpediteTest()
        {
            // 12 days 13 intervals
            // (3 + 3) + 3 + 3 days = 12 + 1 interval

            StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "KanbanSimulator.Tests.SimMLFiles.expedite.xml"));

            SimulationData data = new SimulationData(
                System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            KanbanSimulation target = new KanbanSimulation(data);

            target.SimulationData.Setup.ClassOfServices[0].MaximumAllowedOnBoard = 2;
            target.SimulationData.Setup.Backlog.CustomBacklog[0].Count = 2;

            target.SimulationData.Setup.Columns[0].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[0].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[1].EstimateHighBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateLowBound = 3;
            target.SimulationData.Setup.Columns[2].EstimateHighBound = 3;

            SimulationResultSummary result = null;
            if (target.RunSimulation())
                result = new SimulationResultSummary(target);

            Assert.IsTrue(target.RunSimulation());
            Assert.AreEqual(13, result.Intervals);
        }
    
    }
}
