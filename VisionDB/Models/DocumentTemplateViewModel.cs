using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class DocumentTemplateViewModel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public Enums.TemplateType TemplateTypeEnum { get; set; }

        public Enums.MessageMethod TemplateMethodEnum { get; set; }

        public string TemplateHtml { get; set; }

        public DateTime? Deleted { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}