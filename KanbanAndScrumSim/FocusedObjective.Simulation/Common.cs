using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using Troschuetz.Random;

namespace FocusedObjective.Simulation
{
    internal static class Common
    {



        internal static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        internal static double Derivative(double x)
        {
            double s = Sigmoid(x);

            return s * (1 - s);
        }

        internal static int PickRandomOccurrenceValueInt(OccurrenceTypeEnum type, double scale, double lowBound, double highBound, double sensitivity, SetupPhaseData phase, Distribution distribution = null)
        {
            double random = PickRandomOccurrenceValueDouble(type, scale, lowBound, highBound, sensitivity, phase, distribution);
            return (int)Math.Round(random, 0, MidpointRounding.AwayFromZero);
        }

        internal static double PickRandomOccurrenceValueDouble(OccurrenceTypeEnum type, double scale, double lowBound, double highBound, double sensitivity, SetupPhaseData phase, Distribution distribution = null)
        {
            if (distribution == null)
            {
                if (type == OccurrenceTypeEnum.Percentage)
                {
                    scale = 100;
                    lowBound = Math.Max(0.0, lowBound);
                    highBound = Math.Min(100.0, highBound);
                }

                double low = Common.normalizeOccurrenceToOneInXCards(scale, lowBound);
                double high = Common.normalizeOccurrenceToOneInXCards(scale, highBound);

                // if low > high, transpose...
                if (low > high)
                {
                    double d = low;
                    low = high;
                    high = d;
                }

                double random = TrueRandom.NextDouble(low, high);
                double phaseOccurrenceMultiplier = phase == null ? 1.0 : phase.OccurrenceMultiplier;

                random = random * sensitivity * phaseOccurrenceMultiplier;
                return random;
            }
            else
            {
                double random = distribution.GetNextDoubleForDistribution();
                double phaseOccurrenceMultiplier = phase == null ? 1.0 : phase.OccurrenceMultiplier;
                random = random * sensitivity * phaseOccurrenceMultiplier;
                return random;
            }
        }

        internal static double normalizeOccurrenceToOneInXCards(double scale, double value)
        {
            double normalized;

            if (scale == 1.0)
                normalized = value;
            else
                normalized = scale / value;

            return normalized;
        }


        internal static int PickRandomEstimateValueInt(double scale, double lowBound, double highBound, double sensitivity, SetupPhaseData phase, Distribution distribution = null)
        {
            double random = PickRandomEstimateValueDouble(scale, lowBound, highBound, sensitivity, phase, distribution);
            return (int)Math.Round(random, 0, MidpointRounding.AwayFromZero);
        }

        internal static double PickRandomEstimateValueDouble(double scale, double lowBound, double highBound, double sensitivity, SetupPhaseData phase, Distribution distribution = null)
        {
            double result;

            if (distribution == null)
            {
                double delta = highBound - lowBound;

                if (delta != 0.0)
                    result = TrueRandom.NextDouble(lowBound, highBound);
                else
                    result = lowBound;

                double phaseEstimateMultiplier = phase == null ? 1.0 : phase.EstimateMultiplier;
                result = result * sensitivity * phaseEstimateMultiplier;
            }
            else
            {
                result = distribution.GetNextDoubleForDistribution();
                double phaseEstimateMultiplier = phase == null ? 1.0 : phase.EstimateMultiplier;
                result = result * sensitivity * phaseEstimateMultiplier;
            }

            return result;
        }
    }
}
