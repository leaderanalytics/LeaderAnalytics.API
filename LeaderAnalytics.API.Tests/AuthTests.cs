using IdentityModel.Client;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeaderAnalytics.API.Tests
{
    public class AuthTests
    {
        private HttpClient apiClient;
        private const string apiURL = "https://localhost:5010/api/";    // trailing slash required
        private const string authURL = "https://localhost:5001";

        [SetUp]
        public void Setup()
        {
            
            apiClient = new HttpClient() { BaseAddress = new Uri(apiURL) };
        }

        [Test]
        public async Task server_is_running()
        {
            HttpResponseMessage response = await apiClient.GetAsync("/");
            Assert.AreEqual(200, response.StatusCode);
        }

        [Test]
        public async Task secure_access_is_denied_when_not_logged_in()
        {
            HttpResponseMessage response = await apiClient.GetAsync("status/SecureIdentity");
            Assert.AreEqual(401, response.StatusCode);
        }

        [Test]
        public async Task secure_access_is_granted_when_logged_in()
        {
            // create a httpClient to talk to the auth server
            var authClient = new HttpClient();
            var dissco = await authClient.GetDiscoveryDocumentAsync(authURL);

            var tokenResponse = await authClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = dissco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            Assert.IsFalse(dissco.IsError);
            // Set the token on the apiClient
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            // Acquire a secured resource
            HttpResponseMessage response = await apiClient.GetAsync("status/SecureIdentity");
            Assert.AreEqual(200, response.StatusCode);
        }
    }
}