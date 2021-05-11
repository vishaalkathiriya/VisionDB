using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class ExpenseViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public string Payee { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Cost { get; set; }

        public DateTime? Deleted { get; set; }

        public string Category { get; set; }

        public Enums.ReconciliationStatus StatusEnum { get; set; }

        public string ReferenceNumber { get; set; }

        public decimal VATRate { get; set; }

        public string Status 
        {
            get
            {
                return StatusEnum.ToString();
            }
        }

        public string Colour
        {
            get
            {
                if (StatusEnum == Enums.ReconciliationStatus.Flagged)
                {
                    return "#FA8072"; //HTML colour Salmon
                }
                else if (StatusEnum == Enums.ReconciliationStatus.Reconciled)
                {
                    return "#7FFFD4"; //HTML colour Aquamarine
                }
                else
                {
                    return "white";
                }
            }
        }

        public string ExpenseDateToString
        {
            get
            {
                return ExpenseDate.ToShortDateString();
            }
        }

        public string CostToString
        {
            get
            {
                return string.Concat("£", Math.Round(Cost, 2));
            }
        }
    }
}