﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class Room
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public virtual Practice practice { get; set; }
    }
}