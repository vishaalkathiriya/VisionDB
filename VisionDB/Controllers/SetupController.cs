using MvcReportViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    [Authorize]
    public class SetupController : VisionDBController
    {
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            ViewBag.Practice = practice;
            ViewBag.Company = practice.company;
            return View();
        }

        [AllowAnonymous]
        public ActionResult CalibrationPage()
        {
            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath("CalibrationPage"));
        }
    }
}