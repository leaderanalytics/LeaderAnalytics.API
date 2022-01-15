using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LeaderAnalytics.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        Log.Information("ConfigureServices started");

        // Add framework services.
        services.AddMemoryCache(x => { x.SizeLimit = 100; });
        services.AddSession();
        services.AddDistributedMemoryCache();
        services.AddCors();
        IdentityModelEventSource.ShowPII = true;
        
        // Authentication

        // This configuration is necessary because we are using two jwt handlers - One for user auth, the other for machine-to-machine.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("AzureAdB2C", options =>
            {
                options.Authority = Configuration["AzureAdB2C:Instance"] + Configuration["AzureAdB2C:Domain"] + "/" + Configuration["AzureAdB2C:SignUpSignInPolicyId"] + "/v2.0";
                options.Audience = Configuration["AzureAdB2C:ClientId"];
            })
            .AddMicrosoftIdentityWebApi(Configuration, "AzureAd"); // machine-to-machine

        services.AddAuthorization(options =>
        {
            // The application should only allow tokens which roles claim contains "DaemonAppRole")
            options.AddPolicy("DaemonAppRole", policy => policy.RequireRole("DaemonAppRole"));
        });
        
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
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
                "https://localhost",
                "https://localhost:4200",
                "http://localhost:80",
                "http://localhost:63284",
                "http://dev.leaderanalytics.com",
                "http://leaderanalyticsweb.azurewebsites.net",
                "https://leaderanalyticsweb.azurewebsites.net",
                "https://localhost:5001",
                "https://localhost:5031",
                "https://leaderanalyticstweb-staging.azurewebsites.net"
            }).AllowAnyMethod().AllowAnyHeader());

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseExceptionHandler(options =>
        {
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
            EMailClient e = new EMailClient(Configuration);
            return e;
        }).SingleInstance();
        builder.RegisterType<Services.CaptchService>().SingleInstance();
        // Don't build the container; that gets done for you.
    }
}
