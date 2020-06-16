using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using LeaderAnalytics.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LeaderAnalytics.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string EnvironmentName;
        private IEnumerable<IEndPointConfiguration> EndPoints;
        private readonly string configFilePath;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            string devFilePath = string.Empty;
            EnvironmentName = env.EnvironmentName;

            if (EnvironmentName == "Development")
                devFilePath = "C:\\Users\\sam\\AppData\\Roaming\\LeaderAnalytics\\API";
            
            configFilePath = Path.Combine(devFilePath, $"appsettings.{env.EnvironmentName}.json");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(configFilePath, optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            
            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("ConfigureServices started");

            // Add framework services.
            services.AddMemoryCache();
            services.AddSession();
            services.AddDistributedMemoryCache();
            services.AddControllers();
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            
            // Security ----------------------------------------

            string authURL = Configuration["AuthURL"];

            if (string.IsNullOrEmpty(authURL))
                throw new Exception("Invalid configuration.  AuthURL must be set to a valid URL.");

            HttpWebResponse response = null;

            try
            {
                WebRequest request = HttpWebRequest.Create(authURL);
                request.Method = "HEAD";
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to contact authorization server at url: {authURL}.  Make sure the URL is valid and that the server is running.");
            }

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"The authorization server at url: {authURL} returned a status code indicating an error.  The response was: {response.StatusCode.ToString()}");

            // Add authentication server
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authURL;
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api1";
                });

            Log.Information("ConfigureServices ended");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILifetimeScope container)
        {
            app.UseDeveloperExceptionPage(); // must come before UseMvc.
            app.UseSession();
            app.UseCors(x => x.WithOrigins(new string[] {
                "http://www.leaderanalytics.com",
                "https://www.leaderanalytics.com",
                "http://leaderanalytics.com",
                "https://leaderanalytics.com",
                "http://localhost",
                "http://localhost:80",
                "http://localhost:63284",
                "http://dev.leaderanalytics.com",
                "http://leaderanalyticsweb.azurewebsites.net",
                "https://leaderanalyticsweb.azurewebsites.net",
                "https://localhost:5001",
                "https://localhost:44381",
                "https://leaderanalyticstweb-staging.azurewebsites.net"
            }).AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseExceptionHandler(options => {
                options.Run(
                    async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "text/html";
                        var ex = context.Features.Get<IExceptionHandlerFeature>();
                        if (ex != null)
                        {

                            var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";
                            await context.Response.WriteAsync(err).ConfigureAwait(false);
                            Log.Error(ex.ToString());
                        }
                    });
            });
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {

            // Autofac

            builder.Register<EMailClient>(c =>
            {
                IComponentContext cxt = c.Resolve<IComponentContext>();
                string p = configFilePath;
                EMailClient e = new EMailClient(p);
                return e;
            }).SingleInstance();

            // Don't build the container; that gets done for you.
        }
    }
}
