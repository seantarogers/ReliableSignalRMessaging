namespace Hub.Providers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.OAuth;

    public class TokenProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            await Task.Run(() => { context.Validated(); });
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            await Task.Run(
                () =>
                    {
                        var claimsIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, "token"));
                        claimsIdentity.AddClaim(new Claim("ExpiresOn", 6.ToString()));
                        context.Validated(claimsIdentity);
                    });
        }
    }
}