using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaderAnalytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private EMailClient eMailClient;

        public MessageController(EMailClient eMailClient)
        {
            this.eMailClient = eMailClient;
        }

        [HttpPost]
        [Route("SendEmail")]
        public IActionResult SendEMail(EmailMsg msg)
        {
            eMailClient.Send(msg.To, msg.Msg);
            return Ok("done");
        }
    }

    public class EmailMsg 
    { 
        public string To { get; set; }
        public string Msg { get; set; }
    }
}