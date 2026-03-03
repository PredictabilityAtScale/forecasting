
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
    /// Provides generation from Model simulation
	/// </summary>

    public class ModelDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the path to the model
        /// </summary>
        /// <remarks>Call <see cref="IsValidPath"/> to determine whether a value is valid and therefor assignable.</remarks>
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (this.IsValidPath(value))
                {
                    this.path = value;
                    this.UpdateHelpers();
                }
            }
        }

        private string path = "";
        private SIPDistribution sipDistribution = null;
        XElement sip = null;

        public XElement Sip
        {
            get { return sip; }
            set 
            { 
                sip = value;
                UpdateHelpers();
            }
        }


        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public ModelDistribution()
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
        public ModelDistribution(Generator generator)
            : base(generator)
        {
            this.path = "";
            this.UpdateHelpers();
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Path"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the path is accessible and valid, otherwise <see langword="false"/>.
        /// </returns>
        public bool IsValidPath(string input)
        {
            XDocument doc = null;
            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    doc = XDocument.Load(input);
                    return doc.Elements().Any();
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }


        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of uniformly distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            if (!string.IsNullOrEmpty(path) && sip != null)
            {
                sipDistribution = new SIPDistribution();
                sipDistribution.SIPString = sip.ToString();
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
                return this.sipDistribution.Minimum;
            }
        }

        /// <summary>
        /// Gets the maximum possible value of from sample distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.sipDistribution.Maximum;
            }
        }

        /// <summary>
        /// Gets the mean value of the from sample distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.sipDistribution.Mean;
            }
        }

        /// <summary>
        /// Gets the median of from sample distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.sipDistribution.Median;
            }
        }

        /// <summary>
        /// Gets the variance of from sample distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return this.sipDistribution.Variance;
            }
        }

        /// <summary>
        /// Gets the mode of the from sample distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return this.sipDistribution.Mode;
            }
        }

        /// <summary>
        /// Returns the next model simulation result entry.
        /// </summary>
        /// <returns>A from sample from the model simulation results. Resets tot he beginning if the end is reached.</returns>
        public override double NextDouble()
        {
            return sipDistribution.NextDouble();
        }
        #endregion
    }
}