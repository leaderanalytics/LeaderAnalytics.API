namespace LeaderAnalytics.API;

public class Program
{

    

    public static async Task Main(string[] args)
    {
        LeaderAnalytics.Core.EnvironmentName environmentName = RuntimeEnvironment.GetEnvironmentName();
        string logFolder = "."; // fallback location if we cannot read config
        Exception startupEx = null;
        IConfigurationRoot appConfig = null;

        try
        {
            appConfig = await ConfigBuilder.BuildConfig(environmentName);
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
            .UseSetting("detailedErrors", "true")
            .CaptureStartupErrors(true);
        }).UseServiceProviderFactory(new AutofacServiceProviderFactory());



    
}
