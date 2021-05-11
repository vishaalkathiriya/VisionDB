using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Practice
    {
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// External Id used to go directly to a company without login. 
        /// Different to Id so that it can be changed.
        /// </summary>
        public Guid ExternalId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Postcode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public virtual Company company { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime WorkDayStart { get; set; }
        public DateTime WorkDayEnd { get; set; }
        public string PrimaryCareTrustGOS { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public int PracticeNumber { get; set; }
        public int LastInvoiceNumber { get; set; }

        public virtual ApplicationUser DefaultOptician { get; set; }

        public string PracticeRef { get; set; }

        [DisplayName("Show GOS forms")]
        public bool ShowGOSForms { get; set; }

        public string SMSSenderName { get; set; }

        public bool CanSendMessages { get; set; }

        [DisplayName("Calendar Time Slot Length")]
        public int SchedulerMajorTick { get; set; }

        [DisplayName("Calendar Time Slot Increments")]
        public int SchedulerMinorTickCount { get; set; }
        public int LastPatientNumber { get; set; }

        [DisplayName("Percentage of sale to apply VAT to")]
        public int VATAppliedToSalePercentage { get; set; }

        [DisplayName("Contractor Name")]
        public string ContractorName { get; set; }

        [DisplayName("Contractor Number")]
        public string ContractorNumber { get; set; }

        [DisplayName("Show DOB on patient search")]
        public bool ShowDOBOnPatientSearch { get; set; }

        [DisplayName("Telephone area code")]
        public string TelAreaPrefix { get; set; }

        [DisplayName("Eye exam screen design")]
        public int EyeExamScreenDesign { get; set; }

        [DisplayName("Default eye exam time to patients appointment time")]
        public bool DefaultEyeExamTimeToPatientsAppointment { get; set; }

        [DisplayName("Show domiciliary fields")]
        public bool ShowDomiciliaryFields { get; set; }

        [DisplayName("Show call buttons")]
        public bool ShowCallButtons { get; set; }

        [DisplayName("Show practice notes on dashboard")]
        public bool ShowPracticeNotesOnDashboard { get; set; }

        [DisplayName("Edit patient from calendar")]
        public bool EditPatientFromCalendar { get; set; }

        [DisplayName("Recall date cut off")]
        public DateTime RecallDateCutOff { get; set; }

        public decimal MonthlyRate { get; set; }

        [DisplayName("Patient number prefix")]
        [MaxLength(20)]
        public string PatientNumberPrefix { get; set; }

        public string FullAddress
        {
            get
            {
                return string.Format("{0} {1}", Address, Postcode);
            }
        }

        public string FullAddressHtml
        {
            get
            {
                return Helper.HtmlExtensions.AddressToHtml(Address, Postcode);
            }
        }
    }
}