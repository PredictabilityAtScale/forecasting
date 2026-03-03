using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using System.Threading;

namespace FocusedObjective.Simulation
{

    [Serializable]
    internal class StatisticResults<T> where T:struct
    {
        internal StatisticResults(IEnumerable<T> values, int decimalRounding = 3, bool disconnect = true, string googleChartURL = null)
        {
            _values = values;
            _disconnect = disconnect;
            _decimalRounding = decimalRounding;
            _googleChartURL = googleChartURL;

            _histogram = new Dictionary<double, int>();
            _modes = new List<T>();

            if (disconnect)
                Disconnect();
        }

        internal StatisticResults(XElement sip, int decimalRounding = 3, bool disconnect = true, string googleChartURL = null)
        {
            _values = fromSip(sip);
            _disconnect = disconnect;
            _decimalRounding = decimalRounding;
            _googleChartURL = googleChartURL;

            _histogram = new Dictionary<double, int>();
            _modes = new List<T>();

            if (disconnect)
                Disconnect();
        }

        internal void Add(IEnumerable<T> values)
        {
            if (_disconnect)
                throw new InvalidOperationException("Can't add to a disconnected set of results.");

            _values.Concat(values);
        }

        private IEnumerable<T> _values;
        private bool _disconnect = true;
        private int _count;
        private double _average;

        private double _median;
        private double _min;
        private double _max;
        
        private double _populationStandardDeviation;
        private double _sampleStandardDeviation;
        private IDictionary<double, int> _histogram;
        private int _decimalRounding;
        private string _googleChartURL;
        private IList<T> _modes;
        private bool _precomputedPercentiles = false;
        private bool _precomputedSummaryStatistics = false;
        private XElement _sip = new XElement("sip");

        internal int Count
        {
            get
            {
                if (_values != null && !_precomputedSummaryStatistics)
                    _count = _values.Count();

                return _count;
            }
        }

        internal XElement Sip
        {
            get
            {
                if (_values != null)
                    if (_values.Any())
                    {
                        _sip.Add(new XAttribute("name", "sip1"));
                        _sip.Add(new XAttribute("count", Count));
                        _sip.Add(new XAttribute("type", "CSV"));
                        _sip.Add(new XAttribute("ver", "2.0"));
                        _sip.Add(new XAttribute("numberType", this.GetType().GetGenericArguments()[0].ToString()));

                         if (_values.First() is int)
                             _sip.Value = string.Join(",", _values);
                        else
                            _sip.Value = string.Join(",", _values.Cast<double>().Select(v => Math.Round(v, _decimalRounding)));
                    }

                return _sip;
 
            }
        }

        internal IEnumerable<T> fromSip(XElement sip)
        {

            if (int.Parse(sip.Attribute("count").Value) > 0)
            {
                List<T> result = new List<T>();

                if (this.GetType().GetGenericArguments()[0].ToString() == "System.Int32")
                {
                    foreach (var item in sip.Value.Split(','))
                        result.Add((T)(object)int.Parse(item));
                }
                else
                {
                    foreach (var item in sip.Value.Split(','))
                        result.Add((T)(object)double.Parse(item));

                }

                return result;
               
            }
            else
                return null;
        }

        internal double Average
        {
            get
            {
                if (_values != null && !_precomputedSummaryStatistics)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _average = _values.Cast<int>().Average(v => v);
                        else
                            _average = _values.Cast<double>().Average(v => v);
                    }

                return _average;
            }
        }
        
        
        internal double Minimum
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                        if (_values.First() is int)
                            _min = _values.Cast<int>().Min();
                        else
                            _min = _values.Cast<double>().Min();

                return _min;
            }
        }

        internal double Maximum
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                        if (_values.First() is int)
                            _max = _values.Cast<int>().Max();
                        else
                            _max = _values.Cast<double>().Max();

                return _max;
            }
        }

        internal double PopulationStandardDeviation
        {
            get
            {
                if (_values != null && !_precomputedSummaryStatistics)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _populationStandardDeviation = _values.Cast<int>().PopulationStandardDeviation(v => v);
                        else
                            _populationStandardDeviation = _values.Cast<double>().PopulationStandardDeviation(v => v);
                    }

                return _populationStandardDeviation;
            }
        }

        internal double SampleStandardDeviation
        {
            get
            {
                if (_values != null && !_precomputedSummaryStatistics)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _sampleStandardDeviation = _values.Cast<int>().SampleStandardDeviation(v => v);
                        else
                            _sampleStandardDeviation = _values.Cast<double>().SampleStandardDeviation(v => v);
                    }

                return _sampleStandardDeviation;
            }
        }

        internal double Median
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _median = _values.Cast<int>().Median(v => v);
                        else
                            _median = _values.Cast<double>().Median(v => v);
                    }

                return _median;
            }
        }

        internal IList<T> Modes
        {
            get
            {
                if (_values != null)
                    if (_values.Any())
                    {
                        var q = _values.Mode().OrderBy(v => v);

                        // make a copy to ensure garbage collection of the values
                        _modes.Clear();
                        foreach (var item in q)
                            _modes.Add(item);

                        q = null;
                    }

                return _modes;
            }
        }

        private double _fifthPercentile = 0.0;

        internal double FifthPercentile
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _fifthPercentile = _values.Cast<int>().Percentile(v => v, 5.0);
                        else
                            _fifthPercentile = _values.Cast<double>().Percentile(v => v, 5.0);
                    }

                return _fifthPercentile;
            }
        }

        private double _twentyFifthPercentile = 0.0;

        internal double TwentyFifthPercentile
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _twentyFifthPercentile = _values.Cast<int>().Percentile(v => v, 25.0);
                        else
                            _twentyFifthPercentile = _values.Cast<double>().Percentile(v => v, 25.0);
                    }

                return _twentyFifthPercentile;
            }
        }

        private double _seventyFifthPercentile = 0.0;

        internal double SeventyFifthPercentile
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _seventyFifthPercentile = _values.Cast<int>().Percentile(v => v, 75.0);
                        else
                            _seventyFifthPercentile = _values.Cast<double>().Percentile(v => v, 75.0);
                    }

                return _seventyFifthPercentile;
            }
        }

        private double _ninetyFifthPercentile = 0.0;

        internal double NinetyFifthPercentile
        {
            get
            {
                if (_values != null && !_precomputedPercentiles)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            _ninetyFifthPercentile = _values.Cast<int>().Percentile(v => v, 95.0);
                        else
                            _ninetyFifthPercentile = _values.Cast<double>().Percentile(v => v, 95.0);
                    }

                return _ninetyFifthPercentile;
            }
        }

        internal IDictionary<double, int> Histogram
        {
            get
            {
                if (_values != null)
                    if (_values.Any())
                    {
                        IDictionary<double,int> temp; 

                        if (_values.First() is int)
                            temp = _values.Cast<int>().Histogram(20);
                        else
                            temp = _values.Cast<double>().Select(v => Math.Round(v, _decimalRounding)).Histogram(20);

                        // do we need to make a copy here to ensure _values goes out of scope and is GC'd? Yes.
                        _histogram.Clear();
                        foreach (var g in temp)
                            _histogram.Add(g.Key, g.Value);

                        temp = null;
                    }

                return _histogram;
            }
        }

        internal string HistogramURL
        {
            get
            {
                if (this.Histogram != null && 
                    this.Histogram.Count > 0 &&
                    string.IsNullOrEmpty(_googleChartURL) == false)
                {

                    //@"http://chart.apis.google.com/chart?chxr=0,0,{4}|1,{5},{6}&chxt=y,x&chds=0,{7}&chbh=a&chs=600x400&cht=bvg&chco=3072F3&{0}&chdl={1}&chg=0,10&chtt={2}";

                    int[] values = this.Histogram.Select(g => g.Value).ToArray();
                    int ymin = this.Histogram.Min(g => g.Value);
                    int ymax = this.Histogram.Max(g => g.Value);
                    double xmin = this.Histogram.Min(g => g.Key);
                    double xmax = this.Histogram.Max(g => g.Key);
                    string data = FocusedObjective.Simulation.  GoogleChartHelpers.Encode(values);

                    string legend = "1:|";
                    foreach (var key in this.Histogram.Keys)
                        legend += key.ToString() + "|";

                    // make legend labels
                    if (Histogram.Count == 20)
                    {
                        // range legend
                        legend += "2:||||Note:+Histogram+is+showing+%3C%3D+ranges.|";
                    }
                    else
                    {
                        // explicit legend
                        legend += "2:||||Note:+Histogram+is+showing+exact+values.|";
                    }

                    return string.Format(_googleChartURL, data, "Frequency", "Histogram", ymin, ymax, xmin, xmax, ymax, legend);
                }
                else
                    return string.Empty;
            }
        }

        internal XElement AsXML(string elementName)
        {
            // rangify the mode string if more than 5 elements...
            string modeString = string.Join("|", (this.Modes.Select(v => 
                Math.Round(double.Parse(v.ToString()), _decimalRounding).ToString()).ToArray()));
            
            if (Modes.Count > 5)
            {
                modeString = string.Empty;
                
                // Use Histogram bucket instead
                var maxs = this.Histogram.Where(h => h.Value == this.Histogram.Max(v => v.Value));

                var keys = this.Histogram.Keys.ToList();

                foreach (var item in maxs)
                {
                    // make string ranges
                    string low;
                    int index = keys.IndexOf(item.Key);
                    if (index == 0)
                        low = this.Minimum.ToString();
                    else
                        low = keys[index - 1].ToString();
                    
                    string modeRange = low + " to " + item.Key.ToString();

                    if (modeString != string.Empty)
                        modeString += ", ";

                    modeString += modeRange;
                }
            }

           return new XElement(elementName, 
                new XAttribute("count", this.Count),
                new XAttribute("minimum", Math.Round(double.Parse(this.Minimum.ToString()), _decimalRounding).ToString()),
                new XAttribute("average", Math.Round(double.Parse(this.Average.ToString()), _decimalRounding).ToString()),
                new XAttribute("maximum", Math.Round(double.Parse(this.Maximum.ToString()), _decimalRounding).ToString()),
                new XAttribute("populationStandardDeviation", Math.Round(double.Parse(this.PopulationStandardDeviation.ToString()), _decimalRounding).ToString()),
                new XAttribute("sampleStandardDeviation", Math.Round(double.Parse(this.SampleStandardDeviation.ToString()), _decimalRounding).ToString()),
                new XAttribute("median", Math.Round(double.Parse(this.Median.ToString()), _decimalRounding).ToString()),
                new XAttribute("mode", modeString),
                new XAttribute("fifthPercentile", Math.Round(double.Parse(this.FifthPercentile.ToString()), _decimalRounding).ToString()),
                new XAttribute("twentyFifthPercentile", Math.Round(double.Parse(this.TwentyFifthPercentile.ToString()), _decimalRounding).ToString()),
                new XAttribute("seventyFifthPercentile", Math.Round(double.Parse(this.SeventyFifthPercentile.ToString()), _decimalRounding).ToString()),
                new XAttribute("ninetyFifthPercentile", Math.Round(double.Parse(this.NinetyFifthPercentile.ToString()), _decimalRounding).ToString()),
                histogramToXML(this.Histogram),
                HistogramToCustomDistribution(this.Histogram, this.Histogram.Count() < 20),
                Sip);
        }

        internal XElement HistogramToCustomDistribution(
            IDictionary<double, int> histogram,
            bool exactValues,
            string name = "dist1")
        {
            if (histogram != null && histogram.Any())
            {
                XElement result = new XElement("distribution",
                    new XAttribute("name", name),
                    new XAttribute("numberType", this.GetType().GetGenericArguments().First().Name.ToLower()),
                    new XAttribute("decimalSeparator", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    new XAttribute("thousandsSeparator", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator)
                    );

                string argument = string.Empty;

                if (exactValues) 
                {
                    // exact values
                    result.Add(new XAttribute("shape", "discrete"));

                    foreach (var group in histogram)
                    {
                        if (argument != string.Empty)
                            argument += "|";

                        // value
                        argument += Math.Round(double.Parse(group.Key.ToString()), _decimalRounding).ToString() +"|";

                        // percentage
                        double percentage = Math.Round((group.Value * 1.0) / (this.Count * 1.0), _decimalRounding);
                        argument += percentage.ToString();
                    }
                }
                else
                {
                    // segmented by ranges
                    result.Add( new XAttribute("shape", "customrange"));

                    var nextLow = Math.Round(double.Parse(this.Minimum.ToString()), _decimalRounding).ToString();

                    foreach (var group in histogram)
                    {
                        if (argument != string.Empty)
                            argument += "|";

                        // values
                        argument += nextLow + "|";
                        argument += group.Key.ToString() + "|";
                        nextLow =  Math.Round(double.Parse(group.Key.ToString()), _decimalRounding).ToString();

                        // percentage
                        double percentage = Math.Round((group.Value * 1.0) / (this.Count * 1.0), _decimalRounding);
                        argument += percentage.ToString();
                    }
                }

                result.Add(new XAttribute("parameters", argument));

                return result;
            }
            else
            {
                return null;
            }
        }

        private XElement histogramToXML(IDictionary<double, int> histogram)
        {
            if (histogram != null)
            {
                XElement result = null;

                if (histogram.Count < 20)
                {
                    // exact values
                    int bin = 1;

                    var q = from g in histogram
                            select new XElement("group",
                                new XAttribute("bin", bin++),
                                new XAttribute("value", g.Key.ToString()),
                                new XAttribute("count", g.Value));

                    result = new XElement("histogram",
                        q);
                }
                else
                {
                    // segmented by ranges
                    double step = histogram.ElementAt(1).Key - histogram.ElementAt(0).Key;
                    double start = histogram.ElementAt(0).Key - step - step;

                    int bin = 1;

                    var q = from g in histogram
                            select new XElement("group",
                                new XAttribute("bin", bin++),
                                new XAttribute("upToAndIncluding", g.Key.ToString()),
                                new XAttribute("count", g.Value));
                    
                    result = new XElement("histogram",
                        q);
                }

                if (!string.IsNullOrEmpty(_googleChartURL))
                    result.Add(new XElement("chart", new XCData(this.HistogramURL)));

                return result;
            }
            else
            {
                return new XElement("histogram");
            }
        }


        internal void Disconnect()
        {
            // fast passes...
            computePercentiles(); // sorted data
            computeSummaryStatistics(); // looped data

            object v;
            v = Count; //
            v = Average; //
            v = Minimum; //
            v = Maximum; // 
            v = Median; //
            v = Modes;
            v = PopulationStandardDeviation; //
            v = SampleStandardDeviation; //
            v = Histogram;
            v = HistogramURL;
            v = FifthPercentile; //
            v = TwentyFifthPercentile; //
            v = SeventyFifthPercentile; //
            v = NinetyFifthPercentile; //
            v = Sip; //

            _values = null;
        }

        private void computeSummaryStatistics()
        {
            double[] results = null;

            if (_values != null)
                if (_values.Any())
                {
                    if (_values.First() is int)
                        results = _values.Cast<int>().SummaryStatistics(v => v);
                    else
                        results = _values.Cast<double>().SummaryStatistics();
                }

            if (results != null && results.Length == 5)
            {
                _count = (int)Math.Truncate(results[0]);
                _average = results[2];
                _sampleStandardDeviation = Math.Sqrt(results[3]);
                _populationStandardDeviation = Math.Sqrt(results[4]);
               
                _precomputedSummaryStatistics = true;

            }
        }

        private void computePercentiles()
        {
            double[] results = null;
 
             if (_values != null)
                    if (_values.Any())
                    {
                        if (_values.First() is int)
                            results = _values.Cast<int>().SummaryPercentiles(v => v);
                        else
                            results = _values.Cast<double>().SummaryPercentiles();
                    }

             if (results != null && results.Length == 7)
             {
                 _min = results[0];
                 _fifthPercentile = results[1];
                 _twentyFifthPercentile = results[2];
                 _median = results[3];
                 _seventyFifthPercentile = results[4];
                 _ninetyFifthPercentile = results[5];
                 _max = results[6];

                 _precomputedPercentiles = true;
                 
             }
        }
    }
}
