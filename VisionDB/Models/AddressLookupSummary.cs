using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class AddressLookupSummary
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}