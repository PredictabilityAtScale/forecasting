using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using LiveCharts;

namespace FocusedObjective.KanbanSim
{
    public class StatisticData : INotifyPropertyChanged
    {
        private int _decimals = 3;
        private XElement _source = null;
        private bool _histogramShowingExactValues = false;
        public List<string> HistogramLabels = new List<string> { };
        public ChartValues<int> HistogramCounts = new ChartValues<int> { };


        public StatisticData(int decimalPlaces = 3)
        {
            _decimals = decimalPlaces;
            HistogramLabels = new List<string> { "0-10", "10-20", "20-30", "30-40", "40-50" };
            HistogramCounts = new ChartValues<int> { 5, 10, 15, 20, 25 };
        }

        public void Reset()
        {
            SampleCount = "0";
            Minimum = "0";
            Average = "0";
            Maximum = "0";
            StandardDeviation = "0";
            Median = "0";
            Mode = "0";
            FifthPercentile = "0";
            TwentyFifthPercentile = "0";
            SeventyFifthPercentile = "0";
            NinetyFifthPercentile = "0";
            _source = null;
            _histogramShowingExactValues = false;
            Distribution = string.Empty;
            RandomNumbers = string.Empty;
            _histogramData = null;
        }

        public void FromXML(XElement root)
        {
            _source = root;

            SampleCount = root.Attribute("count").Value;
            Minimum = root.Attribute("minimum").Value;
            Average = root.Attribute("average").Value;
            Maximum = root.Attribute("maximum").Value;
            StandardDeviation = root.Attribute("sampleStandardDeviation").Value;
            Median = root.Attribute("median").Value;
            Mode = root.Attribute("mode").Value;
            FifthPercentile = root.Attribute("fifthPercentile").Value;
            TwentyFifthPercentile = root.Attribute("twentyFifthPercentile").Value;
            SeventyFifthPercentile = root.Attribute("seventyFifthPercentile").Value;
            NinetyFifthPercentile = root.Attribute("ninetyFifthPercentile").Value;
            Distribution = root.Element("distribution") != null ? root.Element("distribution").ToString() : string.Empty;
            RandomNumbers = root.Element("data") != null ? root.Element("data").Value : string.Empty;
        }

        private int _count;
        private double _minimum;
        private double _average;
        private double _maximum;
        private double _standardDeviation;
        private string _mode;
        private double _median;
        private double _fifthPercentile;
        private double _twentyFifthPercentile;
        private double _seventyFifthPercentile;
        private double _ninetyFifthPercentile;
        private string _distribution;
        private string _randomNumbers;

        public string SampleCount
        {
            get { return _count.ToString(); }
            set
            {
                _count = int.Parse(value);
                OnPropertyChanged("SampleCount");
            }
        }

        public string Minimum
        {
            get { return Math.Round(_minimum, _decimals).ToString(); }
            set 
            {
                _minimum = double.Parse(value);
                OnPropertyChanged("Minimum"); 
            }
        }

        public string Average
        {
            get { return Math.Round(_average, _decimals).ToString(); }
            set 
            {
                _average = double.Parse(value);  
                OnPropertyChanged("Average"); 
            }
        }

        public string Maximum
                {
                    get { return Math.Round(_maximum, _decimals).ToString(); }
            set 
            {
                _maximum = double.Parse(value);
                OnPropertyChanged("Maximum"); 
            }
        }

        public string StandardDeviation
                {
                    get { return Math.Round(_standardDeviation, _decimals).ToString(); }
            set 
            {
                _standardDeviation = double.Parse(value);
                OnPropertyChanged("StandardDeviation"); 
            }
        }

        public string Mode
                {
            get { return _mode.ToString(); }
            set 
            { 
                _mode = value; 
                OnPropertyChanged("Mode"); 
            }
        }

        public string Median
                {
                    get { return Math.Round(_median, _decimals).ToString(); }
            set 
            {
                _median = double.Parse(value); 
                OnPropertyChanged("Median"); 
            }
        }

        public string FifthPercentile
                {
                    get { return Math.Round(_fifthPercentile, _decimals).ToString(); }
            set 
            {
                _fifthPercentile = double.Parse(value);
                OnPropertyChanged("FifthPercentile"); 
            }
        }

        public string TwentyFifthPercentile
        {
            get { return Math.Round(_twentyFifthPercentile, _decimals).ToString(); }
            set 
            {
                _twentyFifthPercentile = double.Parse(value);
                OnPropertyChanged("TwentyFifthPercentile"); 
            }
        }

        public string SeventyFifthPercentile
        {
            get { return Math.Round(_seventyFifthPercentile, _decimals).ToString(); }
            set 
            {
                _seventyFifthPercentile = double.Parse(value);
                OnPropertyChanged("SeventyFifthPercentile"); 
            }
        }

        public string NinetyFifthPercentile
        {
            get { return Math.Round(_ninetyFifthPercentile, _decimals).ToString(); }
            set
            {
                _ninetyFifthPercentile = double.Parse(value);
                OnPropertyChanged("NinetyFifthPercentile");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool HistogramShowingExactValues
        {
            get { return _histogramShowingExactValues; }
            set
            {
                _histogramShowingExactValues = value;
                OnPropertyChanged("HistogramShowingExactValues");
            }
        }

        public string Distribution
        {
            get
            {
                return _distribution;
            }
            set
            {
                _distribution = value;
                OnPropertyChanged("Distribution");
            }
        }

        public string RandomNumbers
        {
            get
            {
                return _randomNumbers;
            }
            set
            {
                _randomNumbers = value;
                OnPropertyChanged("RandomNumbers");
            }
        }
                
        List<HistogramBin> _histogramData;

        public List<HistogramBin> HistogramData
        {
            get
            {
                _histogramData = new List<HistogramBin>();

                if (_source != null)
                {
                    bool exactValues = true;
                    double previousValue = this._minimum;
                    int cumulativeCount = 0;

                    foreach (var group in _source.Element("histogram").Elements("group"))
                    {
                        double value = 0.0;
                        int count = 0;

                        // exact or ranges?
                        if (group.Attribute("value") != null)
                        {
                            value = double.Parse(group.Attribute("value").Value);
                            exactValues = true;
                        }
                        else
                        {
                            value = double.Parse(group.Attribute("upToAndIncluding").Value);
                            exactValues = false;
                        }

                        count = int.Parse(group.Attribute("count").Value);

                        // calculate cumulative percentile
                        cumulativeCount += count;
                        double p = Math.Ceiling(((double)cumulativeCount / (double)this._count) * 100);

                        if (exactValues)
                            _histogramData.Add(new HistogramBin(count, value, value, p));
                        else
                            _histogramData.Add(new HistogramBin(count, previousValue, value, p));

                        previousValue = value;
                    }
                }

                return _histogramData;
            }
        }

       
        /*

public ObservableCollection<string> HistogramLabels
        {
            get
            {
                _histogramLabels = new List<string>();
                if (_source != null)
                {
                    foreach (var group in _source.Element("histogram").Elements("group"))
                    {
                        if (group.Attribute("value") != null)
                            _histogramLabels.Add(group.Attribute("value").Value);
                        else
                            _histogramLabels.Add(string.Format("{0} to {1}", group.Attribute("from").Value, group.Attribute("upToAndIncluding").Value));
                    }
                }
                return new ObservableCollection<string>(_histogramLabels);
            }
        }

        public ObservableCollection<string> HistogramCounts
        {
            get
            {
                _histogramCounts = new List<int>();
                if (_source != null)
                {
                    foreach (var group in _source.Element("histogram").Elements("group"))
                    {
                        _histogramCounts.Add(int.Parse(group.Attribute("count").Value));
                    }
                }
                return new ObservableCollection<string>(_histogramCounts.Select(c => c.ToString()));
            }
        }
        */
    }

    public class HistogramBin : INotifyPropertyChanged
    {
        private int count; 
        private double uppervalue;
        private double lowervalue;
        private double cumulativePercentile = 0.0;

        public HistogramBin(int count, double lowervalue, double uppervalue, double cumulativePercentile)
        {
            Count = count;
            LowerValue = lowervalue;
            UpperValue = uppervalue;
            CumulativePercentile = cumulativePercentile;
        }

        public string BinLabel
        {
            get
            {
                if (ExactValue)
                    return UpperValue.ToString();
                else
                    return string.Format("{0} to\n{1}", LowerValue, UpperValue);
            }
        }

        public bool ExactValue
        {
            get { return LowerValue == UpperValue; }
        }

        public int Count
        {
            get
            { return count; }
            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged("Count");
                }
            }
        }

        public double LowerValue
        {
            get
            { return lowervalue; }
            set
            {
                if (this.lowervalue != value)
                {
                    this.lowervalue = value;
                    OnPropertyChanged("LowerValue");
                }
            }
        }

        public double UpperValue
        {
            get
            { return uppervalue; }
            set
            {
                if (this.uppervalue != value)
                {
                    this.uppervalue = value;
                    OnPropertyChanged("UpperValue");
                }
            }
        }

        public double CumulativePercentile
        {
            get
            { return cumulativePercentile; }
            set
            {
                if (this.cumulativePercentile != value)
                {
                    this.cumulativePercentile = value;
                    OnPropertyChanged("CumulativePercentile");
                }
            }
        }
        
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    } 

}
