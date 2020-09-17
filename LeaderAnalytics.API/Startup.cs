using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using LeaderAnalytics.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Microsoft.Identity.Web;
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
            Log.Information("Start constructing Startup class.");
            Configuration = configuration;
            string devFilePath = string.Empty;
            EnvironmentName = env.EnvironmentName;

            if (EnvironmentName == "Development")
                devFilePath = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\API";
            
            configFilePath = Path.Combine(devFilePath, $"appsettings.{env.EnvironmentName}.json");

            if (File.Exists(configFilePath))
                Log.Information("Configuration file {f} exists.", configFilePath);
            else
                Log.Error("Configuration file {f} was not found.", configFilePath);

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(configFilePath, optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Log.Information("Constructing Startup class completed.");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("ConfigureServices started");

            // Add framework services.
            services.AddMemoryCache(x => { x.SizeLimit = 100; });
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

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddProtectedWebApi(Configuration);

            // Additional configuration
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.RoleClaimType = "roles";
            });

            // Creating policies that wraps the authorization requirements.
            services.AddAuthorization(options =>
            {
                // The application should only allow tokens which roles claim contains "DaemonAppRole")
                options.AddPolicy("DaemonAppRole", policy => policy.RequireRole("DaemonAppRole"));
            });
            Log.Information("ConfigureServices ended");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILifetimeScope container)
        {
            Log.Information("Configure Started.");
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
            Log.Information("Configure Completed.");
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
