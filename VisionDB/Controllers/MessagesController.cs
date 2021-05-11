using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Helper;
using VisionDB.Models;
using MvcReportViewer;

namespace VisionDB.Controllers
{
    public class MessagesController : VisionDBController
    {
        [HttpGet]
        public ActionResult NewSMS()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            if (customer.SMSNumber != null)
            {
                ViewBag.Customer = customer;
                return View();
            }
            else
            {
                TempData["Error"] = "Customer does not have a valid SMS number";
                return RedirectToAction("Customer", "Customers", new { customer.Id });
            }
        }

        [HttpPost]
        public ActionResult NewSMS(Message message)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);

            if (ModelState.IsValid)
            {
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);

                message.Id = Guid.NewGuid();
                message.AddedToQueue = DateTime.Now;
                message.SenderUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                message.messageMethod = Enums.MessageMethod.SMS;
                message.ToAddressNumber = customer.SMSNumber; 
                message.Sender = practice.SMSSenderName != null ? practice.SMSSenderName : "VisionDB";
                SMSTransaction smsTransaction = new SMSTransaction();
                smsTransaction.Id = Guid.NewGuid();
                smsTransaction.InsertTimestamp = DateTime.Now;
                smsTransaction.company = practice.company;
                smsTransaction.Quantity = -1;
                message.SMSInventory = smsTransaction;
                db.SMSInventory.Add(smsTransaction);
                message.AddedToQueue = DateTime.Now;
                message.Cancelled = null;
                message.CancelledByUser = null;
                message.Sent = null;
                message.practice = practice;
                message.RecipientId = customer.Id;
                db.Messages.Add(message);
                customer.LastUpdated = DateTime.Now;

                string result = "";

                bool success = SendSMS(message, practice, ref result);

                db.SaveChanges();

                if (success == true)
                {
                    TempData["Message"] = "Message sent";
                }
                else
                {
                    TempData["Message"] = "Unable to send message";
                    AuditLogEntry audit = new AuditLogEntry();
                    audit.Id = Guid.NewGuid();
                    audit.EntryText = "Unable to send message with Id: " + message.Id.ToString() + " error code: " + result;
                    audit.UserName = message.SenderUser.UserName;
                    audit.EntryType = "SMS Failed";
                    audit.Added = DateTime.Now;
                    db.AuditLog.Add(audit);
                    db.SaveChanges();
                }

                return RedirectToAction("Customer", "Customers", new { customer.Id });
            }
            else
            {
                ViewBag.Customer = customer;
                return View();
            }
        }

        public static bool SendSMS(Message message, Practice practice, ref string result)
        {
            bool success;

            if (practice.CanSendMessages)
            {
                FastSMS fastSMS = new FastSMS();
                result = fastSMS.SendText(message.ToAddressNumber, message.MessageText, message.Sender);
            }
            else
            {
                result = "0";
            }

            message.Ref = result;

            int resultCode = int.Parse(result);

            if (resultCode >= 0)
            {
                message.Sent = DateTime.Now;
                success = true;
            }
            else
            {
                success = false;
            }
            return success;
        }

        public ActionResult ViewSMS(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            CustomersDataContext db = new CustomersDataContext();
            Message message = db.Messages.Find(Id);

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            ViewBag.Customer = customer;
            return View(message);
        }

        [HttpGet]
        public ActionResult NewEmail()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            if (customer.Email != null)
            {
                ViewBag.Customer = customer;
                return View();
            }
            else
            {
                TempData["Error"] = "Customer does not have a valid email address";
                return RedirectToAction("Customer", "Customers", new { customer.Id });
            }
        }

        [HttpPost]
        public ActionResult NewEmail(Message message)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Customer customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);

                message.Id = Guid.NewGuid();
                message.AddedToQueue = DateTime.Now;
                message.SenderUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                message.messageMethod = Enums.MessageMethod.Email;
                message.ToAddressNumber = customer.Email;
                message.Sender = practice.Email != null ? practice.Email : "reply@visiondb.co.uk";
                message.AddedToQueue = DateTime.Now;
                message.Cancelled = null;
                message.CancelledByUser = null;
                message.Sent = null;
                message.practice = practice;
                message.RecipientId = customer.Id;
                db.Messages.Add(message);
                customer.LastUpdated = DateTime.Now;

                EmailHelper emailHelper = new EmailHelper();
                try
                {
                    emailHelper.SendEmail(message.ToAddressNumber, message.Subject, message.MessageText, message.Sender, false);
                    TempData["Message"] = "Message sent";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Unable to send message";
                    AuditLogEntry audit = new AuditLogEntry();
                    audit.Id = Guid.NewGuid();
                    audit.EntryText = "Unable to send message with Id: " + message.Id.ToString() + " error: " + ex.Message;
                    audit.UserName = message.SenderUser.UserName;
                    audit.EntryType = "Email Failed";
                    audit.Added = DateTime.Now;
                    db.AuditLog.Add(audit);
                    db.SaveChanges();
                }

                db.SaveChanges();

                return RedirectToAction("Customer", "Customers", new { customer.Id });
            }
            else
            {
                return View();
            }
        }

        public ActionResult ViewEmail(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Message message = db.Messages.Find(Id);

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            ViewBag.Customer = customer;
            return View(message);
        }

        public ActionResult ViewLetter(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Message message = db.Messages.Find(Id);

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            ViewBag.Customer = customer;
            return View(message);
        }

        public ActionResult LetterExport(Guid Id, string ReportName)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, ReportName),
                new { messageId = Id });
        }

        public void ProcessMessages()
        {
            CustomersDataContext db = new CustomersDataContext();
            List<Message> messages = db.Messages.Where(m => m.Sent == null
                && m.Cancelled == null
                && m.IsRecall == false
                && (m.ScheduledToBeSent != null && m.ScheduledToBeSent <= DateTime.Now)).ToList();

            int messagesProcessed = 0;
            foreach (Message message in messages)
            {
                try
                {


                    messagesProcessed += 1;
                }
                catch (Exception ex)
                {
                    VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, string.Concat("Error processing message", Environment.NewLine, ex.Message), message.Id, false);
                }
            }

            VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, string.Format("{0} messages processed", messagesProcessed.ToString()), null, false);
        }
    }
}