using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaderAnalytics.API.Controllers
{
    [Route("")]
    [Route("api")]
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Public unsecured method so we can verify if the server is running without authorization.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Route("Identity")]
        public ActionResult<string> Identity()
        {
            return "Leader Analytics API";
        }

        /// <summary>
        /// Secure method to validate login credentials.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "DaemonAppRole")]
        [HttpGet]
        [Route("SecureIdentity")]
        public ActionResult<string> SecureIdentity()
        {
            return "Leader Analytics API. Security credentials are verified.";
        }
    }
}
