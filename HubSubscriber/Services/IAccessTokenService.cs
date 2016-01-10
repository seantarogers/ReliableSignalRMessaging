namespace HubSubscriber.Services
{
    using System;

    public interface IAccessTokenService
    {
        string GetToken();
        DateTime ExtractAccessTokenExpiryDate(string tokenContent);
        string ExtractAccessToken(string tokenContent);
    }
}