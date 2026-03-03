using System;
using Troschuetz.Random;
using System.Linq;
using System.Collections.Generic;
using FocusedObjective.Simulation.Extensions;

namespace Troschuetz.Random
{
	/// <summary>
    /// Provides generation of custom range distributed random numbers.
	/// </summary>
    public class CustomRangeDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter data which is used for generation of custom random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidData"/> to determine whether a value is valid and therefor assignable.</remarks>
        public string DataString
        {
            get
            {
                return this.data;
            }
            set
            {
                if (this.IsValidData(value))
                {
                    this.data = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter data which is used for generation of custom random numbers.
        /// </summary>
        private string data;

        /// <summary>
        /// Gets or sets the parameter count which is used for generation of custom distributed random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidCount"/> to determine whether a value is valid and therefor assignable.</remarks>
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
        /// Stores an intermediate result for generation of custom distributed random numbers.
        /// </summary>
        /// <remarks>
        /// Speeds up random number generation cause this value only depends on distribution parameters 
        ///   and therefor doesn't need to be recalculated in successive executions of <see cref="NextDouble"/>.
        /// </remarks>
        private List<double> helper1;
        private IEnumerator<double> helper2;
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRangeDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public CustomRangeDistribution()
            : this(new StandardGenerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRangeDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public CustomRangeDistribution(Generator generator)
            : base(generator)
        {
            this.data = "";
            this.count = 1000;
            this.UpdateHelpers();
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Data"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the samples data is a comma-separated list of one or more double (low-bound value),double (high-bound value), double(percentage) otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidData(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                IEnumerable<decimal> dataAsDecimal;

                if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    dataAsDecimal = from v in value.Split(new string[] { ",", "|", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                    select decimal.Parse(v.Trim());
                else
                    dataAsDecimal = from v in value.Split(new string[] { "|", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                    select decimal.Parse(v.Trim());


                decimal sum = 0;
                int count = dataAsDecimal.Count();

                for (int i = 2; i < count; i = i + 3)
                {
                    if (i < count)
                        sum = sum + dataAsDecimal.ElementAt(i);
                }

                // sum of the percentages should add to 1. If the data is empty, this would be 0
                return Math.Abs(sum - 1M) < 0.01M;

                //TODO:check that the low-bound is less than the high bound for each tuple

            }
            catch
            {
                return false;
            }
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
        /// Updates the helper variables that store intermediate results for generation of custom distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            if (!string.IsNullOrEmpty(data) && count > 0)
            {
                List<double> dataAsDoubles;

                if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    dataAsDoubles = (from v in data.Split(new string[] { ",", "|", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                     select double.Parse(v.Trim())).ToList();
                else
                    dataAsDoubles = (from v in data.Split(new string[] { "|", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                     select double.Parse(v.Trim())).ToList();

                List<double> lowBoundvalues = new List<double>();
                List<double> highBoundvalues = new List<double>();
                List<double> percentages = new List<double>();

                for (int i = 0; i < dataAsDoubles.Count; i = i + 3)
                {
                    lowBoundvalues.Add(dataAsDoubles[i]);
                    highBoundvalues.Add(dataAsDoubles[i+1]);
                    percentages.Add(dataAsDoubles[i+2]);
                }

                helper1 = new List<double>(count);
                for (int i = 0; i < Math.Min(lowBoundvalues.Count, percentages.Count); i++)
                {
                    int number = (int)Math.Round(Math.Ceiling(count * percentages[i]));

                    for (int j = 0; j < number; j++)
                    {
                        
                        helper1.Add(this.Generator.NextDouble(
                            lowBoundvalues[i],
                            highBoundvalues[i])
                            );
                    }
                }

                //shuffle
                helper1 = helper1
                    .OrderBy(d => this.Generator.Next())
                    .ToList();

                this.helper2 = helper1.GetEnumerator();
            }
        }
        #endregion

        #region overridden Distribution members
        /// <summary>
        /// Gets the minimum possible value of custom distributed random numbers.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return this.helper1.Min();
            }
        }

        /// <summary>
        /// Gets the maximum possible value of custom distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.helper1.Max();
            }
        }

        /// <summary>
        /// Gets the mean value of the custom distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.helper1.Average();
            }
        }

        /// <summary>
        /// Gets the median of custom distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.helper1.Median();
            }
        }

        /// <summary>
        /// Gets the variance of custom distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return this.helper1.PopulationVariance();
            }
        }

        /// <summary>
        /// Gets the mode of the custom distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return this.helper1.Mode().ToArray();
            }
        }

        /// <summary>
        /// Returns a custom distributed floating point random number.
        /// </summary>
        /// <returns>A custom distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            if (helper2.MoveNext())
                return helper2.Current;
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