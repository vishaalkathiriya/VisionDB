using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Globalization;
using VisionDB.Helper;

namespace VisionDB.Controllers
{
    [Authorize]
    public class AppointmentsController : VisionDBController
    {
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public List<Appointment> GetAppointments(Customer customer)
        {
            CustomersDataContext db = new CustomersDataContext();

            var results = from row in db.Appointments
                          select row;

            if (customer != null)
            {
                results = results.Where(c => c.customer.Id == customer.Id);
            }
            else
            {
                results = results.Where(c => false);
            }

            return results.ToList();
        }

        public ActionResult Appointment(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            var appointment = db.Appointments.Find(Id);

            return View(appointment);
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            var appointment = db.EyeExams.Find(Id);

            return View(appointment);
        }

        [HttpPost]
        public ActionResult Edit(Appointment appointment)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified; //if not working, load existingAppointment object and copy properties to it; then save existingAppointment object
                appointment.customer.LastUpdated = DateTime.Now;
                db.SaveChanges();

                return RedirectToAction("Appointment", new { appointment.Id });
            }

            return Edit(appointment.Id);
        }

        public ActionResult Calendar(DateTime? Start)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            if (Start.HasValue)
            {
                ViewBag.Start = Start.Value.Date;
            }
            ViewBag.Practice = practice;
            HttpContext.Session["customer"] = null;
            ViewBag.Customer = null;
            ViewBag.PreventDraggingAppointments = ((ApplicationUser)HttpContext.Session["user"]).PreventDraggingAppointments;
            ViewBag.AutomaticallyResizeCalendar = ((ApplicationUser)HttpContext.Session["user"]).AutomaticallyResizeCalendar;
            return View();
        }

        public ActionResult Add(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            Customer customer = db.Customers.Find(Id);
            var validationResult = db.Entry(customer).GetValidationResult();

            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                return RedirectToAction("Edit", "Customers", new { customer.Id });
            }
            ViewBag.Customer = customer;
            HttpContext.Session["customer"] = customer;
            ViewBag.Practice = practice;
            ViewBag.PreventDraggingAppointments = ((ApplicationUser)HttpContext.Session["user"]).PreventDraggingAppointments;
            ViewBag.AutomaticallyResizeCalendar = ((ApplicationUser)HttpContext.Session["user"]).AutomaticallyResizeCalendar;
            return View();
        }

        public virtual JsonResult ReadSchedule([DataSourceRequest] DataSourceRequest request)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            using (CustomersDataContext db = new CustomersDataContext())
            {
                DateTime startCutOff = DateTime.Now.AddMonths(-2);
                List<Appointment> appointments = db.Appointments.Where(a => a.Deleted == null && a.practice.Id == practiceId && a.Start > startCutOff).ToList();
                List<Schedule> schedules = new List<Schedule>();

                foreach (Appointment appointment in appointments)
                {
                    schedules.Add(new Schedule
                    {
                        Id = appointment.Id,
                        CustomerId = appointment.customer != null ? (Guid?)appointment.customer.Id : null,
                        customer = appointment.customer != null ? appointment.customer.CustomerToString : appointment.Title,
                        Title = appointment.customer != null ? appointment.customer.CustomerToString : appointment.Title,
                        Description = appointment.Notes,
                        Start = appointment.Start,
                        End = appointment.End,
                        ReminderSent = appointment.ReminderSent,
                        ReminderSentByUserName = appointment.ReminderSentByUserName,
                        SendReminderMessages = appointment.customer != null ? appointment.customer.SendReminderMessages : false,
                        ShowArrivedButton = appointment.ShowArrivedButton,
                        StatusEnum = appointment.StatusEnum,
                        appointmentType = appointment.appointmentType != null ? appointment.appointmentType.Id.ToString() : Guid.Empty.ToString(),
                        DefaultAppointmentLength = appointment.appointmentType != null ? appointment.appointmentType.DefaultAppointmentLength : 30,
                        StaffMember = appointment.StaffMember != null ? appointment.StaffMember.Id.ToString() : Guid.Empty.ToString()
                    });
                }

                return Json(schedules.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        public virtual JsonResult CreateSchedule([DataSourceRequest] DataSourceRequest request, Schedule schedule)
        {
            CustomersDataContext db = new CustomersDataContext();

            List<ModelError> errors = ModelState.Values.SelectMany(v => v.Errors).ToList();

            if (errors.Count() > 0)
            {
                ViewBag.Error = Helper.ErrorHelper.GetErrorText("Unable to create appointment due to validation errors", "109");
                //todo: display error
                //todo: low priority - save error to the database
            }
            else if (schedule != null)
            {
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                Appointment appointment = new Models.Appointment();
                schedule.Id = Guid.NewGuid();
                appointment.Id = schedule.Id;
                appointment.Start = schedule.Start;
                TimeSpan endTime = schedule.End - schedule.End.Date;
                schedule.End = schedule.Start.Date + endTime;
                appointment.End = schedule.End;
                appointment.Notes = schedule.Description;
                appointment.CreatedTimestamp = DateTime.Now;
                appointment.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                if (schedule.customer != null && schedule.customer.Length > 0)
                {
                    Guid customerId;
                    if (Guid.TryParse(schedule.customer, out customerId))
                    {
                        if (new Guid(schedule.customer) != Guid.Empty)
                        {
                            appointment.customer = db.Customers.Find(new Guid(schedule.customer));
                            appointment.customer.LastUpdated = DateTime.Now;
                            schedule.Title = appointment.customer.CustomerToString;
                        }
                        else
                        {
                            //to do: create new patient
                        }
                    }
                    else
                    {
                        schedule.Title = schedule.customer;
                    }
                }
                else if (HttpContext.Session["customer"] != null)
                {
                    appointment.customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
                    appointment.customer.LastUpdated = DateTime.Now;
                    schedule.Title = appointment.customer.CustomerToString;
                }
                else
                {
                    schedule.Title = "";
                }

                appointment.Title = schedule.Title;
                appointment.practice = practice;
                appointment.Notes = schedule.Description;
                appointment.StatusEnum = schedule.StatusEnum;
                if (schedule.appointmentType != null && schedule.appointmentType.Length > 0 && new Guid(schedule.appointmentType) != Guid.Empty)
                {
                    appointment.appointmentType = db.AppointmentTypes.Find(new Guid(schedule.appointmentType));
                    appointment.End = appointment.Start.AddMinutes(db.AppointmentTypes.Find(new Guid(schedule.appointmentType)).DefaultAppointmentLength);
                }
                if (schedule.StaffMember != null && schedule.StaffMember.Length > 0 && new Guid(schedule.StaffMember) != Guid.Empty)
                {
                    appointment.StaffMember = db.ApplicationUsers.Find(schedule.StaffMember);
                }
                if (db.Appointments.Find(appointment.Id) == null)
                {
                    db.Appointments.Add(appointment);
                }
                db.SaveChanges();
            }
            else
            {
                ViewBag.Error = Helper.ErrorHelper.GetErrorText("Unable to create appointment", "110");
                //todo: display error
            }

            return Json(new[] { schedule }.ToDataSourceResult(request));
        }

        public virtual JsonResult UpdateSchedule([DataSourceRequest] DataSourceRequest request, Schedule schedule)
        {
            CustomersDataContext db = new CustomersDataContext();

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            Guid? newCustomerId = null;

            if (errors.Count() > 0)
            {
                ViewBag.Error = Helper.ErrorHelper.GetErrorText("Unable to save appointment due to validation errors", "107");
                //todo: low priority - save error to the database
                //todo: display error on popup dialog
            }
            else if (schedule.Id != Guid.Empty)
            {
                Appointment target = db.Appointments.Find(schedule.Id);
                target.Start = schedule.Start;
                target.End = schedule.End;
                target.Notes = schedule.Description;

                if (schedule.customer != null && schedule.customer.Length > 0)
                {
                    Guid customerId;
                    if (Guid.TryParse(schedule.customer, out customerId))
                    {
                        if (new Guid(schedule.customer) != Guid.Empty)
                        {
                            target.customer = db.Customers.Find(new Guid(schedule.customer));
                            target.customer.LastUpdated = DateTime.Now;
                            schedule.Title = target.customer.CustomerToString;
                        }
                        else
                        {
                            //customerId is empty guid therefore new customer
                            newCustomerId = Guid.NewGuid();
                        }
                    }
                    else if (schedule.Title != schedule.customer)
                    {
                        schedule.Title = schedule.customer;
                        Customer cust = target.customer; //setting the customer to null in the next line does not work without this. for some reason, target.customer has to be read before it can be set to null.
                        target.customer = null;
                    }
                }
                else
                {
                    schedule.Title = "";
                }

                target.Title = schedule.Title;
                if (schedule.StatusEnum == Enums.AppointmentStatus.Arrived && target.StatusEnum != Enums.AppointmentStatus.Arrived)
                {
                    target.Arrived = DateTime.Now;
                    target.ArrivedUserSet = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                }
                target.StatusEnum = schedule.StatusEnum;
                if (schedule.appointmentType != null && schedule.appointmentType.Length > 0 && new Guid(schedule.appointmentType) != Guid.Empty)
                {
                    target.appointmentType = db.AppointmentTypes.Find(new Guid(schedule.appointmentType));
                    target.End = target.Start.AddMinutes(db.AppointmentTypes.Find(new Guid(schedule.appointmentType)).DefaultAppointmentLength);
                }
                else
                {
                    target.appointmentType = null;
                }
                if (schedule.StaffMember != null && schedule.StaffMember.Length > 0 && new Guid(schedule.StaffMember) != Guid.Empty)
                {
                    target.StaffMember = db.ApplicationUsers.Find(schedule.StaffMember);
                }
                else
                {
                    target.appointmentType = null;
                }
                db.Entry(target).State = EntityState.Modified;
                db.SaveChanges();

                if (newCustomerId != null)
                {
                    //redirect doesn't work
                    //return RedirectToActionPermanent("Add", "Customers", new { Id = newCustomerId });
                }
            }
            else
            {
                ViewBag.Error = Helper.ErrorHelper.GetErrorText("Unable to save appointment", "108");
            }

            return Json(new[] { schedule }.ToDataSourceResult(request, ModelState));

        }

        public virtual JsonResult DestroySchedule([DataSourceRequest] DataSourceRequest request, Schedule schedule)
        {
            if (schedule != null)
            {
                if (schedule.Id != Guid.Empty)
                {
                    using (CustomersDataContext dbContext = new CustomersDataContext())
                    {
                        Appointment appointment = dbContext.Appointments.Where(x => x.Id == schedule.Id).FirstOrDefault();
                        appointment.Deleted = DateTime.Now;
                        dbContext.Entry(appointment).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                }
            }

            return Json(new[] { schedule }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult SendReminder(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            var appointment = db.Appointments.Find(Id);

            Message message = null;

            if (appointment.customer.DefaultMessageMethod == Enums.MessageMethod.SMS || appointment.customer.DefaultMessageMethod == Enums.MessageMethod.Email)
            {
                message = new Message();
                message.IsRecall = true;
                message.messageMethod = appointment.customer.DefaultMessageMethod;
                MessageTemplate messageTemplate = db.MessageTemplates.Where(mt => mt.company.Id == practice.company.Id && mt.messageMethod == appointment.customer.DefaultMessageMethod && mt.MessageTypeEnum == Enums.MessageType.Reminder).FirstOrDefault();
                message.RecipientId = appointment.customer.Id;
                message.SenderUser = user;
                message.Id = Guid.NewGuid();
                message.AddedToQueue = DateTime.Now;
                message.Cancelled = null;
                message.CancelledByUser = null;
                message.Sent = null;
                message.practice = practice;

                if (appointment.customer.DefaultMessageMethod == Enums.MessageMethod.SMS)
                {
                    if (appointment.customer.SMSNumber == null || appointment.customer.SMSNumber.Trim().Length == 0)
                    {
                        ViewData["Error"] = "Patient does not have a SMS number";
                        return RedirectToAction("Calendar", new { appointment.Start });
                    }
                    message.ToAddressNumber = appointment.customer.SMSNumber;
                    message.Sender = practice.SMSSenderName;
                    if (messageTemplate.Template != null && messageTemplate.Template.Trim().Length > 0)
                    {
                        message.MessageText = string.Format(messageTemplate.Template, appointment.customer.TitleAndName(),
                            appointment.Start.ToShortDateString(), appointment.Start.ToShortTimeString(),
                            appointment.practice.Tel, appointment.practice.Name);
                    }
                    SMSTransaction smsTransaction = new SMSTransaction();
                    smsTransaction.Id = Guid.NewGuid();
                    smsTransaction.InsertTimestamp = DateTime.Now;
                    smsTransaction.company = practice.company;
                    smsTransaction.Quantity = -1;
                    message.SMSInventory = smsTransaction;
                    db.SMSInventory.Add(smsTransaction);
                    if (practice.CanSendMessages)
                    {
                        string result = "";
                        bool success = MessagesController.SendSMS(message, practice, ref result);
                    }
                }
                else if (appointment.customer.DefaultMessageMethod == Enums.MessageMethod.Email)
                {
                    if (appointment.customer.Email == null || appointment.customer.Email.Trim().Length == 0)
                    {
                        ViewData["Error"] = "Patient does not have an email address";
                        return RedirectToAction("Calendar", new { appointment.Start });
                    }
                    message.ToAddressNumber = appointment.customer.Email;
                    message.Sender = practice.Email != null ? practice.Email : "reply@visiondb.co.uk";
                    if (messageTemplate.Template != null && messageTemplate.Template.Trim().Length > 0)
                    {
                        message.MessageText = string.Format(messageTemplate.Template, appointment.customer.TitleAndName(),
                            appointment.Start.ToShortDateString(), appointment.Start.ToShortTimeString(),
                            appointment.practice.Tel, appointment.practice.Name);
                    }
                    if (messageTemplate.Subject != null && messageTemplate.Subject.Trim().Length > 0)
                    {
                        message.Subject = string.Format(messageTemplate.Subject, appointment.Start.ToShortDateString() + " " + appointment.Start.ToShortTimeString());
                    }
                    else
                    {
                        message.Subject = "Optical appointment reminder";
                    }

                    EmailHelper emailHelper = new EmailHelper();
                    emailHelper.SendEmail(message.ToAddressNumber, message.Subject, message.MessageText, message.Sender, false);
                }

                message.Sent = DateTime.Now;
                db.Messages.Add(message);

                appointment.ReminderSent = DateTime.Now;
                appointment.ReminderSentByUserName = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id).UserToString;

                db.SaveChanges();

                TempData["Message"] = "Appointment reminder sent to " + appointment.customer.ToString();
                return RedirectToAction("Calendar", new { appointment.Start });
            }
            else
            {
                ViewData["Error"] = string.Format("Patient's default message method must be set to {0} or {1} to send them reminders", Enums.MessageMethod.SMS.ToString(), Enums.MessageMethod.Email.ToString());
                return RedirectToAction("Calendar", new { appointment.Start });
            }
        }

        public ActionResult PatientArrived(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            var appointment = db.Appointments.Find(Id);

            appointment.StatusEnum = Enums.AppointmentStatus.Arrived;
            appointment.Arrived = DateTime.Now;
            appointment.ArrivedUserSet = user;

            db.SaveChanges();

            TempData["Message"] = appointment.customer.ToString() + " has been checked in for their appointment";
            return RedirectToAction("Calendar", new { appointment.Start });

        }

        public JsonResult GetStaffMembers()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            List<ApplicationUser> StaffMembers = db.ApplicationUsers.Where(a => a.Hidden == false).OrderBy(a => a.UserName).ToList();

            List<ApplicationUserViewModel> ApplicationUserViewModels = new List<ApplicationUserViewModel>();
            ApplicationUserViewModels.Add(new ApplicationUserViewModel { Id = Guid.Empty.ToString(), Firstnames = "All" });

            foreach (ApplicationUser applicationUser in StaffMembers)
            {
                ApplicationUserViewModels.Add(new ApplicationUserViewModel
                {
                    Id = applicationUser.Id,
                    UserName = applicationUser.UserName,
                    Title = applicationUser.Title,
                    Firstnames = applicationUser.Firstnames,
                    Surname = applicationUser.Surname
                });
            }

            return Json(ApplicationUserViewModels, JsonRequestBehavior.AllowGet);
        }
    }
}