using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class MessageTemplate
    {
        public Guid Id { get; set; }

        public Enums.MessageMethod messageMethod { get; set; }

        public string Template { get; set; }

        public string Subject { get; set; }

        public virtual Company company { get; set; }

        public Enums.MessageType MessageTypeEnum { get; set; }
    }
}