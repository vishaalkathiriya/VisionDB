using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    #region Properties
    public class Attachment
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public virtual Customer customer { get; set; }

        public string FileName { get; set; }

        public string FileComments { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }
        
        public string FileNameWithoutId 
        {
            get
            {
                return FileName.Replace(Id.ToString() + "_", "");
            }
        }
    }
    #endregion
}