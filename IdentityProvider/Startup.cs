using IdentityProvider;

using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace IdentityProvider
{
    using System;
    using System.Web.Http;

    using IdentityInfrastructure;
    using IdentityInfrastructure.Constants;

    using Providers;
    using Services;

    using Microsoft.Owin;
    using Microsoft.Owin.Security.OAuth;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            app.UseWebApi(config);

            SetupAuthorizationProvider(app);
        }

        private static void SetupAuthorizationProvider(
            IAppBuilder app)
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(IdentityConstants.TokenDurationInMinutes),
                Provider = new AuthorizationProvider(),
                AccessTokenFormat = new JwtFormattingService(
                    IdentityConstants.Issuer,
                    IdentityConstants.TokenSigningKey,
                    IdentityConstants.AllowedAudienceCode)
            };

            app.UseOAuthAuthorizationServer(oAuthServerOptions);
        }
    }
}