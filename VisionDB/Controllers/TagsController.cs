using Kendo.Mvc.UI;
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
    public class TagsController : VisionDBController
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


        public ActionResult _Read([DataSourceRequest] DataSourceRequest request)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            Company company = practice.company;

            List<Tag> tags = db.Tags.Where(t =>
                t.company.Id == company.Id
                && t.Deleted == null).OrderBy(t => t.Name).ToList();

            return Json(tags.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Tag tag)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Clear();

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                tag.Id = Guid.NewGuid();
                tag.company = practice.company;

                db.Tags.Add(tag);

                db.SaveChanges();

                TempData["Message"] = "Tag saved";
                return RedirectToAction("Index");
            }

            return Add();
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tag tag)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                db.Entry(tag).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                TempData["Message"] = "Tag saved";
                return RedirectToAction("Index");
            }
            return View(tag);
        }

        public ActionResult Delete(Tag tag)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Tag existingTag = db.Tags.Find(tag.Id);
            existingTag.Deleted = DateTime.Now;
            db.SaveChanges();

            TempData["Message"] = "Tag deleted";
            return RedirectToAction("Index");
        }
    }
}