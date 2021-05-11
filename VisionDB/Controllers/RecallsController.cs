using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;
using VisionDB.Helper;
using System.Web.Configuration;

namespace VisionDB.Controllers
{
    [Authorize]
    public class RecallsController : VisionDBController
    {
        int NumberOfRecallsToProcessInBatch = Convert.ToInt32(WebConfigurationManager.AppSettings["NumberOfRecallsToProcessInBatch"]);

        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult Recall(string LastRecallDays, string IncludeUntested, string IncludeEyeExams, string IncludeContactLenRecalls,
            DateTime? Start, DateTime? End, string RecallTemplateOption)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //set initial values
            //todo: low priority - add to practice config screen
            if (LastRecallDays == null)
            {
                LastRecallDays = "5";
            }
            if (IncludeUntested == null)
            {
                IncludeUntested = "false";
            }
            if (IncludeEyeExams == null)
            {
                IncludeEyeExams = "true";
            }
            if (IncludeContactLenRecalls == null)
            {
                IncludeContactLenRecalls = "true";
            }

            ViewBag.LastRecallDays = LastRecallDays;
            ViewBag.IncludeUntested = bool.Parse(IncludeUntested);
            ViewBag.IncludeEyeExams = bool.Parse(IncludeEyeExams);
            ViewBag.IncludeContactLenRecalls = bool.Parse(IncludeContactLenRecalls);

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            RecallTemplate recallTemplate = null;
            if (RecallTemplateOption != null && RecallTemplateOption.Length > 0 && new Guid(RecallTemplateOption) != Guid.Empty)
            {
                recallTemplate = db.RecallTemplates.Find(new Guid(RecallTemplateOption));
            }
            else
            {
                ModelState.AddModelError("RecallTemplateOption", "A recall template must be selected");
            }

            if (Start == null)
            {
                Start = practice.RecallDateCutOff;
                End = Start.Value.AddMonths(1) > DateTime.Now.Date.AddDays(14) ? Start.Value.AddMonths(1) : DateTime.Now.Date.AddDays(14);
            }

            if (End == null)
            {
                End = Start.Value.AddMonths(1);
            }

            ViewBag.Start = Start.Value;
            ViewBag.End = End.Value;

            ViewBag.RecallLetterCount = db.Messages.Where(r =>
                r.practice.Id == practiceId
                && r.Cancelled == null
                && r.Sent == null).Count();
            ViewBag.AgeOrDOBTitle = practice.ShowDOBOnPatientSearch == false ? "Age" : "DOB";
            return View();
        }

        public ActionResult _Read([DataSourceRequest] DataSourceRequest request, string LastRecallDays, bool IncludeUntested, bool IncludeEyeExams, bool IncludeContactLenRecalls, DateTime Start, DateTime End)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RecallViewModel> recalls = GetRecallList(Start, End, LastRecallDays, IncludeUntested, IncludeEyeExams, IncludeContactLenRecalls).ToList();

            return Json(recalls.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<RecallViewModel> recalls, string DocumentTemplateOption)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (DocumentTemplateOption == null)
            {
                TempData["Error"] = "You must select a recall document"; 
                return View();
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            DocumentTemplate documentTemplate = db.DocumentTemplates.Find(new Guid(DocumentTemplateOption));

            ModelState.Clear();

            if (ModelState.IsValid && recalls != null)
            {
                Customer customer;
                foreach (RecallViewModel recall in recalls)
                {
                    try
                    {
                        customer = db.Customers.Find(recall.Id);
                        SendRecall(db, customer, documentTemplate, recall.appointmentCategory);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Recall_Error, e.Message, recall.Id, false);
                    }
                }

                return RedirectToAction("Recall");
            }

            return Json(ModelState.ToDataSourceResult());
        }

        public ActionResult RecallLetters()
        {
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            ViewBag.Practice = practice;

            return View();
        }


        public ActionResult RecallLettersSent()
        {
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            IQueryable<Message> messages = db.Messages.Where(m => 
                m.practice.Id == practiceId
                && m.Cancelled == null
                && m.Sent == null);

            foreach (Message message in messages)
            {
                message.Sent = DateTime.Now;
                message.StatusMessage = "Sent";
            }

            db.SaveChanges();

            TempData["Message"] = "Recall letters marked as sent";
            return RedirectToAction("Recall");
        }

        public IEnumerable<RecallViewModel> GetRecallList(DateTime Start, DateTime End, string LastRecallDays, bool IncludeUntested, bool IncludeEyeExamRecalls, bool IncludeContactLensRecalls)
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            DateTime recallSentCutOff = DateTime.Now.Date.AddDays(1 - int.Parse(LastRecallDays));
            Start = Start.AddDays(-1);
            End = End.AddDays(1);

            var results = db.Customers.Where(c => c.practice.Id == practiceId
                && c.Deleted == null
                && ((IncludeEyeExamRecalls && c.NextDueDateEyeExam != null && c.NextDueDateEyeExam > Start && c.NextDueDateEyeExam < End && (c.EyeExamRecallSent < recallSentCutOff || c.EyeExamRecallSent == null))
                || (IncludeContactLensRecalls && c.NextDueDateContactLensExam != null && c.NextDueDateContactLensExam > Start && c.NextDueDateContactLensExam < End && (c.ContactLensExamRecallSent < recallSentCutOff || c.ContactLensExamRecallSent == null))
                || (IncludeUntested && (c.NextDueDateEyeExam == null || c.NextDueDateContactLensExam == null) && (c.EyeExamRecallSent < recallSentCutOff || c.EyeExamRecallSent == null) && (c.ContactLensExamRecallSent < recallSentCutOff || c.ContactLensExamRecallSent == null))
                )).OrderBy(c => c.NextDueDateEyeExam).Take(NumberOfRecallsToProcessInBatch).ToList(); 

            List<RecallViewModel> recalls = new List<RecallViewModel>();

            foreach (Customer customer in results)
            {
                recalls.Add(new RecallViewModel
                {
                    Id = customer.Id,
                    CustomerName = customer.CustomerToString,
                    CustomerNumbers = customer.AvailableNumbers,
                    CustomerEmail = customer.Email,
                    CustomerAgeOrDOB = practice.ShowDOBOnPatientSearch == false ? customer.AgeText : (customer.DOB.HasValue ? customer.DOB.Value.ToShortDateString() : "DOB not specified"),
                    appointmentCategory = GetAppointmentCategory(customer, IncludeEyeExamRecalls, IncludeContactLensRecalls),
                    RecallTemplate = GetRecallTemplateName(customer, GetAppointmentCategory(customer, IncludeEyeExamRecalls, IncludeContactLensRecalls)),
                    Due = customer.NextTestDueDate,
                    LastRecallDate = customer.LastRecallMessage != null ? customer.LastRecallMessage.AddedToQueue : (DateTime?) null,
                    RecallCount = customer.RecallCount,
                    StatusMessage = customer.LastRecallMessage != null ? customer.LastRecallMessage.StatusMessage : null
                });
            }

            return recalls.AsEnumerable();
        }

        public IEnumerable<RecallViewModel> GetRecallList(DateTime Start, bool IncludeEyeExamRecalls, bool IncludeContactLensRecalls)
        {
            CustomersDataContext db = new CustomersDataContext();
            DateTime recallSentCutOff = DateTime.Now.Date.AddDays(-7); //todo: look at recall template specified
            DateTime End = DateTime.Now.Date;

            var results = db.Customers.Where(c => c.Deleted == null
                && c.ExcludeFromRecalls == false
                && ((IncludeEyeExamRecalls && c.eyeExamRecallTemplate != null && c.NextDueDateEyeExam != null && c.NextDueDateEyeExam > Start && c.NextDueDateEyeExam < End && (c.EyeExamRecallSent < recallSentCutOff || c.EyeExamRecallSent == null))
                || (IncludeContactLensRecalls && c.contactLensExamRecallTemplate != null && c.NextDueDateContactLensExam != null && c.NextDueDateEyeExam > Start && c.NextDueDateContactLensExam < End && (c.ContactLensExamRecallSent < recallSentCutOff || c.ContactLensExamRecallSent == null))
                )).OrderBy(c => c.NextDueDateEyeExam).Take(NumberOfRecallsToProcessInBatch).ToList();

            List<RecallViewModel> recalls = new List<RecallViewModel>();

            foreach (Customer customer in results)
            {
                recalls.Add(new RecallViewModel
                {
                    Id = customer.Id,
                    CustomerName = customer.CustomerToString,
                    CustomerNumbers = customer.AvailableNumbers,
                    CustomerEmail = customer.Email,
                    CustomerAgeOrDOB = customer.practice.ShowDOBOnPatientSearch == false ? customer.AgeText : (customer.DOB.HasValue ? customer.DOB.Value.ToShortDateString() : "DOB not specified"),
                    appointmentCategory = GetAppointmentCategory(customer, IncludeEyeExamRecalls, IncludeContactLensRecalls),
                    RecallTemplate = GetRecallTemplateName(customer, GetAppointmentCategory(customer, IncludeEyeExamRecalls, IncludeContactLensRecalls)),
                    Due = customer.NextTestDueDate,
                    LastRecallDate = customer.LastRecallMessage != null ? customer.LastRecallMessage.AddedToQueue : (DateTime?) null,
                    RecallCount = customer.RecallCount,
                    StatusMessage = customer.LastRecallMessage != null ? customer.LastRecallMessage.StatusMessage : null
                });
            }

            return recalls.AsEnumerable();
        }

        private VisionDB.Models.Enums.AppointmentCategory GetAppointmentCategory(Customer customer, bool IncludeEyeExamRecalls, bool IncludeContactLensRecalls)
        {
            if (IncludeEyeExamRecalls && customer.eyeExamRecallTemplate != null && customer.NextDueDateContactLensExam < customer.NextDueDateEyeExam)
            {
                return Enums.AppointmentCategory.Eye_Exam;
            }
            else if (IncludeContactLensRecalls && customer.contactLensExamRecallTemplate != null)
            {
                return Enums.AppointmentCategory.Contact_Lens_Exam;
            }
            else
            {
                return Enums.AppointmentCategory.Eye_Exam;
            }
        }

        private string GetRecallTemplateName(Customer customer, VisionDB.Models.Enums.AppointmentCategory appointmentCategory)
        {
            if (appointmentCategory == Enums.AppointmentCategory.Eye_Exam)
            {
                return customer.eyeExamRecallTemplate != null ? customer.eyeExamRecallTemplate.Name : "";
            }

            if (appointmentCategory == Enums.AppointmentCategory.Contact_Lens_Exam)
            {
                return customer.contactLensExamRecallTemplate != null ? customer.contactLensExamRecallTemplate.Name : "";
            }

            return "";
        }

        public void ProcessRecalls()
        {
            CustomersDataContext db = new CustomersDataContext();

            Customer patientWithOldestRecallDue = db.Customers.Where(c => c.Deleted == null && c.NextTestDueDate != null).OrderBy(c => c.NextTestDueDate).FirstOrDefault();

            if (patientWithOldestRecallDue != null && patientWithOldestRecallDue.NextDueDateEyeExam != null)
            {
                DateTime Start = patientWithOldestRecallDue.NextDueDateEyeExam.Value;

                List<RecallViewModel> recalls = GetRecallList(Start, true, true).ToList();

                int recallsProcessed = 0;
                foreach (RecallViewModel recall in recalls)
                {
                    Customer customer = db.Customers.Find(recall.Id);

                    try
                    {
                        if (customer.eyeExamRecallTemplate != null)
                        {
                            foreach (RecallDocument recallDocument in new RecallTemplatesController().GetRecallDocuments(db, customer.eyeExamRecallTemplate, customer.practice))
                            {
                                SendRecall(db, customer, recallDocument.documentTemplate, recall.appointmentCategory);
                                customer.RecallCount += 1;
                            }
                        }
                        //else they are picked up by recalls section

                        db.SaveChanges();

                        recallsProcessed += 1;
                    }
                    catch (Exception ex)
                    {
                        VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Recall_Error, string.Concat("Error processing recall", Environment.NewLine, ex.Message), recall.Id, false);
                    }


                }

                VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, string.Format("{0} recalls processed", recallsProcessed.ToString()), null, false);
            }
        }

        private void SendRecall(CustomersDataContext db, Customer customer, DocumentTemplate documentTemplate, Enums.AppointmentCategory appointmentCategory)
        {
            Message message = new Message();
            message.IsRecall = true;
            message.messageMethod = documentTemplate.TemplateMethodEnum;
            message.MessageText = DocumentTemplatesController.PopulateDocument(documentTemplate, customer);
            message.RecipientId = customer.Id;
            message.SenderUser = db.ApplicationUsers.Where(u => u.UserName == "Daemon").First();
            message.Id = Guid.NewGuid();
            message.AddedToQueue = DateTime.Now;
            message.Cancelled = null;
            message.CancelledByUser = null;
            message.Sent = null;
            message.practice = customer.practice;

            if (message.messageMethod == Enums.MessageMethod.SMS)
            {
                if (customer.SMSNumber == null || customer.SMSNumber.Trim().Length == 0)
                {
                    message.ToAddressNumber = "invalid";
                    TempData["Error"] = "Error sending recalls - some patients did not have a mobile number.";
                    message.StatusMessage = "Unable to send recall due to invalid mobile number";
                    message.Cancelled = DateTime.Now;
                }
                else
                {
                    message.ToAddressNumber = customer.SMSNumber; 
                }
                message.Sender = customer.practice.SMSSenderName;
                if (customer.SMSNumber != null && customer.SMSNumber.Length > 0)
                {
                    SMSTransaction smsTransaction = new SMSTransaction();
                    smsTransaction.Id = Guid.NewGuid();
                    smsTransaction.InsertTimestamp = DateTime.Now;
                    smsTransaction.company = customer.practice.company;
                    smsTransaction.Quantity = -1;
                    message.SMSInventory = smsTransaction;
                    db.SMSInventory.Add(smsTransaction);
                    if (customer.practice.CanSendMessages)
                    {
                        string result = "";
                        MessagesController.SendSMS(message, customer.practice, ref result);
                        message.StatusMessage = "Sent";
                    }
                }
            }
            else if (message.messageMethod == Enums.MessageMethod.Email)
            {
                if (customer.Email == null || customer.Email.Trim().Length == 0)
                {
                    message.ToAddressNumber = "invalid";
                    TempData["Error"] = "Error sending recalls - some patients did not have an email address.";
                    message.StatusMessage = "Unable to send recall due to invalid email address";
                    message.Cancelled = DateTime.Now;
                }
                else
                {
                    message.ToAddressNumber = customer.Email;
                }
                message.Sender = customer.practice.Email != null ? customer.practice.Email : "noreply@visiondb.co.uk";
                message.Subject = "Eye test recall"; //todo: low priority - subject templates
                if (customer.Email != null && customer.Email.Trim().Length > 0)
                {
                    EmailHelper emailHelper = new EmailHelper();
                    emailHelper.SendEmail(message.ToAddressNumber, message.Subject, message.MessageText, message.Sender, true);
                    message.Sent = DateTime.Now;
                    message.StatusMessage = "Sent";
                }
            }
            else if (message.messageMethod == Enums.MessageMethod.Letter)
            {
                //todo: process via docmail if option selected
                //todo: allow manual sending of letters

                message.ToAddressNumber = customer.Address;
                message.Postcode = customer.Postcode;
                message.Sender = customer.practice.Name;
                message.StatusMessage = "Ready for printing";
            }

            if (message.Cancelled == null)
            {
                if (appointmentCategory == Enums.AppointmentCategory.Eye_Exam)
                {
                    customer.EyeExamRecallSent = DateTime.Now;
                }
                else if (appointmentCategory == Enums.AppointmentCategory.Contact_Lens_Exam)
                {
                    customer.ContactLensExamRecallSent = DateTime.Now;
                }
                customer.RecallCount += 1;
            }

            customer.LastRecallMessage = message;

            db.Messages.Add(message);
        }
	}
}
