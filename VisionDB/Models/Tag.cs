using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual Company company { get; set; }
        public DateTime? Deleted { get; set; }

        [DisplayName("Type")]
        public Enums.TagType TagTypeEnum { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}