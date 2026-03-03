using FocusedObjective.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

namespace KanbanSimulator.Tests
{
    
    
    /// <summary>
    ///This is a test class for SimulationDataTest and is intended
    ///to contain all SimulationDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SimulationDataTest
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
        ///A test for SimulationData Constructor
        ///</summary>
        [TestMethod()]
        public void SimulationDataConstructorTest()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                    <execute type=""scrum"" debugInfo=""true"">
                        <visual generateCumulativeFlow=""false"" />
                    </execute>
                    <setup>
                        <backlog>
                        </backlog>
                    </setup>
                  </simulation>");

            SimulationData target = new SimulationData(document);
            Assert.AreEqual(0, target.Errors.Elements("error").Count());

            Assert.AreEqual("test1", target.Name);
            Assert.IsNotNull(target.Execute);
            Assert.AreEqual(FocusedObjective.Contract.SimulationTypeEnum.Scrum, target.Execute.SimulationType);
            Assert.IsNotNull(target.Execute.Visual);
        }


        [TestMethod()]
        public void SimulationDataTestMissingElement()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                  </simulation>");

            SimulationData target = new SimulationData(document);
            Assert.IsTrue(target.Errors.Elements("error").Count() > 0);
            Assert.AreEqual("3", target.Errors.Elements("error").First().Attribute("code").Value);
        }

        [TestMethod()]
        public void SimulationDataTestMissingAttribute()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                    <execute />
                  </simulation>");

            SimulationData target = new SimulationData(document);
            Assert.IsTrue(target.Errors.Elements("information").Count() > 0);
            Assert.AreEqual("41", target.Errors.Elements("information").First().Attribute("code").Value);
        }

        [TestMethod()]
        public void SimulationDataTestBadAttributeValue()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                    <execute type=""xyz"" debugInfo=""true"" />
                    <setup><backlog></backlog></setup>
                  </simulation>");

            SimulationData target = new SimulationData(document);
            Assert.AreEqual(1, target.Errors.Elements("error").Count());
            Assert.AreEqual("42", target.Errors.Elements("error").First().Attribute("code").Value);
        }

        [TestMethod()]
        public void SimulationDataTestBadIntAttributeValue()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                    <execute type=""kanban"" debugInfo=""true"" />
                    <setup><backlog simpleCount=""1x""></backlog></setup>
                  </simulation>");

            SimulationData target = new SimulationData(document);
            Assert.AreEqual(1, target.Errors.Elements("error").Count());
            Assert.AreEqual("55", target.Errors.Elements("error").First().Attribute("code").Value);
        }

        [TestMethod()]
        public void SimulationDataTestColumnsListTest()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""test1"">
                    <execute type=""kanban"" debugInfo=""true"" />
                    <setup>
                        <backlog></backlog>
                        <columns>
                            <column id=""0"" estimateLowBound=""2"" estimateHighBound=""5"" wipLimit=""3"">Column 1</column>
                            <column id=""1"" estimateLowBound=""3"" estimateHighBound=""6"" wipLimit=""4"">Column 2</column>
                            <column id=""2"" estimateLowBound=""4"" estimateHighBound=""7"" wipLimit=""5"">Column 2</column>
                        </columns>
                    </setup>
                  </simulation>");

            SimulationData target = new SimulationData(document);

            Assert.AreEqual(3, target.Setup.Columns.Count);

            Assert.AreEqual(0, target.Setup.Columns[0].Id);
            Assert.AreEqual(2, target.Setup.Columns[0].EstimateLowBound);
            Assert.AreEqual(5, target.Setup.Columns[0].EstimateHighBound);
            Assert.AreEqual("Column 1", target.Setup.Columns[0].Name);


        }

        [TestMethod()]
        public void CustomValidationSimpleBacklogWithErrorsTest()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""Custom Validation Data Test"" version=""1.0"" >

	<execute limitIntervalsTo=""1000"" decimalRounding=""3"">
  		
  		<!-- 0 errors -->
  		<visual />
  		
  		<!-- 1 error -->
  		<monteCarlo cycles=""0"" />
  		
  		<!-- 5 errors -->
  		<addStaff cycles=""0"" count=""1"">
  			<column id=""99"" minWip=""0"" maxWip=""0"" />
  			<column id=""1"" minWip=""10"" maxWip=""5"" />
		</addStaff>

		<!-- 1 error -->
		<sensitivity cycles=""0"" estimateMultiplier=""1"" occurrenceMultiplier=""1"" />
		
 	</execute>
  
 	<setup>
		
		<!-- 5 errors -->
  		<columns>
     			<column id=""1"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""0"" estimateHighBound=""10"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""1"" estimateHighBound=""0"" wipLimit=""1"">Column4</column>
     			<column id=""4"" estimateLowBound=""10"" estimateHighBound=""5"" wipLimit=""0"">Column5</column>
  		</columns>
		
		<!-- 1 error -->
		<backlog type=""simple"" simpleCount=""0"" />

		<!-- 7 errors -->
		<defects>
			   <defect columnId=""99"" startsInColumnId=""3"" 
			    scale=""0"" occurrenceLowBound=""0"" occurrenceHighBound=""0"">Defect1<column id=""3"" estimateLowBound=""1"" estimateHighBound=""2"" />
				    <column id=""4"" estimateLowBound=""2"" estimateHighBound=""1.5"" />
				    <column id=""5"" estimateLowBound=""1"" estimateHighBound=""1"" />
			   </defect>
			   <defect columnId=""1"" startsInColumnId=""3"" 
			    scale=""1"" occurrenceLowBound=""2"" occurrenceHighBound=""1"">Defect2</defect>		
		</defects>
		
		  <!-- 4 errors -->
		  <addedScopes>
		            <addedScope scale=""0"" occurrenceLowBound=""0"" occurrenceHighBound=""0"">Scope Creep 1</addedScope>
		            <addedScope scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""2"">Scope Creep 2</addedScope>
		  </addedScopes>
		
		<!-- 8 errors -->
		  <blockingEvents>
		   <blockingEvent columnId=""99"" scale=""0"" occurrenceLowBound=""0"" occurrenceHighBound=""0""  estimateLowBound=""0"" estimateHighBound=""0"">Block 1</blockingEvent>
		   <blockingEvent columnId=""1"" scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""2""  estimateLowBound=""5"" estimateHighBound=""2"">Block 2</blockingEvent>
		  </blockingEvents>

 </setup>

</simulation>
");

            SimulationData target = new SimulationData(document);
            Assert.AreEqual(false, target.Validate());

            // total
            Assert.AreEqual(32, target.Errors.Elements("error").Count());

            // each error code
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "8").Count());//
            Assert.AreEqual(19,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "43").Count());//
            Assert.AreEqual(2,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "44").Count());
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "45").Count());
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "46").Count());
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "18").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "19").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "20").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "21").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "22").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "23").Count());//
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "24").Count());
            Assert.AreEqual(1,
                target.Errors.Elements("error").Where(e => e.Attribute("code").Value == "25").Count());
        }

        [TestMethod()]
        public void CustomValidationSimpleBacklogNoErrorsTest()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""Custom Validation Data Test"" version=""1.0"" >

	<execute limitIntervalsTo=""1000"" decimalRounding=""3"" type=""Kanban"">
  		
  		<!-- 0 errors -->
  		<visual />
  		
  		<!-- 1 error -->
  		<monteCarlo cycles=""100"" />
  		
  		<!-- 5 errors -->
  		<addStaff cycles=""100"" count=""1"">
  			<column id=""1"" minWip=""1"" maxWip=""10"" />
  			<column id=""2"" minWip=""1"" maxWip=""5"" />
		</addStaff>

		<!-- 1 error -->
		<sensitivity cycles=""100"" estimateMultiplier=""1"" occurrenceMultiplier=""1"" />
		
 	</execute>
  
 	<setup>
		
		<!-- 6 errors -->
  		<columns>
     			<column id=""1"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""1"" estimateHighBound=""10"" wipLimit=""1"">Column4</column>
     			<column id=""5"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column5</column>
  		</columns>
		
		<!-- 1 error -->
		<backlog type=""simple"" simpleCount=""10"" />

		<!-- 7 errors -->
		<defects>
			   <defect columnId=""4"" startsInColumnId=""3"" 
			    scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""10"">Defect1<column id=""3"" estimateLowBound=""1"" estimateHighBound=""2"" />
				    <column id=""4"" estimateLowBound=""2"" estimateHighBound=""3.5"" />
				    <column id=""5"" estimateLowBound=""1"" estimateHighBound=""1"" />
			   </defect>
			   <defect columnId=""1"" startsInColumnId=""3"" 
			    scale=""1"" occurrenceLowBound=""2"" occurrenceHighBound=""5"">Defect2</defect>		
		</defects>
		
		  <!-- 4 errors -->
		  <addedScopes>
		            <addedScope scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""10"">Scope Creep 1</addedScope>
		            <addedScope scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""20"">Scope Creep 2</addedScope>
		  </addedScopes>
		
		<!-- 8 errors -->
		  <blockingEvents>
		   <blockingEvent columnId=""4"" scale=""1"" occurrenceLowBound=""5"" occurrenceHighBound=""10""  estimateLowBound=""5"" estimateHighBound=""10"">Block 1</blockingEvent>
		   <blockingEvent columnId=""1"" scale=""1"" occurrenceLowBound=""2"" occurrenceHighBound=""5""  estimateLowBound=""2"" estimateHighBound=""5"">Block 2</blockingEvent>
		  </blockingEvents>

 </setup>

</simulation>
");

            SimulationData target = new SimulationData(document);
            Assert.AreEqual(true, target.Validate());

            // total
            Assert.AreEqual(0, target.Errors.Elements("error").Count());
        }

        [TestMethod()]
        public void AsXMLTestTest()
        {
            XDocument document =
                XDocument.Parse(
                @"<simulation name=""AsXML Test"" >

	<execute limitIntervalsTo=""1000"" decimalRounding=""3"" type=""Kanban"">
  		
  		<visual />
   		<monteCarlo cycles=""100"" />
  		
  		<addStaff cycles=""100"" count=""1"">
  			<column id=""1"" minWip=""1"" maxWip=""10"" />
  			<column id=""2"" minWip=""1"" maxWip=""5"" />
		</addStaff>

		<sensitivity cycles=""100"" estimateMultiplier=""1"" occurrenceMultiplier=""1"" />

        <modelAudit />
		
        <forecastDate cycles=""250"" startDate=""01Oct2011"" intervalsToOneDay=""1"" workDays=""monday,tuesday,wednesday,thursday,friday"" costPerDay=""3461"" />

 	</execute>
  
 	<setup>
		
  		<columns>
     			<column id=""1"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column1</column>
     			<column id=""2"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column2</column>
     			<column id=""3"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column3</column>
     			<column id=""4"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column4</column>
     			<column id=""5"" estimateLowBound=""1"" estimateHighBound=""5"" wipLimit=""1"">Column5</column>
  		</columns>
		
		<backlog type=""custom"" simpleCount=""10"" >
            <deliverable name=""Deliverable1"">
			   	<custom name=""Small"" count=""6""  percentageLowBound=""0"" percentageHighBound=""66"" /> 
			   	<custom name=""Medium"" count=""4""  percentageLowBound=""33"" percentageHighBound=""100"" /> 

				<custom name=""UI Intensive"" count=""5""  percentageLowBound=""0"" percentageHighBound=""100"" > 
			   		<column id=""1"" estimateLowBound=""2"" estimateHighBound=""5"" />
			   		<column id=""2"" estimateLowBound=""3"" estimateHighBound=""5"" />
			   	</custom>
            </deliverable>
        </backlog>

		<defects>
			   <defect columnId=""4"" startsInColumnId=""3"" 
			     occurrenceLowBound=""5"" occurrenceHighBound=""10"">Defect1<column id=""3"" estimateLowBound=""1"" estimateHighBound=""2"" />
				    <column id=""4"" estimateLowBound=""2"" estimateHighBound=""3.5"" />
				    <column id=""5"" estimateLowBound=""1"" estimateHighBound=""1"" />
			   </defect>
			   <defect columnId=""1"" startsInColumnId=""3"" 
			     occurrenceLowBound=""2"" occurrenceHighBound=""5"">Defect2</defect>		
		</defects>
		
		  <addedScopes>
		            <addedScope occurrenceLowBound=""5"" occurrenceHighBound=""10"">Scope Creep 1</addedScope>
		            <addedScope occurrenceLowBound=""5"" occurrenceHighBound=""20"">Scope Creep 2</addedScope>
		  </addedScopes>
		
		  <blockingEvents>
		   <blockingEvent columnId=""4"" occurrenceLowBound=""5"" occurrenceHighBound=""10""  estimateLowBound=""5"" estimateHighBound=""10"">Block 1</blockingEvent>
		   <blockingEvent columnId=""1"" occurrenceLowBound=""2"" occurrenceHighBound=""5""  estimateLowBound=""2"" estimateHighBound=""5"">Block 2</blockingEvent>
		  </blockingEvents>

 </setup>

</simulation>
");

            SimulationData target = new SimulationData(document);
            bool valid = target.Validate();
            Assert.AreEqual(true, valid, "original failed to validate");

            // now get the XML, make a second document and validate it....
            XDocument export = new XDocument(target.AsXML(SimulationTypeEnum.Kanban));
            SimulationData second = new SimulationData(export);

            Assert.AreEqual(true, second.Validate(), "failed to validate after export");

            //TODO:check the results weren't just default values rather than actual values from original XML
        }
    }
}
