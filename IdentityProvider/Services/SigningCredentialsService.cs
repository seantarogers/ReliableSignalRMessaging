namespace IdentityProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;

    using IdentityInfrastructure.Constants;

    public class SigningCredentialsService : SigningCredentials
    {
        private static readonly Dictionary<int, string> SignatureMapper;
        private static readonly Dictionary<int, string> DigestMapper;

        static SigningCredentialsService()
        {
            SignatureMapper = new Dictionary<int, string>
                                  {
                                      { 32, IdentityConstants.HmacSha256Signature },
                                      { 48, IdentityConstants.HmacSha384Signature },
                                      { 64, IdentityConstants.HmacSha512Signature }
                                  };

            DigestMapper = new Dictionary<int, string>
                               {
                                   { 32, IdentityConstants.Sha256Digest },
                                   { 48, IdentityConstants.Sha384Digest },
                                   { 64, IdentityConstants.Sha512Digest }
                               };

        }

        public SigningCredentialsService(string base64EncodedKey)
            : this(Convert.FromBase64String(base64EncodedKey))
        {

        }

        private SigningCredentialsService(byte[] key)
            : base(new InMemorySymmetricSecurityKey(key),
                  CreateSignatureAlgorithm(key),
                  CreateDigestAlgorithm(key))
        {
        }

        protected static string CreateSignatureAlgorithm(byte[] key)
        {
            return SignatureMapper[key.Length];            
        }

        protected static string CreateDigestAlgorithm(byte[] key)
        {
            return DigestMapper[key.Length];
        }
    }
}