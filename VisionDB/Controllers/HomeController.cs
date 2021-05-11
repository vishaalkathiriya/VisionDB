using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using System.Data.Objects;
using VisionDB.Helper;

namespace VisionDB.Controllers
{
    public class HomeController : VisionDBController
    {
        [Authorize]
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Enums.DefaultHomePage defaultHomePage = ((VisionDB.Models.ApplicationUser)Session["user"]).DefaultHomePageEnum;

            switch (defaultHomePage)
            {
                case Enums.DefaultHomePage.Dashboard:
                    return RedirectToAction("Dashboard");
                case Enums.DefaultHomePage.Launcher:
                    return RedirectToAction("Launcher");
                case Enums.DefaultHomePage.Patients:
                    return RedirectToAction("Search", "Customers");
                case Enums.DefaultHomePage.Calendar:
                    return RedirectToAction("Calendar", "Appointments");
                case Enums.DefaultHomePage.Reports:
                    return RedirectToAction("Practice", "Reports");
            }

            return RedirectToAction("Dashboard");
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Practice practice = db.Practices.Find(user.practiceId);
            ViewBag.Practice = practice;
            if (db.Appointments.Where(a => a.practice.Id == practice.Id && a.Deleted == null && a.Start > DateTime.Now).Count() > 0)
            {
                Appointment appointment = db.Appointments.Where(a => a.practice.Id == practice.Id && a.Deleted == null && a.Start > DateTime.Now).OrderBy(a => a.Start).First();
                ViewBag.NextAppointment = (appointment.customer != null ? appointment.customer.ToString() : appointment.Title) + " ";
                if (appointment.Start.Date == DateTime.Now.Date)
                {
                    ViewBag.NextAppointment += appointment.Start.ToShortTimeString() + " Today";
                }
                else
                {
                    ViewBag.NextAppointment += appointment.Start.ToShortDateString() + " " + appointment.Start.ToShortTimeString();
                }
            }

            LoadTodaysAppointmentChart(db, practice);

            LoadThisWeeksAppointmentChart(db, practice);

            ViewBag.company = practice.company;
            return View();
        }

        [Authorize]
        public ActionResult Launcher()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        private void LoadThisWeeksAppointmentChart(CustomersDataContext db, Practice practice)
        {
            DateTime WeekStartDate = DateHelper.GetFirstDateForWeek();
            DateTime WeekEndDate = WeekStartDate.AddDays(7);

            ViewBag.WeeksPendingAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null
                && a.Deleted == null).ToList().Where(a => 
                    a.Start > DateTime.Now
                    && a.Start < WeekEndDate).Count();

            ViewBag.WeeksAttendedAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null
                && a.Deleted == null).ToList().Where(a =>
                    a.Start > WeekStartDate
                    && a.Start < WeekEndDate
                    && (a.customer.PreviousEyeExamDate > a.Start || a.customer.PreviousContactLensExamDate > a.Start)
                    ).Count();

            int WeeksAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null
                && a.Deleted == null).ToList().Where(a =>
                    a.Start > WeekStartDate
                    && a.Start < WeekEndDate).Count();

            int WeeklyMissedAppointmentCount = WeeksAppointmentCount - (int)ViewBag.WeeksPendingAppointmentCount - (int)ViewBag.WeeksAttendedAppointmentCount;

            if (WeeklyMissedAppointmentCount < 0)
            {
                WeeklyMissedAppointmentCount = 0; //todo: this should not happen and should be logged when it does
            }

            ViewBag.WeeksMissedAppointmentCount = WeeklyMissedAppointmentCount;
        }

        private void LoadTodaysAppointmentChart(CustomersDataContext db, Practice practice)
        {
            DateTime TodaysDate = DateTime.Now;

            ViewBag.TodaysPendingAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null 
                && a.Deleted == null
                && a.Start.Year == DateTime.Now.Year
                && a.Start.Month == DateTime.Now.Month
                && a.Start.Day == DateTime.Now.Day
                && a.Start > DateTime.Now).Count();

            ViewBag.TodaysAttendedAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null 
                && a.Deleted == null
                && a.Start.Year == DateTime.Now.Year
                && a.Start.Month == DateTime.Now.Month
                && a.Start.Day == DateTime.Now.Day
                && a.Start < DateTime.Now
                && (a.customer.PreviousEyeExamDate > a.Start || a.customer.PreviousContactLensExamDate > a.Start)
                ).Count();

            int TodaysAppointmentCount = db.Appointments.Where(a =>
                a.practice.Id == practice.Id
                && a.customer != null 
                && a.Deleted == null
                && a.Start.Year == DateTime.Now.Year
                && a.Start.Month == DateTime.Now.Month
                && a.Start.Day == DateTime.Now.Day).Count();

            ViewBag.TodaysMissedAppointmentCount = TodaysAppointmentCount - (int)ViewBag.TodaysPendingAppointmentCount - (int)ViewBag.TodaysAttendedAppointmentCount;
        }

        public ActionResult Support()
        {
            return View();
        }
    }
}