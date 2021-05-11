using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class RecallDocumentViewModel
    {
        public Guid Id { get; set; }

        public DateTime? Deleted { get; set; }

        public Enums.BeforeOrAfter BeforeOrAfterEnum { get; set; }

        public int DateSpanValue { get; set; }

        public Enums.FrequencyUnit DateSpanUnit { get; set; }

        [DisplayName("Document")]
        public string documentTemplate { get; set; }

        [DisplayName("When to send")]
        public string WhenToSend
        {
            get
            {
                return string.Format("{0} {1} {2}", DateSpanValue.ToString(), DateSpanUnit.ToString().ToLower(), BeforeOrAfterEnum.ToString().ToLower());
            }
        }

        public Enums.MessageMethod TemplateMethodEnum { get; set; }

        [DisplayName("Method")]
        public string TemplateMethodToString
        {
            get
            {
                return TemplateMethodEnum.ToString();
            }
        }
    }
}