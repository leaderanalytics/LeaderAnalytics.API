﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.API.Model
{
    public class ContactHistory
    {
        public string IP_Address { get; set; }
        public string CaptchaCode { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? SendTime { get; set; }
    }
}



