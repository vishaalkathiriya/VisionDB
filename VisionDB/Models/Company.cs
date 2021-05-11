using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Company
    {
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// External Id used to go directly to a company without login. 
        /// Different to Id so that it can be changed.
        /// </summary>
        public Guid ExternalId { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Postcode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public int SMSStockLevel
        {
            get
            {
                try
                {
                    CustomersDataContext db = new CustomersDataContext();
                    if (db.SMSInventory.Where(m => m.company.Id == Id).Count() == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return db.SMSInventory.Where(m => m.company.Id == Id).Sum(m => m.Quantity);
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public string CompanyRef { get; set; }

        public int LastProductNumber { get; set; }

        [DisplayName("Enable multi-site")]
        public bool MultiSite { get; set; }
    }
}