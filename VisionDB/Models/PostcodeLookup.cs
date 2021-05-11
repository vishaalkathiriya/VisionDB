using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class PostcodeLookup
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime InsertTimestamp { get; set; }

        [Required]
        public virtual Company company { get; set; }

        [Required]
        public int Quantity { get; set; }

        public virtual ApplicationUser user { get; set; }

        public string PostcodeSearchedFor { get; set; }

        public int NoOfResults { get; set; }
    }
}