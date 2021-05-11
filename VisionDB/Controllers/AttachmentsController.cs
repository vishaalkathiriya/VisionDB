using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    public class AttachmentsController : VisionDBController
    {
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
            ViewBag.Customer = customer;

            var validationResult = db.Entry(customer).GetValidationResult();

            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                return RedirectToAction("Edit", "Customers", new { customer.Id });
            }

            return View();
        }

        public ActionResult Submit(IEnumerable<HttpPostedFileBase> files)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            Customer customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);

            if (files != null)
            {
                foreach (HttpPostedFileBase file in files)
                {
                    Guid fileId = Guid.NewGuid();
                    string fileName = fileId.ToString() + "_" + Path.GetFileName(file.FileName);
                    string path = Path.Combine(WebConfigurationManager.AppSettings["AttachementFolder"].ToString(), fileName);
                    Attachment attachment = new Attachment();
                    attachment.Id = fileId;
                    attachment.CreatedByUser = user;
                    attachment.CreatedTimestamp = DateTime.Now;
                    attachment.customer = customer;
                    attachment.FileName = fileName;                 
                    
                    file.SaveAs(path);
                    db.Attachments.Add(attachment);
                }
                customer.LastUpdated = DateTime.Now;
                db.SaveChanges();

                TempData["UploadedFiles"] = GetFileInfo(files); //for displaying filenames
            }

            return RedirectToAction("Customer", "Customers", new { customer.Id });
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Attachment attachment = db.Attachments.Find(Id);
            ViewBag.Customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);

            return View(attachment);
        }

        [HttpPost]
        public ActionResult Edit(Attachment attachment)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Attachment existingAttachment = db.Attachments.Find(attachment.Id);
                existingAttachment.FileComments = attachment.FileComments;
                existingAttachment.customer.LastUpdated = DateTime.Now;

                db.SaveChanges();

                return RedirectToAction("Customer", "Customers", new { existingAttachment.customer.Id });
            }
            else
            {
                //todo: show error
                return View();
            }
        }

        public ActionResult Delete(Attachment attachment)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Attachment existingAttachment = db.Attachments.Find(attachment.Id);
                existingAttachment.Deleted = DateTime.Now;
                existingAttachment.DeletedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                db.SaveChanges();

                return RedirectToAction("Customer", "Customers", new { existingAttachment.customer.Id });
            }
            else
            {
                //todo: standardise error messages
                ViewBag.Error = "Unable to delete attachment.";
                return View();
            }
        }

        private IEnumerable<string> GetFileInfo(IEnumerable<HttpPostedFileBase> files)
        {
            return
                from a in files
                where a != null
                select string.Format("{0} ({1} bytes)", Path.GetFileName(a.FileName), a.ContentLength);
        }
	}
}