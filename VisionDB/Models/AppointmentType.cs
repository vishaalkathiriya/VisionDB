using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class AppointmentType
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [DisplayName("Category")]
        public Enums.AppointmentCategory AppointmentCategoryEnum { get; set; }

        public virtual Practice practice { get; set; }

        [DisplayName("Default appointment length (mins)")]
        public int DefaultAppointmentLength { get; set; }
        
        public DateTime Added { get; set; }

        public DateTime? Deleted { get; set; }


    }
}