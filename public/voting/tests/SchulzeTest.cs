using VotingSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VotingAndRanking.Tests
{
    
    
    /// <summary>
    ///This is a test class for SchulzeTest and is intended
    ///to contain all SchulzeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SchulzeTest
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
        ///A test for FindPathStrength
        ///</summary>
        [TestMethod()]
        public void FindPathStrengthTest()
        {
            Schulze target = new Schulze(); 
            int[,] pairWins =  new int[,] { 
                    { 0, 6, 3, 4 }, 
                    { 5, 0, 7, 8 }, 
                    { 8, 4, 0, 10 }, 
                    { 7, 3, 1, 0 } };

            int[,] expected =  new int[,] { 
                    { 0, 6, 6, 6 }, 
                    { 7, 0, 7, 8 }, 
                    { 8, 6, 0, 10 }, 
                    { 7, 6, 6, 0 } };

            int[,] actual;
            
            actual = target.FindPathStrength(pairWins);
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

        /// <summary>
        ///A test for MakeWinners
        ///</summary>
        [TestMethod()]
        public void MakeWinnersTest()
        {
            Schulze target = new Schulze();
            int[,] pathStrengthMatrix = new int[,] { 
                    { 0, 6, 6, 6 }, 
                    { 7, 0, 7, 8 }, 
                    { 8, 6, 0, 10 }, 
                    { 7, 6, 6, 0 } };

            bool[] expected = new bool[] { false, true, false, false }; 
            bool[] actual;
            actual = target.MakeWinners(pathStrengthMatrix);

            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);
            Assert.AreEqual(expected[2], actual[2]);
            Assert.AreEqual(expected[3], actual[3]);
            
        }

        [TestMethod()]
        public void ResultsTestForCondorcetParadoxCase()
        {
            Schulze target = new Schulze();

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

        [TestMethod()]
        public void ResultsTestElectoaramaCase1()
        {
            Schulze target = new Schulze();
            string vs = @"5 ACBED|5 ADECB|8 BEDAC|3 CABED|7 CAEBD|2 CBADE|7 DCEBA|8 EBADC";

            List<VoteTally> votes = 
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] {'|'}));

            int[,] pathStrengths = target.FindPathStrength(target.GetPairWins(votes));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            Assert.IsTrue(CompareResults(actual, "E>A>C>B>D"));
        }

        [TestMethod()]
        public void ResultsTestElectoaramaCase2()
        {
            Schulze target = new Schulze();
            string vs = @"5 ACBD|2 ACDB|3 ADCB|4 BACD|3 CBDA|3 CDBA|1 DACB|5 DBAC|4 DCBA";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            int[,] pathStrengths = target.FindPathStrength(target.GetPairWins(votes));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            Assert.IsTrue(CompareResults(actual, "D>A>C>B"));
        }

        [TestMethod()]
        public void ResultsTestElectoaramaCase3()
        {
            Schulze target = new Schulze();
            string vs = @"3 ABDEC|5 ADEBC|1 ADECB|2 BADEC|2 BDECA|4 CABDE|6 CBADE|2 DBECA|5 DECAB";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            int[,] pathStrengths = target.FindPathStrength(target.GetPairWins(votes));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            Assert.IsTrue(CompareResults(actual, "B>A>D>E>C"));
        }

        [TestMethod()]
        public void ResultsTestElectoaramaCase4()
        {
            Schulze target = new Schulze();
            string vs = @"3 ABCD|2 DABC|2 DBCA|2 CBDA|";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            int[,] pathStrengths = target.FindPathStrength(target.GetPairWins(votes));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            // a tie for first/second, and a tie for third/fourth
            Assert.IsTrue(CompareResults(actual, "BD>AC"));
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

        /// <summary>
        ///A test for GetXmlString
        ///</summary>
        [TestMethod()]
        public void GetXmlStringTest()
        {
            Schulze target = new Schulze();
            string vs = @"5 ACBED|5 ADECB|8 BEDAC|3 CABED|7 CAEBD|2 CBADE|7 DCEBA|3 EBADC|5 EBADC";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            string expected = string.Empty;
            string actual;
            actual = target.GetXmlString(votes);
            Assert.IsFalse(string.IsNullOrEmpty(actual));

        }

        [TestMethod()]
        public void ResultsTestSchulze1Example3_1()
        {
            Schulze target = new Schulze();

            string vs = @"8 acdb|2 badc|4 cdba|4 dbac|3 dcba";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            string xml = target.GetXmlString(votes);
            Assert.IsTrue(CompareResults(actual, "D>A>C>B"));
        }

        [TestMethod()]
        public void ResultsTestSchulze1Example3_2()
        {
            Schulze target = new Schulze();

            string vs = @"3 abcd|2 cbda|2 dabc|2 dbca";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            string xml = target.GetXmlString(votes);
            Assert.IsTrue(CompareResults(actual, "BD>AC"));
        }

        [TestMethod()]
        public void ResultsTestSchulze1Example3_3()
        {
            Schulze target = new Schulze();

            string vs = @"6 abcd|12 acdb|21 bcad|9 cdba|15 dbac";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            string xml = target.GetXmlString(votes);
            Assert.IsTrue(CompareResults(actual, "B>AC>D"));
        }

        [TestMethod()]
        public void ResultsTestSchulze1Example3_4()
        {
            Schulze target = new Schulze();

            string vs = @"3 adebcf|3 bfecda|4 cabfde|1 dbcefa|4 defabc|2 ecbdfa|2 facdbe";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            string xml = target.GetXmlString(votes);
            Assert.IsTrue(CompareResults(actual, "A>B>F>D>E>C"));

            // add 2 a>e>f>c>b>d
            votes.Add(new VoteTally{ Count = 2, PreferanceOrder = "AEFCBD"});
            actual = target.Results(votes);
            xml = target.GetXmlString(votes);
            Assert.IsTrue(CompareResults(actual, "D>E>C>A>B>F"));

        }

        [TestMethod()]
        public void ResultsTestSchulze1Example6()
        {
            Schulze target = new Schulze();

            string vs = @"6 a>b>c>d|8 a=b>c=d|8 a=c>b=d|18 a=c>d>b|8 a=c=d>b|40 b>a=c=d|4 c>b>d>a|9 c>d>a>b|8 c=d>a=b|14 d>a>b>c|11 d>b>c>a|4 d>c>a>b";

            List<VoteTally> votes =
                VotingHelpers.VoteOrderPreferencesAsAlphabeticalVoteTally(vs.Split(new char[] { '|' }));

            IEnumerable<IGrouping<int, VoteCandidateResult>> actual;
            actual = target.Results(votes);

            string xml = target.GetXmlString(votes);
            
            // using the "winning votes" method for path strength win
            Assert.IsTrue(CompareResults(actual, "D>A>B>C"));
        }


    }
}
