using VotingSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VotingAndRanking.Tests
{
    
    
    /// <summary>
    ///This is a test class for BordaCountingTest and is intended
    ///to contain all BordaCountingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BordaCountingTest
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
        ///A test for Results
        ///</summary>
        [TestMethod()]
        public void ResultsTest()
        {
            BordaCounting target = new BordaCounting();
            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 2, PreferanceOrder = "ABCD" },
                new VoteTally { Count = 2, PreferanceOrder = "BACD" } ,               
                new VoteTally { Count = 2, PreferanceOrder = "BDCA" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // 4 candidates should give 4 groups
            Assert.AreEqual(4, actual.Count());

            int i = 1;
            foreach (var g in actual)
            {
                switch (i)
                {
                    case 1:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(16, g.Key);
                        Assert.AreEqual('B', g.First().Candidate);
                        break;
                    case 2:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(10, g.Key);
                        Assert.AreEqual('A', g.First().Candidate);
                        break;
                    case 3:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(6, g.Key);
                        Assert.AreEqual('C', g.First().Candidate);
                        break;
                    case 4:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(4, g.Key);
                        Assert.AreEqual('D', g.First().Candidate);
                        break;
                    default: break;
                }

                i++;
            }

        }

        [TestMethod()]
        public void ResultsTest_TestRunExamples()
        {
            BordaCounting target = new BordaCounting();
            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 4, PreferanceOrder = "BACD" },
                new VoteTally { Count = 2, PreferanceOrder = "ACDB" } ,               
                new VoteTally { Count = 1, PreferanceOrder = "DCBA" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // 4 candidates should give 4 groups
            Assert.AreEqual(4, actual.Count());

            int i = 1;
            foreach (var g in actual)
            {
                switch (i)
                {
                    case 1:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(14, g.Key);
                        Assert.AreEqual('A', g.First().Candidate);
                        break;
                    case 2:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(13, g.Key);
                        Assert.AreEqual('B', g.First().Candidate);
                        break;
                    case 3:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(10, g.Key);
                        Assert.AreEqual('C', g.First().Candidate);
                        break;
                    case 4:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(5, g.Key);
                        Assert.AreEqual('D', g.First().Candidate);
                        break;
                    default: break;
                }

                i++;
            }

        }

        [TestMethod()]
        public void ResultsTest_TestRunExamples_RemovingCandidate()
        {
            BordaCounting target = new BordaCounting();
            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 4, PreferanceOrder = "BAC" },
                new VoteTally { Count = 2, PreferanceOrder = "ACB" } ,               
                new VoteTally { Count = 1, PreferanceOrder = "CBA" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // 3 candidates should give 3 groups
            Assert.AreEqual(3, actual.Count());

            int i = 1;
            foreach (var g in actual)
            {
                switch (i)
                {
                    case 1:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(9, g.Key);
                        Assert.AreEqual('B', g.First().Candidate);
                        break;
                    case 2:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(8, g.Key);
                        Assert.AreEqual('A', g.First().Candidate);
                        break;
                    case 3:
                        Assert.AreEqual(1, g.Count());
                        Assert.AreEqual(4, g.Key);
                        Assert.AreEqual('C', g.First().Candidate);
                        break;
                    default: break;
                }

                i++;
            }

        }


        [TestMethod()]
        public void ResultsTestForCondorcetParadoxCase()
        {
            BordaCounting target = new BordaCounting();

            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 7, PreferanceOrder = "BACD" },
                new VoteTally { Count = 1, PreferanceOrder = "ACDB" },               
                new VoteTally { Count = 2, PreferanceOrder = "CADB" },
                new VoteTally { Count = 2, PreferanceOrder = "DACB" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            Assert.AreEqual('A', actual.First().First().Candidate);
        }


        [TestMethod()]
        public void ResultsTestElectoaramaCase4()
        {
            BordaCounting target = new BordaCounting();
            string vs = @"3 ABCD|2 DABC|2 DBCA|2 CBDA|";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // a tie for first/second, and a tie for third/fourth in schulze - but not borda!
            Assert.IsTrue(CompareResults(actual, "B>D>A>C"));
        }

        private bool CompareResults(IEnumerable<IGrouping<int, VoteCandidateResult>> actual, string expected)
        {
            bool result = true;

            string[] expectedGroups = expected.Split(new char[] { '|', '>' });
            int index = 0;
            int resultIndex = 0;

            // check the number of groups match
            if (actual.Count() != expectedGroups.Length)
                return false;

            foreach (var group in actual)
            {
                foreach (var item in group)
                {
                    if (item.Candidate != expectedGroups[index][resultIndex])
                        return false;

                    resultIndex++;
                }

                index++;
                resultIndex = 0;
            }

            return result;

        }



    }
}
