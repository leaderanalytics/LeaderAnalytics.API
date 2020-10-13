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
using LeaderAnalytics.API.Model;
using LeaderAnalytics.API.Services;
using Serilog;

namespace LeaderAnalytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private EMailClient eMailClient;
        private CaptchService captchaService;

        public MessageController(EMailClient eMailClient, CaptchService captchaService)
        {
            this.eMailClient = eMailClient;
            this.captchaService = captchaService;
        }


        [HttpPost]
        [Route("SendEmail")]
        public IActionResult SendEMail(EmailMsg msg)
        {

            if (string.IsNullOrEmpty(msg.IP_Address) || string.IsNullOrEmpty(msg.CaptchaCode))
                return BadRequest("Invalid or missing Captcha code or IP Address");

            IActionResult result = null;
            
            string canSend = captchaService.CanSubmit(msg.IP_Address, msg.CaptchaCode);

            if (!string.IsNullOrEmpty(canSend))
            {
                result = BadRequest(canSend);
                Log.Information(canSend, msg.IP_Address);
            }
            else
            {
                eMailClient.Send("leaderanalytics@outlook.com", msg.Msg);
                captchaService.SetSubmitTime(msg.IP_Address, msg.CaptchaCode);
                result = CreatedAtAction("SendEMail", "email");
            }
            return result;
        }

        
    }
}