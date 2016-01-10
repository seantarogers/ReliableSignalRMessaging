namespace IdentityInfrastructure.Services
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.OAuth;

    public class BearerTokenInterceptor : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var queryStringToken = context.Request.Query.Get("bearer_token");
            if (string.IsNullOrWhiteSpace(queryStringToken))
            {
                return Task.FromResult<object>(null);
            }

            var regex = new Regex("bearer");
            context.Token = regex.Replace(queryStringToken, string.Empty).Trim();

            return Task.FromResult<object>(null);
        }
    }
}