using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using FocusedObjective.Contract;

namespace KanbanSimulator.Tests
{
    [TestClass]
    public class PartitionerTests
    {
        [TestMethod]
        public void ForecastDatePartitionerDefaultSizeTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PartitionForecastDateTest.xml"));

            SimulationData model = new SimulationData(System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            // forecastDate with 123 cycles using 100 cycle partitions (the default)
            PartitionModel partitioner = new PartitionModel(model);

            Assert.AreEqual(1, partitioner.Partitions.Count);

            Assert.AreEqual(100, partitioner.Partitions[0].Execute.ForecastDate.Cycles);
            

        }

        [TestMethod]
        public void ForecastDatePartitionerCustomPartitionSizeTest()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "KanbanSimulator.Tests.SimMLFiles.PartitionForecastDateTest.xml"));

            SimulationData model = new SimulationData(System.Xml.Linq.XDocument.Parse(reader.ReadToEnd()));

            // forecastDate with 123 cycles partitioned by 10
            PartitionModel partitioner = new PartitionModel(model, 10, false);

            Assert.AreEqual(13, partitioner.Partitions.Count);

            Assert.AreEqual(10, partitioner.Partitions[0].Execute.ForecastDate.Cycles);

        }
    }
}
