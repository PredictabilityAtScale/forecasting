using VotingSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace VotingAndRanking.Tests
{
    
    
    /// <summary>
    ///This is a test class for VotingBaseTest and is intended
    ///to contain all VotingBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VotingBaseTest
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
            VotingBase target = new VotingBase();
            
            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 5, PreferanceOrder = "ABCD" },
                new VoteTally { Count = 5, PreferanceOrder = "BACD" } ,               
                new VoteTally { Count = 6, PreferanceOrder = "BDCA" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // two candidates were voted first
            Assert.AreEqual(2, actual.Count());
            
            int i = 1;
            foreach (var g in actual)
            {
                if (i == 1)
                {
                    Assert.AreEqual(11, g.Key);
                    Assert.AreEqual('B', g.First().Candidate);
                    Assert.AreEqual(11, g.First().Score);
                }
                else
                {
                    Assert.AreEqual(5, g.Key);
                    Assert.AreEqual('A', g.First().Candidate);
                    Assert.AreEqual(5, g.First().Score);
                }

                i++;
            }
        }


        [TestMethod()]
        public void ResultsTestForCondorcetParadoxCase()
        {
            VotingBase target = new VotingBase();

            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 7, PreferanceOrder = "BACD" },
                new VoteTally { Count = 1, PreferanceOrder = "ACDB" },               
                new VoteTally { Count = 2, PreferanceOrder = "CADB" },
                new VoteTally { Count = 2, PreferanceOrder = "DACB" }
            };

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            Assert.AreEqual('B', actual.First().First().Candidate);
        }

        /// <summary>
        ///A test for GetPairWins
        ///</summary>
        [TestMethod()]
        public void GetPairWinsTest()
        {
            VotingBase target = new VotingBase();

            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 3, PreferanceOrder = "ABCD" },
                new VoteTally { Count = 4, PreferanceOrder = "BCDA" } ,               
                new VoteTally { Count = 2, PreferanceOrder = "CDAB" },
                new VoteTally { Count = 1, PreferanceOrder = "DCBA" },
                new VoteTally { Count = 1, PreferanceOrder = "CABD" }
            };

            int[,] expected = new int[,] { 
                    { 0, 6, 3, 4 }, 
                    { 5, 0, 7, 8 }, 
                    { 8, 4, 0, 10 }, 
                    { 7, 3, 1, 0 } };

            int[,] actual;
            actual = target.GetPairWins(votes);

            Assert.IsTrue(TwoDimIntArrayCompare(expected, actual));
        }


        [TestMethod()]
        public void GetPairWinsSymbolsTest()
        {
            VotingBase target = new VotingBase();

            List<VoteTally> votes = new List<VoteTally> {
                new VoteTally { Count = 1, PreferanceOrder = "AB>CD" },
                new VoteTally { Count = 2, PreferanceOrder = "A=B>C=D" },
                new VoteTally { Count = 4, PreferanceOrder = "A>B>C>D" }
            };

            int[,] expected = new int[,] { 
                    { 0, 4, 7, 7 }, 
                    { 0, 0, 7, 7 }, 
                    { 0, 0, 0, 4 }, 
                    { 0, 0, 0, 0 } };

            int[,] actual;
            actual = target.GetPairWins(votes);

            Assert.IsTrue(TwoDimIntArrayCompare(expected, actual));
        }



        public static bool TwoDimIntArrayCompare(int[,] A, int[,] B)
        {
            if (A.Length != B.Length)
                return false;

            if (A.Rank != B.Rank)
                return false;

            for (int i = 0; i < A.GetLength(0); i++)
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    if (A[i, j] != B[i, j])
                        return false;
                }

            return true;
        }

        //public static IEnumerable<T> Flatten<T>(this T[,] items) 
        //{ 
        //    for (int i = 0; i < items.GetLength(0); i++)     
        //        for (int j = 0; j < items.GetLength(1); j++)       
        //            yield return items[i, j]; 
        //}


        static bool ArraysEqual(Array a1, Array a2)
        {
            if (a1 == a2)
            {
                return true;
            }

            if (a1 == null || a2 == null)
            {
                return false;
            }

            if (a1.Length != a2.Length)
            {
                return false;
            }

            IList list1 = a1, list2 = a2;

            for (int i = 0; i < a1.Length; i++)
            {
                if (!Object.Equals(list1[i], list2[i]))
                {
                    return false;
                }
            }
            return true;
        }


    }
}
