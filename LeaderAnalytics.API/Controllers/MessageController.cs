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
using Serilog;

namespace LeaderAnalytics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private EMailClient eMailClient;
        private static List<ContactHistory> contactHistory;
        private static Random random;
        private const int SEND_INTERVAL = 5;

        static MessageController()
        {
            contactHistory = new List<ContactHistory>();
            random = new Random();
        }

        public MessageController(EMailClient eMailClient)
        {
            this.eMailClient = eMailClient;
        }


        [HttpPost]
        [Route("SendEmail")]
        public IActionResult SendEMail(EmailMsg msg)
        {

            if (string.IsNullOrEmpty(msg.IP_Address) || string.IsNullOrEmpty(msg.CaptchaCode))
                return BadRequest("Invalid or missing Captcha code or IP Address");
                

            IActionResult result = null;
            ExpireContactHistory();                                                                             
            string canSend = CanSend(msg.IP_Address, msg.CaptchaCode);

            if (!string.IsNullOrEmpty(canSend))
            {
                result = BadRequest(canSend);
                Log.Information(canSend, msg.IP_Address);
            }
            else
            {
                eMailClient.Send("leaderanalytics@outlook.com", msg.Msg);
                contactHistory.First(x => x.IP_Address == msg.IP_Address && x.CaptchaCode == msg.CaptchaCode).SendTime = DateTime.UtcNow;
                result = CreatedAtAction("SendEMail", "email");
            }
            return result;
        }

        /// <summary>
        /// Creates a captcha code and stores it in the history list along with the ip address of the user who requested it.
        /// Returns an image of the created code.
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CaptchaImage")]
        public ActionResult CaptchaImage(string ipaddress)
        {
            string code = string.Empty;
            DateTime now = DateTime.UtcNow;

            // Check if an unexpired Captcha has already been assigned to this IP.  If so, use it.
            ContactHistory ch = contactHistory.FirstOrDefault(x => x.IP_Address == ipaddress && x.CreateTime.AddHours(1) > now && !x.SendTime.HasValue);

            if (ch == null)
            {
                for (int i = 0; i < 3; i++)
                    code = String.Concat(code, random.Next(10).ToString());

                ch = new ContactHistory { IP_Address = ipaddress, CreateTime = DateTime.UtcNow, CaptchaCode = code };
                contactHistory.Add(ch);
            }

            CaptchaImage ci = new CaptchaImage(ch.CaptchaCode, 100, 50, "Century Schoolbook");
            return new ImageResult(ci.Image, System.Drawing.Imaging.ImageFormat.Jpeg).GetFileStreamResult();
        }


        private void ExpireContactHistory()
        {
            // Delete entries that were created more than one hour ago or where the user sent the email more than five minutes ago.
            DateTime now = DateTime.UtcNow;
            List<ContactHistory> expired = contactHistory.Where(x => x.CreateTime.AddHours(1) < now || (x.SendTime.HasValue && x.SendTime.Value.AddMinutes(SEND_INTERVAL) < now)).ToList();

            foreach (ContactHistory c in expired)
                contactHistory.Remove(c);
        }

        private string CanSend(string ip, string code)
        {
            DateTime now = DateTime.UtcNow;

            // Check for a send in the last five minutes
            if (contactHistory.Any(x => (x.IP_Address == ip && x.SendTime.HasValue && x.SendTime.Value.AddMinutes(SEND_INTERVAL) >= now)))
                return $"Please wait at least { SEND_INTERVAL.ToString() } minutes before sending another message.";
            if (!contactHistory.Any(x => (x.IP_Address == ip && !x.SendTime.HasValue && x.CaptchaCode == code)))
                return "Invalid Captcha code.  Please try again.";
            else
                return null;
        }
    }
}