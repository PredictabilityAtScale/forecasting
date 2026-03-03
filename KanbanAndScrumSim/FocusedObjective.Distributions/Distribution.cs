using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Distributions;

namespace Troschuetz.Random
{
   	public abstract partial class Distribution
    {
        public DistributionData Data { get; set; }

        public double GetNextDoubleForDistribution(int maxAttempts = 1000)
        {
            double result = 0.0;

            switch( Data.BoundProcessing)
            {
                // CLIP
                case DistributionBoundProcessing.Clip:
                    {
                        // try for a max attempts to get a double within range
                        int attempts = 1;
                        while (attempts < maxAttempts)
                        {
                            double d = (this.Data.Location + this.NextDouble()) * this.Data.Multiplier;

                            if (d >= Data.LowBound &&
                                d <= Data.HighBound)
                            {
                                if (d == 0.0 && Data.ZeroHandling == ZeroHandlingEnum.Remove)
                                {
                                    // don't take zero value .. keep trying
                                }
                                else
                                {
                                    result = d;
                                    break;
                                }
                            }

                            attempts++;
                        }

                        //if (!resultFound)
                        //    ??

                        break;
                    }

                case DistributionBoundProcessing.Stretch:
                    {
                        double d = (this.Data.Location + this.NextDouble()) * this.Data.Multiplier;

                        // if the bounds are default, just return the double
                        if (this.Data.LowBound == double.Parse(double.MinValue.ToString("R")) 
                            && this.Data.HighBound == double.Parse(double.MaxValue.ToString("R")))
                            result = d;

                        double distRange = this.Maximum - this.Minimum;
                        double desiredRange = this.Data.HighBound - this.Data.LowBound;

                        // point on distRange
                        // range 0 to 1, v = 0.5 } 1 to 3  v = 2
                        // range 0 to 100, 10 to 30

                        //check for range of 0 in both cases...
                        if (distRange > 0.0 && desiredRange > 0.0)
                        {
                            double p = ((d - this.Minimum) / distRange);
                            result = this.Data.LowBound + (p * desiredRange);
                        }
                        else
                        {
                            // one of the ranges is zero, just return the low-bound 
                            // (which is the same as the high bound anyway)
                            result = this.Data.LowBound;
                        }

                        break;
                    }
            }

            // override explicit zero result here
            if (result == 0.0)
            {
                if (this.Data.ZeroHandling == ZeroHandlingEnum.Value)
                {
                    result = this.Data.ZeroValue;
                }
            }

            return result ;
        }
    }
}
