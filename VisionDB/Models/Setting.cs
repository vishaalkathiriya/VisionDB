using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Setting
    {
        public int Id { get; set; }

        [DisplayName("Daily backup")]
        public DateTime DailyDatabaseBackupTime { get; set; }

        [DisplayName("Backup last taken")]
        public DateTime? DailyBackupLastTaken { get; set; }

        [DisplayName("SMS start")]
        public DateTime SMSStartTime { get; set; }

        [DisplayName("SMS end")]
        public DateTime SMSEndTime { get; set; }
    }
}