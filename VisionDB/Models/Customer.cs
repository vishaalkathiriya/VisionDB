using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VisionDB.Models
{
    public class Customer
    {
        #region Properties
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// External Id used to go directly to a company without login. 
        /// Different to Id so that it can be changed.
        /// </summary>
        public Guid ExternalId { get; set; }

        /// <summary>
        /// Timestamp of when customer last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        public string Number { get; set; }
        
        public virtual Practice practice { get; set; }

        public string Title { get; set; }

        [Required]
        public string Firstnames { get; set; }

        [Required]
        public string Surname { get; set; }

        [DisplayName("Previous surname")]
        public string PreviousSurname { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date of birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOB { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("Address")]
        [MaxLength(1000)]
        public string Address { get; set; }
        
        [DisplayName("Postcode")]
        [MaxLength(20)]
        public string Postcode { get; set; }
        
        public string Telephone { get; set; }

        [DisplayName("Mobile")]
        public string SMSNumber { get; set; }

        public string DefaultNumber 
        {
            get
            {
                if (Telephone != null)
                {
                    return Telephone;
                }
                else if (SMSNumber != null)
                {
                    return SMSNumber;
                }
                else
                {
                    return "";
                }
            }
        }

        public string AvailableNumbers 
        {
            get
            {
                string availableNumbers = "";

                if (Telephone != null)
                {
                    availableNumbers = "Tel: " + Telephone;
                }

                if (SMSNumber != null)
                {
                    if (availableNumbers.Length > 0)
                    {
                        availableNumbers += " / Mobile: " + SMSNumber;
                    }
                    else
                    {
                        availableNumbers = "Mobile: " + SMSNumber;
                    }
                }

                return availableNumbers;
            }
        }
        
        public string Email { get; set; }
        
        public int TestFrequencyId { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public ICollection<EyeExam> EyeExams { get; set; }
        public ICollection<Note> Notes { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Next test date")]
        public DateTime? NextTestDueDate 
        { 
            get
            {
                if (NextDueDateEyeExam != null && NextDueDateContactLensExam != null)
                {
                    return NextDueDateEyeExam < NextDueDateContactLensExam ? NextDueDateEyeExam : NextDueDateContactLensExam;
                }
                else if (NextDueDateEyeExam != null)
                {
                    return NextDueDateEyeExam;
                }
                else if (NextDueDateContactLensExam != null)
                {
                    return NextDueDateContactLensExam;
                }
                else
                {
                    return null;
                }
            }
        } 

        
        public int? Age 
        {
            get
            {
                if (DOB.HasValue)
                {
                    TimeSpan age = DateTime.Now.Date - DOB.Value.Date;
                    return Convert.ToInt32(Math.Truncate((double)age.Days / 365.25));
                }
                else
                {
                    return null;
                }
            }
        }

        public string AgeText
        {
            get
            {
                if (Age != null)
                {
                    return Age.ToString() + " years old";
                }
                else
                {
                    return "Date of birth not specified";
                }
            }
        }

        [DisplayName("Notes")]
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [DisplayName("GP Name")]
        public string Doctor { get; set; }

        public string DoctorContact { get; set; }

        [DisplayName("Practice")]
        public string GPpracticename { get; set; }

        [DisplayName("Practice address")]
        public string GPpracticeaddress { get; set; }

        [DisplayName("Postcode")]
        public string GPpracticepostcode { get; set; }

        [DisplayName("Telephone")]
        public string GPpracticephone{ get; set; }

        [DisplayName("Fax")]
        public string GPpracticefax { get; set; }

        [DisplayName("Email")]
        public string GPpracticeemail{ get; set; }

        [DisplayName("Previous optician")]
        public string LastOptician { get; set; }

        [DisplayName("Contact details")]
        public string LastOpticianContact { get; set; }

        public string Occupation { get; set; }

        [DisplayName("CLFitDrName")]
        public string CLFitDrName { get; set; }

        [DisplayName("CLFitDrAddress")]
        public string CLFitDrAddress { get; set; }

        [DisplayName("CLFitOccupation")]
        public string CLFitOccupation { get; set; }

        [DisplayName("CLFitHobbiesSports")]
        public string CLFitHobbiesSports { get; set; }

        [DisplayName("CLFitExistingWearer")]
        public string CLFitExistingWearer { get; set; }

        [DisplayName("CLFitPreviousWearingDetails")]
        public string CLFitPreviousWearingDetails { get; set; }

        [DisplayName("CLFitType")]
        public string CLFitType { get; set; }

        [DisplayName("CLFitWearingTime")]
        public string CLFitWearingTime { get; set; }

        [DisplayName("CLFitSolutionsUsed")]
        public string CLFitSolutionsUsed { get; set; }

        [DisplayName("CLFitCurrentPreviousProblems")]
        public string CLFitCurrentPreviousProblems { get; set; }

        [DisplayName("CLFitTrialComments")]
        public string CLFitTrialComments { get; set; }

        [DisplayName("CLFitTrialOptometrist")]
        public string CLFitTrialOptometrist { get; set; }

        [DisplayName("CLFitWearingSchedule")]
        public string CLFitWearingSchedule { get; set; }

        [DisplayName("CLFitCleaningRegime")]
        public string CLFitCleaningRegime { get; set; }

        [DisplayName("CLFitDOHFormCompleted")]
        public string CLFitDOHFormCompleted { get; set; }

        [DisplayName("CLFitCollectionLensesIn")]
        public string CLFitCollectionLensesIn { get; set; }

        [DisplayName("CLFitCollectionWearingTime")]
        public string CLFitCollectionWearingTime { get; set; }

        [DisplayName("CLFitCollectionAdvice")]
        public string CLFitCollectionAdvice { get; set; }

        [DisplayName("CLFitCollectionNextAppointment")]
        public string CLFitCollectionNextAppointment { get; set; }

        [DisplayName("CLFitCollectionOptometrist")]
        public string CLFitCollectionOptometrist { get; set; }

        [DisplayName("National Insurance No")]
        public string NINo { get; set; }

        [DisplayName("NHS No")]
        public string NHSNo { get; set; }

        [DisplayName("Title")]
        public string ParentTitle { get; set; }

        [DisplayName("First Name")]
        public string ParentName { get; set; }

        [DisplayName("Surname")]
        public string ParentSurname { get; set; }

        [DisplayName("Address")]
        public string ParentAddress { get; set; }

        [DisplayName("Postcode")]
        public string ParentPostCode { get; set; }


        public string RFV { get; set; }

        public string GH { get; set; }

        public string POH { get; set; }

        public string FH { get; set; }

        [DisplayName("Medications")]
        public string MEDS { get; set; }

        public string Allergies { get; set; }

        [DisplayName("NHS or Private")]
        public NHSPrivateList? NHSPrivate { get; set; }

        [DisplayName("NHS Reason")]
        public string NHSReason { get; set; }

        [DisplayName("NHS Voucher")]
        public string NHSVoucher { get; set; }

        [DisplayName("Eye exam frequency")]
        public int? EyeExamFrequencyValue { get; set; }

        public Enums.FrequencyUnit EyeExamFrequencyUnit { get; set; }

        [DisplayName("Contact lens exam frequency")]
        public int? ContactLensExamFrequencyValue { get; set; }

        public Enums.FrequencyUnit ContactLensExamFrequencyUnit { get; set; }

        public string CustomerToString
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            if (Number != null && Number.Length > 0)
            {
                return string.Format("{0} {1} {2} - {3}", Title, Firstnames, Surname, Number).Trim();
            }
            else
            {
                return string.Format("{0} {1} {2}", Title, Firstnames, Surname).Trim();
            }
        }

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

        public string TitleAndName()
        {
            return string.Format("{0} {1} {2}", Title, Firstnames, Surname).Trim();
        }

        public string Fullname()
        {
            return string.Format("{0} {1}", Firstnames, Surname).Trim();
        }
        
        public IEnumerable<Attachment> Attachments { get; set; }

        public IEnumerable<Appointment> Appointments { get; set; }

        public IEnumerable<Message> Messages { get; set; }

        #endregion 

        public virtual ApplicationUser CreatedByUser { get; set; }

        [DisplayName("Eye exam due")]
        public DateTime? NextDueDateEyeExam { get; set; }

        [DisplayName("Contact lens exam due")]
        public DateTime? NextDueDateContactLensExam { get; set; }

        public DateTime? EyeExamRecallSent { get; set; }

        public DateTime? ContactLensExamRecallSent { get; set; }

        [DisplayName("Last eye exam")]
        public DateTime? PreviousEyeExamDate { get; set; }

        [DisplayName("Last contact lens exam")]
        public DateTime? PreviousContactLensExamDate { get; set; }

        [DisplayName("Default contact option")]
        public Enums.MessageMethod DefaultMessageMethod { get; set; }

        [DisplayName("Contact options")]
        public string ContactOptions 
        { 
            get
            {
                string result = "";
                result += EmailReminders ? "Email, " : "";
                result += SMSReminders ? "SMS, " : "";
                result += LetterReminders ? "Letter, " : "";
                result += TelephoneReminders ? "Telephone" : "";
                result = result.TrimEnd().TrimEnd(',');

                string defaultContact = DefaultMessageMethod == Enums.MessageMethod.None ? "(No default selected)" : string.Format("{0} is default", DefaultMessageMethod.ToString());

                return result + " " + defaultContact;
            }
        }

        public bool SendReminderMessages 
        { 
            get
            {
                if (DefaultMessageMethod == Enums.MessageMethod.SMS || DefaultMessageMethod == Enums.MessageMethod.Email)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }

        [DisplayName("School / College / University")]
        public string SchoolCollegeUniversity { get; set; }

        [DisplayName("School / College / University Address")]
        public string SchoolCollegeUniversityAddress { get; set; }

        [DisplayName("School / College / University Postcode")]
        public string SchoolCollegeUniversityPostcode { get; set; }

        public int RecallCount { get; set; }

        [DisplayName("Symptoms and History")]
        public string SymptomsAndHistory { get; set; }

        public string Benefit { get; set; }

        [DisplayName("Care home")]
        public string CareHome { get; set; }

        [DisplayName("Sheltered accom.")]
        public string ShelteredAccommodation { get; set; }

        [DisplayName("Email reminders")]
        public bool EmailReminders { get; set; }

        [DisplayName("SMS reminders")]
        public bool SMSReminders { get; set; }

        [DisplayName("Letter reminders")]
        public bool LetterReminders { get; set; }

        [DisplayName("Tel. reminders")]
        public bool TelephoneReminders { get; set; }

        [DisplayName("Eye exam recall template")]
        public virtual RecallTemplate eyeExamRecallTemplate { get; set; }

        [DisplayName("Contact lens exam recall template")]
        public virtual RecallTemplate contactLensExamRecallTemplate { get; set; }

        public enum NHSPrivateList
        {
            Unknown = 0,
            NHS = 1,
            Private= 2
        }

        public bool ExcludeFromRecalls { get; set; }

        public string WorkTelephone { get; set; }

        public virtual Message LastRecallMessage { get; set; }
    }
}