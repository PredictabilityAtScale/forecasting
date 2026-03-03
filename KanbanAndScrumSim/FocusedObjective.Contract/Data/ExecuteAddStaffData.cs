using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class ExecuteAddStaffData : ContractDataBase, IValidate
    {
        public ExecuteAddStaffData()
        {
        }

        public ExecuteAddStaffData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _cycles = 1000;
        private int _count = 1;
        private OptimizeForLowestEnum _optimizeForLowest = OptimizeForLowestEnum.Intervals;
        private string _googleImprovementUrlFormat = @"http://chart.apis.google.com/chart?chxt=y&chbh=a&chs=600x400&cht=bvg&chco={1}&chd=t:{2}&chdl={3}&chds={4},{5}&chxr=0,{4},{5}&chg=0,10&chtt={0}+Improvement";
        private List<ExecuteAddStaffColumnsData> _executeAddStaffColumns = new List<ExecuteAddStaffColumnsData>();

        // public properties
        
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public OptimizeForLowestEnum OptimizeForLowest
        {
            get { return _optimizeForLowest; }
            set { _optimizeForLowest = value; }
        }

        public string GoogleImprovementUrlFormat
        {
            get { return _googleImprovementUrlFormat; }
            set { _googleImprovementUrlFormat = value; }
        }

        public List<ExecuteAddStaffColumnsData> Columns
        {
            get { return _executeAddStaffColumns; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            result = result && ContractCommon.ReadAttributeIntValue(
                out _cycles,
                source,
                errors,
                "cycles",
                _cycles
                );

            result = result && ContractCommon.ReadAttributeIntValue(
                out _count,
                source,
                errors,
                "count",
                _count,
                false
                );

            _optimizeForLowest = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "optimizeForLowest",
                "intervals", OptimizeForLowestEnum.Intervals,
                "queuedAndEmpty", OptimizeForLowestEnum.QueuedAndEmpty,
                "cycleTime", OptimizeForLowestEnum.CycleTime,
                "empty", OptimizeForLowestEnum.Empty,
                "queued", OptimizeForLowestEnum.Queued);

            _googleImprovementUrlFormat = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "googleImprovementUrlFormat",
                _googleImprovementUrlFormat,
                false);

            // add the columns data
            foreach (XElement col in source.Elements("column"))
            {
                _executeAddStaffColumns.Add(
                    new ExecuteAddStaffColumnsData(col, errors));
            }

            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("addStaff");

            result.Add(new XAttribute("cycles", _cycles.ToString()));
            result.Add(new XAttribute("count", _count.ToString()));
            result.Add(new XAttribute("optimizeForLowest", _optimizeForLowest.ToString()));
            result.Add(new XAttribute("googleImprovementUrlFormat", _googleImprovementUrlFormat.ToString()));

            foreach (var col in _executeAddStaffColumns)
                result.Add(col.AsXML(simType));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // must be > 0
            success &= ContractCommon.CheckValueGreaterThan(errors, Cycles, 0, "cycles", "execute/addStaff", Source);

            // validate the columns
            foreach (var col in Columns)
                success &= col.Validate(data, errors);

            return success;
        }




    }


}
