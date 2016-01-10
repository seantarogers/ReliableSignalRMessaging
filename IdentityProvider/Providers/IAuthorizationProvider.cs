namespace IdentityProvider.Providers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.OAuth;

    public interface IAuthorizationProvider
    {
        Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context);

        Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context);  
    }
}