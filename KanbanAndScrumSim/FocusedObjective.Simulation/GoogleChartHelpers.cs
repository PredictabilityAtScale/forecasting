using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.Simulation
{
    internal static class GoogleChartHelpers
    {

        internal static string EncodeData(int[] data)
        {
            int maxValue = data.Max();
            
            if (maxValue <= 61)
            {
                return SimpleEncoding(data);
            }
            else if (maxValue <= 4095)
            {
                return ExtendedEncoding(data);
            }

            return null;
        }

        internal static string Encode(ICollection<int[]> data)
        {
            int maxValue = data.SelectMany(i => i).Max();
            if (maxValue <= 61)
            {
                return SimpleEncoding(data);
            }
            else if (maxValue <= 4095)
            {
                return ExtendedEncoding(data);
            }

            return null;
        }

        internal static string Encode(float[] data)
        {
            return TextEncoding(data);
        }

        //internal static string Encode(ICollection<float[]> data)
        //{
        //    return TextEncoding(data);
        //}
        internal static string Encode(int[] data)
        {
            return TextEncoding(data);
        }

        internal static string Encode(double[] data)
        {
            return TextEncoding(data);
        }

        internal static string Encode(ICollection<double[]> data)
        {
            return TextEncoding(data);
        }

        #region Simple Encoding

        internal static string SimpleEncoding(int[] data)
        {
            return "chd=s:" + simpleEncode(data);
        }

        internal static string SimpleEncoding(ICollection<int[]> data)
        {
            string chartData = "chd=s:";

            foreach (int[] objectArray in data)
            {
                chartData += simpleEncode(objectArray) + ",";
            }

            return chartData.TrimEnd(",".ToCharArray());
        }

        private static string simpleEncode(int[] data)
        {
            string simpleEncoding = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string chartData = string.Empty;

            foreach (int value in data)
            {
                if (value == -1)
                {
                    chartData += "_";
                }
                else
                {
                    char c = simpleEncoding[value];
                    chartData += c.ToString();
                }
            }

            return chartData;
        }

        #endregion

        #region Text Encoding

        internal static string TextEncoding(float[] data)
        {
            return TextEncoding(data.Cast<double>().ToArray());
        }

        internal static string TextEncoding(double[] data)
        {
            return "chd=t:" + textEncode(data);
        }

        internal static string TextEncoding(int[] data)
        {
            return "chd=t:" + textEncode(data);
        }
        
        //internal static string TextEncoding(ICollection<float[]> data)
        //{
        //    return TextEncoding(data.Cast<IEnumerable<double>>().ToArray());
        //}

        internal static string TextEncoding(ICollection<double[]> data)
        {
            string chartData = "chd=t:";

            foreach (double[] objectArray in data)
            {
                chartData += textEncode(objectArray) + "|";
            }

            return chartData.TrimEnd("|".ToCharArray());
        }

        private static string textEncode(float[] data)
        {
            return textEncode(data.Cast<double>().ToArray());
        }

        private static string textEncode(int[] data)
        {
            string chartData = string.Empty;

            foreach (int value in data)
            {
                if (value == -1)
                {
                    chartData += "-1,";
                }
                else
                {
                    chartData += value.ToString() + ",";
                }
            }

            return chartData.TrimEnd(",".ToCharArray());
        }

        private static string textEncode(double[] data)
        {
            string chartData = string.Empty;

            foreach (double value in data)
            {
                if (value == -1)
                {
                    chartData += "-1,";
                }
                else
                {
                    chartData += value.ToString() + ",";
                }
            }

            return chartData.TrimEnd(",".ToCharArray());
        }
        #endregion

        #region Extended Encoding

        internal static string ExtendedEncoding(int[] data)
        {
            return "chd=e:" + extendedEncode(data);
        }

        internal static string ExtendedEncoding(ICollection<int[]> data)
        {
            string chartData = "chd=e:";

            foreach (int[] objectArray in data)
            {
                chartData += extendedEncode(objectArray) + ",";
            }

            return chartData.TrimEnd(",".ToCharArray());
        }

        private static string extendedEncode(int[] data)
        {
            string extendedEncoding = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-.";
            string chartData = string.Empty;

            foreach (int value in data)
            {
                if (value == -1)
                {
                    chartData += "__";
                }
                else
                {
                    int firstCharPos = Convert.ToInt32(Math.Floor((double)(value / extendedEncoding.Length)));
                    int secondCharPos = Convert.ToInt32(Math.Floor((double)(value % extendedEncoding.Length)));

                    chartData += extendedEncoding[firstCharPos];
                    chartData += extendedEncoding[secondCharPos];
                }
            }

            return chartData;
        }

        #endregion


    }
}
