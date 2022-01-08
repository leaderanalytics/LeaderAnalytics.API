namespace LeaderAnalytics.API;

public class Program
{

    public const string ConfigFileFolder = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\API";

    public static async Task Main(string[] args)
    {
        LeaderAnalytics.Core.EnvironmentName environmentName = RuntimeEnvironment.GetEnvironmentName();
        string logFolder = "."; // fallback location if we cannot read config
        Exception startupEx = null;
        IConfigurationRoot appConfig = null;

        try
        {
            appConfig = await BuildConfig(environmentName);
            logFolder = appConfig["Logging:LogFolder"];
        }
        catch (Exception ex)
        {
            startupEx = ex;
        }
        finally
        {
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(logFolder, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();
        }


        if (startupEx != null)
        {
            Log.Fatal("An exception occured during startup configuration.  Program execution will not continue.");
            Log.Fatal(startupEx.ToString());
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(2000);
            return;
        }


        try
        {
            Log.Information("Leader Analytics API - Program.Main started.");
            Log.Information("Environment is: {env}", environmentName);
            Log.Information("Log files will be written to {logRoot}", logFolder);
            CreateHostBuilder(args, appConfig).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            Log.Information("========== Leader Analytics API - Program.Main is shutting down. ============");
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
            .UseStartup<Startup>(x => new Startup(config))
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseSetting("detailedErrors", "true")
            .CaptureStartupErrors(true);
        }).UseServiceProviderFactory(new AutofacServiceProviderFactory());



    private async static Task<IConfigurationRoot> BuildConfig(LeaderAnalytics.Core.EnvironmentName envName)
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
            Task<Azure.Response<KeyVaultSecret>> emailAccountTask = client.GetSecretAsync("EmailAccount");
            Task<Azure.Response<KeyVaultSecret>> emailPasswordTask = client.GetSecretAsync("EmailPassword");
            await Task.WhenAll(emailAccountTask, emailPasswordTask);
            cfg["smtp:login"] = cfg["smtp:login"].Replace("{EmailAccount}", emailAccountTask.Result.Value.Value);
            cfg["smtp:auth"] = cfg["smtp:auth"].Replace("{EmailPassword}", emailPasswordTask.Result.Value.Value);
        }

        return cfg;
    }
}
