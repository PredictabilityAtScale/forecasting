using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class SensitivityBase : ContractDataBase
    {
        private double _sensitivityOccurrenceMultiplier = 1.0;
        private double _sensitivityEstimateMultiplier = 1.0;
        private double _sensitivityIterationEstimateMultiplier = 1.0;

        public double SensitivityOccurrenceMultiplier
        {
            get { return _sensitivityOccurrenceMultiplier; }
            set { _sensitivityOccurrenceMultiplier = value; }
        }

        public double SensitivityEstimateMultiplier
        {
            get { return _sensitivityEstimateMultiplier; }
            set { _sensitivityEstimateMultiplier = value; }
        }

        public double SensitivityIterationEstimateMultiplier
        {
            get { return _sensitivityIterationEstimateMultiplier; }
            set { _sensitivityIterationEstimateMultiplier = value; }
        }
    }
}