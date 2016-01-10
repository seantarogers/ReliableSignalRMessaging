namespace HubSubscriber.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using Newtonsoft.Json.Linq;

    public class AccessTokenService : IAccessTokenService
    {
        public string GetToken()
        {
            var dictionary = new Dictionary<string, string>
                                 {
                                     { "grant_type", "password" },
                                     { "username", "123" },
                                     { "password", "mypassword" }
                                 };

            const string IdentityProviderUrl = "http://localhost:8095/token";
            using (var client = new HttpClient())
            using (var postResponse = client.PostAsync(IdentityProviderUrl, new FormUrlEncodedContent(dictionary)).Result)
            {
                if (!postResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException("Cannot get token from identityprovider");
                }

                return postResponse.Content.ReadAsStringAsync()
                    .Result;
            }
        }

        public string ExtractAccessToken(string content)
        {
            return ExtractContentFromToken(content, "access_token");
        }

        public DateTime ExtractAccessTokenExpiryDate(string content)
        {
            var expiryTimeInSeconds = ExtractContentFromToken(content, "expires_in");
            return DateTime.UtcNow.AddSeconds(long.Parse(expiryTimeInSeconds));
        }

        private static string ExtractContentFromToken(string content, string contentName)
        {
            var json = JObject.Parse(content);
            JToken jToken;
            if (!json.TryGetValue(contentName, out jToken))
            {
                throw new ApplicationException(string.Format("Cannot find item: {0} in Json.", contentName));
            }

            return jToken.ToString();
        }
    }
}