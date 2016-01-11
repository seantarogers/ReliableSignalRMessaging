using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Hub.Startup))]
namespace Hub
{
    using System.Collections.Generic;

    using Autofac.Integration.SignalR;

    using IdentityInfrastructure.Constants;
    using IdentityInfrastructure.Services;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Security.Jwt;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseAutofacMiddleware(ServiceHost.Container);
            SetUpMiddleware(app);
            SetUpSignalR(app);
        }

        private static void SetUpSignalR(IAppBuilder app)
        {
            var autofacResolver = new AutofacDependencyResolver(ServiceHost.Container);
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true, Resolver = autofacResolver };

            app.Map(
                "/signalr",
                map =>
                    {
                        map.RunSignalR(hubConfiguration);
                    });
        }

        private static void SetUpMiddleware(IAppBuilder app)
        {
            var jwtOptions = new JwtBearerAuthenticationOptions
            {
                AllowedAudiences =
                                        new List<string>
                                            {
                                                 IdentityConstants.AllowedAudienceCode
                                            },
                IssuerSecurityTokenProviders =
                                        new[] {
                                                 new SymmetricKeyIssuerSecurityTokenProvider
                                                     (IdentityConstants.Issuer,
                                                     IdentityConstants.TokenSigningKey)
                                            },
                Provider = new BearerTokenInterceptor()
            };

            app.UseJwtBearerAuthentication(jwtOptions);
        }
    }
}
