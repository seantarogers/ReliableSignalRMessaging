﻿namespace IdentityInfrastructure.Constants
{
    public static class IdentityConstants
    {
        public const string AudienceClaimType = "aud";

        public const string TokenExpiresOnClaimType = "TokenExpiresOn";

        public const string AuthenticationMethod = "JwtToken";

        public const string Issuer = "IdentityProvider";

        public const string IdentityProviderClaimType =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        public const string HmacSha256Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";

        public const string HmacSha384Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";

        public const string HmacSha512Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";

        public const string Sha256Digest = "http://www.w3.org/2001/04/xmlenc#sha256";

        public const string Sha384Digest = "http://www.w3.org/2001/04/xmlenc#sha384";

        public const string Sha512Digest = "http://www.w3.org/2001/04/xmlenc#sha512";

        public static string AllowedAudienceCode = "aac";

        public const string TokenSigningKey =
            "+mvm3YdArP28yddjlTxe9YbrMB6rd8gGaTugU2Mmy+SR6OiANU6q0Sx5A3QsSmXikmYVkxEi7Ah96AlyUxU92r==";

        public const int TokenDurationInMinutes = 20;
    }
}