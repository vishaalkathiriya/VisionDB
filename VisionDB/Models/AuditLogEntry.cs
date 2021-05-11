using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class AuditLogEntry
    {
        [Required]
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string EntryType { get; set; }

        public string EntryText { get; set; }

        public DateTime Added { get; set; }

        public Uri Address 
        { 
            get
            {
                return new Uri("/Home", UriKind.Relative);
            }
        }

        public Guid? EntryId { get; set; }

        public bool Visible { get; set; }

        public Practice practice { get; set; }
    }
}