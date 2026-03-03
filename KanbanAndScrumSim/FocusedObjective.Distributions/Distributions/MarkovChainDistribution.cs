
using System;
using Troschuetz.Random;
using System.Linq;
using System.Collections.Generic;
using FocusedObjective.Simulation.Extensions;

namespace Troschuetz.Random
{
	/// <summary>
    /// Provides generation of markov-chain distributed random numbers.
	/// </summary>

    public class MarkovChainDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter samples which is used for generation of markov-chain random numbers.
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
        /// Stores the parameter samples which is used for generation of markov chain distributed random numbers.
        /// </summary>
        private string samples;

        /// <summary>
        /// Gets or sets the parameter window which is used for generation of markov chain distributed random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidWindow"/> to determine whether a value is valid and therefor assignable.</remarks>
        public int Window
        {
            get
            {
                return this.window;
            }
            set
            {
                if (this.IsValidWindow(value))
                {
                    this.window = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter 'window' which is used for generation of markov chain distributed random numbers.
        /// </summary>
        private int window;
        
        /// <summary>
        /// Gets or sets the parameter count which is used for generation of markov chain distributed random numbers.
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
        private IEnumerable<double> helper1;
        private List<double> helper2;
        private IEnumerator<double> helper3;
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkovChainDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public MarkovChainDistribution()
            : this(new StandardGenerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkovChainDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public MarkovChainDistribution(Generator generator)
            : base(generator)
        {
            this.samples = "";
            this.window = 0;
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

            // at least window size count and valid doubles
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

                return sampleAsDoubles.Count() >= window;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Beta"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is greater than 1; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidWindow(double value)
        {
            return value > 1;
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
            if (!string.IsNullOrEmpty(samples) && window > 0)
            {
                string value = replaceSpecialCharacters(samples);

                // if not a culture that uses a , as a decimal separator - try , | and newlines, otherwise just | and newlines
                if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    helper2 = (from v in value.Split(new string[] { ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                              select double.Parse(v.Trim())).ToList();
                else
                    helper2 = (from v in value.Split(new string[] { "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                               select double.Parse(v.Trim())).ToList();

                this.helper1 = FocusedObjective.Distributions.Markov.MarkovChain.GenerateOccurrenceRatesBasedOnSample<double>(
                    this.helper2,
                    window,
                    count);

                this.helper3 = this.helper1.GetEnumerator();
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
        #endregion

        #region overridden Distribution members
        /// <summary>
        /// Gets the minimum possible value of uniformly distributed random numbers.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return this.helper2.Min();
            }
        }

        /// <summary>
        /// Gets the maximum possible value of uniformly distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.helper2.Max();
            }
        }

        /// <summary>
        /// Gets the mean value of the uniformly distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.helper2.Average();
            }
        }

        /// <summary>
        /// Gets the median of uniformly distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.helper2.Median();
            }
        }

        /// <summary>
        /// Gets the variance of uniformly distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return this.helper2.PopulationVariance();
            }
        }

        /// <summary>
        /// Gets the mode of the uniformly distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return this.helper2.Mode().ToArray();
            }
        }

        /// <summary>
        /// Returns a uniformly distributed floating point random number.
        /// </summary>
        /// <returns>A markov chain distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            if (helper3.MoveNext())
                return helper3.Current;
            else
            {
                // loop again
                UpdateHelpers();
                return NextDouble();
            }
        }
        #endregion
    }
}