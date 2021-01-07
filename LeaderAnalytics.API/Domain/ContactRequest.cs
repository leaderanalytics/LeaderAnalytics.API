using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.API.Domain
{
    public class ContactRequest
    {
        public string To { get; set; }
        public string Msg { get; set; }
        public string CaptchaCode { get; set; }
        public string IP_Address { get; set; }
    }
}
