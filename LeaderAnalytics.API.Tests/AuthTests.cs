namespace LeaderAnalytics.API.Tests;

public class AuthTests : BaseTest
{

    // --------------------------------------- \\
    // Set the correct environment in BaseTest \\
    // --------------------------------------- \\





    [Test]
    public async Task server_is_running()
    {
        HttpResponseMessage response = await apiClient.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task secure_access_is_denied_when_not_logged_in()
    {
        // create a new plain old http client with no credentials 
        HttpClient client = new HttpClient() { BaseAddress = new Uri(config.APIBaseAddress) };

        // ...should pass when we try to access an unsecured API
        HttpResponseMessage response = await client.GetAsync("api/status/Identity");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // ...should fail when we try to access a secure API
        response = await client.GetAsync("api/status/SecureIdentity");
        string content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Test]
    public async Task secure_access_is_granted_when_logged_in()
    {
        // Acquire a secured resource
        HttpResponseMessage response = await apiClient.GetAsync("api/status/SecureIdentity");
        string content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
