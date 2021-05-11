using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace VisionDB.Helper
{
    public class DateHelper
    {
        public static DateTime GetFirstDateForWeek()
        {
            DateTime input = DateTime.Now.DayOfWeek != DayOfWeek.Sunday ? DateTime.Now : DateTime.Now.Date.AddMinutes(-1); //this was done because incorrect start date for week retrieved on Sunday
            int delta = DayOfWeek.Monday - input.DayOfWeek;
            return input.AddDays(delta).Date;
        }

        /// <summary>
        /// Parse date in format "M/d/yyyy h:m:s tt", for example, 10/16/2014 2:00:00 PM and return as DateTime
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime ParseDate(string input)
        {
            return DateTime.ParseExact(input, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture);
        }
    }
}