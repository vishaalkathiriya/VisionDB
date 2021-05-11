using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class InvoiceDetailViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public Product product { get; set; }

        public string CustomerName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercentage { get; set; }

        public int Quantity { get; set; }

        public decimal VATRate { get; set; }

        public DateTime Added { get; set; }

        public Guid InvoiceId { get; set; }

        public string FullProductName
        {
            get
            {
                if (product != null)
                {
                    string productName;
                    if (product.ProductTypeEnum != Product.ProductTypeList.NHS)
                    {
                        if (Code != null && Code.Length > 0)
                        {
                            productName = string.Concat(product.Name, " - ", Code);
                        }
                        else if (product.Code != null && product.Code.Length > 0)
                        {
                            productName = string.Concat(product.Name, " - ", product.Code);
                        }
                        else
                        {
                            productName = product.Name;
                        }
                    }
                    else
                    {
                        productName = product.Name;
                    }

                    if (DiscountPercentage > 0)
                    {
                        productName += string.Concat(" (", Math.Round(DiscountPercentage).ToString(), "% discount applied)");
                    }

                    return productName;
                }
                else
                {
                    return null;
                }
            }
        }

        public string UnitPriceToString
        {
            get
            {
                return string.Concat("£", Math.Round(UnitPrice * (1 - (DiscountPercentage / 100)), 2));
            }
        }

        public string UnitPriceToStringAbsolute
        {
            get
            {
                return string.Concat("£", Math.Abs(Math.Round(UnitPrice * (1 - (DiscountPercentage / 100)), 2)));
            }
        }
        public int? SpectacleNumber { get; set; }

        public string Description { get; set; }

        public string InvoiceNumber { get; set; }

        public string ReconciliationStatus { get; set; }

        public string AddedToString
        {
            get
            {
                return Added.ToShortDateString();
            }
        }

        public string Colour
        {
            get
            {
                if (ReconciliationStatus == Enums.ReconciliationStatus.Flagged.ToString())
                {
                    return "#FA8072"; //HTML colour Salmon
                }
                else if (ReconciliationStatus == Enums.ReconciliationStatus.Reconciled.ToString())
                {
                    return "#7FFFD4"; //HTML colour Aquamarine
                }
                else
                {
                    return "white";
                }
            }
        }

        public string Status { get; set; }

        public bool Dispensed { get; set; }

        public string Code { get; set; }
    }
}