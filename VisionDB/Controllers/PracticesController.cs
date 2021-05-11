using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    [Authorize]
    public class PracticesController : VisionDBController
    {
        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(Id);
            ViewBag.OpticiansViewModels = new AccountController().GetOpticianViewModels(Id);
            return View(practice);
        }

        [HttpPost]
        public ActionResult Edit(Practice practice, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
                Practice existingPractice = db.Practices.Find(practiceId);

                existingPractice.Name = practice.Name;
                existingPractice.Address = practice.Address;
                existingPractice.Postcode = practice.Postcode;
                existingPractice.Tel = practice.Tel;
                existingPractice.Fax = practice.Fax;
                existingPractice.Email = practice.Email;
                existingPractice.SchedulerMajorTick = practice.SchedulerMajorTick;
                existingPractice.SchedulerMinorTickCount = practice.SchedulerMinorTickCount;
                existingPractice.WorkDayStart = practice.WorkDayStart;
                existingPractice.WorkDayEnd = practice.WorkDayEnd;
                existingPractice.PrimaryCareTrustGOS = practice.PrimaryCareTrustGOS;
                ApplicationUser defaultOptician = db.ApplicationUsers.Find(SelectedOptician);
                if (defaultOptician != null)
                {
                    existingPractice.DefaultOptician = defaultOptician; 
                }
                existingPractice.SMSSenderName = practice.SMSSenderName;
                existingPractice.ContractorName = practice.ContractorName;
                existingPractice.ContractorNumber = practice.ContractorNumber;
                existingPractice.ShowDOBOnPatientSearch = practice.ShowDOBOnPatientSearch;
                existingPractice.TelAreaPrefix = practice.TelAreaPrefix;
                existingPractice.EyeExamScreenDesign = practice.EyeExamScreenDesign;
                existingPractice.DefaultEyeExamTimeToPatientsAppointment = practice.DefaultEyeExamTimeToPatientsAppointment;
                existingPractice.ShowGOSForms = practice.ShowGOSForms;
                existingPractice.ShowDomiciliaryFields = practice.ShowDomiciliaryFields;
                existingPractice.ShowCallButtons = practice.ShowCallButtons;
                existingPractice.ShowPracticeNotesOnDashboard = practice.ShowPracticeNotesOnDashboard;
                existingPractice.EditPatientFromCalendar = practice.EditPatientFromCalendar;
                existingPractice.RecallDateCutOff = practice.RecallDateCutOff;
                db.SaveChanges();
                TempData["Message"] = "Practice settings saved"; 
                return RedirectToAction("Index", "Setup");
            }
            else
            {
                //todo: show error
                return View();
            }
        }

        public decimal GetPracticeBalanceExcVAT(Guid PracticeId)
        {
            CustomersDataContext db = new CustomersDataContext();

            if (db.Invoices.Where(i => i.practice.Id == PracticeId && i.Deleted == null).Count() > 0)
            {
                return db.Invoices.Where(i =>
                    i.practice.Id == PracticeId
                    && i.Deleted == null).Sum(i => i.BalanceExcVAT);
            }
            else
            {
                return 0;
            }
        }

        public decimal GetPracticeTodaysTakingsExcVAT(Guid PracticeId)
        {
            CustomersDataContext db = new CustomersDataContext();

            try
            {
                return db.InvoiceDetails.Where(id =>
                id.invoice.practice.Id == PracticeId
                && id.Deleted == null
                && id.invoice.Deleted == null
                && id.UnitPrice < 0).ToList().Where(id =>
                    id.Added > DateTime.Now.Date).Sum(id => id.TotalExcVAT);
            }
            catch (InvalidOperationException)
            {
                //nothing returned from query causes exception
                return 0;
            }
        }
	}
}