using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.ComponentModel.DataAnnotations;

namespace VisionDB.Models
{
    public class Schedule : ISchedulerEvent
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsAllDay { get; set; }
        public string Recurrence { get; set; }
        public string RecurrenceRule { get; set; }
        public string RecurrenceException { get; set; }
        public string StartTimezone { get; set; }
        public string EndTimezone { get; set; }
        public DateTime? ReminderSent { get; set; }
        public string ReminderSentByUserName { get; set; }
        public string ReminderText 
        { 
            get
            {
                if (SendReminderMessages)
                {
                    if (ReminderSent != null)
                    {
                        return string.Format("Reminder sent {0} {1} by {2}", ReminderSent.Value.ToShortDateString(), ReminderSent.Value.ToShortTimeString(), ReminderSentByUserName);
                    }
                    else
                    {
                        return "Reminder not sent";
                    }
                }
                else
                {
                    return string.Format("Patient's default message method must be set to {0} or {1} to send them reminders", Enums.MessageMethod.SMS.ToString(), Enums.MessageMethod.Email.ToString());
                }
            }
        }

        public bool SendReminderMessages { get; set; }

        public bool ShowArrivedButton { get; set; }

        public Enums.AppointmentStatus StatusEnum { get; set; }

        public string customer { get; set; }

        public string appointmentType { get; set; }

        public int DefaultAppointmentLength { get; set; }

        public string StaffMember { get; set; }
    }
}