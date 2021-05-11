using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class RecallTemplate
    {
        public Guid Id { get; set; }

        [DisplayName("Recall")]
        public string Name { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual Company company { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}