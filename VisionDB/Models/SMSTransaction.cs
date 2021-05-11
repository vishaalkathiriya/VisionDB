using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class SMSTransaction
    {
        public Guid Id { get; set; }
        public virtual Company company { get; set; }
        public DateTime InsertTimestamp { get; set; }
        public int Quantity { get; set; }
    }
}