using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace VisionDB.Models
{
    public class InvoiceDetail
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        //[UIHint("OrderDetailEditor")] //is this required for EditorTemplates\OrderDetailEditor.cshtml
        public virtual Product product { get; set; }

        public virtual Invoice invoice { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal UnitPriceExcVAT
        {
            get
            {
                return UnitPrice * (1 / (1 + (VATRate / 100)));
            }
        }

        public string UnitPriceIncVATToString
        {
            get
            {
                return string.Concat("£", UnitPrice);
            }
        }
        public decimal DiscountPercentage { get; set; }

        [Required]
        public int Quantity { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }

        public decimal VATRate { get; set; }

        public DateTime Added { get; set; }


        public decimal TotalExcVAT
        {
            get
            {
                return Quantity * UnitPrice * (1 / (1 + (VATRate / 100))) * (1 - (DiscountPercentage / 100));
            }
        }
        public decimal TotalIncVAT
        {
            get
            {
                return Quantity * UnitPrice * (1 - (DiscountPercentage / 100));
            }
        }

        public string FullProductName
        {
            get
            {
                if (product != null)
                {
                    if (product.ProductTypeEnum != Product.ProductTypeList.NHS)
                    {
                        return string.Concat(product.Name, " - ", product.Code);
                    }
                    else
                    {
                        return product.Name;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public int? SpectacleNumber { get; set; }

        public string Description { get; set; }

        public Enums.ReconciliationStatus ReconciliationStatusEnum { get; set; }

        public DateTime? ReconciledStatusUpdated { get; set; }

        public virtual ApplicationUser ReconciledStatusUpdatedByUser { get; set; }

        public string ReconciliationNotes { get; set; }

        public Enums.InvoiceDetailStatus StatusEnum { get; set; }

        public decimal? CostPrice { get; set; }

        public bool Dispensed { get; set; }

        public string Code { get; set; }

    }
}