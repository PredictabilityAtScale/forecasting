using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace FocusedObjective.Common
{
    public static class StringExtensions
    {
        public static DateTime? ToSafeDate(this string date, string format, DateTime? defaultValue)
        {
            DateTime temp = new DateTime();

            if (!string.IsNullOrEmpty(date))
            {
                bool success = DateTime.TryParseExact(
                    date, new string[] { format, "yyyyMMdd", "yyyyMd", "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd", "ddMMMyyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);

                if (success)
                    return temp;
            }

            return defaultValue;
         }

        public static DateTime ToSafeDate(this string date, string format, DateTime defaultValue)
        {
            DateTime temp = new DateTime();

            if (!string.IsNullOrEmpty(date))
            {
                bool success = DateTime.TryParseExact(
                    date, new string[] { format, "yyyyMMdd", "yyyyMd", "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd", "ddMMMyyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);

                if (success)
                    return temp;
            }

            return defaultValue;
        }


        public static string ToSafeDateString(this DateTime date, string format)
        {
            return date.ToString(format);
        }

    }
}
