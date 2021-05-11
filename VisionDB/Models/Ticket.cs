using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Ticket
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime CreatedTimestamp { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime? Due { get; set; }

        public Enums.TicketPriority Priority { get; set; }

        public Enums.TicketType Type { get; set; }

        public Enums.TicketStatus Status { get; set; }
    }
}