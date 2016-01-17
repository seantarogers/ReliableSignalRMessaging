namespace IdentityProvider.Services
{
    using Microsoft.Owin.Security;

    public interface IJwtFormatter
    {
        string Protect(AuthenticationTicket authenticationTicket);
        AuthenticationTicket Unprotect(string protectedText);
    }
}