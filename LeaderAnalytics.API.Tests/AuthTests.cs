using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LeaderAnalytics.Core.Azure;

namespace LeaderAnalytics.API.Tests
{
    [TestFixture]
    public class AuthTests
    {
        
        private const string configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Web\\appsettings.development.json";
        private HttpClient apiClient;
        private AzureADConfig config;

        [SetUp]
        public void Setup()
        {
            config = AzureADConfig.ReadFromConfigFile(configFilePath);
            ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
            apiClient = helper.AuthorizedClient();
        }

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
            HttpResponseMessage response = await client.GetAsync("status/Identity");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // ...should fail when we try to access a secure API
            response = await client.GetAsync("status/SecureIdentity");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task secure_access_is_granted_when_logged_in()
        {
            // Acquire a secured resource
            HttpResponseMessage response = await apiClient.GetAsync("status/SecureIdentity");
            string content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}