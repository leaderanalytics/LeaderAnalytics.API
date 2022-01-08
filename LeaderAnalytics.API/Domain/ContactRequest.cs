namespace LeaderAnalytics.API.Domain;

public class ContactRequest
{
    public string To { get; set; }
    public string Msg { get; set; }
    public string CaptchaCode { get; set; }
    public string IP_Address { get; set; }
}
