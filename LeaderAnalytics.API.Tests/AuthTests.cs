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
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task secure_access_is_denied_when_not_logged_in()
    {
        // create a new plain old http client with no credentials 
        HttpClient client = new HttpClient() { BaseAddress = new Uri(production ? localAPI_Address : config.APIBaseAddress) };

        // ...should pass when we try to access an unsecured API
        HttpResponseMessage response = await client.GetAsync("api/status/Identity");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // ...should fail when we try to access a secure API
        response = await client.GetAsync("api/status/SecureIdentity");
        string content = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task secure_access_is_granted_when_logged_in()
    {
        // Acquire a secured resource
        HttpResponseMessage response = await apiClient.GetAsync("api/status/SecureIdentity");
        string content = await response.Content.ReadAsStringAsync();
        Assert.That(HttpStatusCode.OK, Is.EqualTo(response.StatusCode));
    }
}
