using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class RecallDocument
    {
        public Guid Id { get; set; }

        public virtual RecallTemplate recallTemplate { get; set; }

        public virtual DocumentTemplate documentTemplate { get; set; }

        public DateTime? Deleted { get; set; }

        [DisplayName("Before or after")]
        public Enums.BeforeOrAfter BeforeOrAfterEnum { get; set; }

        [DisplayName("When to send recall")]
        public int DateSpanValue { get; set; }

        public Enums.FrequencyUnit DateSpanUnit { get; set; }

    }
}