namespace LeaderAnalytics.API.Controllers;

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
        return new ActionResult<string>(JsonSerializer.Serialize("Leader Analytics API"));
    }

    /// <summary>
    /// Secure method to validate login credentials.
    /// </summary>
    /// <returns></returns>

    [Authorize(AuthenticationSchemes = "AzureAdB2C")]
    [HttpGet]
    [Route("SecureIdentity")]
    [RequiredScope("access_as_user")]
    public ActionResult<string> SecureIdentity()
    {
        return new ActionResult<string>(JsonSerializer.Serialize("Leader Analytics API. Security credentials are verified."));
    }
}
