namespace LeaderAnalytics.API.Tests;

public class EMailTests : BaseTest
{

    [Test]
    public async Task can_send_email()
    {
        IAzureADConfig config = AzureADConfig.ReadFromConfigFile(configFilePath, "AzureAD");
        ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
        apiClient = helper.AuthorizedClient();

        if(production)
            apiClient.BaseAddress = new Uri("https://localhost:5010");

        string ip = "192.1.100.1";
        HttpResponseMessage response = await apiClient.GetAsync($"api/Captcha/CaptchaCode?ipaddress={ip}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        string captchaCode = await response.Content.ReadAsStringAsync();
        ContactRequest msg = new ContactRequest { To = "leaderanalytics@outlook.com", Msg = $"can_send_email test @  {DateTime.UtcNow.ToLongDateString()}", IP_Address = ip, CaptchaCode = captchaCode };
        response = await apiClient.PostAsync("api/Message/SendContactRequest", new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
}
