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
    public class AppointmentTypesController : VisionDBController
    {
        public ActionResult Index()
        {
            return View();
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
        public ActionResult Add(AppointmentType appointmentType)
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

                appointmentType.Id = Guid.NewGuid();
                appointmentType.practice = practice;
                appointmentType.Added = DateTime.Now;

                db.AppointmentTypes.Add(appointmentType);

                db.SaveChanges();

                TempData["Message"] = "Appointment type saved";
                return RedirectToAction("Index");
            }

            return Add();
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            AppointmentType appointmentType = db.AppointmentTypes.Find(Id);
            if (appointmentType == null)
            {
                return HttpNotFound();
            }
            return View(appointmentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentType appointmentType)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                AppointmentType existingAppointmentType = db.AppointmentTypes.Find(appointmentType.Id);

                existingAppointmentType.Name = appointmentType.Name;
                existingAppointmentType.DefaultAppointmentLength = appointmentType.DefaultAppointmentLength;
                existingAppointmentType.AppointmentCategoryEnum = appointmentType.AppointmentCategoryEnum;

                db.SaveChanges();

                TempData["Message"] = "Appointment type saved";
                return RedirectToAction("Index");
            }
            return View(appointmentType);
        }

        public ActionResult Delete(AppointmentType appointmentType)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            AppointmentType existingAppointmentType = db.AppointmentTypes.Find(appointmentType.Id);
            existingAppointmentType.Deleted = DateTime.Now;
            db.SaveChanges();

            TempData["Message"] = "Appointment type deleted";
            return RedirectToAction("Index");
        }

        public JsonResult GetAppointmentTypes()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            List<AppointmentType> appointmentTypes = db.AppointmentTypes.Where(a => a.practice.Id == practiceId && a.Deleted == null).OrderBy(a => a.Name).ToList();
            List<AppointmentTypeViewModel> appointmentTypeViewModels = new List<AppointmentTypeViewModel>();
            appointmentTypeViewModels.Add(new AppointmentTypeViewModel { Id = Guid.Empty, Name = "All" });

            foreach (AppointmentType appointmentType in appointmentTypes)
            {
                appointmentTypeViewModels.Add(new AppointmentTypeViewModel
                {
                    Id = appointmentType.Id,
                    Name = appointmentType.Name,
                    AppointmentCategoryEnum = appointmentType.AppointmentCategoryEnum,
                    DefaultAppointmentLength = appointmentType.DefaultAppointmentLength
                });
            }

            return Json(appointmentTypeViewModels, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _Read([DataSourceRequest] DataSourceRequest request)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            List<AppointmentType> appointmentTypes = db.AppointmentTypes.Where(a => a.practice.Id == practiceId && a.Deleted == null).OrderBy(a => a.Name).ToList();
            List<AppointmentTypeViewModel> appointmentTypeViewModels = new List<AppointmentTypeViewModel>();

            foreach (AppointmentType appointmentType in appointmentTypes)
            {
                appointmentTypeViewModels.Add(new AppointmentTypeViewModel
                {
                    Id = appointmentType.Id,
                    Name = appointmentType.Name,
                    AppointmentCategoryEnum = appointmentType.AppointmentCategoryEnum,
                    DefaultAppointmentLength = appointmentType.DefaultAppointmentLength
                });
            }

            return Json(appointmentTypeViewModels.ToDataSourceResult(request));
        }
    }
}