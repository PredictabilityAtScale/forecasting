using System;
//using Troschuetz.Random.Resources;
using System.Security.Cryptography;

namespace Troschuetz.Random
{
    /// <summary>
    /// Represents a pseudo-random number generator based on the System.Cryptography class.
    /// </summary>
    public class CryptoRandomGenerator : Generator
    {
        #region instance fields
        private RandomNumberGenerator generator = RandomNumberGenerator.Create();
        
        /// <summary>
        /// Stores an <see cref="int"/> used to generate up to 31 random <see cref="Boolean"/> values.
        /// </summary>
        private int bitBuffer;

        /// <summary>
        /// Stores how many random <see cref="Boolean"/> values still can be generated from <see cref="bitBuffer"/>.
        /// </summary>
        private int bitCount;
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandomGenerator"/> class, using a time-dependent default 
        ///   seed value.
        /// </summary>
        public CryptoRandomGenerator()
        {
            ResetGenerator();
        }

        #endregion

        #region instance methods
        /// <summary>
        /// Resets the <see cref="CryptoRandomGenerator"/>, so that it produces the same pseudo-random number sequence again.
        /// </summary>
        private void ResetGenerator()
        {
            // Create a new Random object using the same seed.
            this.generator = RandomNumberGenerator.Create();

            // Reset helper variables used for generation of random bools.
            this.bitBuffer = 0;
            this.bitCount = 0;
        }
        #endregion

        #region overridden Generator members
        /// <summary>
        /// Gets a value indicating whether the <see cref="CryptoRandomGenerator"/> can be reset, so that it produces the 
        ///   same pseudo-random number sequence again.
        /// </summary>
        public override bool CanReset
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the <see cref="CryptoRandomGenerator"/>, so that it produces the same pseudo-random number sequence again.
        /// </summary>
        /// <returns><see langword="true"/>.</returns>
        public override bool Reset()
        {
            this.ResetGenerator();
            return true;
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="Int32.MaxValue"/>.
        /// </returns>
        public override int Next()
        {
            return this.Next();
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated. 
        /// <paramref name="maxValue"/> must be greater than or equal to 0. 
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, 
        ///   the range of return values includes 0 but not <paramref name="maxValue"/>. 
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0. 
        /// </exception>
        public override int Next(int maxValue)
        {
            return this.nextInteger(0, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range. 
        /// </summary>
        /// <param name="minValue">
        /// The inclusive lower bound of the random number to be generated. 
        /// </param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated. 
        /// <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>. 
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/>, and less than 
        ///   <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but 
        ///   not <paramref name="maxValue"/>. 
        /// If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.  
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            return nextInteger(minValue, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative floating point random number less than 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0; that is, 
        ///   the range of return values includes 0.0 but not 1.0.
        /// </returns>
        public override double NextDouble()
        {
            return this.getPercent();
        }

        /// <summary>
        /// Returns a nonnegative floating point random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated. 
        /// <paramref name="maxValue"/> must be greater than or equal to zero. 
        /// </param>
        /// <returns>
        /// A double-precision floating point number greater than or equal to zero, and less than <paramref name="maxValue"/>; 
        ///   that is, the range of return values includes zero but not <paramref name="maxValue"/>. 
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0. 
        /// </exception>
        public override double NextDouble(double maxValue)
        {
            //if (maxValue < 0)
            //{
            //    string message = string.Format(null, ExceptionMessages.ArgumentOutOfRangeGreaterEqual,
            //        "maxValue", "0.0");
            //    throw new ArgumentOutOfRangeException("maxValue", maxValue, message);
            //}

            return this.NextDouble() * maxValue;
        }

        /// <summary>
        /// Returns a floating point random number within the specified range. 
        /// </summary>
        /// <param name="minValue">
        /// The inclusive lower bound of the random number to be generated. 
        /// The range between <paramref name="minValue"/> and <paramref name="maxValue"/> must be less than or equal to
        ///   <see cref="Double.MaxValue"/>
        /// </param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated. 
        /// <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.
        /// The range between <paramref name="minValue"/> and <paramref name="maxValue"/> must be less than or equal to
        ///   <see cref="Double.MaxValue"/>.
        /// </param>
        /// <returns>
        /// A double-precision floating point number greater than or equal to <paramref name="minValue"/>, and less than 
        ///   <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but 
        ///   not <paramref name="maxValue"/>. 
        /// If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.  
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The range between <paramref name="minValue"/> and <paramref name="maxValue"/> is greater than
        ///   <see cref="Double.MaxValue"/>.
        /// </exception>
        public override double NextDouble(double minValue, double maxValue)
        {
            //if (minValue > maxValue)
            //{
            //    string message = string.Format(null, ExceptionMessages.ArgumentOutOfRangeGreaterEqual,
            //        "maxValue", "minValue");
            //    throw new ArgumentOutOfRangeException("maxValue", maxValue, message);
            //}

            double range = maxValue - minValue;

            //if (range == double.PositiveInfinity)
            //{
            //    string message = string.Format(null, ExceptionMessages.ArgumentRangeLessEqual,
            //        "minValue", "maxValue", "Double.MaxValue");
            //    throw new ArgumentException(message);
            //}

            return this.nextDouble(minValue, maxValue);
        }
        
        /// <summary>
        /// Returns a random Boolean value.
        /// </summary>
        /// <remarks>
        /// Buffers 31 random bits (1 int) for future calls, so a new random number is only generated every 31 calls.
        /// </remarks>
        /// <returns>A <see cref="Boolean"/> value.</returns>
        public override bool NextBoolean()
        {
            if (this.bitCount == 0)
            {
                // Generate 31 more bits (1 int) and store it for future calls.
                this.bitBuffer = this.Next();

                // Reset the bitCount and use rightmost bit of buffer to generate random bool.
                this.bitCount = 30;
                return (this.bitBuffer & 0x1) == 1;
            }

            // Decrease the bitCount and use rightmost bit of shifted buffer to generate random bool.
            this.bitCount--;
            return ((this.bitBuffer >>= 1) & 0x1) == 1;
        }
        
        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers. 
        /// </summary>
        /// <remarks>
        /// Each element of the array of bytes is set to a random number greater than or equal to zero, and less than or 
        ///   equal to <see cref="Byte.MaxValue"/>.
        /// </remarks>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is a null reference (<see langword="Nothing"/> in Visual Basic). 
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            this.generator.GetBytes(buffer);
        }
        #endregion



        /// <summary>
        /// Retrieves the next random integer from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>integer</returns>
        private int nextInteger(int lowest, int highest)
        {
            int tl = lowest - 1;      //  this seems clunky
            int th = highest + 1;
            float percent = getPercent();
            return (int)(((th - tl) * percent) + tl);
        }

        /// <summary>
        /// Retrieves the next random double from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>double</returns>
        private double nextDouble(double lowest, double highest)
        {
            float percent = getPercent();
            return ((highest - lowest) * percent) + lowest;
        }

        /// <summary>
        /// Retrieves the next random float from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>float</returns>
        private double nextFloat(float lowest, float highest)
        {
            float percent = getPercent();
            return ((highest - lowest) * percent) + lowest;
        }
        /// <summary>
        /// Determines if the next random number passes the given percentage - used
        /// to determine an event that has a x% chance of occurring
        /// </summary>
        /// <param name="percent">0.0 is 0%, 1.0 is 100%</param>
        /// <returns></returns>
        private bool passes(double percent)
        {
            float rand = getPercent();
            return rand <= percent;
        }

        /// <summary>
        /// Retrieves a percentage from the random buffer
        /// </summary>
        /// <returns>float 0.0 to 1.0</returns> 
        private float getPercent()
        {
            byte[] buffer = new byte[sizeof(UInt32)];
            UInt32 rand;
            generator.GetNonZeroBytes(buffer);
            rand = BitConverter.ToUInt32(buffer, 0);
            return (float)((double)rand / (double)UInt32.MaxValue);
        }
    }
}
