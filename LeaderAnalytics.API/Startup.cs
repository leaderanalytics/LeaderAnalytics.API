using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Mvc;

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
        // Security ----------------------------------------

        // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
        // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
        // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
        // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
        // JwtSecurityTokenHandler.DefaultMapInboundClaims = false;



        // https://docs.microsoft.com/en-us/answers/questions/688165/multiple-authentication-schems-azure-ad-and-azure.html
        //        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer("AzureAdB2C", options =>
        //{
        //            options.TokenValidationParameters.RoleClaimType = "name";
        //        })
        //        .AddJwtBearer("AzureAD", options =>
        //        {
        //            options.TokenValidationParameters.RoleClaimType = "roles";
        //        });



        // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       //         .AddMicrosoftIdentityWebApi(Configuration, "AzureAdB2C");
                .AddMicrosoftIdentityWebApi(Configuration, "AzureAd");

        //{
        //    //Configuration.Bind("AzureAd", options);
        //    Configuration.Bind("AzureAdB2C", options);
        //},        options =>

        //{
        //    //Configuration.Bind("AzureAd", options);
        //    Configuration.Bind("AzureAdB2C", options);
        //}); 



        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddMicrosoftIdentityWebApi(options =>
        //        {
        //            Configuration.Bind("AzureAdB2C", options);

        //            options.TokenValidationParameters.NameClaimType = "name";
        //        },
        //options => { Configuration.Bind("AzureAdB2C", options); });




        // Creating policies that wraps the authorization requirements.


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
