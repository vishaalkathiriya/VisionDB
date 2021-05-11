using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class InvoiceViewModel
    {
        public Guid Id { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }

        public string Summary { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceDateToString
        {
            get
            {
                return InvoiceDate.ToShortDateString() + " " + InvoiceDate.ToShortTimeString();
            }
        }

        public string Colour
        {
            get
            {
                if (!Summary.ToLower().Contains("paid"))
                {
                    return "#FA8072"; //HTML colour Salmon
                }
                else
                {
                    return "Aquamarine";
                }
            }
        }

        public string DispenseColour
        {
            get
            {
                if (DispenseStatus == Enums.InvoiceStatus.Complete)
                {
                    return "Aquamarine";
                }
                if (DispenseStatus == Enums.InvoiceStatus.Awaiting_Goods)
                {
                    return "SandyBrown";
                }
                else
                {
                    return "#FA8072"; //HTML colour Salmon
                }
            }
        }

        public decimal DiscountPercentage { get; set; }

        public Enums.InvoiceStatus DispenseStatus { get; set; }

        public string DispenseStatusToString 
        { 
            get
            {
                return DispenseStatus.ToString().Replace('_', ' ');
            }
        }
    }
}