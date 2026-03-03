using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FocusedObjective.Distributions
{
    public class DistributionData
    {
        public DistributionData()
        {
        }

        // private members and defaults
        private XElement _source;
        private string _name;
        private string _shape = "uniform";
        private string _parameters = "";
        private string _generator = "alf";
        private string _data = "";
        private string _separator = ",";
        private DistributionNumberType _numberType = DistributionNumberType.Double;
        private int _count = 1000;
        private double _location = 0.0;
        private double _lowBound = int.MinValue;
        private double _highBound = int.MaxValue;
        private DistributionBoundProcessing _boundProcessing = DistributionBoundProcessing.Clip;
        private double _zeroValue = 0.0;
        private ZeroHandlingEnum _zeroHandling = ZeroHandlingEnum.Keep;
        private double _multiplier = 1.0;
        private string _path = "";
        private string _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private string _thousandsSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator;

        // public properties
        public XElement Source
        {
            get { return _source; }
            set { _source = value; }
        }
        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        public string Generator
        {
            get { return _generator; }
            set { _generator = value; }
        }

        public DistributionNumberType NumberType
        {
            get { return _numberType; }
            set { _numberType = value; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public double LowBound
        {
            get { return _lowBound; }
            set { _lowBound = value; }
        }

        public double HighBound
        {
            get { return _highBound; }
            set { _highBound = value; }
        }

        public DistributionBoundProcessing BoundProcessing
        {
            get { return _boundProcessing; }
            set { _boundProcessing = value; }
        }

        public double Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public double ZeroValue
        {
            get { return _zeroValue; }
            set { _zeroValue = value; }
        }

        public ZeroHandlingEnum ZeroHandling
        {
            get { return _zeroHandling; }
            set { _zeroHandling = value; }
        }

        public double Multiplier
        {
            get { return _multiplier; }
            set { _multiplier = value; }
        }

        public string Path
        {
            get {  return _path; }
            set { _path = value; }
        }

        public string DecimalSeparator
        {
            get { return _decimalSeparator;  }
            set { _decimalSeparator = value;  }
        }

        public string ThousandsSeparator
        {
            get { return _thousandsSeparator; }
            set { _thousandsSeparator = value; }
        }
    }
}
