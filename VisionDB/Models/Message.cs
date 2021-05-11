using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Id of customer message is for
        /// </summary>
        public Guid RecipientId { get; set; }
        /// <summary>
        /// User sending message
        /// </summary>
        public virtual ApplicationUser SenderUser { get; set; }

        [MaxLength(1000)]
        public string ToAddressNumber { get; set; }

        [MaxLength(20)]
        public string Postcode { get; set; }

        /// <summary>
        /// How message will be sent. E.g. email, letter, SMS message, etc
        /// </summary
        public Enums.MessageMethod messageMethod { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        [DisplayName("Message")]
        public string MessageText { get; set; }
        public DateTime AddedToQueue { get; set; }
        public DateTime? Sent { get; set; }
        /// <summary>
        /// User cancelling message
        /// </summary>
        public virtual ApplicationUser CancelledByUser { get; set; }
        /// <summary>
        /// Date and time when cancelled
        /// </summary>
        public DateTime? Cancelled { get; set; }

        public bool IsRecall { get; set; }

        public virtual Practice practice { get; set; }

        public virtual SMSTransaction SMSInventory { get; set; }

        public string Sender { get; set; }

        public string Ref { get; set; }

        public DateTime? ScheduledToBeSent { get; set; }

        public string StatusMessage { get; set; }
    }
}