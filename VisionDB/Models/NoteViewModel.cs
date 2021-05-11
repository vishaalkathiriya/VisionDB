using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class NoteViewModel
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTimestamp { get; set; }

    }
}