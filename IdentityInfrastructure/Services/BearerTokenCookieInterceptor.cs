namespace IdentityInfrastructure.Services
{
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.OAuth;

    public class OAuthCookieAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var token = context.OwinContext.Request.Cookies["BearerToken"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Token = token;
            }
            return Task.FromResult<object>(null);
        }
    }
}
