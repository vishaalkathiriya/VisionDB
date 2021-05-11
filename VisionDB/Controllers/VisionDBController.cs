using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    public class VisionDBController : Controller
    {
        public new HttpContextBase HttpContext
        {
            get
            {
                HttpContextWrapper context = new HttpContextWrapper(System.Web.HttpContext.Current);
                return (HttpContextBase)context;
            }
        }
        public static void AddAuditLogEntry(ApplicationUser user, Enums.AuditLogEntryType auditLogEntryType, string Message, Guid? EntryId, bool Visible)
        {
            CustomersDataContext db = new CustomersDataContext();
            db.AuditLog.Add(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                UserName = user.UserName,
                EntryType = auditLogEntryType.ToString(),
                EntryText = Message,
                Added = DateTime.Now,
                EntryId = EntryId,
                Visible = Visible
            });

            db.SaveChanges();
        }

        public static void AddAuditLogEntry(string UserName, Enums.AuditLogEntryType auditLogEntryType, string Message, Guid? EntryId, bool Visible)
        {
            CustomersDataContext db = new CustomersDataContext();
            db.AuditLog.Add(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                UserName = UserName != null && UserName.Trim().Length > 0 ? UserName : "System",
                EntryType = auditLogEntryType.ToString(),
                EntryText = Message,
                Added = DateTime.Now,
                EntryId = EntryId,
                Visible = Visible
            });

            db.SaveChanges();       
        }
    }
}