namespace LeaderAnalytics.API;

public static class ConfigBuilder
{
    public const string ConfigFileFolder = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\API";

    public async static Task<IConfigurationRoot> BuildConfig(LeaderAnalytics.Core.EnvironmentName envName)
    {
        // if we are in development, load the appsettings file in the out-of-repo location.
        // if we are in prod, load appsettings.production.json and populate the secrets 
        // from the azure vault.

        string configFilePath = string.Empty;

        if (envName == LeaderAnalytics.Core.EnvironmentName.local || envName == LeaderAnalytics.Core.EnvironmentName.development)
            configFilePath = ConfigFileFolder;

        var cfg = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile(Path.Combine(configFilePath, $"appsettings.{envName}.json"), optional: false)
                    .AddEnvironmentVariables().Build();

        if (envName == LeaderAnalytics.Core.EnvironmentName.production)
        {
            var client = new SecretClient(new Uri("https://leaderanalyticsvault.vault.azure.net/"), new DefaultAzureCredential());
            Task<Azure.Response<KeyVaultSecret>> emailAccountTask = client.GetSecretAsync("LeaderAnalytics-EmailAccount");
            Task<Azure.Response<KeyVaultSecret>> emailPasswordTask = client.GetSecretAsync("LeaderAnalytics-EmailPassword");
            await Task.WhenAll(emailAccountTask, emailPasswordTask);
            cfg["smtp:login"] = cfg["smtp:login"].Replace("{EmailAccount}", emailAccountTask.Result.Value.Value);
            cfg["smtp:auth"] = cfg["smtp:auth"].Replace("{EmailPassword}", emailPasswordTask.Result.Value.Value);
        }

        return cfg;
    }
}
