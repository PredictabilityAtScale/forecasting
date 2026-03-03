using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FocusedObjective.Common
{
    public static class MoveablePArameterHelpers
    {
        public static string DateFormatString(XDocument document)
        {
            string result = "yyyyMMdd";

            var execElement = document.Element("simulation").Element("execute");

            if (execElement != null)
            {
                var dateFormatAttribute = execElement.Attribute("dateFormat");
                if (dateFormatAttribute != null)
                    result = dateFormatAttribute.Value;
            }

            return result;
        }

        public static string CurrencyFormatString(XDocument document)
        {
            string result = "C0";

            var execElement = document.Element("simulation").Element("execute");

            if (execElement != null)
            {
                var dateFormatAttribute = execElement.Attribute("currencyFormat");
                if (dateFormatAttribute != null)
                    result = dateFormatAttribute.Value;
            }

            return result;
        }

        public static void ExtractVariablePI(XDocument document, List<MoveableParameter> parameters)
        {
            string dateFormatString = DateFormatString(document);
            string currencyFormatString = CurrencyFormatString(document);
            MoveablePArameterHelpers.ExtractVariablePI(document.FirstNode, parameters, dateFormatString);
        }

        public static void ExtractVariablePI(XNode node, List<MoveableParameter> parameters, string dateFormatString)
        {
            if (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.ProcessingInstruction)
                {
                    XProcessingInstruction pi = (XProcessingInstruction)node;
                    if (!string.IsNullOrEmpty(pi.Target) && (pi.Target == "variable" || pi.Target == "parameter"))
                    {
                        if (!string.IsNullOrEmpty(pi.Data))
                        {
                            Match m = Regex.Match(pi.Data, "name=\"(.*?)\"", RegexOptions.IgnoreCase);
                            if (m != null && m.Groups.Count > 1)
                            {
                                string name = m.Groups[1].Value;

                                // get the type
                                Match mtype = Regex.Match(pi.Data, "type=\"(.*?)\"", RegexOptions.IgnoreCase);
                                if (mtype != null && mtype.Groups.Count > 1)
                                {
                                    string type = mtype.Groups[1].Value;

                                    // get the value
                                    string originalStringValue = "";
                                    Match mval = Regex.Match(pi.Data, "value=\"(.*?)\"", RegexOptions.IgnoreCase);
                                    if (mval != null && mval.Groups.Count > 1)
                                    {
                                        originalStringValue = (mval.Groups[1].Value);
                                        switch (type)
                                        {
                                            case "numeric":
                                            case "number":
                                                {
                                                    double currentValue = 0;
                                                    if (double.TryParse(originalStringValue, out currentValue))
                                                    {
                                                        double lowest = 0;
                                                        mval = Regex.Match(pi.Data, "lowest=\"(.*?)\"", RegexOptions.IgnoreCase);
                                                        if (mval != null && mval.Groups.Count > 1)
                                                            double.TryParse(mval.Groups[1].Value, out lowest);

                                                        double highest = int.MaxValue;
                                                        mval = Regex.Match(pi.Data, "highest=\"(.*?)\"", RegexOptions.IgnoreCase);
                                                        if (mval != null && mval.Groups.Count > 1)
                                                            double.TryParse(mval.Groups[1].Value, out highest);

                                                        double step = 1;
                                                        mval = Regex.Match(pi.Data, "step=\"(.*?)\"", RegexOptions.IgnoreCase);
                                                        if (mval != null && mval.Groups.Count > 1)
                                                            double.TryParse(mval.Groups[1].Value, out step);

                                                        var p = new MoveableParameter
                                                        {
                                                            ParameterType = MoveableParameterTypeEnum.Numeric,
                                                            Name = name,
                                                            LowestValue = lowest,
                                                            HighestValue = highest,
                                                            DefaultValue = currentValue,
                                                            CurrentValue = currentValue,
                                                            StepSize = step,
                                                            Instruction = pi,
                                                            OriginalValueString = originalStringValue
                                                        };

                                                        parameters.Add(p);
                                                    }

                                                    break;
                                                }

                                            case "date":
                                                {
                                                    DateTime currentValue = DateTime.Now;
                                                    if (DateTime.TryParse(originalStringValue, out currentValue))
                                                    {

                                                        var p = new MoveableParameter
                                                        {
                                                            ParameterType = MoveableParameterTypeEnum.Date,
                                                            Name = name,
                                                            FormatString = dateFormatString,
                                                            DefaultValue = currentValue,
                                                            CurrentValue = currentValue,
                                                            Instruction = pi,
                                                            OriginalValueString = originalStringValue
                                                        };

                                                        parameters.Add(p);
                                                    }

                                                    break;
                                                }

                                            case "list":
                                                {
                                                    string list = "";
                                                    mval = Regex.Match(pi.Data, "list=\"(.*?)\"", RegexOptions.IgnoreCase);
                                                    if (mval != null && mval.Groups.Count > 1)
                                                        list = mval.Groups[1].Value;

                                                    var p = new MoveableParameter
                                                    {
                                                        ParameterType = MoveableParameterTypeEnum.List,
                                                        Name = name,
                                                        DefaultValue = originalStringValue,
                                                        CurrentValue = originalStringValue,
                                                        Instruction = pi,
                                                        OriginalValueString = originalStringValue,
                                                        DisplayList = list
                                                    };

                                                    parameters.Add(p);
                                                    break;
                                                }

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    foreach (var child in ((XElement)node).Nodes())
                        ExtractVariablePI(child, parameters, dateFormatString);
                }
            }
        }

        public static void ReplaceVariablePIWithTestValues(XNode node, List<MoveableParameter> parameters, string dateFormatString)
        {
            if (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.ProcessingInstruction)
                {
                    XProcessingInstruction pi = (XProcessingInstruction)node;
                    if (!string.IsNullOrEmpty(pi.Target) && (pi.Target == "variable" || pi.Target == "parameter"))
                    {
                        if (!string.IsNullOrEmpty(pi.Data))
                        {
                            Match m = Regex.Match(pi.Data, "name=\"(.*?)\"", RegexOptions.IgnoreCase);
                            if (m != null && m.Groups.Count > 1)
                            {
                                string name = m.Groups[1].Value;

                                var e = parameters.Where(p => p.Name == name).FirstOrDefault();
                                if (e != null)
                                {
                                    string was = "value=\"" + parameters.Where(p => p.Name == name).First().OriginalValueString + "\"";
                                    string to = "value=\"" + e.CurrentValue.ToString() + "\"";

                                    if (e.CurrentValue is DateTime)
                                        to = "value=\"" + ((DateTime)e.CurrentValue).ToSafeDateString(e.FormatString) + "\"";


                                    pi.Data = pi.Data.Replace(was, to);
                                }
                            }
                        }
                    }
                }

                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    foreach (var child in ((XElement)node).Nodes())
                        ReplaceVariablePIWithTestValues(child, parameters, dateFormatString);
                }
            }
        }

    }
}
