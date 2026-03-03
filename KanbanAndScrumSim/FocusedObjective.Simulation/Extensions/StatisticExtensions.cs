using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FocusedObjective.Simulation.Extensions
{

    internal static class StatisticExtensions
    {
        /* http://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
        n = 0, sum = 0, sum_sqr = 0
 
        for x in data:
            n = n + 1
            sum = sum + x
            sum_sqr = sum_sqr + x*x
 
        mean = sum/n
        variance = (sum_sqr - sum*mean)/(n - 1) 
        */

        internal static double PopulationVariance(
             this IEnumerable<double> source)
        {
            // check for invalid source conditions
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Aggregate(

                // seed - array of three doubles
                new double[3] { 0.0, 0.0, 0.0 },

                // item aggregation function, run for each element
                (subtotal, item) =>
                {
                    subtotal[0]++; // count
                    subtotal[1] += item; // sum
                    // sum of squares
                    subtotal[2] += (double)item * (double)item;
                    return subtotal;
                },

                // result selector function 
                // (finesses the final sum into the variance)
                // mean = sum / count 
                // variance = (sum_sqr - sum * mean) / (n) 
                // Sources with zero elements return a value of 0
                result => result[0] > 0 ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0]) : 0.0
            );
        }

        internal static double PopulationVariance(
             this ParallelQuery<double> source)
        {
            /* based upon the blog posting by Igor Ostrovsky at -
             * http://blogs.msdn.com/pfxteam/archive/2008/06/05/8576194.aspx
             * which demonstrates how to use the factory functions in an 
             * Aggregate function for efficiency
             */

            // check for invalid source conditions
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Aggregate(

                // seed - array of three doubles constructed 
                // using factory function, initialized to 0
                () => new double[3] { 0.0, 0.0, 0.0 },

                // item aggregation function, run for each element
                (subtotal, item) =>
                {
                    subtotal[0]++; // count
                    subtotal[1] += item; // sum
                    // sum of squares
                    subtotal[2] += (double)item * (double)item;
                    return subtotal;
                },

                // combine function, 
                // run on completion of each "thread"
                (total, thisThread) =>
                {
                    total[0] += thisThread[0];
                    total[1] += thisThread[1];
                    total[2] += thisThread[2];
                    return total;
                },

                // result selector function
                // finesses the final sum into the variance
                // mean = sum / count 
                // variance = (sum_sqr - sum * mean) / (n) 
                // Sources with zero elements return a value of 0
                (result) => (result[0] > 0) ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0]) : 0.0
            );
        }

        internal static double SampleVariance(
             this IEnumerable<double> source)
        {
            // check for invalid source conditions
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Aggregate(

                // seed - array of three doubles
                new double[3] { 0.0, 0.0, 0.0 },

                // item aggregation function, run for each element
                (subtotal, item) =>
                {
                    subtotal[0]++; // count
                    subtotal[1] += item; // sum
                    // sum of squares
                    subtotal[2] += (double)item * (double)item;
                    return subtotal;
                },

                // result selector function 
                // (finesses the final sum into the variance)
                // mean = sum / count 
                // variance = (sum_sqr - sum * mean) / (n - 1) 
                // Sources with zero or one element return a value of 0
                result => result[0] > 1 ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0] - 1) : 0.0
            );
        }

        internal static double SampleVariance(
             this ParallelQuery<double> source)
        {
            /* based upon the blog posting by Igor Ostrovsky at -
             * http://blogs.msdn.com/pfxteam/archive/2008/06/05/8576194.aspx
             * which demonstrates how to use the factory functions in an 
             * Aggregate function for efficiency
             */

            // check for invalid source conditions
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Aggregate(

                // seed - array of three doubles constructed 
                // using factory function, initialized to 0
                () => new double[3] { 0.0, 0.0, 0.0 },

                // item aggregation function, run for each element
                (subtotal, item) =>
                {
                    subtotal[0]++; // count
                    subtotal[1] += item; // sum
                    // sum of squares
                    subtotal[2] += (double)item * (double)item;
                    return subtotal;
                },

                // combine function, 
                // run on completion of each "thread"
                (total, thisThread) =>
                {
                    total[0] += thisThread[0];
                    total[1] += thisThread[1];
                    total[2] += thisThread[2];
                    return total;
                },

                // result selector function
                // finesses the final sum into the variance
                // mean = sum / count 
                // variance = (sum_sqr - sum * mean) / (n-1) 
                // Sources with zero or one element return a value of 0
                (result) => (result[0] > 1) ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0] - 1) : 0.0
            );
        }

        internal static double PopulationStandardDeviation(
            this IEnumerable<double> source)
        {
            return Math.Sqrt(source.PopulationVariance());
        }

        internal static double PopulationStandardDeviation<T>(
            this IEnumerable<T> source,
            Func<T, double> selector)
        {
            return PopulationStandardDeviation(
                Enumerable.Select(source, selector));
        }

        internal static double PopulationStandardDeviation(
            this ParallelQuery<double> source)
        {
            return Math.Sqrt(source.PopulationVariance());
        }

        internal static double PopulationStandardDeviation<T>(
            this ParallelQuery<T> source,
            Func<T, double> selector)
        {
            return PopulationStandardDeviation(
                ParallelEnumerable.Select(source, selector));
        }

        internal static double SampleStandardDeviation(
            this IEnumerable<double> source)
        {
            return Math.Sqrt(source.SampleVariance());
        }

        internal static double SampleStandardDeviation<T>(
            this IEnumerable<T> source,
            Func<T, double> selector)
        {
            return SampleStandardDeviation(
                Enumerable.Select(source, selector));
        }

        internal static double SampleStandardDeviation(
            this ParallelQuery<double> source)
        {
            return Math.Sqrt(source.SampleVariance());
        }

        internal static double SampleStandardDeviation<T>(
            this ParallelQuery<T> source,
            Func<T, double> selector)
        {
            return SampleStandardDeviation(
                ParallelEnumerable.Select(source, selector));
        }

        internal static double[] SummaryStatistics<T>(
             this IEnumerable<T> source,
             Func<T, double> selector)
        {
            return SummaryStatistics(
                Enumerable.Select(source, selector));
        }

        internal static double[] SummaryStatistics(
             this IEnumerable<double> source)
        {
            // do as much as we can in one pass

            // check for invalid source conditions
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Aggregate(

                // seed - array of three doubles
                new double[3] { 0.0, 0.0, 0.0 },

                // item aggregation function, run for each element
                (subtotal, item) =>
                {
                    subtotal[0]++; // count
                    subtotal[1] += item; // sum
                    // sum of squares
                    subtotal[2] += (double)item * (double)item;
                    return subtotal;
                },

                // result selector function 
                // (finesses the final sum into the variance)
                // mean = sum / count 
                // variance = (sum_sqr - sum * mean) / (n - 1) 
                // Sources with zero or one element return a value of 0
                result => new double[] {
                    result[0], // count
                    result[1], // sum
                    result[0] > 0 ? (result[1] / result[0]) : 0.0, // mean
                        result[0] > 1 ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0] - 1) : 0.0, // sample variance
                    result[0] > 1 ?
                    (result[2] - (result[1] * (result[1] / result[0])))
                        / (result[0]) : 0.0 // population variance
                }
            );
        }



        internal class RangeData<T>
        {
            internal T LowBound { get; set; }
            internal T HighBound { get; set; }
        }

        internal static RangeData<T> Range<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var sorted = from element in source
                         orderby element ascending
                         select element;

            return new RangeData<T>
            {
                LowBound = sorted.FirstOrDefault(),
                HighBound = sorted.LastOrDefault()
            };
        }

        internal static RangeData<T> Range<T>(this ParallelQuery<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var sorted = from element in source
                         orderby element ascending
                         select element;

            return new RangeData<T>
            {
                LowBound = sorted.FirstOrDefault(),
                HighBound = sorted.LastOrDefault()
            };
        }

        internal static IEnumerable<T> Mode<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // group the elements, and determine each count
            var groupings = from element in source
                            group element by element into groups
                            select new { Key = groups.Key, Count = groups.Count() };

            int maxCount = groupings.Max(m => m.Count);

            // return the entries with the highest count
            var modes = from grp in groupings
                        where grp.Count == maxCount
                        select grp.Key;

            return modes;
        }

        internal static double Median(this IEnumerable<double> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            double median = 0;

            // first, sort the entries in ascending order
            var sorted = from element in source
                         orderby element ascending
                         select element;

            // Take the middle element if there is an
            // odd number of entries, otherwise average
            // the middle pair of entries.

            int count = sorted.Count();

            if (count % 2 == 0)
                median = sorted
                         .Skip((count / 2) - 1)
                         .Take(2)
                         .Average();
            else
                median = sorted
                         .ElementAt(count / 2);

            return median;
        }

        internal static double Median<T>(
            this IEnumerable<T> source,
            Func<T, double> selector)
        {
            return Median(
                Enumerable.Select(source, selector));
        }


        internal static IDictionary<double, int> Histogram(this IEnumerable<double> source, int numberOfGroups)
        {
            var q = source
                .OrderBy(v => v)
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());

            if (q.Count() <= numberOfGroups)
                return q;

            // need to cut into groups.
            double min = source.Min();
            double max = source.Max();
            double increment = (max - min) / ((double)numberOfGroups);

            var ranges = source
                         .OrderBy(v => v)
                         .GroupBy(v => GetHistogramGroup(v, min, increment))
                         .ToDictionary(g => g.Key, g => g.Count());

            Dictionary<double, int> result = new Dictionary<double, int>();

            for (int i = 0; i < numberOfGroups; i++)
            {
                double key = min + (((i*1.0) + 1.0) * increment);

                // add any missing ranges (where the count was zero)
                if (!ranges.ContainsKey(key))
                    ranges.Add(key, 0);
            }

            return ranges.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }

        internal static IDictionary<double, int> Histogram(this IEnumerable<int> source, int numberOfGroups)
        {


            var q = source
                .OrderBy(v => v)
                .Select(v => v * 1.0)
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());

            if (q.Count() <= numberOfGroups)
                return q;

            // need to cut into groups.
            int min = source.Min();
            int max = source.Max();
            double increment = ((double)max - (double)min) / ((double)numberOfGroups);
            
            var ranges = source
                .OrderBy(v => v)
                .Select(v => v * 1.0)
                .GroupBy(v => GetHistogramGroup(v, (double)min, increment))
                .ToDictionary(g => g.Key, g => g.Count());

            Dictionary<double, int> result = new Dictionary<double, int>();

            for (int i = 0; i < numberOfGroups; i++)
            {
                double key = (min * 1.0) + (((i*1.0) + 1.0) * increment);

                // add any missing ranges (where the count was zero)
                if (!ranges.ContainsKey(key))
                    ranges.Add(key, 0);
            }

            return ranges.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        }
        
        private static double GetHistogramGroup(double val, double min, double increment)
        {
            double result = (val - min);
            
            if (result == 0.0) 
                return min + increment;

            return min + (Math.Ceiling(result / increment) * increment);
        }

        internal static double Percentile<T>(
            this IEnumerable<T> source, 
            Func<T, double> selector, 
            double percentile)
        {
            return Percentile (Enumerable.Select(source, selector), percentile);
        }

        internal static double[] SummaryPercentiles<T>(
            this IEnumerable<T> source,
            Func<T, double> selector)
        {
            return Enumerable.Select(source, selector).SummaryPercentiles();
        }
        
        internal static double[] SummaryPercentiles(this IEnumerable<double> source)
        {
            double[] results = new double[7];

            /* 0 (min), 5%, 25%, 50% (median), 75%, 95%, 100 (max) */
            if (source == null)
                throw new ArgumentNullException("source");

            // first, sort the entries in ascending order
            var sortedData = (from element in source
                              orderby element ascending
                              select element)
                             .ToList();

            results[0] = computePercentile(0.0, sortedData);
            results[1] = computePercentile(5.0, sortedData);
            results[2] = computePercentile(25.0, sortedData);
            results[3] = computePercentile(50.0, sortedData);
            results[4] = computePercentile(75.0, sortedData);
            results[5] = computePercentile(95.0, sortedData);
            results[6] = computePercentile(100.0, sortedData);
            
            return results;
        }

        internal static double Percentile(this IEnumerable<double> source, double percentile)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // first, sort the entries in ascending order
            var sortedData =    from element in source
                                orderby element ascending
                                select element;

            return computePercentile(percentile, sortedData);
        }

        private static double computePercentile(double percentile, IEnumerable<double> sortedData)
        {
            // algo derived from Aczel pg 15 bottom
            if (percentile >= 100.0d)
                return sortedData.LastOrDefault();

            if (percentile <= 0.0d)
                return sortedData.FirstOrDefault();

            int count = sortedData.Count();

            double position = (double)(count + 1) * percentile / 100.0;
            double leftNumber = 0.0d, rightNumber = 0.0d;

            double n = percentile / 100.0d * (count - 1) + 1.0d;

            if (position >= 1)
            {
                leftNumber = sortedData.ElementAtOrDefault((int)System.Math.Floor(n) - 1);
                rightNumber = sortedData.ElementAtOrDefault((int)System.Math.Floor(n));
            }
            else
            {
                leftNumber = sortedData.ElementAtOrDefault(0); // first data
                rightNumber = sortedData.ElementAtOrDefault(1); // first data
            }

            if (leftNumber == rightNumber)
                return leftNumber;
            else
            {
                double part = n - System.Math.Floor(n);
                return leftNumber + part * (rightNumber - leftNumber);
            }
        }
    }



}