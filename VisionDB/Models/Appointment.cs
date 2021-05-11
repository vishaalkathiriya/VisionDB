using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Appointment
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime CreatedTimestamp { get; set; }

        public virtual Practice practice { get; set; }

        public virtual Customer customer { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public string Title { get; set; }

        public string Notes { get; set; }

        [DisplayName("Appointment Type")]
        public virtual AppointmentType appointmentType { get; set; }

        public DateTime? Deleted { get; set; }

        public override string ToString()
        {
            string output = "";
            if (appointmentType != null)
            {
                output = appointmentType.Name;
            }

            if (Notes != null && Notes.Length > 0)
            {
                if (output.Length > 0)
                {
                    output += string.Concat(": ", Notes);
                }
                else
                {
                    output += Notes;
                }
            }

            return output;
        }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public DateTime? ReminderSent { get; set; }

        public string ReminderSentByUserName { get; set; }

        public DateTime? Arrived { get; set; }

        public virtual ApplicationUser ArrivedUserSet { get; set; }

        public Enums.AppointmentStatus StatusEnum { get; set; }

        public virtual ApplicationUser StaffMember { get; set; }

        public bool ShowArrivedButton 
        { 
            get
            {
                if (Arrived == null && (StatusEnum == Enums.AppointmentStatus.None || StatusEnum == Enums.AppointmentStatus.Rebooked))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}