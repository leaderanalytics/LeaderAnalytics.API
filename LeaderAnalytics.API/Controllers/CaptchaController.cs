namespace LeaderAnalytics.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[Authorize(Policy = "DaemonAppRole")]
public class CaptchaController : ControllerBase
{
    private CaptchService captchaService;


    public CaptchaController(CaptchService captchaService)
    {
        this.captchaService = captchaService;
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
        CaptchaImage ci = captchaService.GetCaptchaImage(ipaddress);
        return new ImageResult(ci.Image, System.Drawing.Imaging.ImageFormat.Jpeg).GetFileStreamResult();
    }

    [HttpGet]
    [Route("CanSubmit")]
    public ActionResult<string> CanSubmit(string ipaddress, string code)
    {
        return captchaService.CanSubmit(ipaddress, code);
    }

    [HttpGet]
    [Route("SetSubmitTime")]
    public ActionResult SetSubmitTime(string ipaddress, string code)
    {
        captchaService.SetSubmitTime(ipaddress, code);
        return Ok();
    }


    /// <summary>
    /// Checks to see if the user can submit.
    /// If so, sets submit time so a second request is not needed. 
    /// </summary>
    /// <param name="ipaddress"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Submit")]
    public ActionResult Submit(string ipaddress, string code)
    {
        string s = captchaService.CanSubmit(ipaddress, code);

        if (string.IsNullOrEmpty(s))
        {
            captchaService.SetSubmitTime(ipaddress, code);
            return Ok("ok");
        }
        return StatusCode(300, s);
    }
}
