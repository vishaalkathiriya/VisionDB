using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    [Authorize]
    public class FinanceController : VisionDBController
    {
        public ActionResult Index()
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
            return View();
        }
    }
}