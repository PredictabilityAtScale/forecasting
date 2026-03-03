using VotingSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace VotingAndRanking.Tests
{
    
    
    /// <summary>
    ///This is a test class for VoteTallyTest and is intended
    ///to contain all VoteTallyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VoteTallyTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for InvalidVoteReasons
        ///</summary>
        [TestMethod()]
        public void InvalidVoteReasonsTest_CountInvalid()
        {
            List<string> actual;
            
            // count invalid
            VoteTally target = new VoteTally{ Count = 0, PreferanceOrder = "ABC"}; 
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("0 or negative"));

            target = new VoteTally { Count = -1, PreferanceOrder = "ABC" };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("0 or negative"));
        }

        [TestMethod()]
        public void InvalidVoteReasonsTest_EmptyPreferanceOrder()
        {
            List<string> actual;

            // no candidates
            VoteTally target = new VoteTally { Count = 1, PreferanceOrder = "" };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("no candidates"));

            target = new VoteTally { Count = 1, PreferanceOrder = null };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("no candidates"));

            target = new VoteTally { Count = 1, PreferanceOrder = ">>==>>==" };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("no candidates"));
        }

        [TestMethod()]
        public void InvalidVoteReasonsTest_DuplicateCandidates()
        {
            List<string> actual;

            // duplicate candidates
            VoteTally target = new VoteTally { Count = 1, PreferanceOrder = "ABCA" };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("appear more than once"));

            target = new VoteTally { Count = 1, PreferanceOrder = "ABCa" };
            actual = target.InvalidVoteReasons;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(true, actual[0].Contains("appear more than once"));
        }
        
        /// <summary>
        ///A test for Valid
        ///</summary>
        [TestMethod()]
        public void ValidTest()
        {
            VoteTally target = new VoteTally{ Count = -1, PreferanceOrder = "ABC"}; 
            bool actual;
            actual = target.Valid;
            Assert.IsFalse(actual);

            target.Count = 1;
            actual = target.Valid;
            Assert.IsTrue(actual);
            
        }
    }
}
