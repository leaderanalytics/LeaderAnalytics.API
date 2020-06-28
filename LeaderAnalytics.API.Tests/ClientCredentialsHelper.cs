using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeaderAnalytics.API.Tests
{
    public class ClientCredentialsHelper
    {
        private AzureADConfig config;
        private AuthenticationResult token;
        private IConfidentialClientApplication app;
        private HttpClient httpClient;

        public ClientCredentialsHelper(AzureADConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            this.config = config;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();
        }

        public HttpClient AuthorizedClient()
        {
            if (httpClient == null)
            {
                httpClient = HttpClientFactory.Create(new TokenExpiryHandler(config, app));
                httpClient.BaseAddress = new Uri(config.APIBaseAddress);
            }
            return httpClient;
        }

        private async Task SetAuthToken()
        {
            // if the token is not null and it expires 1 minute or more in the future there is nothing to do.
            if (token != null && token.ExpiresOn > DateTime.UtcNow.AddMinutes(1))
                return;


            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator
            string[] scopes = new string[] { config.APIScope };
            token = null;

            try
            {
                token = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new Exception("Invalid scope. The scope has to be of the form \"https://resourceurl/.default\"", ex);
            }

            if (token != null)
            {
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
            }
        }
    }

    public class TokenExpiryHandler : DelegatingHandler
    {
        private AzureADConfig config;
        private AuthenticationResult authToken;
        private IConfidentialClientApplication app;
        
        public TokenExpiryHandler(AzureADConfig config, IConfidentialClientApplication app)
        {
            this.config = config;
            this.app = app;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if the token is null or it expires less than 1 minute in the future acquire or renew the token.
            if (authToken == null || authToken.ExpiresOn < DateTime.UtcNow.AddMinutes(1))
            {
                // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
                // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
                // a tenant administrator
                string[] scopes = new string[] { config.APIScope };
                authToken = null;

                try
                {
                    authToken = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                }
                catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
                {
                    throw new Exception("Invalid scope. The scope has to be of the form \"https://resourceurl/.default\"", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while attempting to acquire a token.  See the inner exception for detail.", ex);
                }
            }

            if (authToken == null)
                throw new Exception("token is null.  An authorization token was not acquired.  Check configuration and network settings.");

            if (request.Headers.Accept == null || ! request.Headers.Accept.Any(m => m.MediaType == "application/json"))
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", authToken.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
