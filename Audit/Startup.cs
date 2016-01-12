using Audit;
using System.Collections.Generic;
using System.Web.Http;
using Autofac.Integration.WebApi;
using IdentityInfrastructure.Constants;
using IdentityInfrastructure.Services;
using Microsoft.Owin.Security.Jwt;
using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Audit
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            SetUpMiddleware(app);
            SetUpWebApi(app);
        }

        private static void SetUpWebApi(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(EndpointConfig.Container);

            app.UseAutofacWebApi(httpConfiguration);
            app.UseWebApi(httpConfiguration);
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
