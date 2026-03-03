using FocusedObjective.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace KanbanSimulator.Tests
{
    
    /// <summary>
    ///This is a test class for VariableExpressionDecoderTest and is intended
    ///to contain all VariableExpressionDecoderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VariableExpressionDecoderTest
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
        ///A test for ExpressionExist
        ///</summary>
        [TestMethod()]
        public void ExpressionExistTest()
        {
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionExist(null));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=1 + 2"));
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionExist("1"));
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionExist(""));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=(1 + 2)"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=(1 * 2)"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=1 - 2"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=(1 / 2)"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionExist("=Math.Pow(2,2)"));
        }

        /// <summary>
        ///A test for ExpressionVariablesExist
        ///</summary>
        [TestMethod()]
        public void ExpressionVariablesExistTest()
        {
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionVariablesExist(null));
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionVariablesExist("1"));
            Assert.AreEqual(false, VariableExpressionDecoder.ExpressionVariablesExist(""));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionVariablesExist("@test"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionVariablesExist("=1 + @test"));
            Assert.AreEqual(true, VariableExpressionDecoder.ExpressionVariablesExist(" @test "));
        }

        /// <summary>
        ///A test for ReplaceExpressionVariables
        ///</summary>
        [TestMethod()]
        public void ReplaceExpressionVariablesTest()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("test1", "1");
            values.Add("test2", "2.0");
            values.Add("test3", "3.0");
            values.Add("test4", "text");

            Assert.AreEqual("3", VariableExpressionDecoder.ReplaceExpressionVariables(values, "=@test1 + @test2"));
            Assert.AreEqual("3", VariableExpressionDecoder.ReplaceExpressionVariables(values, "=@test2 + @test1"));
            Assert.AreEqual("text", VariableExpressionDecoder.ReplaceExpressionVariables(values, "@test4"));
            Assert.AreEqual("9", VariableExpressionDecoder.ReplaceExpressionVariables(values, "=(@test1 + @test2) * @test3"));
        }

        /// <summary>
        ///A test for EvaluateExpression
        ///</summary>
        [TestMethod()]
        public void EvaluateExpressionTest()
        {
            // using syncfusion by default

            Assert.AreEqual("", VariableExpressionDecoder.EvaluateExpression(null));
            Assert.AreEqual("3", VariableExpressionDecoder.EvaluateExpression("=1 + 2"));
            Assert.AreEqual("1", VariableExpressionDecoder.EvaluateExpression("1"));
            Assert.AreEqual("", VariableExpressionDecoder.EvaluateExpression(""));
            Assert.AreEqual("3", VariableExpressionDecoder.EvaluateExpression("=(1 + 2)"));
            Assert.AreEqual("2", VariableExpressionDecoder.EvaluateExpression("=(1 * 2)"));
            Assert.AreEqual("-1", VariableExpressionDecoder.EvaluateExpression("=1 - 2"));
            Assert.AreEqual("0.5", VariableExpressionDecoder.EvaluateExpression("=(1 / 2)"));
            Assert.AreEqual("0.5", VariableExpressionDecoder.EvaluateExpression("=(1.0 / 2.0)"));


            // syncfusion style
            Assert.AreEqual("2", VariableExpressionDecoder.EvaluateExpression("=sqrt(4)"));
        }


        [TestMethod()]
        public void EvaluateSyncfusionExpressionTest()
        {
            
            var calc = VariableExpressionDecoder.CreateCalculatorInstance();
            try
            {

                Assert.AreEqual("", VariableExpressionDecoder.EvaluateExpression(null, calc));
                Assert.AreEqual("3", VariableExpressionDecoder.EvaluateExpression("=1 + 2", calc));
                Assert.AreEqual("1", VariableExpressionDecoder.EvaluateExpression("1", calc));
                Assert.AreEqual("", VariableExpressionDecoder.EvaluateExpression("", calc));
                Assert.AreEqual("3", VariableExpressionDecoder.EvaluateExpression("=(1 + 2)", calc));
                Assert.AreEqual("2", VariableExpressionDecoder.EvaluateExpression("=(1 * 2)", calc));
                Assert.AreEqual("-1", VariableExpressionDecoder.EvaluateExpression("=1 - 2", calc));
                Assert.AreEqual("0.5", VariableExpressionDecoder.EvaluateExpression("=(1 / 2)", calc));
                Assert.AreEqual("0.5", VariableExpressionDecoder.EvaluateExpression("=(1.0 / 2.0)", calc));
                Assert.AreEqual("4", VariableExpressionDecoder.EvaluateExpression("=Pow(2,2)", calc));
            }
            finally
            {
                calc.Dispose();
            }
             
             
        }
    }
}
