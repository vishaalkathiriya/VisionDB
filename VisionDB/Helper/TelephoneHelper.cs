using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Helper
{
    public static class TelephoneHelper
    {
        public static string GetSkypeNumber(string Number)
        {
            if (Number != null)
            {
                Number = Number.TrimStart('0');

                Number = "+44" + Number;

                Number = Number.Replace("-", "");

                Number = Number.Replace(" ", "");

                return Number;
            }
            else
            {
                return null;
            }
        }
    }
}