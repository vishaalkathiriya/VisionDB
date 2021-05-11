using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Enums
    {
        public enum BaseHorizontal
        {
            None = 0,
            In = 1,
            Out = 2
        }

        public enum BaseVertical
        {
            None = 0,
            Up = 3,
            Down = 4
        }

        public enum AppointmentCategory
        {
            None = 0,
            Eye_Exam = 1,
            Contact_Lens_Exam = 2
            //Contact_Lens_Aftercare = 3
            //Dialation = 4,
            //Visual = 5
        }



        public enum AdviceAndRecallList
        {
            None = 0,
            New_prescription_as_advised = 1,
            No_prescription_necessary = 2,
            No_Clinical_Change = 3,
            referring_to_doctor = 4
        }

        public enum YesNo
        {
            None = 0,
            Yes = 1,
            No = 2

        }

        public enum LensType
        {
            [Description("Not Assigned")]
            Not_Assigned = 0,

            [Description("2 weekly")]
            Two_Weekly = 1
            //    Soft_Daily= 2,
            //    Toric_Daily= 3,
            //    Soft_Monthly= 4,
            //Toric_Monthly=5,
            //    Daily_MultiFocal=6,
            //    Monthly_MultiFocal=7,
            //    RGP_Annual=8,

        }

        public enum FrequencyUnit
        {
            Days = 1,
            Months = 2,
            Years = 3
        }

        public enum NHSVoucherGrade
        {
            None = 0,
            A = 1,
            B = 2,
            C = 3,
            D = 4,
            E = 5,
            F = 6,
            G = 7,
            H = 8,
            I = 9

        }

        public enum ReconciliationStatus
        {
            None = 0,
            Pending = 1,
            Reconciled = 2,
            Flagged = 3
        }

        public enum Compensated
        {
            Compensated = 1,
            Decompensated = 2
        }

        public enum InvoiceDetailStatus
        {
            Not_Ordered = 0,
            Ordered = 1,
            Received = 2,
            Collected = 3,
            Dispatched = 4,
            Awaiting_Collection_or_Dispatch = 5,
            Notified = 6
        }

        public enum InvoiceStatus
        {
            Not_Ordered = 0,
            Awaiting_Goods = 1,
            Awaiting_Collection_Or_Dispatch = 2,
            Awaiting_Payment = 3,
            Complete = 4
        }

        public enum DefaultHomePage
        {
            Dashboard = 0,
            Launcher = 1,
            Patients = 2,
            Calendar = 3,
            Reports = 4
        }

        public enum TagType
        {
            Lifestyle = 0,
            Medical_Condition = 1,
            Care_Home = 2,
            Product = 3
        }

        public enum ObjectiveMethod
        {
            Unknown = 0,
            Retinoscopy = 1,
            Autorefractor = 2
        }

        public enum AppointmentStatus
        {
            None = 0,
            Arrived = 1,
            Cancelled = 2,
            Rebooked = 3,
            No_Show = 4
        }

        public enum MethodSentBy
        {
            Collection = 1,
            Dispatch = 2
        }

        public enum AuditLogEntryType
        {
            Login,
            Login_Failed,
            Patient_Deleted,
            Patient_Delete_Failed,
            Patient_Record_Viewed,
            Patient_Added,
            Patient_Edited,
            Eye_Exam_Added,
            Eye_Exam_Edited,
            Eye_Exam_Deleted,
            Contact_Lens_Exam_Added,
            Contact_Lens_Exam_Edited,
            Contact_Lens_Exam_Deleted,
            Database_Backup,
            Invoice_Deleted,
            User_Permission_Change,
            Recall_Error,
            Log_Off,
            Eye_Exam_Error,
            Daemon
        }

        public enum UserRoles
        {
            Admin,
            Optician,
            Receptionist,
            Locum,
            Product_Manager
        }

        public enum Eye
        {
            Right = 1,
            Left = 2
        }
        public enum MessageType
        {
            Generic = 0,
            Recall = 1,
            Reminder = 2
        }

        public enum MessageMethod
        {
            None = 0,
            SMS = 1,
            Email = 2,
            Letter = 3,
            Telephone = 4
        }

        public enum TemplateType
        {
            Recalls = 1,
            Marketing = 2    
        }

        public enum TemplatePlaceholder
        {
            patient_name = 1,
            patient_address = 2,
            patient_number = 3,
            eye_exam_due_date = 4,
            eye_exam_last_test_date = 5,
            contact_lens_exam_due_date = 6,
            contact_lens_exam_last_test_date = 7,
            practice_name = 8,
            practice_address = 9,
            practice_telephone = 10,
            practice_email = 11,
            todays_date = 12
        }

        public enum BeforeOrAfter
        {
            After = 1,
            Before = 2
        }

        public enum TicketPriority
        {
            High = 1,
            Medium = 2,
            Low = 3
        }

        public enum TicketType
        {
            Task = 1,
            Bug_Fix = 2,
            Feature = 3
        }

        public enum TicketStatus
        {
            New = 1,
            In_Progress = 2,
            Complete = 3
        }
    }
}