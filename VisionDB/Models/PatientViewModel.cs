using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class PatientViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
        public string AgeOrDOB { get; set; }
        public string Colour { get; set; }
    }
}