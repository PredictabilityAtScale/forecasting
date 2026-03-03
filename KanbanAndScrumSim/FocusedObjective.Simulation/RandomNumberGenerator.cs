using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace FocusedObjective.Simulation
{
        //  we use the cryptographically secure random generation for all
        // random numbers - it returns arrays of bytes that we convert
        // for use with the type and range given by the user
    internal class TrueRandom
    {
        static private RandomNumberGenerator _RNG = RandomNumberGenerator.Create();
        /// <summary>
        /// Retrieves the next random integer from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>integer</returns>
        static internal int NextInteger(int lowest, int highest)
        {
            int tl = lowest - 1;      //  this seems clunky
            int th = highest + 1;
            float percent = TrueRandom.GetPercent();
            return (int)(((th - tl) * percent) + tl);
        }
        /// <summary>
        /// Retrieves the next random double from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>double</returns>
        static internal double NextDouble(double lowest, double highest)
        {
            float percent = TrueRandom.GetPercent();
            return ((highest - lowest) * percent) + lowest;
        }
        /// <summary>
        /// Retrieves the next random float from the random buffer
        /// </summary>
        /// <param name="lowest">lowest possible value to return</param>
        /// <param name="highest">highest possible value to return</param>
        /// <returns>float</returns>
        static internal double NextFloat(float lowest, float highest)
        {
            float percent = TrueRandom.GetPercent();
            return ((highest - lowest) * percent) + lowest;
        }
        /// <summary>
        /// Determines if the next random number passes the given percentage - used
        /// to determine an event that has a x% chance of occurring
        /// </summary>
        /// <param name="percent">0.0 is 0%, 1.0 is 100%</param>
        /// <returns></returns>
        static internal bool Passes(double percent)
        {
            float rand = TrueRandom.GetPercent();
            return rand <= percent;
        }

        /// <summary>
        /// Retrieves a percentage from the random buffer
        /// </summary>
        /// <returns>float 0.0 to 1.0</returns> 
        static internal float GetPercent()
        {
            byte[] buffer = new byte[sizeof(UInt32)];
            UInt32 rand;
            TrueRandom._RNG.GetNonZeroBytes(buffer);
            rand = BitConverter.ToUInt32(buffer, 0);
            return (float)((double)rand / (double)UInt32.MaxValue);
        }
    }
    
}
