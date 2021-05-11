using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public virtual Practice practice { get; set; }
        public string Name { get; set; }
        public bool IsCustomReport { get; set; }
        public string DisplayName { get; set; }
    }
}