using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Helper
{
    public class NumericHelper
    {
        public static string GetSignedResult(float? Input)
        {
            if (!Input.HasValue)
            {
                return null;
            }
            else if (Input > 0)
            {
                return string.Concat("+", String.Format("{0:0.00}", Input));
            }
            else
            {
                return String.Format("{0:0.00}", Input);
            }
        }

        public static string GetResult(float? Input)
        {
            if (!Input.HasValue)
            {
                return null;
            }
            else
            {
                return String.Format("{0:0.00}", Input);
            }
        }
    }
}