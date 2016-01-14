namespace IdentityProvider.Providers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using IdentityInfrastructure.Constants;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OAuth;

    public class AuthorizationProvider : OAuthAuthorizationServerProvider, IAuthorizationProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            await Task.Run(
                () =>
                    {
                        if (context == null)
                        {
                            return;
                        }
                        
                        if (context.UserName != "123" && context.Password != "mypassword")
                        {
                            context.Rejected();
                            return;
                        }

                        var claimsIdentity = CreateClaimsIdentity(context.UserName, context.Options.AuthenticationType);

                        var authenticationTicket = new AuthenticationTicket(claimsIdentity, null);
                        context.Validated(authenticationTicket);
                    });
        }

        private static ClaimsIdentity CreateClaimsIdentity(string userName, string authenticationType)
        {
            var claimsIdentity = new ClaimsIdentity(authenticationType);

            AddMandatoryClaims(claimsIdentity, userName.ToUpperInvariant());
            AddAudienceClaims(claimsIdentity);

            return claimsIdentity;
        }

        private static void AddAudienceClaims(ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaim(
                new Claim(IdentityConstants.AudienceClaimType, IdentityConstants.AllowedAudienceCode));
        }

        private static void AddMandatoryClaims(ClaimsIdentity claimsIdentity, string userName)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
            claimsIdentity.AddClaim(new Claim(IdentityConstants.IdentityProviderClaimType, userName));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
            claimsIdentity.AddClaim(new Claim(IdentityConstants.TokenExpiresOnClaimType, DateTime.UtcNow.AddMinutes(IdentityConstants.TokenDurationInMinutes).Ticks.ToString()));

            claimsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, IdentityConstants.AuthenticationMethod));
        }
    }
}