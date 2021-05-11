using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VisionDB.Models
{
    public static class SessionInvoiceRepository
    {
        public static IList<InvoiceDetail> All()
        {
            IList<InvoiceDetail> InvDetailsList = (IList<InvoiceDetail>)HttpContext.Current.Session["InvoiceDetails"];
        
            if (InvDetailsList == null)
            {
               var  results =
                    (from invoiceDetail in new CustomersDataContext().InvoiceDetails
                     select new 
                     {
                          invoiceDetail.Id,
                          invoiceDetail.invoice,
                          UnitPrice = invoiceDetail.UnitPrice,
                          invoiceDetail.Quantity,
                     });
               InvDetailsList = results.ToNonAnonymousList(typeof(InvoiceDetail)) as List<InvoiceDetail>;
            }
            return InvDetailsList;
        }

        public static void Insert(InvoiceDetail invoiceDetail, string Mode, string userId, CustomersDataContext dbContext)
        {
            if (Mode != "Create")

            {
                invoiceDetail.Id = Guid.NewGuid();
                invoiceDetail.CreatedByUser = (ApplicationUser)dbContext.ApplicationUsers.Find(userId);
                dbContext.InvoiceDetails.Add(invoiceDetail);      
            }
            else
            {
                All().Insert(0, invoiceDetail);
            }
        }
        public static void Update(InvoiceDetail invoiceDetail)
        {
            CustomersDataContext db = new CustomersDataContext();

            //InvoiceDetail target = One(p => p.Id == invoiceDetail.Id);
            InvoiceDetail target = db.InvoiceDetails.Find(invoiceDetail.Id);
            if (target != null)
            {
                target.invoice = invoiceDetail.invoice;
                target.UnitPrice = invoiceDetail.UnitPrice;
                target.Quantity = invoiceDetail.Quantity;
                target.product = invoiceDetail.product;

                //dbContext.Entry(target).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        public static void Delete(InvoiceDetail invoiceDetail)
        {
            if (invoiceDetail.Id != Guid.Empty )
            {
                using (CustomersDataContext dbContext = new CustomersDataContext())
                {
                    dbContext.Entry(invoiceDetail).State = System.Data.Entity.EntityState.Modified;
                    dbContext.InvoiceDetails.Remove(invoiceDetail);
                    dbContext.SaveChanges();
                }  
            }
        }
    }
}