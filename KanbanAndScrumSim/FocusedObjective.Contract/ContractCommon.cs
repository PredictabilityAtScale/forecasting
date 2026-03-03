using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

using FocusedObjective.Common;

namespace FocusedObjective.Contract
{
    public static class ContractCommon
    {
        public static bool CheckValueGreaterThan(XElement errors, double value, double testValue, string attributeName, string elementName, XElement source = null)
        {
            bool success = true;

            if (value <= testValue)
            {
                success = false;
                FocusedObjective.Common.Helper.AddError(errors, FocusedObjective.Common.ErrorSeverityEnum.Error, 43, string.Format(FocusedObjective.Common.Strings.Error43, attributeName, elementName, testValue), source);
            }

            return success;
        }

        public static bool CheckValueGreaterThanOrEqualTo(XElement errors, double value, double testValue, string attributeName, string elementName, XElement source = null)
        {
            bool success = true;

            if (value < testValue)
            {
                success = false;
                FocusedObjective.Common.Helper.AddError(errors, FocusedObjective.Common.ErrorSeverityEnum.Error, 68, string.Format(FocusedObjective.Common.Strings.Error68, attributeName, elementName, testValue), source);
            }

            return success;
        }

        public static bool CheckValueLessThan(XElement errors, double value, double testValue, string attributeName, string elementName, XElement source = null)
        {
            bool success = true;

            if (value >= testValue)
            {
                success = false;
                FocusedObjective.Common.Helper.AddError(errors, FocusedObjective.Common.ErrorSeverityEnum.Error, 49, string.Format(FocusedObjective.Common.Strings.Error49, attributeName, elementName, testValue), source);
            }

            return success;
        }

        public static dynamic ReadMandatoryAttributeListValue(
            XElement source,
            XElement errors,
            string attribute, 
            params dynamic[] entriesAndValues)
        {
            dynamic result = entriesAndValues[1];

            XAttribute att = GetAttributeCaseInsensitive(source, attribute);

            if (att != null && !string.IsNullOrEmpty(att.Value))
            {
                for (int i = 0; i < entriesAndValues.Length; i = i + 2)
                {
                    if ( string.Compare(att.Value, entriesAndValues[i].ToString(), true) == 0)
                        return entriesAndValues[i + 1];
                }

                // if we get here, value not found.
                Helper.AddError(errors, ErrorSeverityEnum.Error, 42,
                    string.Format(Strings.Error42, attribute, source.Name.ToString(), att.Value), source);
            }
            else
            {
                Helper.AddError(errors, ErrorSeverityEnum.Information, 41,
                    string.Format(Strings.Error41, attribute, source.Name.ToString(), entriesAndValues[0]), source);
            }

            return result;
        }

        public static XAttribute GetAttributeCaseInsensitive(
            XElement source,
            string attribute)
        {
            XAttribute att = source.Attribute(attribute);
            
            // try lowercase
            if (att == null)
                att = source.Attribute(attribute.ToLower());

            return att;
        }

        public static dynamic ReadAttributeStringValue(
            XElement source,
            XElement errors,
            string attribute,
            string defaultValue = "",
            bool mandatory = true
        )
        {
            XAttribute att = GetAttributeCaseInsensitive(source, attribute);

            if (att != null && !string.IsNullOrEmpty(att.Value))
            {
                return att.Value;
            }
            else
            {
                if (mandatory)
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 4,
                        string.Format(Strings.Error4, attribute, source.Name.ToString()), source);

                return defaultValue;
            }
        }

        public static bool ReadAttributeIntValue(
           out int result,
           XElement source,
           XElement errors,
           string attribute,
           int defaultValue,
           bool mandatory = true
           )
        {
            bool success = false;
            result = defaultValue;

            XAttribute att = GetAttributeCaseInsensitive(source, attribute);

            if (att != null && !string.IsNullOrEmpty(att.Value))
            {
                if (!int.TryParse(att.Value, out result))
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 55, string.Format(Strings.Error55, attribute, source.Name.ToString(), att.Value.ToString()), source);
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                if (mandatory)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 4,
                        string.Format(Strings.Error4, attribute, source.Name.ToString()), source);
                }
                else
                {
                    success = true;
                }
            }

            return success;
        }

        public static bool ReadAttributeDoubleValue(
           out double result,
           XElement source,
           XElement errors,
           string attribute,
            double defaultValue,
            bool mandatory = true
           )
        {
            bool success = false;
            result = defaultValue;

            XAttribute att = GetAttributeCaseInsensitive(source, attribute);

            if (att != null && !string.IsNullOrEmpty(att.Value))
            {
                if (!double.TryParse(
                    att.Value, System.Globalization.NumberStyles.Any, 
                    System.Threading.Thread.CurrentThread.CurrentCulture, 
                    out result))
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 55, string.Format(Strings.Error55, attribute, source.Name.ToString(), att.Value), source);
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                if (mandatory)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 4,
                        string.Format(Strings.Error4, attribute, source.Name.ToString()), source);
                }
                else
                {
                    success = true;
                }
            }

            return success;
        }

        public static dynamic ReadElement(XElement source, Type type, string elementName, XElement errors, bool mandatory = false)
        {
            XElement element = source.Element(elementName);

            // try lowercase
            if (element == null)
                element = source.Element(elementName.ToLower());

            if (element != null)
                return Activator.CreateInstance(type, new object[] { element, errors }); 
            else
            {
                if (mandatory)
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 3, string.Format(Strings.Error3, elementName), source);

                return null;
            }
        }
    }
}
