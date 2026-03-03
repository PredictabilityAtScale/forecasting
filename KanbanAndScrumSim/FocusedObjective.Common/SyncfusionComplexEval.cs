using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using sf = Syncfusion.Calculate;

namespace FocusedObjective.Common
{
    public static class SyncfusionComplexEval
    {
        public static dynamic Eval(object calc, string formula)
        {
            string result = string.Empty;

                if (calc == null)
                    calc = new Syncfusion.Calculate.CalcQuickBase();


                    // syncfusion formula first
                    try
                    {
                        var temp = (calc as sf.CalcQuickBase).TryParseAndCompute(formula);

                        double d;
                        if (!double.TryParse(temp, out d))
                        {
                            // return the text as an error prefixed with ERROR: so it can be rippled up to the user
                            result = "ERROR:" + temp;
                        }
                        else
                        {
                            result = temp;
                        }

                    }
                    catch
                    {
                    }
                

            return result;
        }

        public static object GetEngineSyncfusionInstance()
        {
            return new Syncfusion.Calculate.CalcQuickBase();
        }
    }
}
