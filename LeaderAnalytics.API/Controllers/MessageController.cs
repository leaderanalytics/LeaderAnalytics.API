using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LeaderAnalytics.API.Domain;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace LeaderAnalytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            eMailClient.Send("leaderanalytics@outlook.com", msg.Msg);
            return CreatedAtAction("SendEMail", "email");
        }
    }
}