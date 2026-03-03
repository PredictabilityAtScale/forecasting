
using System;
using Troschuetz.Random;
using System.Linq;
using System.Collections.Generic;
using FocusedObjective.Simulation.Extensions;
using System.Xml.Linq;
using FocusedObjective.Distributions;

namespace Troschuetz.Random
{
	/// <summary>
    /// Provides generation of bootstrapped distributed random numbers.
	/// </summary>

    public class FromSamplesDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter samples which is used for generation of from samples random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidSamples"/> to determine whether a value is valid and therefor assignable.</remarks>
        public string Samples
        {
            get
            {
                return this.samples;
            }
            set
            {
                if (this.IsValidSamples(value))
                {
                    this.samples = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter samples which is used for generation of from samples distributed random numbers.
        /// </summary>
        private string samples;

        /// <summary>
        /// Gets or sets the parameter count which is used for generation of from samples distributed random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidWindow"/> to determine whether a value is valid and therefor assignable.</remarks>
        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                if (this.IsValidCount(value))
                {
                    this.count = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter 'count' which is used for generation of markov chain distributed random numbers.
        /// </summary>
        private int count = 1000;

        /// <summary>
        /// Stores an intermediate result for generation of uniformly distributed random numbers.
        /// </summary>
        /// <remarks>
        /// Speeds up random number generation cause this value only depends on distribution parameters 
        ///   and therefor doesn't need to be recalculated in successive executions of <see cref="NextDouble"/>.
        /// </remarks>
        private Distribution helper1;
        
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="FromSamplesDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public FromSamplesDistribution()
            : this(new StandardGenerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromSamplesDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public FromSamplesDistribution(Generator generator)
            : base(generator)
        {
            this.samples = "";
            this.count = 1000;
            this.UpdateHelpers();
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Samples"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the samples data is a comma-separated list of at least the window size, otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidSamples(string input)
        {
            string value = replaceSpecialCharacters(input);

            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                IEnumerable<double> sampleAsDoubles;

                // if not a culture that uses a , as a decimal separator - try , | and newlines, otherwise just | and newlines
                if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    sampleAsDoubles = from v in value.Split(new string[] { ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                      select double.Parse(v.Trim());
                else
                    sampleAsDoubles = from v in value.Split(new string[] { "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                      select double.Parse(v.Trim());


                return sampleAsDoubles.Any();

            }
            catch
            {
                return false;
            }
        }

        private string replaceSpecialCharacters(string input)
        {
            string result = input;

            result = result.Replace(@"\n", char.Parse("\n").ToString());
            result = result.Replace(@"\r", char.Parse("\r").ToString());
            result = result.Replace("\t", "");
            result = result.Replace(" ", "");

            return result;
        }

        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Count"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is greater than 1; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidCount(double value)
        {
            return value > 1;
        }

        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of uniformly distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            string value = replaceSpecialCharacters(this.Samples);

            if (!string.IsNullOrEmpty(value))
            {
                IEnumerable<double> sampleAsDoubles;

                if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    sampleAsDoubles = from v in value.Split(new string[] { ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                      select double.Parse(v.Trim());
                else
                    sampleAsDoubles = from v in value.Split(new string[] { "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                      select double.Parse(v.Trim());

                FocusedObjective.Simulation.StatisticResults<double> stats = new FocusedObjective.Simulation.StatisticResults<double>(
                    sampleAsDoubles);

                XElement xml = stats.AsXML("stats");

                var distXML = xml.Element("distribution");

                if (distXML != null)
                {
                    DistributionData data = new DistributionData();

                    data.Name = distXML.Attribute("name").Value;
                    data.Shape = distXML.Attribute("shape").Value;
                    data.NumberType = distXML.Attribute("numberType").Value == "double" ? DistributionNumberType.Double : DistributionNumberType.Integer;
                    data.Parameters = distXML.Attribute("parameters").Value;

                    helper1 = FocusedObjective.Distributions.DistributionHelper.CreateDistribution(data);
                }
            }
        }


        #endregion

        #region overridden Distribution members
        /// <summary>
        /// Gets the minimum possible value of from sample distributed random numbers.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return this.helper1.Minimum;
            }
        }

        /// <summary>
        /// Gets the maximum possible value of from sample distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.helper1.Maximum;
            }
        }

        /// <summary>
        /// Gets the mean value of the from sample distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.helper1.Mean;
            }
        }

        /// <summary>
        /// Gets the median of from sample distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.helper1.Median;
            }
        }

        /// <summary>
        /// Gets the variance of from sample distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return this.helper1.Variance;
            }
        }

        /// <summary>
        /// Gets the mode of the from sample distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return this.helper1.Mode;
            }
        }

        /// <summary>
        /// Returns a uniformly distributed floating point random number.
        /// </summary>
        /// <returns>A from sample distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            double result = 0.0;

            if (helper1 != null)
                result = helper1.NextDouble();

            return result;
        }
        #endregion
    }
}