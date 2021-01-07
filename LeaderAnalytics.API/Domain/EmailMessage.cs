using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.API.Domain
{
    public class EmailMessage
    {
        public string[] To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Msg { get; set; }
        public bool IsHTML { get; set; }
    }
}
