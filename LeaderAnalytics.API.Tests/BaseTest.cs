using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LeaderAnalytics.Core.Azure;

namespace LeaderAnalytics.API.Tests
{
    [TestFixture]
    public abstract class BaseTest
    {
        //protected const string configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Web\\appsettings.development.json";
        protected const string configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Web\\appsettings.production.json";
        protected HttpClient apiClient;
        protected AzureADConfig config;

        [SetUp]
        public void Setup()
        {
            config = AzureADConfig.ReadFromConfigFile(configFilePath);
            ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
            apiClient = helper.AuthorizedClient();
        }
    }
}
