using FocusedObjective.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Troschuetz.Random;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace KanbanSimulator.Tests
{
    
    
    /// <summary>
    ///This is a test class for DistributionHelperTest and is intended
    ///to contain all DistributionHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DistributionHelperTest
    {

        /// <summary>
        ///A test for createFromSamplesDistribution
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FocusedObjective.Distributions.dll")]
        public void createFromSamplesDistributionKeepZeroTest()
        {
            DistributionData data = new DistributionData {
                Count = 100,

                ZeroHandling = ZeroHandlingEnum.Keep };

            List<double> result = new List<double>();
            FromSamplesDistribution dist = new FromSamplesDistribution();
            dist.Data = data;
            dist.Samples = "0,0,0,0,0,1,1,1,1,1";

            for (int i = 0; i < 100; i++)
                result.Add(dist.GetNextDoubleForDistribution());

            Assert.AreEqual(100, result.Count);
            Assert.AreEqual(true, result.Any(r => r == 0.0));
            Assert.AreEqual(true, result.Any(r => r == 1.0));
        }

        /// <summary>
        ///A test for createFromSamplesDistribution
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FocusedObjective.Distributions.dll")]
        public void createFromSamplesDistributionRemoveZeroTest()
        {
            DistributionData data = new DistributionData
            {
                Count = 100,
                ZeroHandling = ZeroHandlingEnum.Remove
            };


            List<double> result = new List<double>();

            FromSamplesDistribution dist = new FromSamplesDistribution();
            dist.Data = data;
            dist.Samples = "0,0,0,0,0,1,1,1,1,1";

            for (int i = 0; i < 100; i++)
                result.Add(dist.GetNextDoubleForDistribution());

            Assert.AreEqual(100, result.Count);
            Assert.AreEqual(false, result.Any(r => r == 0.0));
        }

        /// <summary>
        ///A test for createFromSamplesDistribution
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FocusedObjective.Distributions.dll")]
        public void createFromSamplesDistributionValueZeroTest()
        {
            DistributionData data = new DistributionData
            {
                Count = 100,
                ZeroHandling = ZeroHandlingEnum.Value,
                ZeroValue = 0.5
            };


            List<double> result = new List<double>();
            FromSamplesDistribution dist = new FromSamplesDistribution();
            dist.Samples = "0,0,0,0,0,1,1,1,1,1";

            dist.Data = data;
            for (int i = 0; i < 100; i++)
                result.Add(dist.GetNextDoubleForDistribution());

            Assert.AreEqual(100, result.Count);
            Assert.AreEqual(false, result.Any(r => r == 0.0));
            Assert.AreEqual(true, result.Any(r => r == 0.5));
            Assert.AreEqual(true, result.Any(r => r == 1.0));
        }
    
    }
}
