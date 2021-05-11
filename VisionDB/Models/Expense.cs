using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Expense
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Added { get; set; }

        [Required]
        [DisplayName("Date")]
        public DateTime ExpenseDate { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public virtual Practice practice { get; set; }

        [Required]
        public string Payee { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Cost { get; set; }

        public DateTime? Deleted { get; set; }

        public string Category { get; set; }

        [DisplayName("Status")]
        public Enums.ReconciliationStatus StatusEnum { get; set; }

        public string ReferenceNumber { get; set; }

        [DisplayName("VAT Rate")]
        public decimal VATRate { get; set; }
    }
}