using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FocusedObjective.Common
{
    public static class VariableExpressionDecoder
    {
        public static string expressionVariableRegex = @"@[a-z,A-Z,0-9_]+";
        public static string expressionRegex = @"[/+/*///-]+";

        public static bool ExpressionVariablesExist(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            Regex myRegex = new Regex(expressionVariableRegex, RegexOptions.None);
            return myRegex.Matches(expression).Count > 0;
        }

        public static bool ExpressionExist(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            return expression.TrimStart().StartsWith("=");
            
            
            //Regex myRegex = new Regex(expressionRegex, RegexOptions.None);
            //return myRegex.Matches(expression).Count > 0;
        }

        public static string ReplaceExpressionVariables(Dictionary<string, string> values, string expression, int depth = 1, object calc = null)
        {
            // infinate loop bailout
            if (depth > 100)
                return expression;

            string result = ReplaceExpressionVariablesRecursive(values, expression, depth);

            if (result != expression)
            {
                // exit when there are no more replaceable variables 
                return ReplaceExpressionVariables(values, result, depth + 1, calc);
            }
            else
            {
                // can we evaluate/resolve an expression?
                if (ExpressionExist(result))
                    result = EvaluateExpression(result, calc).ToString();

                return result;
            }
        }

        internal static string ReplaceExpressionVariablesRecursive(Dictionary<string, string> values, string expression, int depth)
        {
            // infinate loop bailout
            if (depth > 100)
                return expression;

            string result = expression;

            Regex myRegex = new Regex(expressionVariableRegex, RegexOptions.None);
            foreach (Match myMatch in myRegex.Matches(expression))
            {
                if (myMatch.Success)
                {
                    string key = myMatch.Value.Remove(0, 1); // remove @
                    if (values.ContainsKey(key))
                        result = result.Replace(myMatch.Value, values[key]);
                }
            }

            return result;
        }

        public static string EvaluateExpression(string expression, object calc = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "";

            string result = expression;

            try
            {
                if (!ExpressionVariablesExist(expression) && expression.Length > 1)
                    result = SyncfusionComplexEval.Eval(calc, expression);
            }
            catch
            {
                // intentionally doing nothing here, they may well be editing the source file
            }

            return result;
        }

        
        public static Syncfusion.Calculate.CalcQuickBase CreateCalculatorInstance()
        {
            return new Syncfusion.Calculate.CalcQuickBase();
        }
         

    }
}
