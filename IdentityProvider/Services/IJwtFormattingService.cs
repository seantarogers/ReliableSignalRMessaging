namespace IdentityProvider.Services
{
    using Microsoft.Owin.Security;

    public interface IJwtFormattingService
    {
        string Protect(AuthenticationTicket authenticationTicket);

        AuthenticationTicket Unprotect(string protectedText);
    }
}