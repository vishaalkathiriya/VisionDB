using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class AppointmentTypeViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Enums.AppointmentCategory AppointmentCategoryEnum { get; set; }

        public int DefaultAppointmentLength { get; set; }

        public string AppointmentCategoryToString 
        {
            get
            {
                return AppointmentCategoryEnum.ToString().Replace('_', ' ');
            }
        }
    }
}