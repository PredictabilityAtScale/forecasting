using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public class SetupThroughputData : SensitivityBase, IValidate
    {
        public SetupThroughputData()
        {
        }

        public SetupThroughputData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private double _itemsPerIterationLowBound = 0.0;
        private double _itemsPerIterationHighBound = 0.0;
        private string _itemsPerIterationDistribution = "";

        // public properties
        public double ItemsPerIterationLowBound
        {
            get { return _itemsPerIterationLowBound; }
            set { _itemsPerIterationLowBound = value; }
        }

        public double ItemsPerIterationHighBound
        {
            get { return _itemsPerIterationHighBound; }
            set { _itemsPerIterationHighBound = value; }
        }

        public string ItemsPerIterationDistribution
        {
            get { return _itemsPerIterationDistribution; }
            set { _itemsPerIterationDistribution = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _itemsPerIterationLowBound,
                source,
                errors,
                "itemsPerIterationLowBound",
                _itemsPerIterationLowBound,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _itemsPerIterationHighBound,
                source,
                errors,
                "itemsPerIterationHighBound",
                _itemsPerIterationHighBound,
                false);

            _itemsPerIterationDistribution = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "itemsPerIterationDistribution",
                _itemsPerIterationDistribution,
                false);

                return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("iteration");

            result.Add(new XAttribute("itemsPerIterationLowBound", _itemsPerIterationLowBound.ToString()));
            result.Add(new XAttribute("itemsPerIterationHighBound", _itemsPerIterationHighBound.ToString()));
            result.Add(new XAttribute("itemsPerIterationDistribution", _itemsPerIterationDistribution.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (ItemsPerIterationDistribution == "")
                {
                    success = ContractCommon.CheckValueGreaterThan(errors, ItemsPerIterationLowBound, 0, "itemsPerIterationLowBound", "throughput", Source) && success;
                    success = ContractCommon.CheckValueGreaterThan(errors, ItemsPerIterationHighBound, 0, "itemsPerIterationHighBound", "throughput", Source) && success;
                }
                else
                {
                    SetupDistributionData dist =
                        data.Setup.Distributions.Where(d => string.Compare(d.Name, ItemsPerIterationDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, ItemsPerIterationDistribution, "setup/throughput", "throughput"), Source);
                    }

                    // distribution validated in setup/distribution

                }
            }

            return success;
        }
    }


}
