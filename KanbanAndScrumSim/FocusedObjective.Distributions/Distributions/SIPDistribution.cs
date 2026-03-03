
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
    /// Provides generation SIP values
	/// </summary>

    public class SIPDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter samples which is used for generation of from samples random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidSamples"/> to determine whether a value is valid and therefor assignable.</remarks>
        public string SIPString
        {
            get
            {
                return this.sipString;
            }
            set
            {
                if (this.IsValidSIPString(value))
                {
                    this.sipString = value;
                    this.UpdateHelpers();
                }
            }
        }

        private string sipString = "";
        private List<double> helper1;
        private IEnumerator<double> helper2;

        
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="FromSamplesDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public SIPDistribution()
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
        public SIPDistribution(Generator generator)
            : base(generator)
        {
            this.sipString = "";
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
        public bool IsValidSIPString(string input)
        {
            XDocument doc = null;
            XElement sip = null;
            string value = replaceSpecialCharacters(input);
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    doc = XDocument.Parse(value);
                    sip = doc.Element("sip");

                    if (sip != null)
                    {
                        if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                            helper1 = (from v in sip.Value.Split(new string[] { ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                       select double.Parse(v.Trim())).ToList();
                        else
                            helper1 = (from v in sip.Value.Split(new string[] { "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                       select double.Parse(v.Trim())).ToList();

                        return helper1.Any();
                    }

                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private string replaceSpecialCharacters(string input)
        {
            string result = input;

            result = result.Replace(@"\n", char.Parse("\n").ToString());
            result = result.Replace(@"\r", char.Parse("\r").ToString());
            
            return result;
        }


        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of uniformly distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            XDocument doc = null;
            XElement sip = null;
            

            string value = replaceSpecialCharacters(this.SIPString);
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    doc = XDocument.Parse(value);
                    sip = doc.Element("sip");
                }
                catch
                {
                    // should never occur. isValidSip should always have caught this during validation
                    sip = null;
                }

                if (sip != null)
                {
                    if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                        helper1 = (from v in sip.Value.Split(new string[] { ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                   select double.Parse(v.Trim())).ToList();
                    else
                        helper1 = (from v in sip.Value.Split(new string[] { "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                   select double.Parse(v.Trim())).ToList();

                    helper2 = helper1.GetEnumerator();
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
                return this.helper1.Min();
            }
        }

        /// <summary>
        /// Gets the maximum possible value of from sample distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.helper1.Max();
            }
        }

        /// <summary>
        /// Gets the mean value of the from sample distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.helper1.Average();
            }
        }

        /// <summary>
        /// Gets the median of from sample distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.helper1.Median();
            }
        }

        /// <summary>
        /// Gets the variance of from sample distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return this.helper1.SampleVariance();
            }
        }

        /// <summary>
        /// Gets the mode of the from sample distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return this.helper1.Mode().ToArray();
            }
        }

        /// <summary>
        /// Returns the next SIP entry.
        /// </summary>
        /// <returns>A from sample from the SIP. Resets tot he beginning if the end is reached.</returns>
        public override double NextDouble()
        {
            double result = 0.0;
            
            if (helper2 != null)
            {
                if (helper2.MoveNext() == false)
                { 
                    helper2.Reset();
                    return NextDouble();
                }

                result = helper2.Current;
             }   

            return result;
        }
        #endregion
    }
}