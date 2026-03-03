using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public class ExecuteForecastDateData : ContractDataBase, IValidate
    {
        public ExecuteForecastDateData()
        {
        }
        
        public ExecuteForecastDateData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private int _cycles = 1000;
        private ForecastPermutationsEnum _permutations = ForecastPermutationsEnum.None;
        private double _likelihood = 95.0;
        private bool _returnProgressData = false;

        // public properties
        
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        public ForecastPermutationsEnum Permutations
        {
            get { return _permutations; }
            set { _permutations = value; }
        }

        public double Likelihood
        {
            get { return _likelihood; }
            set { _likelihood = value; }
        }


        public bool ReturnProgressData
        {
            get { return _returnProgressData; }
            set { _returnProgressData = value; }
        }

        // methods
        protected bool fromXML(XElement source, XElement errors)
        {
            bool result = true;

            result = result && ContractCommon.ReadAttributeIntValue(
                out _cycles,
                source,
                errors,
                "cycles",
                _cycles
                );

            _permutations = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "permutations",
                "none", ForecastPermutationsEnum.None,
                "deliverables", ForecastPermutationsEnum.Deliverables,
                "risk", ForecastPermutationsEnum.Deliverables,
                "sequentialdeliverables", ForecastPermutationsEnum.SequentialDeliverables,
                "sequentialbacklog", ForecastPermutationsEnum.SequentialBacklog,
                "targetDate", ForecastPermutationsEnum.StartDate

                );

            result = result && ContractCommon.ReadAttributeDoubleValue(
                out _likelihood,
                source,
                errors,
                "likelihood",
                _likelihood,
                false
                );

            _returnProgressData = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "returnProgressData",
                "false", false,
                "no", false,
                "No", false,
                "true", true,
                "yes", true,
                "Yes", true);


            return result;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("forecastDate");
        
            result.Add(new XAttribute("cycles", _cycles.ToString()));
            result.Add(new XAttribute("permutations", _permutations.ToString()));
            result.Add(new XAttribute("likelihood", _likelihood.ToString()));
            result.Add(new XAttribute("returnProgressData", _returnProgressData.ToString()));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            
            success &= ContractCommon.CheckValueGreaterThan(errors, Cycles, 0, "cycles", "execute/forecastDate", Source);
            
            return success;
        }

    }


}
