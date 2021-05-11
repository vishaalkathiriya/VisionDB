using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class RecallTemplateViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime? Deleted { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}