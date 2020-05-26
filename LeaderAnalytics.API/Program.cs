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

namespace LeaderAnalytics.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create logs in D:\home\serilog
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File("..\\..\\serilog\\API\\log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
               .CreateLogger();
            Log.Information("Leader Analytics API - logger created");
            Log.Information("ConfigureServices started");

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSetting("detailedErrors", "true")
                .CaptureStartupErrors(true);
            }).UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
