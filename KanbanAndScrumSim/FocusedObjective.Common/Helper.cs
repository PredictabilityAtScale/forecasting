using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections;
using System.Security.Cryptography;
using System.Globalization;

namespace FocusedObjective.Common
{
    public static class Helper
    {
        public static string CalculateMD5HashFromString(string content)
        {
            string result = "";
            try
            {
                using (MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    // step 1, calculate MD5 hash from input
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(content);
                    byte[] hash = md5.ComputeHash(inputBytes);

                    // step 2, convert byte array to hex string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                        sb.Append(hash[i].ToString("X2"));

                    result = sb.ToString();
                }
            }
            catch
            { }

            return result;
        }

        public static string Sha256HashSimMLString(string contents)
        {
            string result = "";
            try
            {
                HMACSHA256 sha256 = new HMACSHA256(
                    new byte[] {
                        0xBF,0xC4,0x9F,0xC4,0xCB,0x39,0x3B,0xA2,0x76,0x3D,
                        0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,0x3C,0x47,
                        0x97,0x68,0xB3,0xF8,0x1A,0xDA,0x9C,0xD8,0x85,0xB5,
                        0x7A,0x22,0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,
                        0xBF,0xC4,0x9F,0xC4,0xCB,0x39,0x3B,0xA2,0x76,0x3D,
                        0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,0x3C,0x47,
                        0x9F,0xC4,0xCB,0x39
                    });

                result = BitConverter.ToString(
                    sha256.ComputeHash(System.Text.Encoding.Unicode.GetBytes(contents)));

                sha256.Clear();
            }
            catch
            { }

            return result;
        }

        public static string Sha256HashSimMLFile(string path)
        {
            string result = "";

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    HMACSHA256 sha256 = new HMACSHA256(
                    new byte[] {
                        0xBF,0xC4,0x9F,0xC4,0xCB,0x39,0x3B,0xA2,0x76,0x3D,
                        0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,0x3C,0x47,
                        0x97,0x68,0xB3,0xF8,0x1A,0xDA,0x9C,0xD8,0x85,0xB5,
                        0x7A,0x22,0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,
                        0xBF,0xC4,0x9F,0xC4,0xCB,0x39,0x3B,0xA2,0x76,0x3D,
                        0xDE,0x3A,0x4F,0x3C,0x7F,0x9E,0xAC,0x9A,0x3C,0x47,
                        0x9F,0xC4,0xCB,0x39
                    });
                    result = BitConverter.ToString(sha256.ComputeHash(sr.BaseStream));
                    sha256.Clear();
                }

            }
            catch
            {
            }

            return result;
        }

        public static object CopyObject(object input)
        {
            if (input != null)
            {
                object result = Activator.CreateInstance(input.GetType());
                foreach (FieldInfo field in input.GetType().GetFields(System.Reflection.BindingFlags.NonPublic))
                {
                    if (field.FieldType.GetInterface("IList", false) == null)
                    {
                        field.SetValue(result, field.GetValue(input));
                    }
                    else
                    {
                        IList listObject = (IList)field.GetValue(result);
                        if (listObject != null)
                        {
                            foreach (object item in ((IList)field.GetValue(input)))
                            {
                                listObject.Add(CopyObject(item));
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static void AddError(XElement errors, ErrorSeverityEnum severity, int code, string message, XElement source = null, XAttribute sourceAttribue = null)
        {
            XElement thisError = new XElement(Enum.GetName(typeof(ErrorSeverityEnum), severity).ToLower(),
                new XAttribute("code", code),
                message);


            if (sourceAttribue != null)
            {
                // check attribute first

                var attLineInfo = (IXmlLineInfo)sourceAttribue;
                if (attLineInfo.HasLineInfo())
                {
                    thisError.Add(
                        new XAttribute("line", attLineInfo.LineNumber),
                        new XAttribute("pos", attLineInfo.LinePosition));

                }


            }
            else
            {

                if (source != null)
                {
                    var lineInfo = (IXmlLineInfo)source;
                    if (lineInfo.HasLineInfo())
                    {
                        thisError.Add(
                            new XAttribute("line", lineInfo.LineNumber),
                            new XAttribute("pos", lineInfo.LinePosition));
                    }
                }
            }

            errors.Add(thisError);
        }

        /* Maybe needed in the future. 
        public static IFormatProvider GetFormatProvider(string decimalSeparator = null, string thousandsSeparator = null)
        {
            NumberFormatInfo result = NumberFormatInfo.CurrentInfo;

            // quick exit if using the defaults
            if (string.IsNullOrWhiteSpace(decimalSeparator)
                  && string.IsNullOrWhiteSpace(thousandsSeparator))
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(decimalSeparator))
            {
                result.CurrencyDecimalSeparator = decimalSeparator;
                result.NumberDecimalSeparator = decimalSeparator;
                result.CurrencyDecimalSeparator = decimalSeparator;
                result.PercentDecimalSeparator = decimalSeparator;
            }

            if (!string.IsNullOrWhiteSpace(thousandsSeparator))
            {
                result.CurrencyGroupSeparator = thousandsSeparator;
                result.NumberGroupSeparator = thousandsSeparator;
                result.CurrencyGroupSeparator = thousandsSeparator;
                result.PercentGroupSeparator = thousandsSeparator;
            }

            return result;
        }
        */

    }
}
