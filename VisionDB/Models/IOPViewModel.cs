﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class IOPViewModel
    {
        public Guid Id { get; set; }

        public DateTime Added { get; set; }
        public float? Value1 { get; set; }
        public float? Value2 { get; set; }
        public float? Value3 { get; set; }
        public float? Value4 { get; set; }
        public float? Value5 { get; set; }
        public float Average { get; set; }
        public Enums.Eye eye { get; set; }

        public DateTime? Deleted { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }
}