using Kendo.Mvc.UI;
using MvcReportViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;

namespace VisionDB.Controllers
{
    [Authorize]
    public class SettingsController : VisionDBController
    {
        public ActionResult Index()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                CustomersDataContext db = new CustomersDataContext();
                Setting setting = db.Settings.Find(1);
                if (setting != null)
                {
                    return View(setting);
                }

                int settingsCount = db.Settings.Count();
                if (settingsCount > 1)
                {
                    TempData["Error"] = "Too many settings. Check the Settings table. There should only be 1 row.";
                    RedirectToAction("Index", "Admin");
                }
                else if (settingsCount == 0)
                {
                    TempData["Error"] = "No settings. Check the Settings table. There should be 1 row.";
                    RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please use the Setup section or contact Click Software.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Edit()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                CustomersDataContext db = new CustomersDataContext();
                Setting setting = db.Settings.Find(1);
                if (setting != null)
                {
                    return View(setting);
                }

                int settingsCount = db.Settings.Count();
                if (settingsCount > 1)
                {
                    TempData["Error"] = "Too many settings. Check the Settings table. There should only be 1 row.";
                    RedirectToAction("Index", "Admin");
                }
                else if (settingsCount == 0)
                {
                    TempData["Error"] = "No settings. Check the Settings table. There should be 1 row.";
                    RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please use the Setup section or contact Click Software.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Setting setting)
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                if (ModelState.IsValid)
                {
                    CustomersDataContext db = new CustomersDataContext();

                    Setting existingSetting = db.Settings.Find(setting.Id);
                    existingSetting.DailyDatabaseBackupTime = setting.DailyDatabaseBackupTime;
                    existingSetting.DailyBackupLastTaken = setting.DailyBackupLastTaken;
                    existingSetting.SMSStartTime = setting.SMSStartTime;
                    existingSetting.SMSEndTime = setting.SMSEndTime;

                    db.SaveChanges();

                    TempData["Message"] = "Settings saved";
                    return RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please use the Setup section or contact Click Software.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}