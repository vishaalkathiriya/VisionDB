using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using VisionDB.Controllers;

namespace VisionDB.Models
{
    public class Invoice
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime CreatedTimestamp { get; set; }

        [Required]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}")]
        public DateTime InvoiceDate { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public virtual Customer customer { get; set; }

        public virtual Practice practice { get; set; }

        public string Notes { get; set; }

        public decimal BalanceExcVAT { get; set; }

        public decimal TotalExcVAT { get; set; }

        public decimal BalanceIncVAT { get; set; }

        public decimal TotalIncVAT { get; set; }

        [XmlIgnore]
        public decimal VAT 
        {
            get
            {
                return TotalIncVAT - TotalExcVAT;
            }
        }

        [XmlIgnore]
        public Enums.ReconciliationStatus ReconciliationStatus
        {
            get
            {
                CustomersDataContext db = new CustomersDataContext();
                IList<InvoiceDetail> InvoiceDetails = new Controllers.InventoryController().GetInvoiceDetails(this, db);

                if (InvoiceDetails != null && InvoiceDetails.Count > 0)
                {
                    foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                    {
                        if (invoiceDetail.ReconciliationStatusEnum == Enums.ReconciliationStatus.Flagged)
                        {
                            return Enums.ReconciliationStatus.Flagged;
                        }

                        if (invoiceDetail.ReconciliationStatusEnum == Enums.ReconciliationStatus.Pending)
                        {
                            return Enums.ReconciliationStatus.Pending;
                        }
                    }
                }

                if (BalanceIncVAT == 0m)
                {
                    return Enums.ReconciliationStatus.Reconciled;
                }
                else
                {
                    return Enums.ReconciliationStatus.Pending;
                }
            }
        }


        [XmlIgnore]
        public Enums.InvoiceStatus DispenseStatus
        {
            get
            {
                CustomersDataContext db = new CustomersDataContext();

                IList<InvoiceDetail> InvoiceDetails = new Controllers.InventoryController().GetInvoiceDetails(this, db);

                if (InvoiceDetails != null && InvoiceDetails.Count > 0)
                {
                    IList<InvoiceDetail> GoodsInProductDetails = InvoiceDetails.Where(id => id.product.GoodsInProduct).ToList();

                    if (GoodsInProductDetails.Count > 0)
                    {
                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Not_Ordered).Count() == GoodsInProductDetails.Count)
                        {
                            return Enums.InvoiceStatus.Not_Ordered;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Ordered).Count() > 0)
                        {
                            return Enums.InvoiceStatus.Awaiting_Goods;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Received).Count() > 0 && BalanceIncVAT != 0m)
                        {
                            return Enums.InvoiceStatus.Awaiting_Payment;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Awaiting_Collection_or_Dispatch).Count() > 0 && BalanceIncVAT != 0m)
                        {
                            return Enums.InvoiceStatus.Awaiting_Payment;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Received).Count() > 0 && BalanceIncVAT == 0m)
                        {
                            return Enums.InvoiceStatus.Awaiting_Collection_Or_Dispatch;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Awaiting_Collection_or_Dispatch).Count() > 0 && BalanceIncVAT == 0m)
                        {
                            return Enums.InvoiceStatus.Awaiting_Collection_Or_Dispatch;
                        }

                        if (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Dispatched).Count() + GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Collected).Count() == GoodsInProductDetails.Count)
                        {
                            return Enums.InvoiceStatus.Complete;
                        }

                        if ((GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Ordered).Count() > 0
                            || GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Not_Ordered).Count() > 0)
                            && (GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Awaiting_Collection_or_Dispatch).Count() > 0
                            || GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Collected).Count() > 0
                            || GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Dispatched).Count() > 0
                            || GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Notified).Count() > 0
                            || GoodsInProductDetails.Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Received).Count() > 0))
                        {
                            return Enums.InvoiceStatus.Awaiting_Goods;
                        }

                        if (BalanceIncVAT != 0m)
                        {
                            return Enums.InvoiceStatus.Awaiting_Payment;
                        }
                    }

                    if (BalanceIncVAT != 0m)
                    {
                        return Enums.InvoiceStatus.Awaiting_Payment;
                    }
                    else
                    {
                        return Enums.InvoiceStatus.Complete;
                    }

                    throw new IndexOutOfRangeException(Helper.ErrorHelper.GetErrorText(string.Format("Unable to determine order status. Invoice ID: {0}", Id.ToString()), "112"));
                }
                else
                {
                    return Enums.InvoiceStatus.Complete;
                }
            }
        }

        [XmlIgnore]
        public string NHSVoucher
        {
            get
            {
                CustomersDataContext db = new CustomersDataContext();

                IList<InvoiceDetail> InvoiceDetails = new Controllers.InventoryController().GetInvoiceDetails(this, db);

                if (InvoiceDetails != null && InvoiceDetails.Count > 0)
                {
                    foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                    {
                        if (invoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.NHS)
                        {
                            return invoiceDetail.product.Name;
                        }
                    }
                }

                return "";
            }
        }

        public override string ToString()
        {
            if (Math.Round(BalanceIncVAT, 2) > 0)
            {
                return string.Format("£{0} remaining. {1} {2}", Math.Round(BalanceIncVAT, 2), ReconciliationStatus.ToString(), NHSVoucher);
            }
            else if (Math.Round(BalanceIncVAT, 2) < 0)
            {
                return string.Format("£{0}. {1} {2}", Math.Round(BalanceIncVAT, 2), ReconciliationStatus.ToString(), NHSVoucher);
            }
            else
            {
                return string.Format("Paid. {0} {1}", ReconciliationStatus.ToString(), NHSVoucher);
            }
        }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }

        public string InvoiceNumber { get; set; }

        public decimal DiscountPercentage { get; set; }

        public Enums.MethodSentBy MethodSentByEnum { get; set; }

        public virtual EyeExam DispenseEyeExam { get; set; }
    }
}