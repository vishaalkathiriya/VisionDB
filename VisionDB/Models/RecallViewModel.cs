using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class RecallViewModel
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNumbers { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerAgeOrDOB { get; set; }

        public bool Selected { get; set; }

        public Enums.AppointmentCategory appointmentCategory { get; set; }

        public string AppointmentCategoryText 
        {
            get
            {
                return appointmentCategory.ToString().Replace("_", " ");
            }
        }

        public int RecallCount { get; set; }

        public string RecallTemplate { get; set; }

        public DateTime? Due { get; set; }

        public DateTime? LastRecallDate { get; set; }

        public string StatusMessage { get; set; }
    }
}