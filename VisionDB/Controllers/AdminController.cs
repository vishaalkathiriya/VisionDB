using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;
using System.Data.Entity;
using System.ServiceModel.Syndication;
using VisionDB.Helper;
using System.Web.Configuration;
using System.Data.SqlClient;

namespace VisionDB.Controllers
{
    [Authorize]
    public class AdminController : VisionDBController
    {
        public ActionResult Index()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                return View();
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please use the Setup section or contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult AuditLog()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                return View();
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult CompaniesAndPractices()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                return View();
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult SummaryReport()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                return View();
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult BackupDatabase()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                CustomersDataContext db = new CustomersDataContext();
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "exec sp_VDB_Backup_Database");
                AddAuditLogEntry("clark", Enums.AuditLogEntryType.Database_Backup, "Backup successful", null, false);
                Setting setting = db.Settings.Find(1);
                setting.DailyBackupLastTaken = DateTime.Now;
                db.SaveChanges();
                TempData["Message"] = "Backup successful";
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        public ActionResult AuditLogRss(string Id)
        {
            if (Id != null)
            {
                CustomersDataContext db = new CustomersDataContext();

                ApplicationUser user = db.ApplicationUsers.Where(u => u.SupportCode.Contains(Id)).FirstOrDefault();

                if (user != null && user.UserName == "clark")
                {
                    //source: http://damieng.com/blog/2010/04/26/creating-rss-feeds-in-asp-net-mvc

                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

                    List<SyndicationItem> postItems = new List<SyndicationItem>();

                    foreach (AuditLogEntry item in db.AuditLog.Where(p => 
                        p.UserName.Length > 1 
                        && p.UserName != "clark"
                        && p.EntryText != "0 recalls processed"
                        && p.EntryText != "0 messages processed"
                        && (
                        p.EntryType == Enums.AuditLogEntryType.Database_Backup.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Login.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Login_Failed.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Contact_Lens_Exam_Deleted.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Eye_Exam_Deleted.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Invoice_Deleted.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Patient_Delete_Failed.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Patient_Deleted.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Recall_Error.ToString()
                        || p.EntryType == Enums.AuditLogEntryType.Log_Off.ToString()
                        )
                        ).OrderByDescending(p => p.Added).Take(100))
                    {
                        postItems.Add(new SyndicationItem(item.EntryType, item.Added.ToString() + " " + item.UserName, item.Address, item.Id.ToString(), new DateTimeOffset(item.Added)));
                    }

                    var feed = new SyndicationFeed(builder.InitialCatalog, "Audit Log", new Uri("/Home", UriKind.Relative), postItems)
                    {
                        Language = "en-GB"
                    };

                    return new FeedResult(new Rss20FeedFormatter(feed));
                }
            }

            TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PopulateLastEyeExamsForInvoices()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                InvoicesController invoicesController = new InvoicesController();
                invoicesController.PopulateLastEyeExamsForInvoices();
                TempData["Message"] = "Populated last eye exam for all invoices";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult UpdateInvoiceTotals()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                InvoicesController invoicesController = new InvoicesController();
                invoicesController.UpdateTotalsForInvoices();
                TempData["Message"] = "Updated totals for all invoices";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult TestPostcodeLookup()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                PostcodeAnywhere postcodeAnywhere = new PostcodeAnywhere();
                int count = postcodeAnywhere.GetAddresses("B249FE").Count;
                if (count > 0)
                {
                    TempData["Message"] = "Postcode lookup working";
                }
                else 
                {
                    TempData["Message"] = "Error finding postcode";
                }

                return RedirectToAction("Index");
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Assemblies()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                var Assemblies = new List<string>();
                var AssemblyList = "";
                foreach (var item in loadedAssemblies)
                {
                    Assemblies.Add(item.ToString());
                    AssemblyList += item.ToString() + Environment.NewLine;
                }

                ViewBag.AssemblyList = AssemblyList;

                return View(Assemblies);
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}