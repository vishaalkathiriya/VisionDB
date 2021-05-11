using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VisionDB.Controllers;

namespace VisionDB.Models
{
    public class Product
    {
        [Required]
        public Guid Id { get; set; }

        public virtual Company company { get; set; }

        [DisplayName("Code")]
        public string Code { get; set; }
        
        [Required]
        [DisplayName("Product")]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [DisplayName("VAT Rate")]
        public decimal VATRate { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }

        public override string ToString()
        {
            if (Code != null && Code.Length > 0)
            {
                return string.Concat(Name, " - ", Code);
            }
            else
            {
                return Name;
            }
        }

        public string ProductToString
        {
            get
            {
                return ToString();
            }
        }

        public string ProductToStringIncPrice
        {
            get
            {
                if (Price != 0m)
                {
                    return ToString() + " - £" + Math.Round(Price, 2);
                }
                else
                {
                    return ToString();
                }
            }
        }

        [DisplayName("Product Type")]
        public ProductTypeList ProductTypeEnum { get; set; }

        public enum ProductTypeList
        {
            None = 0,
            Frame = 1,
            Lens = 2,
            Service = 3,
            Discount = 4,
            Payment = 5,
            NHS = 6,
            Sunglasses = 7,
            Contact_Lens = 8,
            Tint = 9,
            Professional_Fee = 10,
            Repair_Replacement = 11,
            Sight_Test = 12,
            Refund = 13
        }

        public DateTime LastUpdated { get; set; }

        [DisplayName("Stock Level")]
        public int PracticeStockLevel 
        {
            get
            {
                InventoryController inventoryController = new InventoryController();
                return inventoryController.GetPracticeStockLevel(Id, ((ApplicationUser)HttpContext.Current.Session["user"]).practiceId);
            }
        }

        [DisplayName("Company Stock Level")]
        public int CompanyStockLevel
        {
            get
            {
                InventoryController inventoryController = new InventoryController();
                return inventoryController.GetCompanyStockLevel(Id);
            }
        }

        public string Colour { get; set; }
        [DisplayName("Reference Number")]
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        [DisplayName("Frequently Used")]
        public bool FrequentlyUsed { get; set; }
        [DisplayName("Cost Price")]
        public decimal? CostPrice { get; set; }

        public string Manufacturer { get; set; }
        [DisplayName("Lens Type")]
        public string LensType { get; set; }

        public bool NegativeValue 
        { 
            get
            {
                if (this.ProductTypeEnum == ProductTypeList.Discount || this.ProductTypeEnum == ProductTypeList.Payment || this.ProductTypeEnum == ProductTypeList.NHS)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanBeAddedToAdhocOrder 
        { 
            get
            {
                if (this.ProductTypeEnum == ProductTypeList.Discount || this.ProductTypeEnum == ProductTypeList.None || this.ProductTypeEnum == ProductTypeList.Payment
                    || this.ProductTypeEnum == ProductTypeList.Service || this.ProductTypeEnum == ProductTypeList.Sunglasses)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool GoodsInProduct 
        {
            get
            {
                if (this.ProductTypeEnum == ProductTypeList.Frame || this.ProductTypeEnum == ProductTypeList.Lens || this.ProductTypeEnum == ProductTypeList.Sunglasses 
                    || this.ProductTypeEnum == ProductTypeList.None || this.ProductTypeEnum == ProductTypeList.Contact_Lens || this.ProductTypeEnum == ProductTypeList.Tint)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}