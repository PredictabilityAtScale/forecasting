using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("execute", "Simulation instructions for this model.", true)]
    public class ExecuteData : ContractDataBase, IValidate
    {
        public ExecuteData()
        {
        }

        public ExecuteData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private SimulationTypeEnum _simulationType = SimulationTypeEnum.Kanban;
        private int _decimalRounding = 3;
        private int _limitIntervalsTo = 9000;
        private string _intervalUnit = "days";
        private string _googleHistogramUrlFormat = @"http://chart.apis.google.com/chart?chxr=0,0,{4}|1,{5},{6}&chxt=y,x,x&chds=0,{7}&chbh=a&chs=600x400&cht=bvg&chco=3072F3&{0}&chdl={1}&chg=0,10&chtt={2}&chxl={8}";
        private string _dateFormat = "yyyyMMdd";
        private string _currencyFormat = "C2";
        private string _percentageFormat = "P";
        private string _deliverables = "";
        private ShufflePositionsEnum _shufflePositions = ShufflePositionsEnum.afterOrdering;
        private PullOrderEnum _pullOrder = PullOrderEnum.randomAfterOrdering;
        private AggregationValueEnum _aggregationValue = AggregationValueEnum.Average;
        private string _returnResults = "";
        private string _defaultDistribution = "uniform";
        private string _basePath = "";

        // when is DONE? zero backlog and less than x% positions filled on board
        //private double _backlogRemainingCompletePercentage = 0.0;
        private double _completePercentage = 100.0;

        private double _activePositionsCompletePercentage = 0.0;

        private ExecuteVisualData _executeVisualData = null;
        private ExecuteMonteCarloData _executeMonteCarloData = null;
        private ExecuteSensitivityData _executeSensitivityData = null; 
        private ExecuteAddStaffData _executeAddStaffData = null;
        private ExecuteForecastDateData _executeForecastDateData = null;
        private ExecuteSummaryStatisticsData _executeSummaryStatisticsData = null;
        private ExecuteModelAuditData _executeModelAuditData = null;
        private ExecuteBallotData _executeBallotData = null;

        // public properties

        [SimMLAttribute("type", "Simulation model type. Kanban or scrum. Kanban uses cycle time ranges for estimates, scrum uses points or throughput.", false, ValidValues="kanban|scrum")]
        public SimulationTypeEnum SimulationType
        {
            get { return _simulationType; }
            set { _simulationType = value; }
        }

        [SimMLAttribute("limitIntervalsTo", "Return even if simulation hasn't completed by this number of simulation steps. This avoids simulations that will never finish due to model error.", false)]
        public int LimitIntervalsTo
        {
            get { return _limitIntervalsTo; }
            set { _limitIntervalsTo = value; }
        }

        [SimMLAttribute("intervalUnit", "String to represent what usin each simulation step represents. days, or hours for example. Just used for display.", false)]
        public string IntervalUnit
        {
            get { return _intervalUnit; }
            set { _intervalUnit = value; }
        }

        [SimMLAttribute("decimlRounding", "Number of decimal places numbers are rounded to in the returned results.", false)]
        public int DecimalRounding
        {
            get { return _decimalRounding; }
            set { _decimalRounding = value; }
        }

        public string GoogleHistogramUrlFormat
        {
            get { return _googleHistogramUrlFormat; }
            set { _googleHistogramUrlFormat = value; }
        }

        [SimMLAttribute("dateFormat", "Date format string to parse date values entered within this model correctly. See the knowledge base for full documentation.", false)]
        public string DateFormat
        {
            get { return _dateFormat; }
            set { _dateFormat = value; }
        }

        [SimMLAttribute("currencyFormat", "Currency format string to parse financial values entered within this model correctly. See the knowledge base for full documentation.", false)]
        public string CurrencyFormat
        {
            get { return _currencyFormat; }
            set { _currencyFormat = value; }
        }

        [SimMLAttribute("percentageFormat", "Percentage format string to parse percentage values entered within this model correctly. See the knowledge base for full documentation.", false)]
        public string PercentageFormat
        {
            get { return _percentageFormat; }
            set { _percentageFormat = value; }
        }



        [SimMLAttribute("aggregationValue", "The value used to aggregate monte carlo results. Normally the average value, but it can be any of the following average, median, fifth, twentyfifth, seventyfifth or ninetyfifth.", false, ValidValues = "average|median|fifth|twentyfifth|seventyfifth|ninetyfifth")]
        public AggregationValueEnum AggregationValue
        {
            get { return _aggregationValue; }
            set { _aggregationValue = value; }
        }

        public string ReturnResults
        {
            get { return _returnResults; }
            set { _returnResults = value; }
        }

        [SimMLAttribute("deliverables", "Pipe separated list of the deliverables by name to include in simulation. Leave empty for all defined deliverables.", false)]
        public string Deliverables
        {
            get { return _deliverables; }
            set { _deliverables = value; }
        }

        [SimMLAttribute("defaultDistribution", "Estimate low and high bounds by default return a uniform distribution across its range. Set this to 'weibull' to make unspecified distributions be left-skewed.", false, ValidValues="uniform|weibull")]
        public string DefaultDistribution
        {
            get { return _defaultDistribution; }
            set { _defaultDistribution = value; }
        }

        [SimMLAttribute("completePercentage", "What is the lowest percentage of completed work allowed before simulation is recorded as complete. Used in conjunction with activePositionsCompletePercentage (results are ANDed) to exit simulation early for cases where there are just a few items remaining. Default is 100 which means EVERY piece of work needs to be completed. Often reduced to 85%", false)]
        public double CompletePercentage
        {
            get { return _completePercentage; }
            set { _completePercentage = value; }
        }

        [SimMLAttribute("activePositionsCompletePercentage", "When the number of cards on the board falls below this lower limit, AND the completePercentage value is satisfied, simulation is considered complete. Used to exit simulation early when just a few items remain on the board. Default is 0 (every item needs to be finished).", false)]
        public double ActivePositionsCompletePercentage
        {
            get { return _activePositionsCompletePercentage; }
            set { _activePositionsCompletePercentage = value; }
        }

        public ExecuteVisualData Visual
        {
            get { return _executeVisualData; }
            set { _executeVisualData = value; }
        }

        public ExecuteBallotData Ballot
        {
            get { return _executeBallotData; }
            set { _executeBallotData = value; }
        }

        public ExecuteMonteCarloData MonteCarlo
        {
            get { return _executeMonteCarloData; }
            set { _executeMonteCarloData = value; }
        }

        public ExecuteSensitivityData Sensitivity
        {
            get { return _executeSensitivityData; }
            set { _executeSensitivityData = value; }
        }

        public ExecuteAddStaffData AddStaff
        {
            get { return _executeAddStaffData; }
            set { _executeAddStaffData = value; }
        }

        public ExecuteForecastDateData ForecastDate
        {
            get { return _executeForecastDateData; }
            set { _executeForecastDateData = value; }
        }

        public ExecuteSummaryStatisticsData SummaryStatistics
        {
            get { return _executeSummaryStatisticsData; }
            set { _executeSummaryStatisticsData = value; }
        }

        public ExecuteModelAuditData ModelAudit
        {
            get { return _executeModelAuditData; }
            set { _executeModelAuditData = value; }
        }

        //[SimMLAttribute("shufflePositions", "OBSOLETE - use pullOrder instead.", false, ValidValues = "afterOrdering|true|false|fifo|fifoStrict")]
        public ShufflePositionsEnum ShufflePositions
        {
            get { return _shufflePositions; }
            set { _shufflePositions = value; }
        }

                       

        [SimMLAttribute("pullOrder", "Determines the pull selection order of a column when more than one item is completed during a simulation interval. Default is 'afterOrdering' which means the order values are honored first then its random. 'random' means totally random, 'index' uses card creation order, 'fifo' means the work that entered the column first if more than one item is complete, and 'fifoStrict' means block anything moving until fifo is enforced even for incomplete work.", false, ValidValues = "afterOrdering|random|index|fifo|fifoStrict")]
        public PullOrderEnum PullOrder
        {
            get { return _pullOrder; }
            set { _pullOrder = value; }
        }

        [SimMLAttribute("basePath", "Base path for any file reference in this model. Allows paths to be relative and changed in one spot.", false)]
        public string BasePath
        {
            get { return _basePath;  }
            set { _basePath = value;  }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            _simulationType = ContractCommon.ReadMandatoryAttributeListValue(
                source, 
                errors,
                "type", 
                "kanban", SimulationTypeEnum.Kanban, 
                "scrum", SimulationTypeEnum.Scrum,
                "lean", SimulationTypeEnum.Kanban,
                "agile", SimulationTypeEnum.Scrum);

            result = result && ContractCommon.ReadAttributeIntValue(
                out _limitIntervalsTo,
                source,
                errors,
                "limitIntervalsTo",
                _limitIntervalsTo,
                false
                );

            _intervalUnit = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "intervalUnit",
                _intervalUnit,
                false
                );

            result = result && ContractCommon.ReadAttributeIntValue(
                out _decimalRounding,
                source,
                errors,
                "decimalRounding",
                _decimalRounding,
                false
                );

            _googleHistogramUrlFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "googleHistogramUrlFormat",
                _googleHistogramUrlFormat,
                false
                );


            _currencyFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "currencyFormat",
                _currencyFormat,
                false
                );

            _percentageFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "percentageFormat",
                _percentageFormat,
                false
                );

            _aggregationValue = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "aggregationValue",
                "average", AggregationValueEnum.Average,
                "median", AggregationValueEnum.Median,
                "fifth", AggregationValueEnum.Fifth, 
                "twentyfifth", AggregationValueEnum.TwentyFifth,
                "seventyfifth", AggregationValueEnum.SeventyFifth,
                "ninetyfifth", AggregationValueEnum.NinetyFifth);
            
            _deliverables = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "deliverables",
                _deliverables,
                false
                );

            _returnResults = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "returnResults",
                _returnResults,
                false
                );

            _dateFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "dateFormat",
                _dateFormat,
                false
                );

            _shufflePositions = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "shufflePositions",
                "afterOrdering", ShufflePositionsEnum.afterOrdering,
                "true", ShufflePositionsEnum.@true,
                "yes", ShufflePositionsEnum.@true,
                "false", ShufflePositionsEnum.@false,
                "no", ShufflePositionsEnum.@false,
                "FIFO", ShufflePositionsEnum.FIFO,
                "fifo", ShufflePositionsEnum.FIFO,
                "FIFOStrict", ShufflePositionsEnum.FIFOStrict,
                "fifoStrict", ShufflePositionsEnum.FIFOStrict
                );

            _pullOrder = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "pullOrder",
                "afterOrdering", PullOrderEnum.randomAfterOrdering,
                "randomAfterOrdering", PullOrderEnum.randomAfterOrdering,
                "random", PullOrderEnum.random,
                "index", PullOrderEnum.indexSequence,
                "indexSequence", PullOrderEnum.indexSequence,
                "FIFO", PullOrderEnum.FIFO,
                "fifo", PullOrderEnum.FIFO,
                "FIFOStrict", PullOrderEnum.FIFOStrict,
                "fifoStrict", PullOrderEnum.FIFOStrict
                );

            _defaultDistribution = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "defaultDistribution",
                _defaultDistribution,
                false
                );

            //result = result && ContractCommon.ReadAttributeDoubleValue(
            //    out _backlogRemainingCompletePercentage,
            //    source,
            //    errors,
            //    "backlogRemainingCompletePercentage",
            //    _backlogRemainingCompletePercentage,
            //    false
            //    );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _completePercentage,
                source,
                errors,
                "completePercentage",
                _completePercentage,
                false
                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _activePositionsCompletePercentage,
                source,
                errors,
                "activePositionsCompletePercentage",
                _activePositionsCompletePercentage,
                false
                );

            _basePath = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "basePath",
                _basePath,
                false
                );
            

            // sub elements
            _executeVisualData = ContractCommon.ReadElement(source, typeof(ExecuteVisualData), "visual", errors, false);
            _executeBallotData = ContractCommon.ReadElement(source, typeof(ExecuteBallotData), "ballot", errors, false);
            _executeMonteCarloData = ContractCommon.ReadElement(source, typeof(ExecuteMonteCarloData), "monteCarlo", errors, false);
            _executeSensitivityData = ContractCommon.ReadElement(source, typeof(ExecuteSensitivityData), "sensitivity", errors, false);
            _executeAddStaffData = ContractCommon.ReadElement(source, typeof(ExecuteAddStaffData), "addStaff", errors, false);
            _executeForecastDateData = ContractCommon.ReadElement(source, typeof(ExecuteForecastDateData), "forecastDate", errors, false);
            _executeSummaryStatisticsData = ContractCommon.ReadElement(source, typeof(ExecuteSummaryStatisticsData), "summaryStatistics", errors, false);
            _executeModelAuditData = ContractCommon.ReadElement(source, typeof(ExecuteModelAuditData), "modelAudit", errors, false);
           
            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("execute");

            result.Add(new XAttribute("type", _simulationType.ToString()));
            result.Add(new XAttribute("deliverables", _deliverables.ToString()));
            result.Add(new XAttribute("intervalUnit", _intervalUnit.ToString()));
            result.Add(new XAttribute("limitIntervalsTo", _limitIntervalsTo.ToString()));
            result.Add(new XAttribute("decimalRounding", _decimalRounding.ToString()));
            result.Add(new XAttribute("googleHistogramUrlFormat", _googleHistogramUrlFormat.ToString()));
            result.Add(new XAttribute("currencyFormat", _currencyFormat.ToString()));
            result.Add(new XAttribute("percentageFormat", _percentageFormat.ToString()));
            result.Add(new XAttribute("dateFormat", _dateFormat.ToString()));
            result.Add(new XAttribute("aggregationValue", _aggregationValue.ToString()));
            result.Add(new XAttribute("returnResults", _returnResults.ToString()));
            result.Add(new XAttribute("defaultDistribution", _defaultDistribution.ToString()));
            //result.Add(new XAttribute("backlogRemainingCompletePercentage", _backlogRemainingCompletePercentage.ToString()));
            result.Add(new XAttribute("completePercentage", _completePercentage.ToString()));
            result.Add(new XAttribute("activePositionsCompletePercentage", _activePositionsCompletePercentage.ToString()));
            result.Add(new XAttribute("basePath", _basePath.ToString()));


            switch (_pullOrder)
            {
                case PullOrderEnum.randomAfterOrdering:
                    result.Add(new XAttribute("pullOrder", "randomAfterOrdering"));
                    break;
                case PullOrderEnum.random:
                    result.Add(new XAttribute("pullOrder", "random"));
                    break;
                case PullOrderEnum.indexSequence:
                    result.Add(new XAttribute("pullOrder", "indexSequence"));
                    break;
                case PullOrderEnum.FIFO:
                    result.Add(new XAttribute("pullOrder", "FIFO"));
                    break;
                case PullOrderEnum.FIFOStrict:
                    result.Add(new XAttribute("pullOrder", "FIFOStrict"));
                    break;
                default:
                    result.Add(new XAttribute("pullOrder", "randomAfterOrdering"));
                    break;
            }

            // this is obsolete, only add it if it was used in a model previously
            if (_shufflePositions != ShufflePositionsEnum.afterOrdering)
            {
                switch (_shufflePositions)
                {
                    case ShufflePositionsEnum.afterOrdering:
                        result.Add(new XAttribute("shufflePositions", "afterOrdering"));
                        break;
                    case ShufflePositionsEnum.@true:
                        result.Add(new XAttribute("shufflePositions", "true"));
                        break;
                    case ShufflePositionsEnum.@false:
                        result.Add(new XAttribute("shufflePositions", "false"));
                        break;
                    case ShufflePositionsEnum.FIFO:
                        result.Add(new XAttribute("shufflePositions", "fifo"));
                        break;
                    case ShufflePositionsEnum.FIFOStrict:
                        result.Add(new XAttribute("shufflePositions", "fifoStrict"));
                        break;
                    default:
                        result.Add(new XAttribute("shufflePositions", "afterOrdering"));
                        break;
                }              
            }
            
            if (_executeVisualData != null) result.Add(_executeVisualData.AsXML(simType));
            if (_executeBallotData != null) result.Add(_executeBallotData.AsXML(simType));
            if (_executeMonteCarloData != null) result.Add(_executeMonteCarloData.AsXML(simType));
            if (_executeSensitivityData != null) result.Add(_executeSensitivityData.AsXML(simType));
            if (_executeAddStaffData != null) result.Add(_executeAddStaffData.AsXML(simType));
            if (_executeForecastDateData != null) result.Add(_executeForecastDateData.AsXML(simType));
            if (_executeSummaryStatisticsData != null) result.Add(_executeSummaryStatisticsData.AsXML(simType));
            if (_executeModelAuditData != null) result.Add(_executeModelAuditData.AsXML(simType));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // at least one execute action is needed
            if (this.Visual == null &&
                this.MonteCarlo == null &&
                this.AddStaff == null &&
                this.Sensitivity == null &&
                this.ForecastDate == null &&
                this.SummaryStatistics == null &&
                this.ModelAudit == null &&
                this.Ballot == null)
            {
                success = false;
                Helper.AddError(errors, ErrorSeverityEnum.Error, 5, string.Format(Strings.Error5, "execute"), Source);
            }

            if (AddStaff != null)
                success &= AddStaff.Validate(data, errors);
            
            if (MonteCarlo != null)
                success &= MonteCarlo.Validate(data, errors);

            if (Sensitivity != null)
                success &= Sensitivity.Validate(data, errors);

            if (Visual != null)
                success &= Visual.Validate(data, errors);

            if (Ballot != null)
                success &= Ballot.Validate(data, errors);

            if (ForecastDate != null)
                success &= ForecastDate.Validate(data, errors);

            if (SummaryStatistics != null)
                success &= SummaryStatistics.Validate(data, errors);

            if (ModelAudit != null)
                success &= ModelAudit.Validate(data, errors);

            return success;
        }
    }


}
