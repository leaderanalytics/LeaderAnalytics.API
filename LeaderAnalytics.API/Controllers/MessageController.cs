using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LeaderAnalytics.API.Domain;

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
            //return Ok("ok"); returning a string is necessary for jquery $.ajax
            return Ok();
        }
    }
}