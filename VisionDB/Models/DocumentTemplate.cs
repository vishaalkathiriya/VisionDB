using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VisionDB.Models
{
    public class DocumentTemplate
    {
        public Guid Id { get; set; }

        [DisplayName("Template")]
        [Required]
        public string Name { get; set; }

        public virtual Company company { get; set; }

        [DisplayName("Type")]
        public Enums.TemplateType TemplateTypeEnum { get; set; }

        [DisplayName("Method")]
        public Enums.MessageMethod TemplateMethodEnum { get; set; }

        [Required]
        [DisplayName("document body")]
        public string TemplateHtml { get; set; }

        public DateTime? Deleted { get; set; }

        public string TemplateTypeToString 
        { 
            get
            {
                return TemplateTypeEnum.ToString().Replace('_', ' ');
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}