using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;
using VisionDB.Helper;
using System.Data.Entity.Validation;
using MvcReportViewer;

namespace VisionDB.Controllers
{
    [Authorize]
    public class CustomersController : VisionDBController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Search", "Customers");
        }

        public ActionResult Search(string Search)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.SearchText = Search;
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            ViewBag.AgeOrDOBTitle = practice.ShowDOBOnPatientSearch == false ? "Age" : "DOB";
            IEnumerable<Customer> results = null;

            results = CustomerSearch(Search, db, practice);

            List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
            foreach (Customer customer in results)
            {
                patientViewModels.Add(new PatientViewModel
                {
                    Id = customer.Id,
                    Name = customer.CustomerToString,
                    Address = customer.FullAddress,
                    Number = customer.DefaultNumber,
                    AgeOrDOB = practice.ShowDOBOnPatientSearch == false ? customer.AgeText : (customer.DOB.HasValue ? customer.DOB.Value.ToShortDateString() : customer.AgeText),
                    Colour = GetColour(customer.Id)
                });
            }

            return View(patientViewModels);
        }

        private static IEnumerable<Customer> CustomerSearch(string Search, CustomersDataContext db, Practice practice)
        {
            IEnumerable<Customer> results = null;

            string splitFirstname = null;
            string splitSurname = null;
            if (!string.IsNullOrWhiteSpace(Search))
            {
                string[] substrings = Search.Split(new char[] { ' ' });

                if (substrings.Count() == 2)
                {
                    splitFirstname = substrings[0].Trim().Length > 2 ? substrings[0].Trim() : null;
                    splitSurname = substrings[1].Trim().Length > 2 ? substrings[1].Trim() : null;
                }
            }

            DateTime DOB = new DateTime();
            bool isDOB = false;
            if (Search != null)
            {
                isDOB = DateTime.TryParse(Search, out DOB);
            }

            bool IsMultiSite = practice.company.MultiSite;

            var practices = new List<Practice>();
            if (practice.company.MultiSite)
            {
                practices = db.Practices.Where(p => p.company.Id == practice.company.Id).ToList();
            }
            else
            {
                practices.Add(practice);
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                results = db.Customers.Where(c => 
                            ((!IsMultiSite && c.practice.Id == practice.Id) || (IsMultiSite && c.practice.company.Id == practice.company.Id))
                            && c.Deleted == null
                            && (c.Number.ToLower().Contains(Search.ToLower())
                            || c.Number.Replace(" ", "").ToLower().Contains(Search.Replace(" ", "").ToLower())
                            || c.Firstnames.ToLower().Contains(Search.ToLower())
                            || c.Surname.ToLower().Contains(Search.ToLower())
                            || c.Address.ToLower().Contains(Search.ToLower())
                            || c.CareHome.ToLower().Contains(Search.ToLower())
                            || c.ShelteredAccommodation.ToLower().Contains(Search.ToLower())
                            || c.Postcode.ToLower().Contains(Search.ToLower())
                            || c.Postcode.Replace(" ", "").ToLower().Contains(Search.Replace(" ", "").ToLower())
                            || c.Telephone.ToLower().Contains(Search.ToLower())
                            || c.SMSNumber.ToLower().Contains(Search.ToLower())
                            || c.SMSNumber.Replace(" ", "").ToLower().Contains(Search.Replace(" ", "").ToLower())
                            || c.Email.ToLower().Contains(Search.ToLower())
                            || (c.DOB.HasValue && isDOB && c.DOB.Value == DOB)
                            || ((splitFirstname != null && c.Firstnames.Contains(splitFirstname.ToLower()))
                            && (splitSurname != null && c.Surname.Contains(splitSurname.ToLower())))));
            }
            else
            {
                results = new List<Customer>();
            }
            return results;
        }

        public ActionResult Customer(Guid Id, string message)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            //get customer by id
            Customer customer = db.Customers.Find(Id);
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

            PopulateSubObjects(customer);
            if (message != null)
            {
                ViewData["Message"] = message;
            }

            ViewBag.LastEyeExam = new EyeExamsController().GetLatestEyeExam(db, customer);
            ViewBag.CustomerBalanceIncVAT = new CustomersController().GetCustomerBalanceIncVAT(Id);
            ViewBag.MedicalConditionsCSV = String.Join(", ", GetSelectedMedicalCondtionsText(db, customer).ToArray());
            ViewBag.CustomerTagsCSV = String.Join(", ", GetSelectedTagsText(db, customer).ToArray());
            HttpContext.Session["customer"] = customer;
            VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Patient_Record_Viewed, string.Concat(customer.CustomerToString, " viewed"), customer.Id, true);

            return View(customer);
        }

        public JsonResult _ReadJournalEntries([DataSourceRequest] DataSourceRequest request, Guid Id)
        {
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(Id);
            PopulateSubObjects(customer);
            List<JournalViewModel> journalViewModels = GetJournalEntries(db, customer);

            return Json(journalViewModels.ToDataSourceResult(request));
        }

        private void PopulateSubObjects(Customer customer)
        {
            customer.EyeExams = new EyeExamsController().GetEyeExams(customer);
            customer.Attachments = GetAttachments(customer);
            customer.Invoices = new InvoicesController().GetInvoices(customer);
            customer.Notes = new NotesController().GetNotes(customer);
            customer.Appointments = new AppointmentsController().GetAppointments(customer);

            var results = from row in new CustomersDataContext().Messages.Where(m => m.RecipientId == customer.Id)
                          select row;
            customer.Messages = results.ToList();
        }

        private static List<JournalViewModel> GetJournalEntries(CustomersDataContext db, Customer customer)
        {
            List<JournalViewModel> journal = new List<JournalViewModel>();

            foreach (EyeExam eyeExam in customer.EyeExams)
            {
                if (eyeExam.Deleted == null)
                {
                    journal.Add(new JournalViewModel(eyeExam.Id, eyeExam.ToString(), eyeExam.TestDateAndTime, JournalViewModel.EntryType.Exam));
                }
            }

            foreach (Note note in customer.Notes)
            {
                if (note.Deleted == null)
                {
                    journal.Add(new JournalViewModel(note.Id, note.Description, note.CreatedTimestamp, JournalViewModel.EntryType.Note));
                }
            }

            foreach (Invoice invoice in customer.Invoices)
            {
                if (invoice.Deleted == null)
                {
                    journal.Add(new JournalViewModel(invoice.Id, string.Format("Invoice No: {0} {1}", invoice.InvoiceNumber, invoice.ToString()), invoice.InvoiceDate, JournalViewModel.EntryType.Invoice));
                }
            }

            foreach (Attachment attachment in customer.Attachments)
            {
                if (attachment.Deleted == null)
                {
                    journal.Add(new JournalViewModel(attachment.Id, attachment.FileComments != null ? attachment.FileNameWithoutId + " " + attachment.FileComments : attachment.FileNameWithoutId, attachment.CreatedTimestamp, JournalViewModel.EntryType.Attachment));
                }
            }

            foreach (Appointment appointment in customer.Appointments)
            {
                if (appointment.Deleted == null)
                {
                    journal.Add(new JournalViewModel(appointment.Id, appointment.Title, appointment.Start, JournalViewModel.EntryType.Appointment));
                }
            }

            foreach (Message message in customer.Messages)
            {
                if (message.Cancelled == null)
                {
                    string Description = HtmlExtensions.StripTags(message.MessageText);
                    Description = Description.Length > Models.Note.MaxCharsForDescriptionSummary ? Description.Substring(0, Models.Note.MaxCharsForDescriptionSummary) + "..." : Description;
                    journal.Add(new JournalViewModel(message.Id, message.messageMethod.ToString() + ": " + Description, message.AddedToQueue, JournalViewModel.EntryType.Message));
                }
            }

            journal.Sort((c1, c2) => c2.EntryDate.CompareTo(c1.EntryDate));

            return journal;
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(Id);
            ViewBag.Occupations = GetOccupations();
            ViewBag.Benefits = GetBenefits();
            ViewBag.Tags = GetTagsText();
            ViewBag.SelectedTags = GetSelectedTagsText(db, customer);
            ViewBag.MedicalConditions = GetMedicalConditionsText();
            ViewBag.SelectedMedicalConditions = GetSelectedMedicalCondtionsText(db, customer);

            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
            if (practice.ShowDomiciliaryFields)
            {
                ViewBag.CareHomes = GetCareHomes();
                ViewBag.ShelteredAccommodationsList = GetShelteredAccommodationList();
            }

            return View(customer);

        }

        [HttpPost]
        public ActionResult Edit([Bind(Exclude = "Age,NextTestDueDate")]Models.Customer customer, List<string> SelectedTags, List<string> SelectedMedicalConditions,
            string EyeExamRecallTemplateOption, string ContactLensExamRecallTemplateOption)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ValidateCustomer(customer);

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
            if (db.Customers.Where(c => c.practice.Id == practice.Id
                && c.Id != customer.Id
                && c.Number != null && c.Number.Trim().Length > 0 && c.Number == customer.Number).Count() > 0)
            {
                ModelState.AddModelError("Number", "Duplicate patient number");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                Customer existingCustomer = db.Customers.Find(customer.Id);
                Copy(ref customer, ref existingCustomer);

                if (existingCustomer.Id == Guid.Empty)
                {
                    existingCustomer.Id = Guid.NewGuid();
                }
                if (existingCustomer.ExternalId == Guid.Empty)
                {
                    existingCustomer.ExternalId = Guid.NewGuid();
                }

                if (EyeExamRecallTemplateOption != null && EyeExamRecallTemplateOption.Length > 0 && new Guid(EyeExamRecallTemplateOption) != Guid.Empty)
                {
                    existingCustomer.eyeExamRecallTemplate = db.RecallTemplates.Find(new Guid(EyeExamRecallTemplateOption));
                }
                else
                {
                    existingCustomer.eyeExamRecallTemplate = null;
                }

                if (ContactLensExamRecallTemplateOption != null && ContactLensExamRecallTemplateOption.Length > 0 && new Guid(ContactLensExamRecallTemplateOption) != Guid.Empty)
                {
                    existingCustomer.contactLensExamRecallTemplate = db.RecallTemplates.Find(new Guid(ContactLensExamRecallTemplateOption));
                }
                else
                {
                    existingCustomer.contactLensExamRecallTemplate = null;
                }

                UpdateTags(db, existingCustomer, SelectedTags);
                UpdateMedicalConditions(db, existingCustomer, SelectedMedicalConditions);

                existingCustomer.LastUpdated = DateTime.Now;

                db.SaveChanges();

                TempData["Message"] = "Patient saved";
                VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Patient_Edited, string.Concat(customer.CustomerToString, " saved"), customer.Id, true);
                return RedirectToAction("Customer", new { customer.Id });
            }

            return Edit(customer.Id);
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Occupations = GetOccupations();
            ViewBag.Benefits = GetBenefits();
            ViewBag.Tags = GetTagsText();
            ViewBag.MedicalConditions = GetMedicalConditionsText();
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = new Customer();
            customer.EyeExamFrequencyUnit = Enums.FrequencyUnit.Months;
            customer.ContactLensExamFrequencyUnit = Enums.FrequencyUnit.Months;

            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
            customer.practice = practice;
            if (practice.LastPatientNumber > 0)
            {
                customer.Number = (practice.PatientNumberPrefix != null ? practice.PatientNumberPrefix : "") + Convert.ToString(practice.LastPatientNumber + 1);
            }

            if (practice.ShowDomiciliaryFields)
            {
                ViewBag.CareHomes = GetCareHomes();
                ViewBag.ShelteredAccommodationsList = GetShelteredAccommodationList();
            }
            return View(customer);
        }

        [HttpPost]
        public ActionResult Add(Models.Customer customer, List<string> SelectedTags, List<string> SelectedMedicalConditions)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ValidateCustomer(customer);

            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);

            if (db.Customers.Where(c => c.Number != null && c.Number.Trim().Length > 0 && c.practice.Id == practice.Id && c.Number == customer.Number).Count() > 0)
            {
                ModelState.AddModelError("Number", "Duplicate patient number");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                if (customer.Id == Guid.Empty)
                {
                    customer.Id = Guid.NewGuid();
                }

                if (customer.ExternalId == Guid.Empty)
                {
                    customer.ExternalId = Guid.NewGuid();
                }

                customer.LastUpdated = DateTime.Now;
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                customer.CreatedByUser = user;
                customer.practice = practice;
                UpdateTags(db, customer, SelectedTags);
                UpdateMedicalConditions(db, customer, SelectedMedicalConditions);

                db.Customers.Add(customer);
                if (customer.practice.LastPatientNumber > 0)
                {
                    customer.practice.LastPatientNumber += 1;
                }
                db.SaveChanges();
                TempData["Message"] = "Patient saved";
                VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Patient_Added, string.Concat(customer.CustomerToString, " saved"), customer.Id, true);
                return RedirectToAction("Customer", new { customer.Id });
            }

            return Add();
        }

        private void ValidateCustomer(Models.Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Telephone) && string.IsNullOrWhiteSpace(customer.SMSNumber))
            {
                ModelState.AddModelError("Telephone", "Either a telephone or SMS number must be supplied");
                ModelState.AddModelError("SMSNumber", "Either a SMS or telephone number must be supplied");
            }

            if (customer.Address == null || customer.Address.Length == 0)
            {
                ModelState.AddModelError("Address", "Address must be supplied");
            }

            if (customer.Postcode == null || customer.Postcode.Length == 0)
            {
                ModelState.AddModelError("Postcode", "Postcode must be supplied");
            }

            if (customer.DOB == null)
            {
                ModelState.AddModelError("DOB", "DOB required");
            }
        }

        public ActionResult OpenJournalEntry(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new CustomersDataContext())
            {
                Note note = db.Notes.Find(Id);
                if (note != null)
                {
                    return RedirectToAction("Note", "Notes", new { Id });
                }

                EyeExam eyeExam = db.EyeExams.Find(Id);
                if (eyeExam != null)
                {
                    if (eyeExam.appointmentCategory == Enums.AppointmentCategory.Eye_Exam)
                    {
                        return RedirectToAction("EyeExam", "EyeExams", new { Id });
                    }
                    else if (eyeExam.appointmentCategory == Enums.AppointmentCategory.Contact_Lens_Exam)
                    {
                        return RedirectToAction("ContactLensExam", "EyeExams", new { Id });
                    }
                }

                Attachment attachment = db.Attachments.Find(Id);
                if (attachment != null)
                {
                    return RedirectToAction("Edit", "Attachments", new { Id });
                }

                Invoice invoice = db.Invoices.Find(Id);
                if (invoice != null)
                {
                    return RedirectToAction("Invoice", "Invoices", new { Id, ReturnTo = "customer" });
                }

                Appointment appointment = db.Appointments.Find(Id);
                if (appointment != null)
                {
                    HttpContext.Session["customer"] = appointment.customer;
                    return RedirectToAction("Calendar", "Appointments", new { appointment.Start, ReturnTo = "customer" });
                }

                Message message = db.Messages.Find(Id);
                if (message != null)
                {
                    if (message.messageMethod == Enums.MessageMethod.SMS)
                    {
                        return RedirectToAction("ViewSMS", "Messages", new { Id });
                    }
                    else if (message.messageMethod == Enums.MessageMethod.Email)
                    {
                        return RedirectToAction("ViewEmail", "Messages", new { Id });
                    }
                    else if (message.messageMethod == Enums.MessageMethod.Letter)
                    {
                        return RedirectToAction("ViewLetter", "Messages", new { Id });
                    }
                }
            }

            return null;
        }

        public List<Attachment> GetAttachments(Customer customer)
        {
            CustomersDataContext db = new CustomersDataContext();

            var results = from row in db.Attachments
                          select row;

            if (customer != null)
            {
                results = results.Where(c => c.customer.Id == customer.Id);
            }
            else
            {
                results = results.Where(c => false);
            }

            return results.ToList();
        }

        public ActionResult _ReadLastUpdatedCustomers([DataSourceRequest] DataSourceRequest request, string Search)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            var customers = (from c in db.Customers
                             where c.practice.Id == practiceId
                             && c.Deleted == null
                             orderby c.LastUpdated descending
                             select c).Take(10);

            List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
            foreach (Customer customer in customers)
            {
                patientViewModels.Add(new PatientViewModel
                {
                    Id = customer.Id,
                    Name = customer.CustomerToString,
                    Address = customer.FullAddress,
                    Number = customer.DefaultNumber,
                    AgeOrDOB = practice.ShowDOBOnPatientSearch == false ? customer.AgeText : (customer.DOB.HasValue ? customer.DOB.Value.ToShortDateString() : customer.AgeText),
                    Colour = GetColour(customer.Id)
                });
            }

            return Json(patientViewModels.ToDataSourceResult(request));
        }

        public ActionResult _ReadCustomerSearch([DataSourceRequest] DataSourceRequest request, string Search)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            IEnumerable<Customer> results = null;

            results = CustomerSearch(Search, db, practice);

            List<PatientViewModel> patientViewModels = new List<PatientViewModel>();
            foreach (Customer customer in results)
            {
                patientViewModels.Add(new PatientViewModel
                {
                    Id = customer.Id,
                    Name = customer.CustomerToString,
                    Address = customer.FullAddress,
                    Number = customer.DefaultNumber,
                    AgeOrDOB = practice.ShowDOBOnPatientSearch == false ? customer.AgeText : (customer.DOB.HasValue ? customer.DOB.Value.ToShortDateString() : customer.AgeText),
                    Colour = GetColour(customer.Id)
                });
            }

            return Json(patientViewModels.ToDataSourceResult(request));
        }

        public ActionResult Delete(Customer customer)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            ModelState.Clear();
            if (ModelState.IsValid)
            {
                Customer existingCustomer = db.Customers.Find(customer.Id);
                existingCustomer.Deleted = DateTime.Now;

                existingCustomer.DeletedByUser = user;
                if (existingCustomer.Address.Length == 0)
                {
                    existingCustomer.Address = "N/A";
                }
                if (existingCustomer.Postcode.Length == 0)
                {
                    existingCustomer.Postcode = "N/A";
                }
                if ((existingCustomer.Telephone == null || existingCustomer.Telephone == "") && (existingCustomer.SMSNumber == null || existingCustomer.SMSNumber == ""))
                {
                    existingCustomer.Telephone = "N/A";
                }
                var validationResult = db.Entry(existingCustomer).GetValidationResult();

                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                    return RedirectToAction("Edit", "Customers", new { customer.Id });
                }
                string message = string.Format("Patient {0} deleted", existingCustomer.CustomerToString);
                VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Patient_Deleted, message, customer.Id, true);
                db.SaveChanges();

                TempData["Message"] = message;

                return RedirectToAction("Search", "Customers");
            }
            else
            {
                string message = string.Format("Unable to delete patient {0}", customer.CustomerToString);
                VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Patient_Delete_Failed, message, customer.Id, false);
                db.SaveChanges();
                TempData["Message"] = message;
                return View();
            }
        }

        public decimal GetCustomerBalanceIncVAT(Guid CustomerId)
        {
            CustomersDataContext db = new CustomersDataContext();
            decimal result = 0m;

            foreach (Invoice invoice in db.Invoices.Where(model => model.customer.Id == CustomerId && model.Deleted == null))
            {
                result += invoice.BalanceIncVAT;
            }

            return result;
        }

        private List<string> GetOccupations()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> occupations = new List<string>();
            IEnumerable<Customer> existingCustomers = db.Customers.Where(c => c.practice.company.Id == companyId && c.Deleted == null && c.Occupation != null && c.Occupation.Trim().Length > 0);

            foreach (Customer existingCustomer in existingCustomers)
            {
                if (!occupations.Contains(existingCustomer.Occupation))
                {
                    occupations.Add(existingCustomer.Occupation);
                }
            }
            return occupations.OrderBy(m => m.ToString()).ToList();
        }

        private List<string> GetBenefits()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> benefits = new List<string>();
            IEnumerable<Customer> existingCustomers = db.Customers.Where(c => c.practice.company.Id == companyId && c.Deleted == null && c.Benefit != null && c.Benefit.Trim().Length > 0);

            foreach (Customer existingCustomer in existingCustomers)
            {
                if (!benefits.Contains(existingCustomer.Benefit))
                {
                    benefits.Add(existingCustomer.Benefit);
                }
            }
            return benefits.OrderBy(m => m.ToString()).ToList();
        }

        private List<string> GetCareHomes()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> careHomes = new List<string>();
            IEnumerable<Customer> existingCustomers = db.Customers.Where(c => c.practice.company.Id == companyId && c.Deleted == null && c.CareHome != null && c.CareHome.Trim().Length > 0);

            foreach (Customer existingCustomer in existingCustomers)
            {
                if (!careHomes.Contains(existingCustomer.CareHome))
                {
                    careHomes.Add(existingCustomer.CareHome);
                }
            }
            return careHomes.OrderBy(m => m.ToString()).ToList();
        }

        private List<string> GetShelteredAccommodationList()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> shelteredAccommodationList = new List<string>();
            IEnumerable<Customer> existingCustomers = db.Customers.Where(c => c.practice.company.Id == companyId && c.Deleted == null && c.ShelteredAccommodation != null && c.ShelteredAccommodation.Trim().Length > 0);

            foreach (Customer existingCustomer in existingCustomers)
            {
                if (!shelteredAccommodationList.Contains(existingCustomer.ShelteredAccommodation))
                {
                    shelteredAccommodationList.Add(existingCustomer.ShelteredAccommodation);
                }
            }
            return shelteredAccommodationList.OrderBy(m => m.ToString()).ToList();
        }

        private List<Tag> GetTags()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            return db.Tags.Where(t => t.Deleted == null
                && (t.TagTypeEnum == Enums.TagType.Lifestyle || t.TagTypeEnum == Enums.TagType.Care_Home)
                && (t.company.Id == companyId || t.company == null)).OrderBy(t => t.Name).ToList();
        }

        private List<string> GetTagsText()
        {
            List<Tag> tags = GetTags();
            List<string> tagsList = new List<string>();
            foreach (Tag tag in tags)
            {
                if (!tagsList.Contains(tag.Name))
                {
                    tagsList.Add(tag.Name);
                }
            }

            return tagsList;
        }


        private List<Tag> GetMedicalConditions()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            return db.Tags.Where(t => t.Deleted == null
                && t.TagTypeEnum == Enums.TagType.Medical_Condition
                && (t.company.Id == companyId || t.company == null)).OrderBy(t => t.Name).ToList();
        }

        private List<string> GetMedicalConditionsText()
        {
            List<Tag> tags = GetMedicalConditions();
            List<string> tagsList = new List<string>();
            foreach (Tag tag in tags)
            {
                if (!tagsList.Contains(tag.Name))
                {
                    tagsList.Add(tag.Name);
                }
            }

            return tagsList;
        }

        private List<CustomerTag> GetSelectedTags(CustomersDataContext db, Customer customer)
        {
            return db.CustomerTags.Where(t =>
                t.customer.Id == customer.Id
                && t.Deleted == null
                && (t.tag.TagTypeEnum == Enums.TagType.Lifestyle || t.tag.TagTypeEnum == Enums.TagType.Care_Home)
                ).OrderBy(t => t.Added).ToList();
        }

        private List<string> GetSelectedTagsText(CustomersDataContext db, Customer customer)
        {
            List<CustomerTag> customerTags = GetSelectedTags(db, customer);
            List<string> tagsList = new List<string>();

            foreach (CustomerTag customerTag in customerTags)
            {
                if (!tagsList.Contains(customerTag.tag.Name))
                {
                    tagsList.Add(customerTag.tag.Name);
                }
            }

            return tagsList;
        }

        private List<CustomerTag> GetSelectedMedicalCondtions(CustomersDataContext db, Customer customer)
        {
            return db.CustomerTags.Where(t =>
                t.customer.Id == customer.Id
                && t.Deleted == null
                && t.tag.TagTypeEnum == Enums.TagType.Medical_Condition
                ).OrderBy(t => t.Added).ToList();
        }

        private List<string> GetSelectedMedicalCondtionsText(CustomersDataContext db, Customer customer)
        {
            List<CustomerTag> customerTags = GetSelectedMedicalCondtions(db, customer);
            List<string> tagsList = new List<string>();

            foreach (CustomerTag customerTag in customerTags)
            {
                if (!tagsList.Contains(customerTag.tag.Name))
                {
                    tagsList.Add(customerTag.tag.Name);
                }
            }

            return tagsList;
        }

        private void UpdateTags(CustomersDataContext db, Customer customer, List<string> SelectedTags)
        {
            if (SelectedTags != null)
            {
                Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;

                //remove old tags
                foreach (CustomerTag customerTag in GetSelectedTags(db, customer))
                {
                    if (SelectedTags.Contains(customerTag.tag.Name) == false)
                    {
                        customerTag.Deleted = DateTime.Now;
                    }
                }

                //add new tags
                foreach (string tagText in SelectedTags)
                {
                    CustomerTag customerTag = db.CustomerTags.Where(t => t.customer.Id == customer.Id && t.tag.Name == tagText).FirstOrDefault();
                    if (customerTag == null)
                    {
                        Tag tag = db.Tags.Where(t => t.Name == tagText
                            && t.Deleted == null
                            && (t.company.Id == companyId || t.company == null)).FirstOrDefault();
                        customerTag = new CustomerTag() { Id = Guid.NewGuid(), customer = customer, tag = tag, Added = DateTime.Now };
                        db.CustomerTags.Add(customerTag);
                    }
                    else if (customerTag.Deleted != null)
                    {
                        customerTag.Deleted = null;
                    }
                }
            }
            else
            {
                //remove all tags
                foreach (CustomerTag customerTag in GetSelectedTags(db, customer))
                {
                    customerTag.Deleted = DateTime.Now;
                }
            }
        }

        private void UpdateMedicalConditions(CustomersDataContext db, Customer customer, List<string> SelectedMedicalConditions)
        {
            if (SelectedMedicalConditions != null)
            {
                Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;

                //remove old tags
                foreach (CustomerTag customerTag in GetSelectedMedicalCondtions(db, customer))
                {
                    if (SelectedMedicalConditions.Contains(customerTag.tag.Name) == false)
                    {
                        customerTag.Deleted = DateTime.Now;
                    }
                }

                //add new tags
                foreach (string tagText in SelectedMedicalConditions)
                {
                    CustomerTag customerTag = db.CustomerTags.Where(t => t.customer.Id == customer.Id && t.tag.Name == tagText).FirstOrDefault();
                    if (customerTag == null)
                    {
                        Tag tag = db.Tags.Where(t => t.Name == tagText
                            && t.Deleted == null
                            && (t.company.Id == companyId || t.company == null)).FirstOrDefault();
                        customerTag = new CustomerTag() { Id = Guid.NewGuid(), customer = customer, tag = tag, Added = DateTime.Now };
                        db.CustomerTags.Add(customerTag);
                    }
                    else if (customerTag.Deleted != null)
                    {
                        customerTag.Deleted = null;
                    }
                }
            }
            else
            {
                //remove all tags
                foreach (CustomerTag customerTag in GetSelectedMedicalCondtions(db, customer))
                {
                    customerTag.Deleted = DateTime.Now;
                }
            }
        }

        public ActionResult GOSFormExport(Guid Id, string GOSForm)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, GOSForm),
                new { CustomerId = Id });
        }

        public JsonResult GetCustomers(string Search)
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<Customer> customers = CustomerSearch(Search, db, practice).ToList();
            List<PatientViewModel> patients = new List<PatientViewModel>();

            //todo: implement add new patient option:
            patients.Add(new PatientViewModel { Id = Guid.Empty, Name = "Add new patient" });

            foreach (Customer customer in customers)
            {
                patients.Add(new PatientViewModel { Id = customer.Id, Name = customer.CustomerToString });
            }

            return Json(patients, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAddresses(string Search)
        {
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<AddressLookupSummary> addresses = new List<AddressLookupSummary>();
            if (Search.Length != 0)
            {
                PostcodeAnywhere postcodeAnywhere = new PostcodeAnywhere();
                addresses = postcodeAnywhere.GetAddresses(Search);
            }

            return Json(addresses, JsonRequestBehavior.AllowGet);
        }

        private string GetColour(Guid CustomerId)
        {
            CustomersDataContext db = new CustomersDataContext();
            if (db.CustomerTags.Where(t => t.customer.Id == CustomerId && t.Deleted == null && t.tag.TagTypeEnum == Enums.TagType.Medical_Condition).Count() > 0)
            {
                return "#FA8072"; //HTML colour Salmon
            }
            else
            {
                return "white";
            }
        }

        private static void Copy(ref Customer From, ref Customer To)
        {
            To.Number = From.Number;
            To.Title = From.Title;
            To.Firstnames = From.Firstnames;
            To.Surname = From.Surname;
            To.PreviousSurname = From.PreviousSurname;
            To.DOB = From.DOB;
            To.Address = From.Address;
            To.Postcode = From.Postcode;
            To.Telephone = From.Telephone;
            To.SMSNumber = From.SMSNumber;
            To.Email = From.Email;
            To.TestFrequencyId = From.TestFrequencyId;
            To.Comments = From.Comments;
            To.LastOptician = From.LastOptician;
            To.LastOpticianContact = From.LastOpticianContact;
            To.Occupation = From.Occupation;
            To.CLFitDrName = From.CLFitDrName;
            To.CLFitDrAddress = From.CLFitDrAddress;
            To.CLFitOccupation = From.CLFitOccupation;
            To.CLFitHobbiesSports = From.CLFitHobbiesSports;
            To.CLFitExistingWearer = From.CLFitExistingWearer;
            To.CLFitPreviousWearingDetails = From.CLFitPreviousWearingDetails;
            To.CLFitType = From.CLFitType;
            To.CLFitWearingTime = From.CLFitWearingTime;
            To.CLFitSolutionsUsed = From.CLFitSolutionsUsed;
            To.CLFitCurrentPreviousProblems = From.CLFitCurrentPreviousProblems;
            To.CLFitTrialComments = From.CLFitTrialComments;
            To.CLFitTrialOptometrist = From.CLFitTrialOptometrist;
            To.CLFitWearingSchedule = From.CLFitWearingSchedule;
            To.CLFitDOHFormCompleted = From.CLFitDOHFormCompleted;
            To.CLFitCollectionLensesIn = From.CLFitCollectionLensesIn;
            To.CLFitCollectionWearingTime = From.CLFitCollectionWearingTime;
            To.CLFitCollectionAdvice = From.CLFitCollectionAdvice;
            To.CLFitCollectionNextAppointment = From.CLFitCollectionNextAppointment;
            To.CLFitCollectionOptometrist = From.CLFitCollectionOptometrist;
            To.CLFitCleaningRegime = From.CLFitCleaningRegime;
            To.NINo = From.NINo;
            To.NHSNo = From.NHSNo;
            To.ParentTitle = From.ParentTitle;
            To.ParentName = From.ParentName;
            To.ParentAddress = From.ParentAddress;
            To.ParentPostCode = From.ParentPostCode;
            To.ParentSurname = From.ParentSurname;
            To.RFV = From.RFV;
            To.GH = From.GH;
            To.MEDS = From.MEDS;
            To.POH = From.POH;
            To.FH = From.FH;
            To.NHSPrivate = From.NHSPrivate;
            To.NHSReason = From.NHSReason;
            To.NHSVoucher = From.NHSVoucher;
            To.GPpracticename = From.GPpracticename;
            To.GPpracticepostcode = From.GPpracticepostcode;
            To.GPpracticephone = From.GPpracticephone;
            To.GPpracticefax = From.GPpracticefax;
            To.GPpracticeemail = From.GPpracticeemail;
            To.ExternalId = From.ExternalId;
            To.LastUpdated = From.LastUpdated;
            To.CreatedByUser = From.CreatedByUser;
            To.EyeExamFrequencyValue = From.EyeExamFrequencyValue;
            To.EyeExamFrequencyUnit = From.EyeExamFrequencyUnit;
            To.ContactLensExamFrequencyValue = From.ContactLensExamFrequencyValue;
            To.ContactLensExamFrequencyUnit = From.ContactLensExamFrequencyUnit;
            To.NextDueDateEyeExam = From.NextDueDateEyeExam;
            To.NextDueDateContactLensExam = From.NextDueDateContactLensExam;
            To.EyeExamRecallSent = From.EyeExamRecallSent;
            To.ContactLensExamRecallSent = From.ContactLensExamRecallSent;
            To.PreviousEyeExamDate = From.PreviousEyeExamDate;
            To.GPpracticeaddress = From.GPpracticeaddress;
            To.Doctor = From.Doctor;
            To.DoctorContact = From.DoctorContact;
            To.DefaultMessageMethod = From.DefaultMessageMethod;
            To.Deleted = From.Deleted;
            To.DeletedByUser = From.DeletedByUser;
            To.SchoolCollegeUniversity = From.SchoolCollegeUniversity;
            To.SchoolCollegeUniversityAddress = From.SchoolCollegeUniversityAddress;
            To.SchoolCollegeUniversityPostcode = From.SchoolCollegeUniversityPostcode;
            To.RecallCount = From.RecallCount;
            To.SymptomsAndHistory = From.SymptomsAndHistory;
            To.Allergies = From.Allergies;
            To.PreviousContactLensExamDate = From.PreviousContactLensExamDate;
            To.Benefit = From.Benefit;
            To.CareHome = From.CareHome;
            To.ShelteredAccommodation = From.ShelteredAccommodation;
            To.EmailReminders = From.EmailReminders;
            To.SMSReminders = From.SMSReminders;
            To.LetterReminders = From.LetterReminders;
            To.TelephoneReminders = From.TelephoneReminders;
            To.eyeExamRecallTemplate = From.eyeExamRecallTemplate;
            To.contactLensExamRecallTemplate = From.contactLensExamRecallTemplate;
        }
    }
}