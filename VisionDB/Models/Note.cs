using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Note
    {
        public const int MaxCharsForDescriptionSummary = 100;

        public Guid Id { get; set; }

        public virtual Customer customer { get; set; }

        [NotMapped]
        public Guid? CustomerId { get; set; }

        public virtual Practice practice { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }
    }
}