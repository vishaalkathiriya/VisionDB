using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class CustomerTag
    {
        public Guid Id { get; set; }

        public virtual Customer customer { get; set; }

        public virtual Tag tag { get; set; }

        public DateTime Added { get; set; }
        
        public DateTime? Deleted { get; set; }
    }
}