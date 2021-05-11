using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace VisionDB.Models
{
    public static class Constants
    {
        public const string ErrorMessageTemplate = "{0}. Contact Click Software Limited for support on {2} with the error code: VDB{1}.";
        

        #region Enum Constants
        public enum User
        {
            [DescriptionAttribute("cgvak_mail")]
            USERNAME,
            [DescriptionAttribute("cgvak123")]
            PASSWORD           
        }
        public enum Mail
        {
            [DescriptionAttribute("Text")]
            ReturnMessageFormat,
            [DescriptionAttribute("TestPostcard1")]
            MailName,//mailingname
            [DescriptionAttribute("From test app - demo of quick postcard mailing")]
            MailDesc,
            [DescriptionAttribute("Postcard A5 right address")]
            MailTemplateLayout,//Current postcard template layouts:  "Postcard A5 left address", "Postcard A5 right address","Postcard A6 left address","Postcard A6 right address"
            [DescriptionAttribute("Postcard")]
            ProductType,
            [DescriptionAttribute("PostcardA5Right")]
            DocType,////Current Document types: “GreetingCardA5”, “PostcardA5”, “PostcardA6”, “PostcardA5Right” (address on right), “PostcardA6Right” (address on right).
            [DescriptionAttribute("../Content/Docmail-AddressList.csv")]
            FilePath,
            [DescriptionAttribute("AddressList.csv")]
            FileName,
            [DescriptionAttribute("../Images/Lighthouse.jpg")]
            ImageFilePath,
            [DescriptionAttribute("mypic.png")]
            ImageFileName,
            [DescriptionAttribute("Crop")]
            ImageFitOption,
            [DescriptionAttribute("0")]
            ImageRotation,
            [DescriptionAttribute("jegan_anlayst@live.com")]
            EmailOnError,
            [DescriptionAttribute("jegan_anlayst@live.com")]
            EmailOnSuccess,
            [DescriptionAttribute("jegan_anlayst@live.com")]
            SaveProofPDFTo
        }
        
        #endregion

        #region Helper Functions

        public static string Description(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static int ToInt(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            int returnValue;
            int.TryParse(attribute == null ? value.ToString() : attribute.Description, out returnValue);

            return returnValue;
        }

        #endregion
        
    }
}