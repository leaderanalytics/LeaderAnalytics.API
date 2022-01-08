namespace LeaderAnalytics.API.Services;

// This class must be registered with the DI container as a singleton.
public class CaptchService
{
    private List<SubmitHistory> submitHistory;
    private Random random;
    private int SEND_INTERVAL = 5;

    public CaptchService()
    {
        submitHistory = new List<SubmitHistory>();
        random = new Random();
    }


    public CaptchaImage GetCaptchaImage(string ipaddress)
    {
        string code = string.Empty;
        DateTime now = DateTime.UtcNow;

        // Check if an unexpired Captcha has already been assigned to this IP.  If so, use it.
        SubmitHistory ch = submitHistory.FirstOrDefault(x => x.IP_Address == ipaddress && x.CreateTime.AddHours(1) > now && !x.SubmitTime.HasValue);

        if (ch == null)
        {
            for (int i = 0; i < 3; i++)
                code = String.Concat(code, random.Next(10).ToString());

            ch = new SubmitHistory { IP_Address = ipaddress, CreateTime = DateTime.UtcNow, CaptchaCode = code };
            submitHistory.Add(ch);
        }

        CaptchaImage ci = new CaptchaImage(ch.CaptchaCode, 100, 50, "Century Schoolbook");
        return ci;
    }

    public string CanSubmit(string ip, string code)
    {
        ExpireSubmitHistory();
        DateTime now = DateTime.UtcNow;

        // Check for a send in the last five minutes
        if (submitHistory.Any(x => (x.IP_Address == ip && x.SubmitTime.HasValue && x.SubmitTime.Value.AddMinutes(SEND_INTERVAL) >= now)))
            return $"Please wait at least { SEND_INTERVAL.ToString() } minutes before attempting this activity again.";
        if (!submitHistory.Any(x => (x.IP_Address == ip && !x.SubmitTime.HasValue && x.CaptchaCode == code)))
            return "Invalid Captcha code.  Please try again.";
        else
            return null;
    }

    public void SetSubmitTime(string ip, string code)
    {
        submitHistory.First(x => x.IP_Address == ip && x.CaptchaCode == code).SubmitTime = DateTime.UtcNow;
    }

    private void ExpireSubmitHistory()
    {
        // Delete entries that were created more than one hour ago or where the user sent the email more than five minutes ago.
        DateTime now = DateTime.UtcNow;
        List<SubmitHistory> expired = submitHistory.Where(x => x.CreateTime.AddHours(1) < now || (x.SubmitTime.HasValue && x.SubmitTime.Value.AddMinutes(SEND_INTERVAL) < now)).ToList();

        foreach (SubmitHistory c in expired)
            submitHistory.Remove(c);
    }
}
