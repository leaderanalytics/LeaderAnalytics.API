using Microsoft.Extensions.Configuration;

namespace LeaderAnalytics.API.Tests;

[TestFixture]
public abstract class BaseTest
{
    protected string configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Web\\appsettings.development.json";
    protected HttpClient apiClient;
    protected IAzureADConfig config;
    protected bool production = false; // <------ Environment
    protected const string localAPI_Address = "https://localhost:5010";

    [SetUp]
    public async Task Setup()
    {
        
        string clientSecret = string.Empty;

        if (production)
        {
            configFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Web\\appsettings.production.json";
            IConfigurationRoot cfg = await ConfigBuilder.BuildConfig(Core.EnvironmentName.production);
        }

        config = AzureADB2CConfig.ReadFromConfigFile(configFilePath, "AzureADB2C");
        ClientCredentialsHelper helper = new ClientCredentialsHelper(config);
        apiClient = helper.AuthorizedClient();

        if (production)
            apiClient.BaseAddress = new Uri(localAPI_Address);
    }
}
