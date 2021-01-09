using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace LeaderAnalytics.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string logRoot = null;
            
            if (env == "Development")
                logRoot = "c:\\serilog\\API\\log";
            else
                logRoot = "..\\..\\serilog\\API\\log";   // Create logs in D:\home\serilog


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logRoot, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, buffered: true)
                .CreateLogger();

            try
            {
                Log.Information("Leader Analytics API - Program.Main started.");
                Log.Information("Environment is: {env}", env);
                CreateHostBuilder(args).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSetting("detailedErrors", "true")
                .CaptureStartupErrors(true);
            }).UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }

    // Note:  dont use string interpolation when logging. Example:
    // string userName = "sam";
    // Log.Information($"My name is {userName}");               // WRONG:  serilog cannot generate a variable for username
    // Log.Information("My name is {userName}", userName);      // Correct: userName:"sam" can optionally be generated in the log file as a searchable variable
    // Log.Information("User is: {@user}", user);               // Will serialize user
}
