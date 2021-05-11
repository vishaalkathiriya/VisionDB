using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VisionDB.Models;

namespace VisionDB.Helper
{
    public class ErrorHelper
    {
        /// <summary>
        /// [ErrorMessage]. Contact Click Software Limited for support on System.Web.Configuration.WebConfigurationManager.AppSettings["SupportTel"].ToString() with the error code: [ErrorCode].
        /// E.g. Unable to save appointment. Contact Click Software Limited for support on 0121 661 4477 with the error code: VDB107.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        /// <param name="ErrorCode"></param>
        /// <returns></returns>
        public static string GetErrorText(string ErrorMessage, string ErrorCode)
        {
            return string.Format(Constants.ErrorMessageTemplate, ErrorMessage, ErrorCode, System.Web.Configuration.WebConfigurationManager.AppSettings["SupportTel"].ToString());
        }
    }
}