namespace LeaderAnalytics.API.Controllers;

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
    [Route("SendContactRequest")]
    public IActionResult SendContactRequest(ContactRequest msg)
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
            eMailClient.Send("leaderanalytics@outlook.com, sam.wheat@outlook.com", msg.Msg);
            captchaService.SetSubmitTime(msg.IP_Address, msg.CaptchaCode);
            result = CreatedAtAction("SendContactRequest", "email");
        }
        return result;
    }

    [HttpPost]
    [Route("SendEMailMessage")]
    public IActionResult SendEmailMessage(EmailMessage msg)
    {
        if (msg == null)
            return BadRequest("Required EmailMessage parameter is null or invalid.");
        if (msg.To == null || msg.To.Length == 0)
            return BadRequest("At least one to address is required.");
        else if (string.IsNullOrEmpty(msg.Msg))
            return BadRequest("Message cannot be null.");
        else if (string.IsNullOrEmpty(msg.From))
            return BadRequest("From address cannot be null.");

        try
        {
            eMailClient.Send(msg.To, msg.From, msg.Subject, msg.Msg, msg.IsHTML);
        }
        catch (Exception ex)
        {
            string s = ex.ToString();
            Log.Error(s);
            return StatusCode(500, s);
        }
        return CreatedAtAction("SendEMailMessage", "email");
    }


    [HttpPost]
    [Route("SendMailMessage")]
    public IActionResult SendMailMessage(MailMessage msg)
    {
        if (msg == null)
            return BadRequest("Required EmailMessage parameter is null or invalid.");
        if (msg.To == null || !msg.To.Any())
            return BadRequest("At least one to address is required.");
        else if (string.IsNullOrEmpty(msg.Body))
            return BadRequest("Message cannot be null.");
        else if (msg.From == null)
            return BadRequest("From address cannot be null.");

        try
        {
            eMailClient.Send(msg);
        }
        catch (Exception ex)
        {
            string s = ex.ToString();
            Log.Error(s);
            return StatusCode(500, s);
        }
        return CreatedAtAction("SendMailMessage", "email");
    }
}
