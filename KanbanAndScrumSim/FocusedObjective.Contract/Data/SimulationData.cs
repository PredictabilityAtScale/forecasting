using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    
    [SimMLElement("simulation", "Root tag for all SimML files.", true, HasMandatoryAttributes=false)]
    public class SimulationData
    {
        private string _originalAfterIncludes = null;
        
        public SimulationData()
        {
        }

        public SimulationData(XDocument document, object calculationEngine = null)
        {
            // store the calculation engine....
            if (calculationEngine == null)
                calculationEngine = SyncfusionComplexEval.GetEngineSyncfusionInstance();
            
            _calculationEngine = calculationEngine;
            _errors = new XElement("errors");

            // store this as the original; this allows recalculation of the 
            processIncludes(document.FirstNode);
            _originalAfterIncludes = document.ToString(SaveOptions.DisableFormatting);
            
            // process variable pi's
            processVariablePI(document.FirstNode);
            processVariables();
            replaceVariablesInSimMl(document.FirstNode);

            fromXML(document.Element("simulation"), _errors);
        }

        public void RecalculateExpressionsAndVariables()
        {
            // reload the original XML
            var document = XDocument.Parse(_originalAfterIncludes);
            _errors = new XElement("errors");

            // process variable pi's
            _variables.Clear();
            processVariablePI(document.FirstNode);
            processVariables();
            replaceVariablesInSimMl(document.FirstNode);

            fromXML(document.Element("simulation"), _errors);

            //TODO: return false on error....shouldn't occur?
        }

        // private members and defaults
        private XElement _errors;
        
        private string _name = string.Empty;
        private string _locale = System.Threading.Thread.CurrentThread.CurrentCulture.Name; 
        private ExecuteData _executeData = null;
        private SetupData _setupData = null;
        private LicenseData _licenseData = null;
        private Dictionary<string, string> _variables = new Dictionary<string, string>();
        private object _calculationEngine = null;

        // partitioning info
        private string _partitionCommand = "";
        private int _partitionNumber = 1;
        private int _numberOfPartitions = 1;

        // public properties
        public object CalculationEngine
        {
            get { return _calculationEngine; }
        }
        
        public XElement Errors
        {
            get { return _errors; }
        }


        [SimMLAttribute("name", "A user defined name for this simulation model. This is for reference only.", false)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [SimMLAttribute("locale", "ISO locale cuture string. If omitted, the culture of the operating system running the simulatio engine will be used. Setting the locale string allows models to be executed in other cultures without errors due to number formatting. See http://msdn.microsoft.com/en-us/library/ms533052%28v=vs.85%29.aspx for a full list. ", false)]
        public string Locale
        {
            get { return _locale; }
            set { _locale = value; }
        }


        [SimMLElement("execute", "The execution instruction for simulation.", true)]
        public ExecuteData Execute
        {
            get { return _executeData; }
            set { _executeData = value; }
        }

        [SimMLElement("setup", "The model section of the simulation.", true, HasAnyAttributes=false, HasMandatoryAttributes=false)]
        public SetupData Setup
        {
            get { return _setupData; }
            set { _setupData = value; }
        }

        public LicenseData LicenseData
        {
            get { return _licenseData; }
            set { _licenseData = value; }
        }

        public Dictionary<string, string> Variables
        {
            get { return _variables; }
        }

        public string PartitionCommand
        {
            get { return _partitionCommand; }
            set { _partitionCommand = value; }
        }

        public int PartitionNumber
        {
            get { return _partitionNumber;  }
            set { _partitionNumber = value; }
        }

        public int NumberOfPartitions
        {
            get { return _numberOfPartitions; }
            set { _numberOfPartitions = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            _name = ContractCommon.ReadAttributeStringValue(
                source, 
                errors,
                "name",
                "",
                false);

            _locale = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "locale",
                _locale,
                false
                );

            _partitionCommand = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "partitionCommand",
                _partitionCommand,
                false
                );

            ContractCommon.ReadAttributeIntValue(
               out _partitionNumber,
               source,
               errors,
               "partitionNumber",
               _partitionNumber,
               false);

            ContractCommon.ReadAttributeIntValue(
               out _numberOfPartitions,
               source,
               errors,
               "numberOfPartitions",
               _numberOfPartitions,
               false);

            SetCurrentThreadsCulture();

            /* this may be needed in the future.
            // check for decimal separator override
            XElement execute = source.Element("execute");
            if (execute != null)
            {
                XAttribute decimalSeparatorAtt = execute.Attribute("decimalSeparator");
                if (decimalSeparatorAtt != null)
                    DecimalSeparator = decimalSeparatorAtt.Value;

                XAttribute thousandsSeparatorAtt = execute.Attribute("thousandsSeparator");
                if (thousandsSeparatorAtt != null)
                    ThousandsSeparator = thousandsSeparatorAtt.Value;
            }
             */

            // sub elements
            _executeData = ContractCommon.ReadElement(source, typeof(ExecuteData), "execute", errors, false);
            _setupData = ContractCommon.ReadElement(source, typeof(SetupData), "setup", errors, true);
            _licenseData = ContractCommon.ReadElement(source, typeof(LicenseData), "license", errors, false);

            return true;
        }

        public void SetCurrentThreadsCulture()
        {
            // set the culture of this thread
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(_locale);
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(_locale);
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("simulation");

            result.Add(new XAttribute("name", _name));
            result.Add(new XAttribute("locale", _locale));

            result.Add(new XAttribute("partitionCommand", _partitionCommand));
            result.Add(new XAttribute("partitionNumber", _partitionNumber.ToString()));
            result.Add(new XAttribute("numberOfPartitions", _numberOfPartitions.ToString()));

            if (_executeData != null) result.Add(_executeData.AsXML(simType));
            if (_setupData != null) result.Add(_setupData.AsXML(simType));
            if (_licenseData != null) result.Add(_licenseData.AsXML(simType));

            //TODO:License?

            return result;
        }

        private void processIncludes(XNode node)
        {
            if (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.ProcessingInstruction)
                {
                    XProcessingInstruction pi = (XProcessingInstruction)node;
                    if (!string.IsNullOrEmpty(pi.Target) && pi.Target == "include")
                    {
                        if (!string.IsNullOrEmpty(pi.Data))
                        {
                            Match m = Regex.Match(pi.Data, "source=\"(.*)\"", RegexOptions.IgnoreCase);
                            if (m != null && m.Groups.Count > 1)
                            {
                                //TODO: support basePath here.

                                string uri = m.Groups[1].Value;
                                XElement inc = XElement.Load(uri);
                                node.ReplaceWith(inc);
                            }
                        }
                    }
                    
                }

                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    foreach (var child in ((XElement)node).Nodes())
                    {
                        processIncludes(child);
                    }
                }
            }
        }

        private Dictionary<string, string> _variableLookup = new Dictionary<string, string>();

        private void processVariables()
        {
            bool change = processVariablesRecursive();

            if (change)
                processVariables();
        }

        private bool processVariablesRecursive()
        {
            bool hasChanges = false;

            for (int i = 0; i < _variables.Count; i++)
            {
                // evaluate just expressions and add to the lookup
                if (!VariableExpressionDecoder.ExpressionVariablesExist(_variables.ElementAt(i).Value) &&
                    VariableExpressionDecoder.ExpressionExist(_variables.ElementAt(i).Value))
                {
                    _variables[_variables.ElementAt(i).Key] = VariableExpressionDecoder.EvaluateExpression(_variables.ElementAt(i).Value, _calculationEngine).ToString();
                    hasChanges = true;
                }
                else
                {
                    if (VariableExpressionDecoder.ExpressionVariablesExist(_variables.ElementAt(i).Value))
                    {
                        _variables[_variables.ElementAt(i).Key] = VariableExpressionDecoder.ReplaceExpressionVariables(
                        _variables,
                        _variables.ElementAt(i).Value);

                        hasChanges = true;
                    }
                }
            }

            return hasChanges;
        }

        public bool Validate()
        {
            bool success = true;

            //TODO:Check the culture string

            // errors in attributes and values
            if (Errors.Elements("error").Count() > 0)
                success = false;

            success = Execute.Validate(this, _errors) && success;
            
            if (Setup != null)
                success = Setup.Validate(this, _errors) && success;

            return success;
        }

        private void replaceVariablesInSimMl(XNode node)
        {
            if (node != null && node.NodeType == System.Xml.XmlNodeType.Element)
            {
                foreach (var element in ((XElement)node).Elements())
                {
                    foreach (XAttribute att in element.Attributes())
                    {
                        if (!string.IsNullOrEmpty(att.Value))
                        {
                            if (VariableExpressionDecoder.ExpressionVariablesExist(att.Value))
                            {
                                string oldValue = att.Value;
                                string newValue = VariableExpressionDecoder.ReplaceExpressionVariables(_variables, att.Value, 1, _calculationEngine);

                                if (newValue.StartsWith("ERROR:"))
                                {
                                    Common.Helper.AddError(_errors, ErrorSeverityEnum.Error, 59, string.Format(Common.Strings.Error59, att.Name, element.Name, oldValue, newValue.Remove(0, 6)), element);
                                    att.Value = string.Empty;
                                }
                                else
                                {
                                    Common.Helper.AddError(_errors, ErrorSeverityEnum.Information, 1001, string.Format(Common.Strings.Info1, att.Name, element.Name, oldValue, newValue), element);
                                    att.Value = newValue;
                                }
                            }

                            // no variable, but perhaps inline expression
                            if (VariableExpressionDecoder.ExpressionExist(att.Value))
                            {
                                string oldValue = att.Value;
                                string newValue = VariableExpressionDecoder.EvaluateExpression(oldValue, _calculationEngine).ToString();
                                Common.Helper.AddError(_errors, ErrorSeverityEnum.Information, 1001, string.Format(Common.Strings.Info2, att.Name, oldValue, newValue), att.Parent);
                                att.Value = newValue;
                            }
                        }
                    }

                    replaceVariablesInSimMl(element);
                }
            }
        }

        private void processVariablePI(XNode node)
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
                                                                Match mval = Regex.Match(pi.Data, "value=\"(.*?)\"", RegexOptions.IgnoreCase);
                                if (mval != null && mval.Groups.Count > 1)
                                {
                                    string oldValue = mval.Groups[1].Value;

                                    if (!_variables.ContainsKey(name))
                                    {
                                        string newValue = oldValue;

                                        if (VariableExpressionDecoder.ExpressionVariablesExist(oldValue))
                                        {
                                            newValue = VariableExpressionDecoder.ReplaceExpressionVariables(_variables, oldValue, 1, _calculationEngine);
                                            Common.Helper.AddError(_errors, ErrorSeverityEnum.Information, 1001, string.Format(Common.Strings.Info2, name, oldValue, newValue), null);
                                        }

                                        // support formulas in variabe definitions
                                        if (VariableExpressionDecoder.ExpressionExist(newValue))
                                        {
                                            oldValue = newValue;
                                            newValue = VariableExpressionDecoder.EvaluateExpression(newValue, _calculationEngine).ToString();
                                            Common.Helper.AddError(_errors, ErrorSeverityEnum.Information, 1001, string.Format(Common.Strings.Info2, name, oldValue, newValue), null);
                                        }

                                        _variables.Add(name, newValue);
                                    }
                                }
                            }
                        }
                    }
                }

                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    foreach (var child in ((XElement)node).Nodes())
                        processVariablePI(child);
                }
            }
        }

    }


}
