namespace LeaderAnalytics.API.Tests;

public class EMailTests : BaseTest
{
    [Test]
    public async Task can_send_email()
    {
        ContactRequest msg = new ContactRequest { To = "leaderanalytics@outlook.com", Msg = $"can_send_email test @  {DateTime.UtcNow.ToLongDateString()}" };
        HttpResponseMessage response = await apiClient.PostAsync("api/Message/SendContactRequest", new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
