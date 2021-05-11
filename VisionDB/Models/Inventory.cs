using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Inventory
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public virtual Product product { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal VATRateValue { get; set; }
        public virtual Customer customer { get; set; }
        public virtual Supplier supplier { get; set; }
        public virtual Practice practice { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime Added { get; set; }
    }
}