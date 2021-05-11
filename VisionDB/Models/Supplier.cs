using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Supplier
    {
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Postcode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}