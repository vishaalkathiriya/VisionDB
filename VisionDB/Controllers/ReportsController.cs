using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using MvcReportViewer;

namespace VisionDB.Controllers
{
    [Authorize]
    public class ReportsController : VisionDBController
    {
        public ActionResult Practice()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            ViewBag.Practice = db.Practices.Find(practiceId);
            PracticesController practicesController = new PracticesController();
            ViewBag.PracticeBalanceExcVAT = practicesController.GetPracticeBalanceExcVAT(practiceId);
            ViewBag.PracticeTodaysTakingsExcVAT = practicesController.GetPracticeTodaysTakingsExcVAT(practiceId);
            List<VisionDB.Models.Report> reports = db.Reports.Where(r => r.practice.Id == practiceId && r.IsCustomReport).OrderBy(r => r.Name).ToList();
            return View(reports);
        }

        public string GetReportPath(Guid practiceId, string ReportName)
        {
            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(practiceId);
            string path = System.Web.Configuration.WebConfigurationManager.AppSettings["SSRSEnvironment"];

            Report report = db.Reports.Where(r =>
                r.Name == ReportName
                && (r.practice == null || (r.practice != null && r.practice.Id == practice.Id))).OrderByDescending(r => r.practice.Id).FirstOrDefault();

            if (report != null)
            {
                path += string.Concat(System.Web.Configuration.WebConfigurationManager.AppSettings["SSRSPracticeReportPathPrefix"], @"/", practice.PracticeRef, @"/");
            }

            path += ReportName;

            return path;
        }

        public string GetReportPath(string ReportName)
        {
            CustomersDataContext db = new CustomersDataContext();
            string path = System.Web.Configuration.WebConfigurationManager.AppSettings["SSRSEnvironment"];

            path += ReportName;

            return path;
        }

        public ActionResult ExportToPDF(string ReportName)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, ReportName),
                new { practiceId = practiceId });
        }

        public ActionResult ExportToExcel(string ReportName)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            var ReportFile = this.Report(
                ReportFormat.Excel,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, ReportName),
                new { practiceId = practiceId });

            ReportFile.FileDownloadName = ReportName + ".xls";

            return ReportFile;
        }
	}
}