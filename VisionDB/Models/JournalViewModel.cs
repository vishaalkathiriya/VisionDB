using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class JournalViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime EntryDate { get; set; }
        public EntryType entryType { get; set; }

        public enum EntryType
        {
            Note = 0,
            Exam = 1,
            Invoice = 2,
            Attachment = 3,
            Appointment = 4,
            Message = 5
        }

        public JournalViewModel(Guid Id, string Description, DateTime EntryDate, EntryType entryType)
        {
            this.Id = Id;
            this.Description = Description;
            this.EntryDate = EntryDate;
            this.entryType = entryType;
        }

        public JournalViewModel()
        {
            //default constructor
        }

        public override string ToString()
        {
            return this.Description;
        }

        public string EntryDateToString
        {
            get
            {
                return EntryDate.ToShortDateString() + " " + EntryDate.ToShortTimeString();
            }
        }

        public string Colour
        {
            get
            {
                if (entryType == EntryType.Exam)
                {
                    return "#7FFFD4"; //HTML colour Aquamarine
                }
                else if (entryType == EntryType.Invoice)
                {
                    return "#FA8072"; //HTML colour Salmon
                }
                else if (entryType == EntryType.Note)
                {
                    return "PapayaWhip"; 
                }
                else if (entryType == EntryType.Appointment)
                {
                    return "LightCyan"; 
                }
                else if (entryType == EntryType.Attachment)
                {
                    return "LightGray";
                }
                else if (entryType == EntryType.Message)
                {
                    return "Snow";
                }
                else
                {
                    return "white";
                }
            }
        }

        public string EntryTypeToString
        {
            get
            {
                return entryType.ToString();
            }
        }
    }
}